using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Müşteriyi yönetir: canavar kapıda bekler, kapıya tıklanınca içeri girer,
// siparişini harf harf söyler (ağzı oynar), sabrı azalır, ruh hali değişir, kahveye tepki verir.
// Hangi kahvelerin istenebileceğini ve sabrı GunYoneticisi belirler.
public class Musteri : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public GunYoneticisi gunYoneticisi; // Günleri yöneten obje
    public KahveDoldurma kahveDoldurma; // Kahve makinesi
    public Mutluluk mutluluk;           // Kafenin mutluluk çubuğu
    public Muzik muzik;                 // Kafe müziği
    public int karanlikMuzikHali = 2;   // Hangi halde karanlık müzik başlasın (1 = nötr, 2 = kızgın)
    public Transform sabirCubugu;       // Yeşil sabır çubuğu
    public TMP_Text siparisYazisi;      // Müşterinin söylediği cümle
    public TMP_Text sonucYazisi;        // Ortadaki büyük sonuç yazısı
    public SpriteRenderer kapi;         // Tıklanınca müşteriyi içeri alan kapı
    public GameObject baloncuk;         // Konuşma baloncuğu (boş kalabilir)
    public ParticleSystem gecisEfekti;  // Hal değişirken çıkan efekt (boş kalabilir)

    [Header("Canavarlar (Canavar Turu kartlarını buraya sürükle)")]
    public CanavarTuru[] canavarlar;

    [Header("Ayarlar")]
    public float sonucBeklemeSuresi = 2f;    // Sonuç ekranda kaç saniye kalır
    public float kapiSabri = 6f;             // Canavar kapıda kaç saniye bekler
    public Color kapiParlamaRengi = new Color(1f, 0.85f, 0.3f); // Kapı yanıp sönerken rengi

    [Header("Konuşma")]
    public float agizHizi = 0.12f;       // Konuşurken ağız kaç saniyede bir açılıp kapanır

    [Header("Geçiş efekti")]
    public float titremeSuresi = 0.3f;   // Kaç saniye titrer
    public float titremeGucu = 0.06f;    // Ne kadar sert titrer

    SpriteRenderer gorsel;
    Daktilo daktilo;
    Vector3 asilKonum;
    Vector3 asilBoyut;
    int hal = 0;           // 0 = mutlu, 1 = nötr, 2 = kızgın
    CanavarTuru tur;       // Şu anki canavar
    Sprite halResmi;       // Şu anki halin normal resmi
    bool gecisOynuyor = false;
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
        daktilo = siparisYazisi.GetComponent<Daktilo>();
        asilKonum = transform.position;
        asilBoyut = transform.localScale;
        kapiNormalRenk = kapi.color;

        cubukTamBoy = sabirCubugu.localScale.x;
        cubukSolKenar = sabirCubugu.position.x - cubukTamBoy / 2f;

        // Gün başlayana kadar ortada kimse yok
        gorsel.enabled = false;
        sabirCubugu.gameObject.SetActive(false);
        if (baloncuk != null) baloncuk.SetActive(false);
        YaziTemizle();
        sonucYazisi.text = "";
    }
    // Müşteri içeride kahve bekliyor mu (korku efektleri için)
    public bool IcerideBekliyor => bekliyor;

    // GunYoneticisi yeni gün başlayınca çağırır
    public void GunuBaslat()
    {
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

        AgizOynat();

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
            Bitir("GEÇ KALDIN!", "GEC");
        }
    }

    // Sipariş yazısını harf harf göster
    void YaziGoster(string metin)
    {
        if (daktilo != null) daktilo.Yaz(metin);
        else siparisYazisi.text = metin;
    }

    void YaziTemizle()
    {
        if (daktilo != null) daktilo.Temizle();
        else siparisYazisi.text = "";
    }

    // Konuşurken ağız açılıp kapanır (ağzı açık resim varsa)
    void AgizOynat()
    {
        if (gecisOynuyor || tur == null || halResmi == null) return;

        Sprite agizAcik = tur.AgizAcikResim(hal);
        if (agizAcik == null) return;

        bool konusuyor = daktilo != null && daktilo.YaziyorMu;
        bool acik = konusuyor && Mathf.Repeat(Time.time, agizHizi * 2f) < agizHizi;
        gorsel.sprite = acik ? agizAcik : halResmi;
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
                YaziTemizle();
                sonucYazisi.text = "";
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
        if (baloncuk != null) baloncuk.SetActive(true);

        HalGoster(kizgin ? 2 : 0);
        if (kizgin) StartCoroutine(Titre());

        YaziGoster(tur.CumleSec(siparis));
        sonucYazisi.text = kizgin ? "ÇOK BEKLETTİN!" : "";
        if (kizgin) Invoke(nameof(BeklettinYazisiniSil), 1.5f);

        kalanSabir = sabirSuresi;
        bekliyor = true;
        CubuguCiz();
        kahveDoldurma.YeniBardak(siparis);
    }

    void BeklettinYazisiniSil()
    {
        if (sonucYazisi.text == "ÇOK BEKLETTİN!") sonucYazisi.text = "";
    }

    // Kahve makinesi servis edilince bunu çağırır
    public void KahveGeldi(string kalite)
    {
        if (!bekliyor) return;

        if (kalite == "MUKEMMEL")
        {
            HaleGec(0);
            Bitir("MÜKEMMEL!", kalite);
        }
        else if (kalite == "IYI")
        {
            Bitir("İYİ", kalite);
        }
        else if (kalite == "TASTI")
        {
            HaleGec(2);
            Bitir("TAŞTI!", kalite);
        }
        else if (kalite == "YANLIS")
        {
            HaleGec(2);
            Bitir("YANLIŞ KAHVE!", kalite);
        }
        else
        {
            HaleGec(2);
            Bitir("KÖTÜ...", kalite);
        }
    }

    void Bitir(string mesaj, string sonuc)
    {
        bekliyor = false;
        sonucYazisi.text = mesaj;
        YaziTemizle();
        if (baloncuk != null) baloncuk.SetActive(false);
        if (halResmi != null && !gecisOynuyor) gorsel.sprite = halResmi; // ağız açık kalmasın

        // Kafenin mutluluğunu değiştir
        mutluluk.SonucaGoreDegistir(sonuc);

        // Mutluluk bittiyse yeni müşteri gelmesin
        if (mutluluk.OyunBitti)
        {
            kahveDoldurma.Temizle();
            sonucYazisi.text = "";
            return;
        }

        // Günün müşterileri bitti mi
        bool memnun = sonuc == "MUKEMMEL" || sonuc == "IYI";
        bool gunBitti = gunYoneticisi.MusteriBitti(memnun);

        if (gunBitti) Invoke(nameof(GunuKapat), sonucBeklemeSuresi);
        else Invoke(nameof(YeniMusteri), sonucBeklemeSuresi);
    }

    // Gün bitti: ortalığı temizle, gün sonu ekranını aç
    void GunuKapat()
    {
        StopAllCoroutines();
        gecisOynuyor = false;
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        gorsel.enabled = false;
        sabirCubugu.gameObject.SetActive(false);
        if (baloncuk != null) baloncuk.SetActive(false);
        sonucYazisi.text = "";
        kahveDoldurma.Temizle();
        if (muzik != null) muzik.KaranlikOlsun(false);

        gunYoneticisi.GunSonuGoster();
    }

    // Yeni canavar seçilir ve kapının önünde beklemeye başlar
    void YeniMusteri()
    {
        // Yarıda kalan animasyon ya da titreme varsa düzelt
        StopAllCoroutines();
        gecisOynuyor = false;
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        if (canavarlar == null || canavarlar.Length == 0)
        {
            Debug.LogError("Musteri: Canavarlar listesi bos! En az bir Canavar Turu karti ekle.");
            return;
        }

        // Rastgele bir canavar, günün kahvelerinden rastgele bir kahve
        tur = canavarlar[Random.Range(0, canavarlar.Length)];
        sabirSuresi = tur.sabirSuresi * gunYoneticisi.SabirCarpani;
        HalGoster(0);
        siparis = gunYoneticisi.RastgeleKahve();
        Debug.Log(gunYoneticisi.GunNumarasi + ". gun: " + tur.canavarAdi + " kapida, istedigi: " + siparis);

        // Canavar henüz görünmüyor, kapıda bekliyor
        gorsel.enabled = false;
        sabirCubugu.gameObject.SetActive(false);
        if (baloncuk != null) baloncuk.SetActive(false);
        YaziTemizle();
        sonucYazisi.text = "Kapıda biri var...";

        kapidaBekliyor = true;
        kapiSayac = kapiSabri;
    }

    // Ruh halini değiştir: geçiş animasyonunu oynat ve titret
    void HaleGec(int yeniHal)
    {
        if (yeniHal == hal) return;

        int eskiHal = hal;
        StopAllCoroutines();
        gecisOynuyor = false;
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        StartCoroutine(GecisOynat(eskiHal, yeniHal));
        StartCoroutine(Titre());
    }

    // Hal değişimi: geçiş kareleri varsa onları oynatır,
    // yoksa canavar siyaha kararır, yeni hale geçer ve siyahtan açılır.
    IEnumerator GecisOynat(int eskiHal, int yeniHal)
    {
        gecisOynuyor = true;
        hal = yeniHal; // Oyun mantığı açısından hal hemen değişir
        if (muzik != null) muzik.KaranlikOlsun(hal >= karanlikMuzikHali);
        if (gecisEfekti != null) gecisEfekti.Play();

        // Tasarımcının çizdiği geçiş kareleri varsa onları oynat
        List<Sprite> kareler = tur.GecisKareleri(eskiHal, yeniHal);
        if (kareler.Count > 0)
        {
            foreach (Sprite kare in kareler)
            {
                if (kare == null) continue;
                gorsel.sprite = kare;
                gorsel.color = Color.white;
                yield return new WaitForSeconds(tur.kareSuresi);
            }
            HalGoster(yeniHal);
            gecisOynuyor = false;
            yield break;
        }

        // Kare yoksa: siyaha karar
        float sure = tur.siyahGecisSuresi;
        Color baslangicRenk = gorsel.color;
        float t = 0f;
        while (t < sure)
        {
            t += Time.deltaTime;
            gorsel.color = Color.Lerp(baslangicRenk, Color.black, t / sure);
            yield return null;
        }

        // Tam siyahken yeni hale geç
        HalGoster(yeniHal);
        Color hedefRenk = gorsel.color;
        gorsel.color = Color.black;

        // Siyahtan açıl
        t = 0f;
        while (t < sure)
        {
            t += Time.deltaTime;
            gorsel.color = Color.Lerp(Color.black, hedefRenk, t / sure);
            yield return null;
        }
        gorsel.color = hedefRenk;
        gecisOynuyor = false;
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

        halResmi = resim;

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