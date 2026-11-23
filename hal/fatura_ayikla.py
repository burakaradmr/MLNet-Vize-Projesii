#!/usr/bin/env python3
"""
Hal faturalarından temiz veri çıkarır.

Girdi : bir klasördeki müstahsil/komisyoncu faturaları (PDF)
Çıktı : veri/hal_satislari.csv (anonim fatura kodlarıyla)

Sütunlar: tarih, fatura_no, urun, adet, kg, birim_fiyat_tl, tutar_tl, net_oran
Kişisel veri (TCKN, telefon, e-posta, adres) hiç yazılmaz.
"""
import csv
import re
import subprocess
import sys
from pathlib import Path

URUN_RE = r"(KEREV[İI]Z|KARALAHANA|PIRASA|BROKOL[İI]|KARNABAHAR|LAHANA|DOMATES)"
SATIR_RE = re.compile(
    rf"^\s*\d+\s+{URUN_RE}\s+\d+\s+(\d+)\s+(\d+)\s+kg\s+([\d.]+)\s*TL\s+%0\s+[\d.]+\s*TL\s+([\d,]+\.\d{{2}})\s*TL"
)


def sayi(s: str) -> float:
    """'5,040.00' -> 5040.00 (TR binlik ayırıcı)"""
    return float(s.replace(",", "").replace(" TL", "").strip())


def pdf_metni(yol: Path) -> str:
    return subprocess.run(["pdftotext", "-layout", str(yol), "-"],
                          capture_output=True, text=True).stdout


def ayikla(yol: Path):
    m = pdf_metni(yol)
    no = re.search(r"Fatura No\s+(\S+)", m)
    tarih = re.search(r"Fatura Tarihi\s+(\d{2}-\d{2}-\d{4})", m)
    brut = re.search(r"Mal Hizmet Toplam Tutar[ıi]\s+([\d,]+\.\d{2})", m)
    net = re.search(r"Ödenecek Tutar\s+([\d,]+\.\d{2})", m)
    if not (no and tarih):
        return None, []

    brut_tl = sayi(brut.group(1)) if brut else 0.0
    net_tl = sayi(net.group(1)) if net else 0.0
    net_oran = round(net_tl / brut_tl, 4) if brut_tl else ""

    kayitlar = []
    for satir in m.splitlines():
        g = SATIR_RE.match(satir)
        if g:
            kayitlar.append({
                "tarih": tarih.group(1),
                "fatura_no": no.group(1),
                "urun": g.group(1).replace("İ", "I"),
                "adet": int(g.group(2)),
                "kg": int(g.group(3)),
                "birim_fiyat_tl": float(g.group(4)),
                "tutar_tl": sayi(g.group(5)),
                "net_oran": net_oran,
            })

    # Doğrulama: satır toplamı faturanın brüt tutarına eşit olmalı
    toplam = sum(k["tutar_tl"] for k in kayitlar)
    if brut_tl and abs(toplam - brut_tl) > 0.01:
        print(f"  UYARI: satır toplamı {toplam:.2f} != fatura {brut_tl:.2f}", file=sys.stderr)
    return no.group(1), kayitlar


def main():
    if len(sys.argv) < 2:
        raise SystemExit("Kullanım: python hal/fatura_ayikla.py <pdf_klasoru> [cikti.csv]")
    kaynak = Path(sys.argv[1])
    if not kaynak.is_dir():
        raise SystemExit(f"PDF klasörü bulunamadı: {kaynak}")
    cikti = Path(sys.argv[2] if len(sys.argv) > 2 else "veri/hal_satislari.csv")
    cikti.parent.mkdir(parents=True, exist_ok=True)

    pdfs = sorted(kaynak.rglob("*.pdf"))
    hepsi, gorulen = [], set()
    for p in pdfs:
        no, kayitlar = ayikla(p)
        if not kayitlar or no in gorulen:
            continue
        gorulen.add(no)
        hepsi.extend(kayitlar)
        print(f"  Dosya {len(gorulen):02d}: {len(kayitlar)} kalem")

    # Gerçek fatura numaralarını dosyaya veya konsola yazma.
    kodlar = {no: f"F{i:02d}" for i, no in enumerate(sorted(gorulen), 1)}
    for kayit in hepsi:
        kayit["fatura_no"] = kodlar[kayit["fatura_no"]]

    with open(cikti, "w", newline="", encoding="utf-8") as f:
        yaz = csv.DictWriter(f, fieldnames=["tarih", "fatura_no", "urun", "adet",
                                            "kg", "birim_fiyat_tl", "tutar_tl", "net_oran"])
        yaz.writeheader()
        yaz.writerows(hepsi)

    print(f"\n{len(gorulen)} fatura, {len(hepsi)} satır -> {cikti}")


if __name__ == "__main__":
    main()
