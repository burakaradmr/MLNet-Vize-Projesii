# Sistem analizi — ML.NET ile üç farklı eğitim örneği

## Amaç
ML.NET ile (1) UCI Student Performance verisiyle sınav notu regresyonu, (2) gerçek İngilizce yorumlardan ikili duygu sınıflandırması ve (3) anonimleştirilmiş hal satışlarından ürün fiyatı regresyonu + kural tabanlı düşük fiyat uyarısını uygulamak.

## Yöntem ve kaynaklar
| Proje | Veri | Özellik | Hedef | Yöntem |
|---|---|---|---|---|
| Sınav notu | **UCI Student Performance** (student-por), 649 satır | studytime, failures, higher, absences, G1, G2 | G3 (0–20) | `SDCA` ve `FastTree` karşılaştırması → SDCA |
| Duygu | UCI'den 3.000 İngilizce yorum | metin | olumlu/olumsuz | `FeaturizeText + SdcaLogisticRegression` |
| Hal | 7 faturada 74 satır | ürün adı | TL/kg | `OneHotEncoding + Sdca` |

Not örneği gerçek öğrenci verisi kullanmaz; ML.NET eğitim adımlarını göstermek için üretilmiştir. Yeni not modeli için test metrikleri konsol çalıştırıldığında hesaplanır ve önceden doğrulanmış gibi yazılmaz.

Duygu modelinin bilinen test sonuçları: doğruluk yaklaşık %75,6, AUC 0,837 ve F1 0,762.

Hal fiyatında beş katlı satır bazlı CV R² yaklaşık 0,764, MAE 3,32 TL/kg'dır; aynı fatura satırları bağımsız olmadığından gerçek yeni fatura başarısını fazla gösterebilir. %20 altında fiyat uyarısı ayrıca eğitilmiş anomali modeli değil, `if` koşuludur.

## Yapı
`01-SinavNotu`, `02-DuyguAnalizi`, `03-HalFiyat`: konsol örnekleri.
`04-WebArayuzu`: MVC arayüzü, GET/POST kontrolleri.
`05-ModelEgitici`: ZIP model üretici.

## Kısıtlar
.NET 7 güvenlik desteği sona erdi. Sınav notu verisi Portekiz lise öğrencilerine aittir (Cortez & Silva, 2008); Türkiye için genelleme iddiası taşımaz. Repo Git geçmişindeki gerçek fatura numaraları nedeniyle private tutulmalıdır.

### Sınav notu modeli — ölçüm

| Model | R² | MAE | RMSE |
|---|---|---|---|
| SDCA (seçilen) | 0,782 | 0,95 puan | 1,66 puan |
| FastTree | 0,754 | 1,09 puan | 1,76 puan |

Aynı sabit %80/%20 bölünmesi, 649 satır (512 eğitim / 137 test). Normalizasyon pipeline içinde
yalnızca eğitim verisiyle öğrenilir; test verisine önceden uygulanmaz. Seçilen model
`04-WebArayuzu/Ml/Modeller/not-model.zip` olarak kaydedilir ve web uygulaması ZIP'ten yükler.
