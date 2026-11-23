# Veri Kaynakları

Bu klasörde iki kamuya açık UCI veri seti ve özel kaynaklı hal faturaları bulunmaktadır.

## 1. `sinav_notlari.csv` — Sentetik sınav notu verileri

- **Kaynak:** Eğitim gösterimi için yapay oluşturulmuş 80 kayıt; gerçek öğrencilerden elde edilmemiştir.
- **Kolonlar:** CalismaSaati (0–6 saat), SinavNotu (0–100 puan).
- **Sınır:** Gerçek hayat başarısını temsil etmez; modelin amacı ML.NET regresyon kodunu öğretmektir.

## 2. `duygu.csv` — Yorum duygu analizi

- **Kaynak:** UCI "Sentiment Labelled Sentences" (Kotzias ve ark.)
- **Asıl adres:** https://archive.ics.uci.edu/dataset/331/sentiment+labelled+sentences
- **İçerik:** 3 dosya birleştirildi — IMDb (1.000), Yelp (1.000), Amazon (1.000)
- **Satır:** 3.000 cümle
- **Format:** `yorum<TAB>duygu` — 1 = olumlu, 0 = olumsuz
- **Not:** Cümleler **İngilizce**dir (veri setinin orijinali böyle). Ayırıcı TAB çünkü cümlelerde virgül çok var.
- **Lisans:** UCI, araştırma/eğitim amaçlı

## 3. `hal_satislari.csv` — Hal satış fiyatları (gerçek faturalar)

- **Kaynak:** kendi ürünlerimizin Antalya Toptancı Hali (Kepez) komisyoncu faturaları
- **Faturalar:** 7 XML fatura, 25 Eylül–12 Kasım 2026.
- **Satır:** 74 (ürün, paket adedi, kg, fiyat, tutar).
- **Ürünler:** kereviz, pırasa, brokoli, karalahana, karnabahar.
- **Çıkarım:** UBL XML'den satış satırı ve ADET alt satırı alındı; satır tutarları doğrulandı. Fatura numaraları F01–F07 olarak anonimleştirildi.
- **Gizlilik:** TCKN, telefon, adres ve gerçek fatura numaraları yeni CSV'de yok; tarihler ve ticari fiyatlar özel tutulmalıdır.
- **Kesinti:** komisyon %8 + KDV %20 + tevkiifat %2 + diğer %1,5 ≈ net/brüt oranı 0,869.

## Kaldırılan veri
`wine.csv` / `wine.data` (UCI Wine) projeden çıkarıldı; şarap örneği yerine hal faturası örneği geldi.
