using _04_WebArayuzu.Services;
using _04_WebArayuzu.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace _04_WebArayuzu.Controllers;

// ============================================================
//  DUYGU ANALİZİ CONTROLLER
//  Kullanıcı İngilizce bir yorum yazar; model olumlu/olumsuz der.
// ============================================================

public class DuyguController : Controller
{
    private readonly TahminServisi _tahmin;

    public DuyguController(TahminServisi tahmin)
    {
        _tahmin = tahmin;
    }

    // GET /Duygu
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Aktif"] = "duygu";
        return View(new DuyguFormu());
    }

    // POST /Duygu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(DuyguFormu form)
    {
        ViewData["Aktif"] = "duygu";

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var (olumlu, guven) = _tahmin.DuyguTahmin(form.Yorum);

        form.Olumlu = olumlu;
        form.Guven = guven;
        form.TahminYapildi = true;

        return View(form);
    }
}
