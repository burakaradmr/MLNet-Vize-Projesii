# MVC — Sınav Notu, Duygu ve Hal

- **Model:** `04-WebArayuzu/Models/ModelSiniflari.cs` içindeki `Ogrenci`, `Yorum` ve `HalSatisi` veri sınıfları.
- **View:** `Views/Not/Index.cshtml`, `Views/Duygu/Index.cshtml`, `Views/Hal/Index.cshtml`.
- **Controller:** `NotController`, `DuyguController`, `HalController`; HTTP GET formu gösterir, POST veriyi doğrular ve `TahminServisi` çağırır.
- **ViewModel:** `ViewModels/FormModelleri.cs`; form değerleri ve tahmin sonuçları.
- **Servis:** `TahminServisi`, `AddSingleton` ile kayıtlıdır. Not ve hal modelleri CSV'den bir kez eğitilir, duygu modeli ZIP'ten yüklenir. `PredictionEngine` kullanımında eşzamanlı istekler kilitle korunur.
- Örnek akış: `GET /Not` → boş form → `POST /Not` → `NotFormu` doğrulaması → `NotTahmin` → `Predict` → sonuç View'da görünür.
- Not örneği Portekiz lise verisiyle (UCI Student Performance) eğitilir; gerçek sınav sonucunu garanti etmez.
