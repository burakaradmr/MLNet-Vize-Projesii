using _04_WebArayuzu.Services;
using _04_WebArayuzu.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace _04_WebArayuzu.Controllers;

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
        form.TahminiNot = _servis.NotTahmin(form.CalismaSaati);
        form.TahminYapildi = true;
        return View(form);
    }
}
