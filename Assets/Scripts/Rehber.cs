using UnityEngine;
using TMPro;

// 1. günün ilk müşterisinde ekranın altında adım adım ipucu gösterir.
// Sıradaki tıklanacak objenin çerçevesi yanıp söner (kapı, tarif kitabı, kahve makinesi).
// Oyuncu kahve yapmadan önce tarif kitabını bir kez açmak zorundadır.
// Sonraki günlerde yeni tarif geldiyse kitap açılana kadar kitap yanıp söner.
public class Rehber : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public Musteri musteri;
    public KahveDoldurma kahveDoldurma;
    public GunYoneticisi gunYoneticisi;
    public Mutluluk mutluluk;
    public GameObject kutu;      // Alttaki koyu şerit (yazıyla birlikte)
    public TMP_Text yazi;        // Şeridin içindeki yazı

    [Header("Yanıp sönecek objeler (SecilebilirVurgu ekli olmalı)")]
    public SecilebilirVurgu kapiVurgu;
    public SecilebilirVurgu kitapVurgu;
    public SecilebilirVurgu makineVurgu;

    [Header("İpuçları")]
    public string kapiIpucu = "Kapı çalıyor! Kapıya tıkla.";
    public string soruIpucu = "Bir cevap seç.";
    public string kitapIpucu = "Kahve nasıl yapılır? Tezgâhtaki TARİF KİTABINA tıkla!";
    public string kitapAcikIpucu = "Tarifi oku, sonra kitabı kapat.";
    public string makineIpucu = "Tarif: sadece kahve. Kahve makinesine tıkla.";
    public string dokIpucu = "SPACE'e basılı tut, kırmızı çizgiye gelince bırak.";
    public string servisIpucu = "Hazır! E ile servis et.";
    public string sonIpucu = "Her yeni kahvede önce tarif kitabına bak. İyi şanslar...";
    public string yeniTarifIpucu = "Yeni tarif: {0}! Nasıl yapılır? Tarif kitabına bak.";
    public float sonIpucuSuresi = 4f;

    [Header("Dikkat çekme")]
    public Color vurguRengi = new Color(1f, 0.83f, 0.3f);   // #FFD34D sarı
    public float nabizHizi = 1.5f;                           // Yazı ne hızlı sarı-beyaz arası gider
    public float ziplamaBoyu = 0.2f;                         // Yeni ipucu gelince yazı ne kadar büyüyüp küçülür
    public AudioClip ipucuSesi;                              // Yeni ipucu gelince çalar (boş kalabilir)

    bool basladi = false;       // İlk müşteri içeri girdi mi
    bool siparisVerdi = false;  // İlk müşteri sipariş verdi mi
    bool ilkGunBitti = false;   // 1. gün rehberi bitti mi
    float sonSayac = -1f;

    int sonGun = -1;
    bool kitapBugunAcildi = false;

    Color yaziRengi;
    Vector3 yaziBoyut;
    float zipla;

    void Start()
    {
        if (yazi != null)
        {
            yaziRengi = yazi.color;
            yaziBoyut = yazi.transform.localScale;
        }
        Goster("");
    }

    void Update()
    {
        if (gunYoneticisi == null || musteri == null || (mutluluk != null && mutluluk.OyunBitti))
        {
            Goster("");
            Isaretle(null);
            return;
        }

        // Yeni gün başladıysa "kitap bugün açıldı mı" sıfırlanır
        int gunNo = gunYoneticisi.GunNumarasi;
        if (gunNo != sonGun)
        {
            sonGun = gunNo;
            kitapBugunAcildi = false;
        }
        if (TarifKitabi.Acik) kitapBugunAcildi = true;

        if (gunNo == 1 && !ilkGunBitti) IlkGunRehberi();
        else YeniTarifHatirlatmasi(gunNo);

        Efekt();
    }

    // 1. günün ilk müşterisi: adım adım
    void IlkGunRehberi()
    {
        // Son ipucu birkaç saniye görünür, sonra rehber biter
        if (sonSayac >= 0f)
        {
            sonSayac -= Time.deltaTime;
            Isaretle(null);
            if (sonSayac < 0f)
            {
                Goster("");
                ilkGunBitti = true;
            }
            return;
        }

        if (musteri.KapidaBekliyor)
        {
            // İlk müşteri bittiyse ve ikincisi kapıdaysa: son ipucuna geç
            if (basladi) { SonIpucu(); return; }
            Goster(kapiIpucu);
            Isaretle(kapiVurgu);
            return;
        }

        if (!musteri.IcerideMi)
        {
            // Müşteri gitti (ya da henüz gelmedi)
            if (basladi) SonIpucu();
            else { Goster(""); Isaretle(null); }
            return;
        }

        basladi = true;

        // Canavar soru soruyor ya da tepki veriyor
        if (!musteri.IcerideBekliyor)
        {
            // Sipariş vermeden önceyse soru soruyordur, sonraysa kahve servis edildi
            Goster(siparisVerdi ? "" : soruIpucu);
            Isaretle(null);
            return;
        }
        siparisVerdi = true;

        // Önce tarif kitabı
        if (TarifKitabi.Acik)
        {
            Goster(kitapAcikIpucu);
            Isaretle(null);
            return;
        }
        if (!kitapBugunAcildi)
        {
            Goster(kitapIpucu);
            Isaretle(kitapVurgu);
            return;
        }

        // Kahve yapımı
        bool kahveSecili = kahveDoldurma.Secili != null && kahveDoldurma.Secili.malzemeAdi == "Kahve";
        if (kahveDoldurma.KahveVar && !kahveDoldurma.Dokuyor) { Goster(servisIpucu); Isaretle(null); }
        else if (kahveSecili) { Goster(dokIpucu); Isaretle(null); }
        else { Goster(makineIpucu); Isaretle(makineVurgu); }
    }

    // 2. günden sonra: bugün yeni tarif varsa, kitap açılana kadar hatırlat
    void YeniTarifHatirlatmasi(int gunNo)
    {
        string yeni = YeniTarif(gunNo);
        bool hatirlat = yeni != "" && !kitapBugunAcildi && !TarifKitabi.Acik && musteri.IcerideBekliyor;

        Goster(hatirlat ? string.Format(yeniTarifIpucu, yeni) : "");
        Isaretle(hatirlat ? kitapVurgu : null);
    }

    // Bu güne yeni eklenen kahvelerin adı (yoksa boş)
    string YeniTarif(int gunNo)
    {
        GunAyari[] gunler = gunYoneticisi.gunler;
        int i = gunNo - 1;
        if (gunler == null || i <= 0 || i >= gunler.Length) return "";

        string sonuc = "";
        foreach (string k in gunler[i].kahveler)
        {
            if (System.Array.IndexOf(gunler[i - 1].kahveler, k) >= 0) continue;
            sonuc += (sonuc == "" ? "" : ", ") + k;
        }
        return sonuc;
    }

    void SonIpucu()
    {
        Goster(sonIpucu);
        Isaretle(null);
        sonSayac = sonIpucuSuresi;
    }

    // Sadece verilen obje yanıp söner, diğerleri söner
    void Isaretle(SecilebilirVurgu hedef)
    {
        if (kapiVurgu != null) kapiVurgu.Isaretli = kapiVurgu == hedef;
        if (kitapVurgu != null) kitapVurgu.Isaretli = kitapVurgu == hedef;
        if (makineVurgu != null) makineVurgu.Isaretli = makineVurgu == hedef;
    }

    void Goster(string metin)
    {
        bool goster = !string.IsNullOrEmpty(metin);
        if (kutu != null && kutu.activeSelf != goster) kutu.SetActive(goster);
        if (yazi != null && yazi.text != metin)
        {
            yazi.text = metin;
            // Yeni ipucu geldi: yazı bir kez zıplasın, ses çalsın
            if (goster)
            {
                zipla = 1f;
                if (ipucuSesi != null) Sesler.Oynat(ipucuSesi);
            }
        }
    }

    // Yazı sarı ile kendi rengi arasında yavaşça gidip gelir, yeni ipucunda bir kez büyüyüp küçülür
    void Efekt()
    {
        if (yazi == null || string.IsNullOrEmpty(yazi.text)) return;

        float t = (Mathf.Sin(Time.unscaledTime * nabizHizi * Mathf.PI * 2f) + 1f) / 2f;
        yazi.color = Color.Lerp(yaziRengi, vurguRengi, t);

        if (zipla > 0f) zipla = Mathf.Max(0f, zipla - Time.unscaledDeltaTime * 3f);
        float buyume = 1f + ziplamaBoyu * Mathf.Sin(zipla * Mathf.PI);
        yazi.transform.localScale = yaziBoyut * buyume;
    }
}
