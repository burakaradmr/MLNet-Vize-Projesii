using Microsoft.ML;
using Microsoft.ML.Data;

// ============================================================
//  ÖRNEK 1 — SINAV NOTU TAHMİNİ (GERÇEK VERİ: UCI Student Performance)
//  Kaynak: Cortez & Silva (2008), "Using Data Mining to Predict Secondary
//          School Student Performance", Portekiz lise verisi (student-por.csv)
//  Hedef : G3 (yıl sonu notu, 0-20)
//  Girdi : studytime (haftalık çalışma kategorisi 1-4), failures, higher,
//          absences, G1 (1. dönem), G2 (2. dönem)
//
//  İki model AYNI eğitim/test bölünmesiyle karşılaştırılır:
//    - SDCA regresyon (doğrusal)
//    - FastTree regresyon (ağaç topluluğu)
//  Kazanan model 04-WebArayuzu/Ml/Modeller/not-model.zip olarak kaydedilir.
// ============================================================

var ml = new MLContext(seed: 1);

var veri = ml.Data.LoadFromTextFile<Ogrenci>(
    path: Path.Combine("..", "veri", "sinav_notu_model.csv"),
    separatorChar: ',',
    hasHeader: true);

// Sabit bölünme: iki model de tam olarak aynı satırları görsün.
var bolum = ml.Data.TrainTestSplit(veri, testFraction: 0.2, seed: 1);

// Özellikler: 6 sütun. NormalizeMinMax yalnızca EĞİTİM verisiyle öğrenilir
// (pipeline'ın içinde olduğu için test verisine sızma olmaz).
var ozellikler = ml.Transforms.Concatenate("Ozellikler",
        nameof(Ogrenci.studytime), nameof(Ogrenci.failures), nameof(Ogrenci.higher),
        nameof(Ogrenci.absences), nameof(Ogrenci.G1), nameof(Ogrenci.G2))
    .Append(ml.Transforms.NormalizeMinMax("Ozellikler"));

var sdcaPipeline = ozellikler
    .Append(ml.Regression.Trainers.Sdca(
        labelColumnName: nameof(Ogrenci.G3),
        featureColumnName: "Ozellikler",
        maximumNumberOfIterations: 200));

var fastTreePipeline = ozellikler
    .Append(ml.Regression.Trainers.FastTree(
        labelColumnName: nameof(Ogrenci.G3),
        featureColumnName: "Ozellikler",
        numberOfLeaves: 31,
        numberOfTrees: 300,
        minimumExampleCountPerLeaf: 5));

var sdcaModel = sdcaPipeline.Fit(bolum.TrainSet);
var fastTreeModel = fastTreePipeline.Fit(bolum.TrainSet);

var sdcaMetrik = ml.Regression.Evaluate(sdcaModel.Transform(bolum.TestSet), labelColumnName: nameof(Ogrenci.G3));
var fastTreeMetrik = ml.Regression.Evaluate(fastTreeModel.Transform(bolum.TestSet), labelColumnName: nameof(Ogrenci.G3));

Console.WriteLine("=== SINAV NOTU TAHMİNİ — UCI Student Performance (Portekiz) ===");
int egitimSatir = ml.Data.CreateEnumerable<Ogrenci>(bolum.TrainSet, false).Count();
int testSatir = ml.Data.CreateEnumerable<Ogrenci>(bolum.TestSet, false).Count();
Console.WriteLine($"Eğitim: {egitimSatir} satır | Test: {testSatir} satır");
Console.WriteLine($"Hedef: G3 (0-20) | Özellik: studytime, failures, higher, absences, G1, G2");
Console.WriteLine();
Console.WriteLine("MODEL KARŞILAŞTIRMASI (aynı test seti)");
Console.WriteLine($"  SDCA      -> R² {sdcaMetrik.RSquared:F3} | MAE {sdcaMetrik.MeanAbsoluteError:F2} | RMSE {sdcaMetrik.RootMeanSquaredError:F2}");
Console.WriteLine($"  FastTree  -> R² {fastTreeMetrik.RSquared:F3} | MAE {fastTreeMetrik.MeanAbsoluteError:F2} | RMSE {fastTreeMetrik.RootMeanSquaredError:F2}");
Console.WriteLine();

// Seçim ölçütü: test RMSE (düşük olan kazanır).
bool fastTreeKazandi = fastTreeMetrik.RootMeanSquaredError < sdcaMetrik.RootMeanSquaredError;
// İki eğitici farklı TransformerChain tipi döndürür; ortak arayüz ITransformer kullanılır.
ITransformer secilenModel = fastTreeKazandi ? (ITransformer)fastTreeModel : sdcaModel;
var secilenAd = fastTreeKazandi ? "FastTree" : "SDCA";
Console.WriteLine($"Seçilen model: {secilenAd} (daha düşük RMSE)");

// Modeli ZIP olarak kaydet; web uygulaması bunu yükler.
var cikti = Path.Combine("..", "04-WebArayuzu", "Ml", "Modeller", "not-model.zip");
Directory.CreateDirectory(Path.GetDirectoryName(cikti)!);
ml.Model.Save(secilenModel, bolum.TrainSet.Schema, cikti);
Console.WriteLine($"Kaydedildi: {cikti} ({new FileInfo(cikti).Length / 1024} KB)");
Console.WriteLine();

// Örnek tahminler (G1/G2 0-20 ölçeğinde)
var motor = ml.Model.CreatePredictionEngine<Ogrenci, NotTahmini>(secilenModel);
var ornekler = new[]
{
    new Ogrenci { studytime = 1, failures = 0, higher = 1, absences = 4,  G1 = 8,  G2 = 9  },
    new Ogrenci { studytime = 2, failures = 0, higher = 1, absences = 2,  G1 = 11, G2 = 12 },
    new Ogrenci { studytime = 3, failures = 1, higher = 1, absences = 10, G1 = 13, G2 = 14 },
    new Ogrenci { studytime = 4, failures = 0, higher = 1, absences = 0,  G1 = 16, G2 = 17 }
};

Console.WriteLine("Örnek tahminler (G3, 0-20):");
foreach (var o in ornekler)
{
    float tahmin = Math.Clamp(motor.Predict(o).Skor, 0f, 20f);
    Console.WriteLine($"  studytime {o.studytime} | failures {o.failures} | G1 {o.G1}, G2 {o.G2} -> G3 ≈ {tahmin:F1}/20 ({tahmin * 5:F0}/100)");
}

Console.WriteLine();
Console.WriteLine("Not: Veri Portekiz'deki lise öğrencilerine aittir (2005-2006).");
Console.WriteLine("     Türkiye'deki bir öğrencinin gerçek sınav sonucunu garanti etmez.");

// ------------------------------------------------------------
public class Ogrenci
{
    [LoadColumn(0)] public float studytime { get; set; }   // 1-4 kategori (haftalık)
    [LoadColumn(1)] public float failures { get; set; }    // 0-3
    [LoadColumn(2)] public float higher { get; set; }      // 1 = yükseköğrenim istiyor
    [LoadColumn(3)] public float absences { get; set; }    // 0-93
    [LoadColumn(4)] public float G1 { get; set; }          // 0-20
    [LoadColumn(5)] public float G2 { get; set; }          // 0-20
    [LoadColumn(6)] public float G3 { get; set; }          // hedef, 0-20
}

public class NotTahmini
{
    [ColumnName("Score")] public float Skor { get; set; }
}
