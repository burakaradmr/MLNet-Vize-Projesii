using System.Diagnostics;
using _04_WebArayuzu.Models;
using Microsoft.AspNetCore.Mvc;

namespace _04_WebArayuzu.Controllers;

// ============================================================
//  ANA SAYFA CONTROLLER
//  Sadece tanıtım sayfasını ve hata sayfasını döndürür.
// ============================================================

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // GET /  -> ana sayfa
    public IActionResult Index()
    {
        ViewData["Aktif"] = "home";
        return View();
    }

    // Beklenmeyen hata olursa gösterilen sayfa
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
