using _04_WebArayuzu.Services;
using _04_WebArayuzu.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace _04_WebArayuzu.Controllers;

// ============================================================
//  HAL SATIŞ FİYATI CONTROLLER
//  Gerçek hal faturalarından öğrenilmiş model:
//    yalnızca ürün adı -> beklenen TL/kg
//  Kullanıcı gerçek fiyatı da girerse "ucuza mı gitti" uyarısı çıkar.
// ============================================================

public class HalController : Controller
{
    private readonly TahminServisi _tahmin;

    // Formdaki ürün listesi (veri setindeki ürünler)
    public static readonly string[] Urunler = { "KEREVIZ", "KARALAHANA", "PIRASA", "BROKOLI", "KARNABAHAR", "BEYAZ LAHANA" };

    public HalController(TahminServisi tahmin)
    {
        _tahmin = tahmin;
    }

    // GET /Hal
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Aktif"] = "hal";
        return View(new HalFormu { Sevkiyat = _tahmin.BugunkuSevkiyat() });
    }

    // POST /Hal
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(HalFormu form)
    {
        ViewData["Aktif"] = "hal";
        form.Sevkiyat = _tahmin.BugunkuSevkiyat();

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        // Model tahmini: beklenen TL/kg
        float? beklenenOlası = _tahmin.HalTahmin(form.Urun);
        if (!beklenenOlası.HasValue)
        {
            form.Uyari = "Bu ürün için geçmiş satış verisi yok. Fiyat tahmini üretilemez.";
            form.TahminYapildi = true;
            return View(form);
        }
        float beklenen = beklenenOlası.Value;
        form.BeklenenFiyat = beklenen;
        form.TahminiGelir = beklenen * form.Kg;

        // Kullanıcı gerçek fiyatı girdiyse anomali kontrolü yap.
        // Konsol projesindeki kuralın aynısı: beklenenin %20 altı = uyarı.
        if (form.GercekFiyat.HasValue)
        {
            float gercek = form.GercekFiyat.Value;
            if (gercek < beklenen * 0.80f)
            {
                float sapma = (1 - gercek / beklenen) * 100;
                form.Uyari = $"UYARI: Bu fiyat beklenenin %{sapma:F0} altında. Mal ucuza gidiyor olabilir.";
            }
        }

        form.TahminYapildi = true;
        return View(form);
    }
}
