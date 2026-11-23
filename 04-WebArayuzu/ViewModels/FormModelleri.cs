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
    [Range(0, 6, ErrorMessage = "Çalışma saati 0-6 arasında olmalı")]
    [Display(Name = "Çalışma süresi (saat)")]
    public float CalismaSaati { get; set; } = 3;
    public float? TahminiNot { get; set; }
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
