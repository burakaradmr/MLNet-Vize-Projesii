using Microsoft.ML;
using Microsoft.ML.Data;

// ============================================================
//  ÖRNEK 2 — METİN DUYGU ANALİZİ (İKİLİ SINIFLANDIRMA)
//  Veri: UCI "Sentiment Labelled Sentences" (3.000 gerçek yorum)
//        IMDb + Yelp + Amazon cümleleri, 1 = olumlu, 0 = olumsuz
//  Soru: Bir yorumun olumlu mu olumsuz mu olduğunu tahmin et.
//  Not: Veri seti İngilizce cümlelerden oluşuyor.
// ============================================================

// 1) ML.NET ana nesnesi.
var ml = new MLContext(seed: 1);

// 2) TSV dosyasini oku: 1. kolon yorum metni, 2. kolon etiket (1/0).
//    Metinlerde virgul bol oldugu icin ayirici olarak TAB kullaniliyor.
//    Etiket bool'a çevriliyor: 1 -> true (olumlu), 0 -> false (olumsuz).
var veri = ml.Data.LoadFromTextFile<Yorum>(
    path: Path.Combine("..", "veri", "duygu.csv"),
    separatorChar: '\t',
    hasHeader: true);

// 3) %80 eğitim, %20 test.
var bolum = ml.Data.TrainTestSplit(veri, testFraction: 0.2, seed: 1);

// 4) Pipeline:
//    a) FeaturizeText -> metni kelime kelime sayısal vektöre çevirir (TF-IDF benzeri).
//    b) SdcaLogisticRegression -> iki sınıflı (evet/hayır) sınıflandırıcı.
var pipeline = ml.Transforms.Text.FeaturizeText("Ozellikler", nameof(Yorum.yorum))
    .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression(
        labelColumnName: nameof(Yorum.duygu),
        featureColumnName: "Ozellikler",
        maximumNumberOfIterations: 100));

// 5) Eğit.
var model = pipeline.Fit(bolum.TrainSet);

// 6) Test verisiyle ölç.
var metrik = ml.BinaryClassification.Evaluate(
    model.Transform(bolum.TestSet),
    labelColumnName: nameof(Yorum.duygu));

Console.WriteLine("=== ÖRNEK 2: Yorum duygu analizi (ikili sınıflandırma) ===");
Console.WriteLine($"Eğitim satırı : {SatirSayisi(ml, bolum.TrainSet)}");
Console.WriteLine($"Test satırı   : {SatirSayisi(ml, bolum.TestSet)}");
Console.WriteLine($"Doğruluk (Accuracy) : {metrik.Accuracy:P1}");
Console.WriteLine($"AUC                 : {metrik.AreaUnderRocCurve:F3}  (0.5 = rastgele, 1.0 = mükemmel)");
Console.WriteLine($"F1 skoru            : {metrik.F1Score:F3}");
Console.WriteLine($"Kesinlik / Duyarlılık: {metrik.PositivePrecision:P1} / {metrik.PositiveRecall:P1}");
Console.WriteLine();

// 7) Yeni cümlelerle tahmin.
var motor = ml.Model.CreatePredictionEngine<Yorum, YorumTahmini>(model);

var denemeler = new[]
{
    "This movie was absolutely wonderful, I loved every minute of it.",
    "Terrible product, it broke after two days and support ignored me.",
    "The battery life is okay but the screen is a bit dim.",
    "Best purchase I have made this year, works perfectly."
};

Console.WriteLine("Örnek tahminler:");
foreach (var cumle in denemeler)
{
    var t = motor.Predict(new Yorum { yorum = cumle });
    var etiket = t.olumlu ? "OLUMLU" : "OLUMSUZ";
    // Probability her zaman OLUMLU sınıfın olasılığıdır.
    // Olumsuz tahminde modelin güveni: 1 - Probability.
    float guven = t.olumlu ? t.olasilik : 1f - t.olasilik;
    Console.WriteLine($"  [{etiket}] %{guven * 100:F1} güven — {cumle}");
}
Console.WriteLine();
Console.WriteLine("Not: Aynı metin farklı veri setinde farklı sonuç verebilir; model 3.000 yorumla eğitildi.");

// Yardımcı: bir IDataView kaç satır içeriyor? (bölünmüş veride GetRowCount null döner)
static int SatirSayisi(MLContext ml, IDataView dv)
    => ml.Data.CreateEnumerable<Yorum>(dv, reuseRowObject: false).Count();

// ------------------------------------------------------------
// Veri sınıfı
// ------------------------------------------------------------
public class Yorum
{
    [LoadColumn(0)] public string yorum { get; set; } = "";
    [LoadColumn(1)] public bool duygu { get; set; }        // true = olumlu
}

// Tahmin çıktısı: PredictedLabel = tahmin, Probability = güven, Score = ham skor
public class YorumTahmini
{
    [ColumnName("PredictedLabel")] public bool olumlu { get; set; }
    [ColumnName("Probability")] public float olasilik { get; set; }
    [ColumnName("Score")] public float skor { get; set; }
}
