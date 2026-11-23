using _04_WebArayuzu.Models;
using _04_WebArayuzu.Services;
using _04_WebArayuzu.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace _04_WebArayuzu.Controllers;

// ============================================================
//  SINAV NOTU CONTROLLER
//  Girdi : studytime, failures, higher, absences, G1, G2
//  Çıktı : G3 tahmini (0-20 ve 0-100 karşılığı)
//
//  Model 0-20 ölçeğiyle eğitildiği için formda girilen 0-100 notlar
//  burada 0-20'ye çevrilir; tahmin sonucu tekrar 100'lük ölçekte gösterilir.
// ============================================================

public class NotController : Controller
{
    private readonly TahminServisi _servis;

    public NotController(TahminServisi servis) => _servis = servis;

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Aktif"] = "not";
        return View(new NotFormu());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(NotFormu form)
    {
        ViewData["Aktif"] = "not";

        if (!ModelState.IsValid) return View(form);

        var ogrenci = new Ogrenci
        {
            studytime = form.Studytime,
            failures = form.Failures,
            higher = form.Higher ? 1f : 0f,
            absences = form.Absences,
            G1 = form.G1 / 5f,   // 0-100 -> 0-20
            G2 = form.G2 / 5f
        };

        float tahmin = _servis.NotTahmin(ogrenci);

        form.TahminiG3 = tahmin;
        form.TahminiYuzluk = tahmin * 5f;
        form.TahminYapildi = true;

        return View(form);
    }
}
