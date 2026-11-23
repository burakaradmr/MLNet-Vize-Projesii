using Microsoft.ML;
using Microsoft.ML.Data;

// Dağıtım için ZIP model üretici: duygu ve hal modelleri.
// Sınav notu modeli 01-SinavNotu projesinde üretilir (UCI Student Performance).
string veriYolu = Path.Combine("..", "veri");
string ciktiKlasoru = Path.Combine("..", "04-WebArayuzu", "Ml", "Modeller");
Directory.CreateDirectory(ciktiKlasoru);
var ml = new MLContext(seed: 1);

// NOT: not-model.zip artık 01-SinavNotu projesinde üretiliyor (UCI Student Performance).

var duyguVeri = ml.Data.LoadFromTextFile<Yorum>(Path.Combine(veriYolu, "duygu.csv"), '\t', hasHeader: true);
var duyguPipeline = ml.Transforms.Text.FeaturizeText("Ozellikler", nameof(Yorum.yorum))
    .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression(
        labelColumnName: nameof(Yorum.duygu), featureColumnName: "Ozellikler",
        maximumNumberOfIterations: 100));
var duyguModel = duyguPipeline.Fit(duyguVeri);
ml.Model.Save(duyguModel, duyguVeri.Schema, Path.Combine(ciktiKlasoru, "duygu-model.zip"));
Console.WriteLine("duygu-model.zip oluşturuldu");

var halVeri = ml.Data.LoadFromTextFile<FaturaSatiri>(Path.Combine(veriYolu, "hal_satislari.csv"), ',', hasHeader: true);
var halHam = ml.Data.CreateEnumerable<FaturaSatiri>(halVeri, reuseRowObject: false).ToList();
var halEgitim = ml.Data.LoadFromEnumerable(halHam.Select(h =>
    new HalSatisi { Urun = h.urun, Kg = h.kg, Fiyat = h.birim_fiyat_tl }));
var halPipeline = ml.Transforms.Categorical.OneHotEncoding("UrunKod", nameof(HalSatisi.Urun))
    .Append(ml.Transforms.Concatenate("Ozellikler", "UrunKod"))
    .Append(ml.Regression.Trainers.Sdca(
        labelColumnName: nameof(HalSatisi.Fiyat), featureColumnName: "Ozellikler",
        maximumNumberOfIterations: 200));
var halModel = halPipeline.Fit(halEgitim);
ml.Model.Save(halModel, halEgitim.Schema, Path.Combine(ciktiKlasoru, "hal-model.zip"));
Console.WriteLine("hal-model.zip oluşturuldu");

public class Ogrenci
{
    [LoadColumn(0)] public float CalismaSaati { get; set; }
    [LoadColumn(1)] public float SinavNotu { get; set; }
}
public class Yorum
{
    [LoadColumn(0)] public string yorum { get; set; } = "";
    [LoadColumn(1)] public bool duygu { get; set; }
}
public class FaturaSatiri
{
    [LoadColumn(0)] public string tarih { get; set; } = "";
    [LoadColumn(1)] public string fatura_no { get; set; } = "";
    [LoadColumn(2)] public string urun { get; set; } = "";
    [LoadColumn(3)] public float adet { get; set; }
    [LoadColumn(4)] public float kg { get; set; }
    [LoadColumn(5)] public float birim_fiyat_tl { get; set; }
    [LoadColumn(6)] public float tutar_tl { get; set; }
    [LoadColumn(7)] public float net_oran { get; set; }
}
public class HalSatisi
{
    public string Urun { get; set; } = "";
    public float Kg { get; set; }
    public float Fiyat { get; set; }
}
