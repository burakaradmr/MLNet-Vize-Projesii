# Veri Kaynakları

Bu klasörde iki kamuya açık UCI veri seti ve özel kaynaklı hal faturaları bulunmaktadır.

## 1. `student-por.csv` — UCI Student Performance (gerçek veri)

- **Kaynak:** UCI Machine Learning Repository, Student Performance (ID 320)
- **Atıf:** Cortez, P., & Silva, A. (2008). *Using Data Mining to Predict Secondary School Student Performance.*
- **Lisans:** CC BY 4.0 (UCI)
- **İçerik:** Portekiz'de iki okulun dil dersi öğrencileri, 649 satır, 33 sütun (2005-2006)
- **Kullanılan alanlar:** studytime (haftalık kategori 1-4), failures, higher, absences, G1, G2 → hedef G3 (0-20)
- **Türetilmiş dosya:** `sinav_notu_model.csv` — `araclar/student_por_hazirla.py` üretir
  (higher: yes→1/no→0; studytime kategorisi korunur; G1-G2-G3 0-20 aralığında kalır)
- **Ham açıklama:** `student-por-aciklama.txt` (UCI'nin kendi sütun sözlüğü)

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
