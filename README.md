# ML.NET Vize Projesi

C# ve ML.NET ile hazırlanmış üç makine öğrenmesi örneği ve ASP.NET Core MVC web arayüzü.

## Projeler

| Proje | Yöntem | Veri |
|---|---|---|
| `01-SinavNotu` | Regresyon (SDCA) | 80 sentetik öğrenci örneği; çalışma saati → not |
| `02-DuyguAnalizi` | İkili sınıflandırma (SDCA Logistic Regression) | IMDb, Amazon ve Yelp kaynaklı 3.000 İngilizce yorum |
| `03-HalFiyat` | Regresyon (SDCA) ve eşik kontrolü | 7 anonimleştirilmiş faturadan 74 satış satırı; ürün → TL/kg |

## Gereksinimler

- .NET 7 SDK ve ASP.NET Core Runtime
- ML.NET 4.0.2 (NuGet paketleri derleme sırasında indirilir)

> .NET 7'nin resmi desteği sona ermiştir. Yeni üretim ortamları için desteklenen bir .NET sürümüne geçilmelidir.

## Çalıştırma

Proje kök dizininde:

```bash
dotnet build MLNet-Vize-Projesi.sln -c Release
```

Konsol projeleri, veri dosyaları için göreli yollar kullandığından kendi dizinlerinden çalıştırılır:

```bash
cd 01-SinavNotu && dotnet run
cd ../02-DuyguAnalizi && dotnet run
cd ../03-HalFiyat && dotnet run
```

Web arayüzünü çalıştırmak için proje kökünden:

```bash
cd 04-WebArayuzu
dotnet run --urls http://127.0.0.1:5300
```

Web sayfaları: `/Not`, `/Duygu` ve `/Hal`.

## Model akışı

**Sınav notu:** CSV → eğitim/test ayrımı → `Concatenate` → `NormalizeMinMax` → `Sdca` → `Fit` → `Evaluate` → `Predict`. Sentetik veri eğitim amaçlıdır; sonuçlar gerçek öğrenciler için güvenilir tahmin sayılmaz.

**Duygu analizi:** İngilizce yorumlar `FeaturizeText` ile sayısal özelliklere dönüştürülür. `SdcaLogisticRegression` olumlu/olumsuz sınıflandırması yapar. Test doğruluğu yaklaşık %75,6'dır.

**Hal fiyatı:** Ürün adı `OneHotEncoding` ile sayısal vektöre çevrilir; `Sdca` beklenen TL/kg fiyatını tahmin eder. Gerçek fiyat beklenenin %80'inden düşükse uygulama kural tabanlı uyarı verir. Bu uyarı ayrı bir anomali öğrenme modeli değildir. Satır bazlı çapraz doğrulama aynı faturadaki ilişkili satırlar nedeniyle iyimser olabilir.

## Model dosyaları

`05-ModelEgitici`, modelleri eğitip `04-WebArayuzu/Ml/Modeller/` altında ZIP olarak kaydetmek için kullanılır. Web uygulaması duygu modelini mevcut ZIP dosyasından yükler; sınav notu ve hal modellerini CSV dosyalarından servis ilk oluşturulduğunda eğitir.

## Veri kaynakları

Verilerin açıklamaları ve kaynak bağlantıları için [veri/KAYNAK.md](veri/KAYNAK.md) dosyasına bakın.

## Canlı uygulama

https://mlnet.burakkarademir.dev
