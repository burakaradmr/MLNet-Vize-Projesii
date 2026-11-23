// ============================================================
//  UYGULAMANIN BAŞLANGIÇ DOSYASI
//  Burada sadece iki şey yapıyoruz:
//   1) MVC'yi açıyoruz (controller + razor view çalışsın diye)
//   2) Tahmin servisini SINGLETON olarak kaydediyoruz
//      -> servis bir kez oluşur, modeller bir kez yüklenir,
//         her HTTP isteğinde yeniden yüklenmez veya eğitilmez.
// ============================================================

using _04_WebArayuzu.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC altyapısı
builder.Services.AddControllersWithViews();

// Modelleri uygulama açılışında yükleyen servis (tek örnek)
builder.Services.AddSingleton<TahminServisi>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Statik dosyalar: css, js ve model zip'leri
app.UseStaticFiles();

// Adres yönlendirme: /yakit -> YakitController
app.UseRouting();

// Varsayılan sayfa: Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
