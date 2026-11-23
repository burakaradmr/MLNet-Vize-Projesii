using Microsoft.ML.Data;

// ============================================================
//  MODEL GİRDİ/ÇIKTI SINIFLARI
//  Eğitimde kullanılan sütun isimleri ve türleriyle uyumlu olmalıdır.
//  Özellik isimlerini değiştirmek kaydedilmiş modelin giriş şemasını bozabilir.
// ============================================================

namespace _04_WebArayuzu.Models;

// ---------- 1) SINAV NOTU TAHMİNİ (UCI VERİSİ) ----------
// UCI Student Performance (Portekiz) — 6 özellik, hedef G3 (0-20)
public class Ogrenci
{
    public float studytime { get; set; }   // 1-4 haftalık çalışma kategorisi
    public float failures { get; set; }    // 0-3
    public float higher { get; set; }      // 1 = yükseköğrenim istiyor
    public float absences { get; set; }    // 0-93
    public float G1 { get; set; }          // 0-20
    public float G2 { get; set; }          // 0-20
    public float G3 { get; set; }          // hedef
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
