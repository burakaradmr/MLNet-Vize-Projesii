using Microsoft.ML.Data;

// ============================================================
//  MODEL GİRDİ/ÇIKTI SINIFLARI
//  Bu sınıflar, modeli eğiten 05-ModelEgitici projesindeki
//  sınıflarla aynı isim ve sıradadır. Model dosyası bu isimlere
//  göre bağlanır; isim değişirse tahmin çalışmaz.
// ============================================================

namespace _04_WebArayuzu.Models;

// ---------- 1) SINAV NOTU TAHMİNİ (SENTETİK VERİ) ----------
public class Ogrenci
{
    [LoadColumn(0)] public float CalismaSaati { get; set; }
    [LoadColumn(1)] public float SinavNotu { get; set; }
}
public class NotTahmini
{
    [ColumnName("Score")] public float Skor { get; set; }
}

// ---------- 2) DUYGU ANALİZİ ----------
public class Yorum
{
    public string yorum { get; set; } = "";
    public bool duygu { get; set; }        // eğitimde etiket; tahminde kullanılmaz
}

public class YorumTahmini
{
    [ColumnName("PredictedLabel")] public bool olumlu { get; set; }
    [ColumnName("Probability")] public float olasilik { get; set; }  // OLUMLU sınıfın olasılığı
    [ColumnName("Score")] public float skor { get; set; }
}

// ---------- 3) HAL SATIŞ FİYATI (gerçek faturalardan) ----------
public class HalSatisi
{
    public string Urun { get; set; } = "";   // KEREVIZ / KARALAHANA / PIRASA / BROKOLI / KARNABAHAR
    public float Kg { get; set; }
    public float Fiyat { get; set; }         // TL/kg — modelin öğrendiği hedef
}

public class HalTahmini
{
    [ColumnName("Score")] public float BeklenenFiyat { get; set; }   // TL/kg
}
