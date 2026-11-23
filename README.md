# ML.NET — Üç Örnek

.NET 7 + ML.NET 4.0.2 ile üç eğitim projesi ve ASP.NET Core MVC uygulaması.

| Proje | Tür | Veriler |
|---|---|---|
| `01-SinavNotu` | SDCA / FastTree regresyon, 6 özellik → G3 | **UCI Student Performance** (student-por), 649 satır |
| `02-DuyguAnalizi` | SDCA lojistik regresyon, yorum → olumlu/olumsuz | UCI'den 3.000 İngilizce yorum |
| `03-HalFiyat` | SDCA regresyon, ürün → TL/kg + eşik uyarısı | Anonimleştirilmiş 7 faturadan 74 satış satırı |

## Çalıştırma

```bash
cd 01-SinavNotu && dotnet run
cd ../02-DuyguAnalizi && dotnet run
cd ../03-HalFiyat && dotnet run
cd ../04-WebArayuzu && dotnet run --urls http://127.0.0.1:5300
```

Her konsol projesi kendi klasöründen çalıştırılır. Modellerin ZIP sürümünü yeniden üretmek için `05-ModelEgitici` içinde `dotnet run` çalıştırılabilir. Web uygulaması not ve hal modellerini servis ilk oluşturulduğunda CSV'den eğitir; duygu modelini ZIP'ten yükler.

## Sınav notu örneği

Veri: **UCI Student Performance** — Cortez & Silva (2008), Portekiz lise öğrencileri, `student-por.csv` (649 satır, CC BY 4.0). Kullanılan alanlar: `studytime` (haftalık çalışma **kategorisi** 1-4, günlük saat değil), `failures`, `higher`, `absences`, `G1`, `G2`; hedef `G3` (0-20). Ham dosya `veri/student-por.csv`, modele uygun sayısal sürüm `veri/sinav_notu_model.csv` (`araclar/student_por_hazirla.py` üretir). Aynı sabit %80/%20 bölünmesinde **SDCA** (R² 0,782 · MAE 0,95 · RMSE 1,66) ve **FastTree** (R² 0,754 · MAE 1,09 · RMSE 1,76) karşılaştırıldı; düşük test RMSE nedeniyle SDCA seçildi ve `not-model.zip` olarak kaydedildi. Normalizasyon pipeline içinde yalnızca eğitim verisiyle öğrenilir; test verisine önceden uygulanmaz. Atıf: Cortez, P., & Silva, A. (2008). Using Data Mining to Predict Secondary School Student Performance. Sonuçlar Portekiz verisine dayanır; Türkiye'deki bir öğrencinin gerçek sınav sonucunu garanti etmez.

## Hal fiyatı sınırı

Hal modeli yalnızca **ürün adından** fiyat tahmin eder; KG gerçek model özelliği değildir. Uyarı `gerçek fiyat < beklenen × 0,8` şeklinde elle tanımlanmış bir kuraldır, ayrı anomali modeli değildir. 5-kat CV R² yaklaşık 0,764 ve MAE 3,32 TL/kg olsa da aynı faturadaki ilişkili satırlar nedeniyle iyimser olabilir.

## Yayın

Web arayüzü Cloudflare Tunnel üzerinden `https://mlnet.burakkarademir.dev` alan adına yönlendirilebilir. Yayını bu commit otomatik gerçekleştirmez. Local servisin gerçek durumu sunucuda doğrulanmalıdır. Kurulum için `DEPLOY-HERMES.md` dosyasına bakın.

## Güvenlik

**Repo private kalmalı:** eski Git geçmişinde gerçek fatura numaraları bulunuyor. GitHub görünürlüğünü değiştirmeden önce temiz geçmişli yeni bir repo oluşturulmalı. .NET 7 destek dışıdır; üretimde desteklenen .NET LTS'e geçiş planlanmalı.
