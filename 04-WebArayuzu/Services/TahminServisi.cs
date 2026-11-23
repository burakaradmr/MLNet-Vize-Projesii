using _04_WebArayuzu.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace _04_WebArayuzu.Services;

// ============================================================
//  TAHMİN SERVİSİ
//  Görev: duygu ZIP modelini yüklemek; sınav notu ve hal modellerini CSV ile eğitmek,
//         sonra gelen isteklerde sadece tahmin üretmek.
//
//  Neden singleton? Program.cs'te AddSingleton ile kaydedildi.
//  Böylece model her istekte yeniden yüklenmez (bu çok yavaş olurdu).
// ============================================================

public class TahminServisi
{
    private readonly PredictionEngine<Ogrenci, NotTahmini> _notMotoru;
    private readonly PredictionEngine<Yorum, YorumTahmini> _duyguMotoru;
    private readonly PredictionEngine<HalSatisi, HalTahmini> _halMotoru;

    // PredictionEngine aynı anda iki isteği güvenle işleyemez; kilit ile sıraya sokuyoruz.
    private readonly object _kilit = new();
    private readonly Dictionary<string, (float Kg, float Adet)> _paketVerileri = new();

    public TahminServisi(IWebHostEnvironment ortam)
    {
        // Modeller proje kökündeki Ml/Modeller klasöründe durur.
        var klasor = Path.Combine(ortam.ContentRootPath, "Ml", "Modeller");

        var ml = new MLContext(seed: 1);

        var notCsvYolu = Path.Combine(ortam.ContentRootPath, "veri", "sinav_notlari.csv");
        if (!File.Exists(notCsvYolu))
            notCsvYolu = Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "veri", "sinav_notlari.csv"));
        if (!File.Exists(notCsvYolu)) throw new FileNotFoundException("Sınav notu veri dosyası bulunamadı", notCsvYolu);
        var notVeri = ml.Data.LoadFromTextFile<Ogrenci>(notCsvYolu, separatorChar: ',', hasHeader: true);
        var notPipeline = ml.Transforms.Concatenate("Ozellikler", nameof(Ogrenci.CalismaSaati))
            .Append(ml.Transforms.NormalizeMinMax("Ozellikler"))
            .Append(ml.Regression.Trainers.Sdca(
                labelColumnName: nameof(Ogrenci.SinavNotu),
                featureColumnName: "Ozellikler",
                maximumNumberOfIterations: 200));
        var notModel = notPipeline.Fit(notVeri);
        var duyguModel = ml.Model.Load(Path.Combine(klasor, "duygu-model.zip"), out _);

        // Hal modelini eski zip yerine yedi faturanın 74 satırından eğitiyoruz.
        var csvYolu = Path.Combine(ortam.ContentRootPath, "veri", "hal_satislari.csv");
        if (!File.Exists(csvYolu))
            csvYolu = Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "veri", "hal_satislari.csv"));
        if (!File.Exists(csvYolu)) throw new FileNotFoundException("Hal verisi bulunamadı", csvYolu);
        var ham = ml.Data.LoadFromTextFile<HalFatura>(csvYolu, separatorChar: ',', hasHeader: true);
        var satirlar = ml.Data.CreateEnumerable<HalFatura>(ham, reuseRowObject: false).ToList();
        if (satirlar.Count == 0)
            throw new InvalidOperationException("Hal verisi boş; tahmin modeli eğitilemez.");
        foreach (var grup in satirlar.GroupBy(s => s.urun))
            _paketVerileri[grup.Key] = (grup.Sum(s => s.kg), grup.Sum(s => s.adet));
        var egitim = ml.Data.LoadFromEnumerable(
            satirlar.Select(s => new HalSatisi { Urun = s.urun, Kg = s.kg, Fiyat = s.birim_fiyat_tl }));
        var halPipeline = ml.Transforms.Categorical.OneHotEncoding("UrunKod", nameof(HalSatisi.Urun))
            .Append(ml.Transforms.Concatenate("Ozellikler", "UrunKod"))
            .Append(ml.Regression.Trainers.Sdca(
                labelColumnName: nameof(HalSatisi.Fiyat),
                featureColumnName: "Ozellikler",
                maximumNumberOfIterations: 200));
        var halModel = halPipeline.Fit(egitim);

        _notMotoru = ml.Model.CreatePredictionEngine<Ogrenci, NotTahmini>(notModel);
        _duyguMotoru = ml.Model.CreatePredictionEngine<Yorum, YorumTahmini>(duyguModel);
        _halMotoru = ml.Model.CreatePredictionEngine<HalSatisi, HalTahmini>(halModel);
    }

    // 1) Sentetik çalışma saati -> tahmini sınav notu
    public float NotTahmin(float calismaSaati)
    {
        lock (_kilit)
        {
            return Math.Clamp(_notMotoru.Predict(new Ogrenci { CalismaSaati = calismaSaati }).Skor, 0f, 100f);
        }
    }

    // 2) Duygu: olumlu mu + modelin bu karara güveni (%)
    //    Probability her zaman OLUMLU sınıfın olasılığıdır;
    //    olumsuz tahminde güven = 1 - Probability.
    public (bool olumlu, float guven) DuyguTahmin(string yorum)
    {
        lock (_kilit)
        {
            var t = _duyguMotoru.Predict(new Yorum { yorum = yorum });
            float guven = t.olumlu ? t.olasilik : 1f - t.olasilik;
            return (t.olumlu, guven);
        }
    }

    // 3) Hal: beklenen TL/kg. Model yalnızca ürünü kullanıyor (74 satır, 7 fatura).

    public float? HalTahmin(string urun)
    {
        if (!_paketVerileri.ContainsKey(urun)) return null;
        lock (_kilit)
        {
            return _halMotoru.Predict(new HalSatisi { Urun = urun }).BeklenenFiyat;
        }
    }

    public List<SevkiyatTahmini> BugunkuSevkiyat()
    {
        var siparis = new (string Urun, int Adet)[]
        {
            ("PIRASA", 10), ("KEREVIZ", 18), ("BROKOLI", 4),
            ("BEYAZ LAHANA", 4), ("KARALAHANA", 53)
        };
        return siparis.Select(s =>
        {
            if (!_paketVerileri.TryGetValue(s.Urun, out var toplam) || toplam.Adet <= 0)
                return new SevkiyatTahmini(s.Urun, s.Adet, null, null, null);
            float kg = s.Adet * toplam.Kg / toplam.Adet;
            var fiyat = HalTahmin(s.Urun);
            return new SevkiyatTahmini(s.Urun, s.Adet, kg, fiyat, fiyat * kg);
        }).ToList();
    }
}

public record SevkiyatTahmini(string Urun, int Paket, float? Kg, float? Fiyat, float? Brut);

public class HalFatura
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
