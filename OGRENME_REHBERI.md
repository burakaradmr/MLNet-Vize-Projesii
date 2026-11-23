# ML.NET sözlü sınav rehberi (güncel)

## 1) Sınav notu tahmini
- **UCI Student Performance**, 649 satır (Portekiz lise): studytime, failures, higher, absences, G1, G2 → G3 (0-20).
- Amaç bir **sayı** tahmin etmek, yani regresyon.
- `LoadFromTextFile` → `TrainTestSplit` → `Concatenate` → `NormalizeMinMax` → `Sdca` → `Fit` → `Evaluate` → `Predict`.
- `R²` açıklanan değişkenliğin ölçüsü; `MAE` ortalama mutlak hata, `RMSE` büyük hatalara daha duyarlı hata.
- Sonuçlar **gerçek öğrencileri temsil etmez**; gerçek veri olmadığı açıkça söylenmeli.

## 2) Duygu analizi
- UCI 3.000 İngilizce etiketli yorum; olumlu/olumsuz **ikili sınıflandırma**.
- `FeaturizeText` metinden sayısal özellikler çıkarır.
- `SdcaLogisticRegression` sınıflandırıcıdır.
- Test doğruluğu yaklaşık %75,6, AUC 0,837, F1 0,762.

## 3) Hal fiyat tahmini
- 74 satış satırı, 7 anonim fatura; ürün adı `OneHotEncoding` ile vektöre çevrilir.
- `Concatenate` özellik vektörünü hazırlar; `Sdca` ile TL/kg regresyonu yapılır.
- `Fit` modeli eğitir, `Predict` tahmin üretir.
- `gerçek < beklenen*0.80f` yalnızca elle yazılmış eşik uyarısıdır.
- `GroupBy` aynı ürünü bir araya toplar, `Sum(x => x.Kg * x.Fiyat)` satış gelirlerini toplar.
