# ML.NET Vize Projesi

C# ve ML.NET ile geliştirilmiş üç makine öğrenmesi örneği ve ASP.NET Core MVC web uygulaması.

## Projeler

| Klasör | Problem | Veri ve yöntem |
|---|---|---|
| `01-SinavNotu` | Yıl sonu not tahmini (regresyon) | UCI Student Performance, 649 öğrenci; 6 özellik; SDCA / FastTree karşılaştırması |
| `02-DuyguAnalizi` | Olumlu/olumsuz metin sınıflandırması | UCI Sentiment Labelled Sentences, 3.000 İngilizce cümle; FeaturizeText + SDCA Logistic Regression |
| `03-HalFiyat` | Ürün bazlı TL/kg tahmini | Anonimleştirilmiş 7 fatura, 74 satış satırı; OneHotEncoding + SDCA |
| `04-WebArayuzu` | Web arayüzü | ASP.NET Core MVC; `/Not`, `/Duygu`, `/Hal` |
| `05-ModelEgitici` | Model eğitimi | Duygu ve hal modeli ZIP çıktıları; not modeli `01-SinavNotu` tarafından oluşturulur |

## Gereksinimler

- .NET 7 SDK
- ML.NET 4.0.2
- İnternet erişimi (NuGet bağımlılıklarını indirmek için)

> .NET 7'nin destek süresi dolmuştur. Uzun süreli kullanım için desteklenen bir .NET sürümüne geçilmelidir.

## Derleme ve çalıştırma

```bash
dotnet build MLNet-Vize-Projesi.sln -c Release
```

Konsol projeleri kendi dizinlerinden çalıştırılır:

```bash
cd 01-SinavNotu && dotnet run
cd ../02-DuyguAnalizi && dotnet run
cd ../03-HalFiyat && dotnet run
```

Web uygulaması:

```bash
cd 04-WebArayuzu
dotnet run --urls http://127.0.0.1:5300
```

Web uygulaması sınav notu ve duygu modellerini `Ml/Modeller` klasöründeki ZIP dosyalarından yükler. Hal fiyat modelini ise başlangıçta `veri/hal_satislari.csv` verileriyle eğitir. Yeni kullanıcı girdileri modeli otomatik olarak yeniden eğitmez.

## Sınav notu tahmini

Kaynak: **UCI Student Performance** (`veri/student-por.csv`, 649 Portekiz lise öğrencisi).

- Girdiler: `studytime` (haftalık çalışma süresi kategorisi 1–4), `failures`, `higher`, `absences`, `G1`, `G2`
- Hedef: `G3` (0–20); arayüzde 100 üzerinden karşılığı da gösterilir
- Sabit eğitim/test ayrımında raporlanan sonuçlar:

| Model | R² | MAE (0–20) | RMSE (0–20) |
|---|---:|---:|---:|
| SDCA | 0,782 | 0,95 | 1,66 |
| FastTree | 0,754 | 1,09 | 1,76 |

Bu çalışmada SDCA, daha düşük test RMSE ile seçilmiştir. Algoritmanın aynı test kümesine göre seçilmiş olması nedeniyle bu değerler bağımsız bir nihai değerlendirme olarak yorumlanmamalıdır. Sonuçlar Türkiye'deki öğrenciler için doğrulanmış bir tahmin sistemi değildir.

## Duygu analizi

İngilizce yorumlar sayısal özelliklere çevrilir ve ikili sınıflandırma yapılır. Raporlanan test doğruluğu yaklaşık %75,6'dır. `Predict()` yeni yorumlardan otomatik öğrenmez.

## Hal fiyatı tahmini

Model yalnızca **ürün adından** beklenen TL/kg değerini tahmin eder; kg bilgisi tahmini gelir hesabında kullanılır. Gerçek fiyatın beklenen değerin %80'inden düşük olması durumunda gösterilen uyarı **kural tabanlıdır**, ayrı bir anomali modeli değildir. Yedi faturadaki satırlar birbirine bağlı olduğundan satır bazlı çapraz doğrulama sonuçları iyimser olabilir.

Fatura işleme betiği, yerel PDF klasörü açıkça verilerek çalıştırılır:

```bash
python hal/fatura_ayikla.py <pdf_klasoru> [cikti.csv]
```

Betiğin ürettiği fatura kodları anonimdir. CSV'yi yayımlamadan önce kişisel ve ticari bilgileri ayrıca kontrol edin.

## Veri kaynakları ve atıf

Lisans ve kaynak açıklamaları: [veri/KAYNAK.md](veri/KAYNAK.md).

- Cortez, P. & Silva, A. (2008), *Using Data Mining to Predict Secondary School Student Performance*.
- [UCI Student Performance](https://archive.ics.uci.edu/dataset/320/student+performance).
- [UCI Sentiment Labelled Sentences](https://archive.ics.uci.edu/dataset/331/sentiment+labelled+sentences).

## Canlı uygulama

https://mlnet.burakkarademir.dev
