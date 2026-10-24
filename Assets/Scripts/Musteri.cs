using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Müşteriyi yönetir: canavar kapıda bekler, kapıya tıklanınca içeri girer,
// sipariş cümlesini söyler, sabrı azalır, ruh hali değişir, kahveye tepki verir.
public class Musteri : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public KahveDoldurma kahveDoldurma; // Kahve makinesi
    public Mutluluk mutluluk;           // Kafenin mutluluk çubuğu
    public Muzik muzik;                 // Kafe müziği
    public int karanlikMuzikHali = 2;   // Hangi halde karanlık müzik başlasın (1 = nötr, 2 = kızgın)
    public Transform sabirCubugu;       // Yeşil sabır çubuğu
    public TMP_Text siparisYazisi;      // Müşterinin söylediği cümle
    public TMP_Text sonucYazisi;        // Ortadaki büyük sonuç yazısı
    public SpriteRenderer kapi;         // Tıklanınca müşteriyi içeri alan kapı

    [Header("Canavarlar (Canavar Turu kartlarını buraya sürükle)")]
    public CanavarTuru[] canavarlar;

    [Header("Ayarlar")]
    public float sonucBeklemeSuresi = 2f;    // Sonuç ekranda kaç saniye kalır
    public float kapiSabri = 6f;             // Canavar kapıda kaç saniye bekler
    public Color kapiParlamaRengi = new Color(1f, 0.85f, 0.3f); // Kapı yanıp sönerken rengi
    public string[] siparisler = { "Espresso", "Americano", "Latte", "Pumpkin Spice Latte" };

    [Header("Geçiş efekti")]
    public float titremeSuresi = 0.3f;   // Kaç saniye titrer
    public float titremeGucu = 0.06f;    // Ne kadar sert titrer

    SpriteRenderer gorsel;
    Vector3 asilKonum;
    Vector3 asilBoyut;
    int hal = 0;           // 0 = mutlu, 1 = nötr, 2 = kızgın
    CanavarTuru tur;       // Şu anki canavar
    float sabirSuresi = 20f;

    float kalanSabir;
    bool bekliyor = false;       // İçeride, kahve bekliyor mu
    bool kapidaBekliyor = false; // Kapının önünde mi
    float kapiSayac;
    Color kapiNormalRenk;
    float cubukTamBoy;
    float cubukSolKenar;
    string siparis;

    void Start()
    {
        gorsel = GetComponent<SpriteRenderer>();
        asilKonum = transform.position;
        asilBoyut = transform.localScale;
        kapiNormalRenk = kapi.color;

        cubukTamBoy = sabirCubugu.localScale.x;
        cubukSolKenar = sabirCubugu.position.x - cubukTamBoy / 2f;
        YeniMusteri();
    }

    void Update()
    {
        if (kapidaBekliyor)
        {
            KapiBekle();
            return;
        }

        if (!bekliyor) return;

        kalanSabir -= Time.deltaTime;
        CubuguCiz();

        // Sabır azaldıkça ruh hali kötüleşir
        float oran = kalanSabir / sabirSuresi;
        if (oran < 0.33f) HaleGec(2);
        else if (oran < 0.66f) HaleGec(1);

        if (kalanSabir <= 0f)
        {
            kahveDoldurma.Durdur();
            HaleGec(2);
            Bitir("GEC KALDIN!", "GEC");
        }
    }

    // Canavar kapıda: kapı yanıp söner, tıklanmayı bekler
    void KapiBekle()
    {
        kapiSayac -= Time.deltaTime;

        // Kapı yanıp sönsün
        float t = Mathf.PingPong(Time.time * 3f, 1f);
        kapi.color = Color.Lerp(kapiNormalRenk, kapiParlamaRengi, t);

        // Kapıya tıklandı mı
        var fare = Mouse.current;
        if (fare != null && fare.leftButton.wasPressedThisFrame)
        {
            Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());
            Collider2D tiklanan = Physics2D.OverlapPoint(nokta);
            if (tiklanan != null && tiklanan.gameObject == kapi.gameObject)
            {
                IceriAl(false);
                return;
            }
        }

        // Çok beklettiysek: mutluluk düşer, canavar kızgın girer
        if (kapiSayac <= 0f)
        {
            mutluluk.SonucaGoreDegistir("YANLIS");
            if (mutluluk.OyunBitti)
            {
                kapidaBekliyor = false;
                kapi.color = kapiNormalRenk;
                siparisYazisi.text = "";
                return;
            }
            IceriAl(true);
        }
    }

    // Canavarı içeri al
    void IceriAl(bool kizgin)
    {
        kapidaBekliyor = false;
        kapi.color = kapiNormalRenk;
        Sesler.Kapi();
        Kapi kapiAnim = kapi.GetComponent<Kapi>();
        if (kapiAnim != null) kapiAnim.AcVeKapat();

        gorsel.enabled = true;
        sabirCubugu.gameObject.SetActive(true);

        HalGoster(kizgin ? 2 : 0);
        if (kizgin) StartCoroutine(Titre());

        siparisYazisi.text = tur.CumleSec(siparis);
        sonucYazisi.text = kizgin ? "COK BEKLETTIN!" : "";
        if (kizgin) Invoke(nameof(BeklettinYazisiniSil), 1.5f);

        kalanSabir = sabirSuresi;
        bekliyor = true;
        CubuguCiz();
        kahveDoldurma.YeniBardak(siparis);
    }

    void BeklettinYazisiniSil()
    {
        if (sonucYazisi.text == "COK BEKLETTIN!") sonucYazisi.text = "";
    }

    // Kahve makinesi servis edilince bunu çağırır
    public void KahveGeldi(string kalite)
    {
        if (!bekliyor) return;

        if (kalite == "MUKEMMEL")
        {
            HaleGec(0);
            Bitir("MUKEMMEL!", kalite);
        }
        else if (kalite == "IYI")
        {
            Bitir("IYI", kalite);
        }
        else if (kalite == "TASTI")
        {
            HaleGec(2);
            Bitir("TASTI!", kalite);
        }
        else if (kalite == "YANLIS")
        {
            HaleGec(2);
            Bitir("YANLIS KAHVE!", kalite);
        }
        else
        {
            HaleGec(2);
            Bitir("KOTU...", kalite);
        }
    }

    void Bitir(string mesaj, string sonuc)
    {
        bekliyor = false;
        sonucYazisi.text = mesaj;

        // Kafenin mutluluğunu değiştir
        mutluluk.SonucaGoreDegistir(sonuc);

        // Mutluluk bittiyse yeni müşteri gelmesin
        if (mutluluk.OyunBitti)
        {
            kahveDoldurma.Durdur();
            siparisYazisi.text = "";
            sonucYazisi.text = "";
            return;
        }

        Invoke(nameof(YeniMusteri), sonucBeklemeSuresi);
    }

    // Yeni canavar seçilir ve kapının önünde beklemeye başlar
    void YeniMusteri()
    {
        // Yarıda kalan animasyon ya da titreme varsa düzelt
        StopAllCoroutines();
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        if (canavarlar == null || canavarlar.Length == 0)
        {
            Debug.LogError("Musteri: Canavarlar listesi bos! En az bir Canavar Turu karti ekle.");
            return;
        }

        // Rastgele bir canavar ve rastgele bir kahve
        tur = canavarlar[Random.Range(0, canavarlar.Length)];
        sabirSuresi = tur.sabirSuresi;
        HalGoster(0);
        siparis = siparisler[Random.Range(0, siparisler.Length)];
        Debug.Log(tur.canavarAdi + " kapida, istedigi: " + siparis);

        // Canavar henüz görünmüyor, kapıda bekliyor
        gorsel.enabled = false;
        sabirCubugu.gameObject.SetActive(false);
        siparisYazisi.text = "Kapida biri var...";
        sonucYazisi.text = "";

        kapidaBekliyor = true;
        kapiSayac = kapiSabri;
    }

    // Ruh halini değiştir: geçiş animasyonunu oynat ve titret
    void HaleGec(int yeniHal)
    {
        if (yeniHal == hal) return;

        int eskiHal = hal;
        StopAllCoroutines();
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        StartCoroutine(GecisOynat(eskiHal, yeniHal));
        StartCoroutine(Titre());
    }

    // Geçiş karelerini sırayla gösterir, sonunda yeni halin resmine geçer
    IEnumerator GecisOynat(int eskiHal, int yeniHal)
    {
        hal = yeniHal; // Oyun mantığı açısından hal hemen değişir
        if (muzik != null) muzik.KaranlikOlsun(hal >= karanlikMuzikHali);

        List<Sprite> kareler = tur.GecisKareleri(eskiHal, yeniHal);
        foreach (Sprite kare in kareler)
        {
            if (kare == null) continue;
            gorsel.sprite = kare;
            gorsel.color = Color.white;
            yield return new WaitForSeconds(tur.kareSuresi);
        }

        HalGoster(yeniHal);
    }

    // Hale uygun resmi göster, resim yoksa rengi değiştir
    void HalGoster(int yeniHal)
    {
        hal = yeniHal;
        if (muzik != null) muzik.KaranlikOlsun(hal >= karanlikMuzikHali);

        Sprite resim;
        Color renk;
        if (hal == 0) { resim = tur.mutluResim; renk = tur.mutluRenk; }
        else if (hal == 1) { resim = tur.notrResim; renk = tur.notrRenk; }
        else { resim = tur.kizginResim; renk = tur.kizginRenk; }

        if (resim != null)
        {
            gorsel.sprite = resim;
            gorsel.color = Color.white;
        }
        else
        {
            gorsel.color = renk;
        }
    }

    // Kısa süre titreyip büyüyerek geçişi hissettirir
    IEnumerator Titre()
    {
        float t = 0f;
        while (t < titremeSuresi)
        {
            t += Time.deltaTime;
            float kalan = 1f - t / titremeSuresi; // giderek azalır
            transform.position = asilKonum + (Vector3)(Random.insideUnitCircle * titremeGucu * kalan);
            transform.localScale = asilBoyut * (1f + 0.08f * kalan);
            yield return null;
        }
        transform.position = asilKonum;
        transform.localScale = asilBoyut;
    }

    void CubuguCiz()
    {
        float oran = Mathf.Clamp01(kalanSabir / sabirSuresi);

        Vector3 s = sabirCubugu.localScale;
        s.x = cubukTamBoy * oran;
        sabirCubugu.localScale = s;

        Vector3 p = sabirCubugu.position;
        p.x = cubukSolKenar + s.x / 2f;
        sabirCubugu.position = p;
    }
}