using Microsoft.ML;
using Microsoft.ML.Data;

// ============================================================
//  ÖRNEK 3 — HAL SATIŞ FİYATI TAHMİNİ + UCUZA GİDEN MAL UYARISI
//  Veri: anonimleştirilmiş hal satış satırları
//        Ürünlerin satış dökümü
//  İki iş tek modelde:
//    1) TAHMİN (regresyon): yalnızca ürün -> beklenen TL/kg
//    2) ANOMALİ: gerçek fiyat, beklenenin çok altındaysa "ucuza gitti" uyarısı
//
//  Anomali neden ayrı bir model değil? Çünkü ayrı etiket yok.
//  Modelin hatası (gerçek - tahmin) büyük ve negatifse mal ucuza gitmiş demektir.
// ============================================================

var ml = new MLContext(seed: 1);

// 1) Fatura CSV'sini oku. Ayırıcı virgül, ilk satır başlık.
var hamVeri = ml.Data.LoadFromTextFile<FaturaSatiri>(
    path: Path.Combine("..", "veri", "hal_satislari.csv"),
    separatorChar: ',',
    hasHeader: true);

var satirlar = ml.Data.CreateEnumerable<FaturaSatiri>(hamVeri, reuseRowObject: false).ToList();

// 2) Modelin göreceği satırlar.
//    ÖNEMLİ 1: "gün" özelliğini modele vermiyoruz. Eğitim verisinde yalnızca bazı tarihleri
//    içeriyor; 22. gün girilince model doğrusal olarak uçuk fiyat üretiyordu (ekstrapolasyon).
//    ÖNEMLİ 2: "kg" de modele verilmiyor. 74 satırda kg-fiyat ilişkisi gürültüden ibaret;
//    kg yalnızca beklenen geliri hesaplamak için kullanılıyor.
var veriListesi = satirlar.Select(s => new HalSatisi
{
    Urun = s.urun,
    Kg = s.kg,
    Fiyat = s.birim_fiyat_tl,              // MODELİN ÖĞRENDİĞİ HEDEF (TL/kg)
    Gun = float.Parse(s.tarih.Split('-')[2])   // YYYY-MM-DD biçiminden gün (01-31)
}).ToList();

Console.WriteLine("=== ÖRNEK 3: Hal satış fiyatı tahmini + ucuza giden mal uyarısı ===");
Console.WriteLine($"Toplam satır: {veriListesi.Count} (gerçek faturalardan)");
Console.WriteLine("Ürünler: " + string.Join(", ", veriListesi.Select(v => v.Urun).Distinct()));
Console.WriteLine();

var veri = ml.Data.LoadFromEnumerable(veriListesi);

// 3) Veri az (74 satır, 7 fatura) olduğu için tek bir %80/%20 bölünmesi şansa bağlı kalır.
//    5 katlı çapraz doğrulama: veri 5 parçaya bölünür, her parça sırayla test olur,
//    metriklerin ortalaması alınır. Küçük veri setlerinde daha dürüst ölçüm.

// 4) Pipeline:
//    a) Ürün metin -> one-hot vektör (kereviz/pırasa gibi kategoriler sayıya çevrilir)
//    b) Yalnızca ürün kodu özellik vektörüne girer
//    c) Sdca regresyon: TL/kg tahmini
// Tek özellik: ürün. Model pratikte "her ürünün beklenen fiyat seviyesini" öğrenir.
// Bu, 74 satırlık veriyle savunulabilir en sade model.
var pipeline = ml.Transforms.Categorical.OneHotEncoding("UrunKod", nameof(HalSatisi.Urun))
    .Append(ml.Transforms.Concatenate("Ozellikler", "UrunKod"))
    .Append(ml.Regression.Trainers.Sdca(
        labelColumnName: nameof(HalSatisi.Fiyat),
        featureColumnName: "Ozellikler",
        maximumNumberOfIterations: 200));

var katmanlar = ml.Regression.CrossValidate(veri, pipeline, numberOfFolds: 5, labelColumnName: nameof(HalSatisi.Fiyat));

// 5) Metrikler: 5 katın ortalaması
double r2 = katmanlar.Average(k => k.Metrics.RSquared);
double mae = katmanlar.Average(k => k.Metrics.MeanAbsoluteError);
double rmse = katmanlar.Average(k => k.Metrics.RootMeanSquaredError);

