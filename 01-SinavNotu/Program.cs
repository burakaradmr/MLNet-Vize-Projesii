using Microsoft.ML;
using Microsoft.ML.Data;

// EĞİTİM DEMOSU: 80 SENTETİK öğrenci örneği. Gerçek öğrencilerin notları değildir.
// Öğrenilen ilişki: çalışma saati -> sınav notu (regresyon).
var ml = new MLContext(seed: 1);
var dosya = Path.Combine("..", "veri", "sinav_notlari.csv");
var veri = ml.Data.LoadFromTextFile<Ogrenci>(
    path: dosya, separatorChar: ',', hasHeader: true);

var bolum = ml.Data.TrainTestSplit(veri, testFraction: 0.2, seed: 1);
var pipeline = ml.Transforms.Concatenate("Ozellikler", nameof(Ogrenci.CalismaSaati))
    .Append(ml.Transforms.NormalizeMinMax("Ozellikler"))
    .Append(ml.Regression.Trainers.Sdca(
        labelColumnName: nameof(Ogrenci.SinavNotu),
        featureColumnName: "Ozellikler",
        maximumNumberOfIterations: 200));

var model = pipeline.Fit(bolum.TrainSet);
var metrik = ml.Regression.Evaluate(
    model.Transform(bolum.TestSet), labelColumnName: nameof(Ogrenci.SinavNotu));

Console.WriteLine("=== SINAV NOTU TAHMİNİ — SENTETİK VERİ DEMOSU ===");
Console.WriteLine("Veri: 80 sentetik kayıt; yaklaşık %80 eğitim / %20 test");
Console.WriteLine($"R²: {metrik.RSquared:F3} | MAE: {metrik.MeanAbsoluteError:F2} puan | RMSE: {metrik.RootMeanSquaredError:F2} puan");

var motor = ml.Model.CreatePredictionEngine<Ogrenci, NotTahmini>(model);
foreach (float saat in new float[] { 1, 3, 5 })
{
    float hamNot = motor.Predict(new Ogrenci { CalismaSaati = saat }).Skor;
    Console.WriteLine($"{saat:F0} saat çalışma -> tahmini {Math.Clamp(hamNot, 0f, 100f):F1} / 100");
}
Console.WriteLine("Uyarı: Sentetik ve tek özellikli örnek gerçek öğrenci başarısını öngörmez.");

public class Ogrenci
{
    [LoadColumn(0)] public float CalismaSaati { get; set; }
    [LoadColumn(1)] public float SinavNotu { get; set; }
}

public class NotTahmini
{
    [ColumnName("Score")] public float Skor { get; set; }
}
