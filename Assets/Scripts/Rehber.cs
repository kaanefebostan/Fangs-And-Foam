using UnityEngine;
using TMPro;

// 1. günün ilk müşterisinde ekranın altında adım adım ipucu gösterir.
// İlk müşteri gidince bir son ipucu verir ve bir daha görünmez.
public class Rehber : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public Musteri musteri;
    public KahveDoldurma kahveDoldurma;
    public GunYoneticisi gunYoneticisi;
    public Mutluluk mutluluk;
    public GameObject kutu;      // Alttaki koyu şerit (yazıyla birlikte)
    public TMP_Text yazi;        // Şeridin içindeki yazı

    [Header("İpuçları")]
    public string kapiIpucu = "Kapı çalıyor! Kapıya tıkla.";
    public string soruIpucu = "Bir cevap seç.";
    public string makineIpucu = "Siparişi oku. Önce kahve makinesine tıkla.";
    public string dokIpucu = "SPACE'e basılı tut, kırmızı çizgiye gelince bırak.";
    public string servisIpucu = "Hazır! E ile servis et.";
    public string sonIpucu = "Tarifler tezgahtaki kitapta. İyi şanslar...";
    public float sonIpucuSuresi = 4f;

    bool basladi = false;   // İlk müşteri içeri girdi mi
    bool siparisVerdi = false; // İlk müşteri sipariş verdi mi
    bool bitti = false;
    float sonSayac = -1f;

    void Start()
    {
        Goster("");
    }

    void Update()
    {
        if (bitti) return;

        // Sadece 1. günde ve oyun bitmemişken
        if (gunYoneticisi == null || gunYoneticisi.GunNumarasi != 1 || (mutluluk != null && mutluluk.OyunBitti))
        {
            Goster("");
            return;
        }

        // Son ipucu birkaç saniye görünür, sonra rehber biter
        if (sonSayac >= 0f)
        {
            sonSayac -= Time.deltaTime;
            if (sonSayac < 0f)
            {
                Goster("");
                bitti = true;
            }
            return;
        }

        if (musteri.KapidaBekliyor)
        {
            // İlk müşteri bittiyse ve ikincisi kapıdaysa: son ipucuna geç
            if (basladi) { SonIpucu(); return; }
            Goster(kapiIpucu);
            return;
        }

        if (!musteri.IcerideMi)
        {
            // Müşteri gitti (ya da henüz gelmedi)
            if (basladi) SonIpucu();
            else Goster("");
            return;
        }

        basladi = true;

        // Canavar soru soruyor ya da tepki veriyor
        if (!musteri.IcerideBekliyor)
        {
            // Sipariş vermeden önceyse soru soruyordur, sonraysa kahve servis edildi
            Goster(siparisVerdi ? "" : soruIpucu);
            return;
        }
        siparisVerdi = true;

        // Kahve yapımı
        bool kahveSecili = kahveDoldurma.Secili != null && kahveDoldurma.Secili.malzemeAdi == "Kahve";
        if (kahveDoldurma.KahveVar && !kahveDoldurma.Dokuyor) Goster(servisIpucu);
        else if (kahveSecili) Goster(dokIpucu);
        else Goster(makineIpucu);
    }

    void SonIpucu()
    {
        Goster(sonIpucu);
        sonSayac = sonIpucuSuresi;
    }

    void Goster(string metin)
    {
        bool goster = !string.IsNullOrEmpty(metin);
        if (kutu != null && kutu.activeSelf != goster) kutu.SetActive(goster);
        if (yazi != null && yazi.text != metin) yazi.text = metin;
    }
}
