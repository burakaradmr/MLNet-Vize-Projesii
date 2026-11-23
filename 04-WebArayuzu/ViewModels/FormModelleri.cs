using System.ComponentModel.DataAnnotations;

namespace _04_WebArayuzu.ViewModels;

// ============================================================
//  FORM MODELLERİ (ViewModel)
//  ViewModel = ekranda gösterilen/kullanıcıdan alınan veri.
//  Entity'den farkı: veritabanı tablosu değil, sayfanın ihtiyacı.
//  Üçünün de içinde "sonuç" alanları var; tahmin yapılınca doldurulur.
// ============================================================

// ---------- 1) SINAV NOTU TAHMİNİ ----------
public class NotFormu
{
    // studytime alanı günlük saat değil, haftalık çalışma KATEGORİSİdir (1-4).
    [Range(1, 4, ErrorMessage = "Çalışma kategorisi 1-4 arasında olmalı")]
    [Display(Name = "Haftalık çalışma kategorisi (1-4)")]
    public int Studytime { get; set; } = 2;

    [Range(0, 3, ErrorMessage = "Başarısızlık sayısı 0-3 arasında olmalı")]
    [Display(Name = "Geçmiş başarısızlık sayısı")]
    public int Failures { get; set; } = 0;

    [Display(Name = "Yükseköğrenim hedefi var mı?")]
    public bool Higher { get; set; } = true;

    [Range(0, 93, ErrorMessage = "Devamsızlık 0-93 arasında olmalı")]
    [Display(Name = "Devamsızlık (gün)")]
    public int Absences { get; set; } = 4;

    [Range(0, 100, ErrorMessage = "1. dönem notu 0-100 arasında olmalı")]
    [Display(Name = "1. dönem notu (0-100)")]
    public float G1 { get; set; } = 55;

    [Range(0, 100, ErrorMessage = "2. dönem notu 0-100 arasında olmalı")]
    [Display(Name = "2. dönem notu (0-100)")]
    public float G2 { get; set; } = 60;

    // Sonuçlar
    public float? TahminiG3 { get; set; }        // 0-20
    public float? TahminiYuzluk { get; set; }    // 0-100
    public bool TahminYapildi { get; set; }
}

// ---------- 2) DUYGU ANALİZİ ----------
public class DuyguFormu
{
    [Required(ErrorMessage = "Bir yorum yazmalısın")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Yorum 3-500 karakter olmalı")]
    [Display(Name = "İngilizce yorum")]
    public string Yorum { get; set; } = "";

    public bool? Olumlu { get; set; }
    public float? Guven { get; set; }
    public bool TahminYapildi { get; set; }
}

// ---------- 3) HAL SATIŞ FİYATI ----------
public class HalFormu
{
    public List<_04_WebArayuzu.Services.SevkiyatTahmini> Sevkiyat { get; set; } = new();

    [Required(ErrorMessage = "Ürün seç")]
    [Display(Name = "Ürün")]
    public string Urun { get; set; } = "KEREVIZ";

    [Range(1, 1000, ErrorMessage = "Kg 1-1000 arasında olmalı")]
    [Display(Name = "Miktar (kg)")]
    public float Kg { get; set; } = 50;

    // İsteğe bağlı: halde gerçekten kaça gitti? Girilirse uyarı hesaplanır.
    [Display(Name = "Gerçek fiyat (TL/kg) — isteğe bağlı")]
    public float? GercekFiyat { get; set; }

    public float? BeklenenFiyat { get; set; }
    public float? TahminiGelir { get; set; }
    public string? Uyari { get; set; }
    public bool TahminYapildi { get; set; }
}