Console.WriteLine($"Çapraz doğrulama: 5 kat, {veriListesi.Count} satır");
Console.WriteLine($"R2            : {r2:F3}   (1.0 mükemmel, 0 ortalama kadar iyi)");
Console.WriteLine($"MAE           : {mae:F2} TL/kg (ortalama sapma)");
Console.WriteLine($"RMSE          : {rmse:F2} TL/kg");
Console.WriteLine();

// 6) Anomali taramasını tüm veride yapabilmek için modeli tüm veriyle bir kez daha eğit.
var model = pipeline.Fit(veri);

// 7) TÜM satırlar için beklenen fiyatı hesapla ve hatayı bul.
var motor = ml.Model.CreatePredictionEngine<HalSatisi, HalTahmini>(model);

Console.WriteLine("Ürün          Tarih   Kg   Gerçek  Beklenen   Fark    Durum");
Console.WriteLine("--------------------------------------------------------------");

int anomaliSayisi = 0;
foreach (var s in veriListesi.OrderBy(v => v.Urun).ThenBy(v => v.Gun))
{
    var tahmin = motor.Predict(s);
    float fark = s.Fiyat - tahmin.Beklenen;

    // Kural: gerçek fiyat, beklenen fiyatın %20'sinden fazla altındaysa uyarı.
    // (Küçük sapmalar normal; asıl kayıp büyük düşüşlerde oluyor.)
    bool uyari = s.Fiyat < tahmin.Beklenen * 0.80f;
    if (uyari) anomaliSayisi++;

    Console.WriteLine($"{s.Urun,-13} {s.Gun,5:00}  {s.Kg,4:0}  {s.Fiyat,6:F2}  {tahmin.Beklenen,8:F2}  {fark,6:F2}   {(uyari ? "UYARI: ucuza gitti" : "normal")}");
}

Console.WriteLine("--------------------------------------------------------------");
Console.WriteLine($"Eşik altında kalan satır: {anomaliSayisi} / {veriListesi.Count}");
Console.WriteLine();

// 8) Ürün bazlı özet: hangi ürün ne kadar getirmiş, ortalama kaça gitmiş.
Console.WriteLine("Ürün bazlı özet:");
foreach (var grup in veriListesi.GroupBy(v => v.Urun).OrderByDescending(g => g.Sum(x => x.Kg * x.Fiyat)))
{
    float kg = grup.Sum(x => x.Kg);
    float tl = grup.Sum(x => x.Kg * x.Fiyat);
    Console.WriteLine($"  {grup.Key,-12} {kg,5:0} kg  {tl,10:N0} TL  ortalama {tl / kg:F2} TL/kg");
}
Console.WriteLine();
Console.WriteLine("Not: 7 faturadaki 74 satır azdır; model yalnızca ürün bazında fiyat seviyesi öğrenir.");
Console.WriteLine("     Aynı faturadaki satırlar ilişkili olduğundan 5 katlı CV iyimser olabilir.");

// ------------------------------------------------------------
// Sınıflar
// ------------------------------------------------------------
// CSV satırı: her property bir kolona bağlı olmak zorunda.
public class FaturaSatiri
{
    [LoadColumn(0)] public string tarih { get; set; } = "";
    [LoadColumn(1)] public string fatura_no { get; set; } = "";
    [LoadColumn(2)] public string urun { get; set; } = "";
    [LoadColumn(3)] public float adet { get; set; }
    [LoadColumn(4)] public float kg { get; set; }
    [LoadColumn(5)] public float birim_fiyat_tl { get; set; }
    [LoadColumn(6)] public float tutar_tl { get; set; }
    [LoadColumn(7)] public float net_oran { get; set; }
}

// Modele verilen satır
public class HalSatisi
{
    public string Urun { get; set; } = "";
    public float Kg { get; set; }
    public float Fiyat { get; set; }
    public float Gun { get; set; }   // modele özellik olarak verilmez, sadece gösterim
}

// Modelin çıktısı: Score = beklenen TL/kg
public class HalTahmini
{
    [ColumnName("Score")] public float Beklenen { get; set; }
}
