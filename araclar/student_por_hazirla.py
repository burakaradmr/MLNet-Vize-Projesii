#!/usr/bin/env python3
"""
UCI Student Performance (student-por.csv) -> modele uygun sayısal CSV.

Neden ayrı dosya?
  Orijinal dosya noktalı virgülle ayrılmış ve metin alanlar içeriyor
  ("GP";"F";"female";...). ML.NET'in metin okuyucusu bu karışık yapıyı
  doğrudan modele veremez; bu betik yalnızca kullanılan 7 sütunu sayıya çevirir.

Dönüşümler:
  higher  : "yes" -> 1, "no" -> 0
  studytime: 1-4 kategorisi KORUNUR (günlük saat değil, haftalık süre kategorisi)
  G1, G2, G3: 0-20 aralığı korunur

Çıktı: veri/sinav_notu_model.csv
"""
import csv
from pathlib import Path

kaynak = Path("veri/student-por.csv")
hedef = Path("veri/sinav_notu_model.csv")

with open(kaynak, encoding="utf-8", newline="") as f:
    satirlar = list(csv.DictReader(f, delimiter=";"))

with open(hedef, "w", encoding="utf-8", newline="") as f:
    yaz = csv.writer(f)
    yaz.writerow(["studytime", "failures", "higher", "absences", "G1", "G2", "G3"])
    for s in satirlar:
        yaz.writerow([
            int(s["studytime"]),
            int(s["failures"]),
            1 if s["higher"] == "yes" else 0,
            int(s["absences"]),
            int(s["G1"]),
            int(s["G2"]),
            int(s["G3"]),
        ])

print(f"{len(satirlar)} satır -> {hedef}")
