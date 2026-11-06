using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Müşteriyi yönetir: canavar kapıda bekler, kapıya tıklanınca içeri girer,
// bazen önce bir soru sorar (iki cevap butonu), sonra siparişini harf harf söyler (ağzı oynar),
// sabrı azalır, ruh hali değişir, kahveye tepki verir.
// Kafenin güveni: kötü cevaplar güveni düşürür, güven düştükçe herkesin sabrı kısalır.
// Hangi kahvelerin istenebileceğini ve sabrı GunYoneticisi belirler.
public class Musteri : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public GunYoneticisi gunYoneticisi; // Günleri yöneten obje
    public KahveDoldurma kahveDoldurma; // Kahve makinesi
    public Mutluluk mutluluk;           // Kafenin mutluluk çubuğu
    public Muzik muzik;                 // Kafe müziği (hal değiştikçe kararır)
    public Transform sabirCubugu;       // Yeşil sabır çubuğu
    public TMP_Text siparisYazisi;      // Müşterinin söylediği cümle
    public TMP_Text sonucYazisi;        // Ortadaki büyük sonuç yazısı
    public SpriteRenderer kapi;         // Tıklanınca müşteriyi içeri alan kapı
    public GameObject baloncuk;         // Konuşma baloncuğu (boş kalabilir)
    public ParticleSystem gecisEfekti;  // Hal değişirken çıkan efekt (boş kalabilir)
    public SpriteRenderer kapiUyarisi;  // Canavar kapıdayken kapının üstünde zıplayan ünlem (boş kalabilir)
    public TepkiEfekti tepkiEfekti;     // Kalp / duman efekti (boş kalabilir)
    public TMP_Text siparisEtiketi;     // Bardağın üstündeki "Sipariş: Latte" yazısı (boş kalabilir)
    public float etiketGecikmesi = 0.5f; // Canavar siparişini söyleyip bitirdikten kaç saniye sonra etiket çıksın
    public float okumaPayi = 0.5f;       // Sipariş cümlesi bittikten sonra sabır kaç saniye daha beklesin

    [Header("Canavarlar (Canavar Turu kartlarını buraya sürükle)")]
    public CanavarTuru[] canavarlar;

    [Header("Ayarlar")]
    public float sonucBeklemeSuresi = 2f;    // Sonuç ekranda kaç saniye kalır
    public float kapiSabri = 6f;             // Canavar kapıda kaç saniye bekler
    public Color kapiParlamaRengi = new Color(1f, 0.85f, 0.3f); // Kapı yanıp sönerken rengi

    [Header("Konuşma")]
    public float agizHizi = 0.1f;        // Konuşurken ağız kaç saniyede bir değişir

    [Header("Geçiş efekti")]
    public float titremeSuresi = 0.3f;   // Kaç saniye titrer
    public float titremeGucu = 0.06f;    // Ne kadar sert titrer

    [Header("Canlılık")]
    public float nefesHizi = 0.6f;       // Saniyede kaç kez nefes alır
    public float nefesMiktari = 0.012f;  // Nefes alırken ne kadar uzar (0.012 = %1.2)
    public float girisSuresi = 0.35f;    // İçeri girerken belirme süresi
    public float cikisSuresi = 0.35f;    // Giderken kaybolma süresi
    public float girisKaymasi = 0.15f;   // Girerken aşağıdan ne kadar yukarı kayar

    [Header("Sabır barı resmi (boş kalabilir)")]
    public SpriteRenderer sabirResmi;     // Yüzlü bar resmi (Visible Inside Mask)
    public GameObject sabirArkaPlani;     // Barın arkadaki soluk kopyası

    [Header("Sabır çubuğu renkleri (bar resmi yoksa kullanılır)")]
    public Color sabirIyiRenk = new Color(0.2f, 0.9f, 0.2f);   // yeşil
    public Color sabirOrtaRenk = new Color(1f, 0.8f, 0.2f);    // sarı
    public Color sabirKotuRenk = new Color(0.9f, 0.1f, 0.1f);  // kırmızı

    [Header("Soru butonları")]
    public SpriteRenderer cevapButonu1;  // Birinci cevap butonu (Box Collider 2D olmalı)
    public SpriteRenderer cevapButonu2;  // İkinci cevap butonu (Box Collider 2D olmalı)
    public TMP_Text cevapYazisi1;        // Birinci butonun yazısı
    public TMP_Text cevapYazisi2;        // İkinci butonun yazısı
    public Color butonNormalRenk = new Color(0.75f, 0.75f, 0.75f);
    public Color butonUzerindeRenk = Color.white;

    [Header("Soru ayarları")]
    [Range(0f, 1f)] public float soruIhtimali = 0.35f; // Müşterilerin ne kadarı soru sorar
    public float iyiCevapMutluluk = 5f;     // İyi cevapta mutluluk değişimi
    public float kotuCevapMutluluk = -10f;  // Kötü cevapta mutluluk değişimi
    public float tepkiSonrasiBekleme = 1.2f; // Tepkiden sonra siparişe geçmeden önce bekleme

    [Header("Oyuncunun konuşma kutusu (alttaki şerit, boş kalabilir)")]
    public GameObject oyuncuKutusu;         // Ekranın altındaki koyu şerit
    public TMP_Text oyuncuYazisi;           // Şeridin içindeki yazı (Daktilo eklenirse harf harf yazar)
    public float oyuncuSonrasiBekleme = 1.2f; // Oyuncunun sözü bittikten sonra bekleme

    [Header("Mükemmel serisi")]
    public float seriBonusu = 5f;        // Serinin her adımında ekstra mutluluk (x2 = +5, x3 = +10...)
    public float enFazlaSeriBonusu = 15f;

    [Header("Kafenin güveni")]
    public TMP_Text guvenYazisi;            // "Güven %85" yazısı (boş kalabilir)
    public float iyiCevapGuven = 5f;        // İyi cevapta güven artışı
    public float kotuCevapGuven = -15f;     // Kötü cevapta güven azalışı
    [Range(0.1f, 1f)] public float enDusukSabirOrani = 0.5f; // Güven 0 olunca sabır bu orana iner

    const string GuvenAnahtari = "kafeGuveni";

    // Ortadaki sonuç yazısının renkli mesajları
    const string YesilRenk = "#7CFC6A";
    const string SariRenk = "#FFD34D";
    const string KirmiziRenk = "#FF4A3D";
    const string GuvenArttiMesaji = "<color=" + YesilRenk + ">GÜVEN ARTTI!</color>";
    const string GuvenAzaldiMesaji = "<color=" + KirmiziRenk + ">GÜVEN AZALDI!</color>";
    const string BeklettinMesaji = "<color=" + KirmiziRenk + ">ÇOK BEKLETTİN!</color>";

    SpriteRenderer gorsel;
    Daktilo daktilo;
    Vector3 asilKonum;
    Vector3 asilBoyut;
    int hal = 0;           // 0 = mutlu, 1 = nötr, 2 = kızgın
    CanavarTuru tur;       // Şu anki canavar
    Sprite halResmi;       // Şu anki halin normal (ağzı kapalı) resmi
    bool gecisOynuyor = false;
    bool titriyor = false;          // Titre animasyonu oynuyor mu
    bool animasyonOynuyor = false;  // Giriş/çıkış animasyonu oynuyor mu
    SpriteRenderer sabirGorsel;
    Vector3 kapiUyarisiKonum;
    float sabirSuresi = 20f;
    float agizSayac;
    int sonAgizKaresi = -1;
    int seri = 0;          // Arka arkaya kaç mükemmel kahve
    bool etiketBekliyor = false; // Sipariş cümlesi bitince etiket gösterilecek mi
    float etiketSayac;
    bool siparisOkunuyor = false; // Canavar siparişini söylerken sabır azalmaz
    float okumaSayac;
    string bekleyenCumle;  // Geçiş animasyonu bitince söylenecek cümle

    // Torba sistemi: her canavar bir kez gelmeden hiçbiri tekrar gelmez
    List<CanavarTuru> torba = new List<CanavarTuru>();
    CanavarTuru sonCanavar; // Bir önceki müşteri

    // Soru sistemi
    enum Asama { Yok, Soru, Tepki }
    Asama asama = Asama.Yok;
    Soru aktifSoru;
    bool butonlarGorunuyor = false;
    float tepkiSayac;
    HashSet<string> sorulanlar = new HashSet<string>(); // Bu oyunda sorulmuş sorular ("kartAdi:sira")
    const string SorulanlarAnahtari = "sorulanSorular";

    // Kafenin güveni (0 - 100)
    float guven = 100f;

    // Saldırı sahnesi (postere bakılınca)
    bool saldiriyor = false;
    int saldiriAsama = 0;           // 1 = oyuncu konuşuyor, 2 = canavar konuşuyor
    float saldiriSayac;
    string saldiriCanavarCumlesi;
    string saldiriMetni;

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
        Istatistik.Sifirla();
        if (siparisEtiketi != null) siparisEtiketi.text = "";
        gorsel = GetComponent<SpriteRenderer>();
        daktilo = siparisYazisi.GetComponent<Daktilo>();
        asilKonum = transform.position;
        asilBoyut = transform.localScale;
        kapiNormalRenk = kapi.color;

        cubukTamBoy = sabirCubugu.localScale.x;
        cubukSolKenar = sabirCubugu.position.x - cubukTamBoy / 2f;
        sabirGorsel = sabirCubugu.GetComponent<SpriteRenderer>();
        if (kapiUyarisi != null)
        {
            kapiUyarisiKonum = kapiUyarisi.transform.position;
            kapiUyarisi.enabled = false;
        }

        // Gün başlayana kadar ortada kimse yok
        gorsel.enabled = false;
        SabirGoster(false);
        if (baloncuk != null) baloncuk.SetActive(false);
        ButonlariGizle();
        OyuncuKutusunuKapat();
        YaziTemizle();
        sonucYazisi.text = "";

        guven = PlayerPrefs.GetFloat(GuvenAnahtari, 100f);
        GuvenYazisiniGuncelle();
    }

    // Müşteri içeride kahve bekliyor mu (korku efektleri için)
    public bool IcerideBekliyor => bekliyor;

    // Canavar kapıda mı bekliyor (rehber için)
    public bool KapidaBekliyor => kapidaBekliyor;

    // Canavar içeride mi (soru sorarken, beklerken ya da tepki verirken)
    public bool IcerideMi => gorsel != null && gorsel.enabled && !kapidaBekliyor;

    // Şu an gelen canavar (jumpscare için)
    public CanavarTuru SuankiCanavar => tur;

    // Kafenin güveni (0 - 100)
    public float Guven => guven;

    // Jumpscare başlarken müşteriyi, sabır çubuğunu ve baloncuğu gizler
    public void JumpscareIcinGizle()
    {
        if (siparisEtiketi != null) siparisEtiketi.text = "";
        etiketBekliyor = false;
        siparisOkunuyor = false;
        CancelInvoke();
        saldiriyor = false;
        saldiriAsama = 0;
        OyuncuKutusunuKapat();
        KorutinleriDurdur();
        gecisOynuyor = false;
        bekleyenCumle = null;
        asama = Asama.Yok;
        ButonlariGizle();
        transform.position = asilKonum;
        transform.localScale = asilBoyut;
        gorsel.enabled = false;
        SabirGoster(false);
        if (baloncuk != null) baloncuk.SetActive(false);
        YaziTemizle();
    }

    // Canavar saldırıyor (örn. ikizler postere bakıldığını gördü).
    // Önce oyuncu alttaki kutuda bir şey der, canavar kızgın hale geçip cevap verir,
    // sonra saldırır ve oyun biter.
    public void Saldir(string oyuncuCumlesi, string canavarCumlesi, string metin)
    {
        if (mutluluk.OyunBitti || saldiriyor) return;

        CancelInvoke();
        saldiriyor = true;
        saldiriCanavarCumlesi = canavarCumlesi;
        saldiriMetni = metin;

        bekliyor = false;
        kapidaBekliyor = false;
        asama = Asama.Yok;
        bekleyenCumle = null;
        ButonlariGizle();
        kapi.color = kapiNormalRenk;
        kahveDoldurma.Temizle();
        YaziTemizle();
        sonucYazisi.text = "";

        // Canavar gitmek üzereyse bile geri gelsin, net görünsün
        KorutinleriDurdur();
        gecisOynuyor = false;
        transform.position = asilKonum;
        transform.localScale = asilBoyut;
        gorsel.enabled = true;
        HalGoster(hal);

        // 1. adım: oyuncu konuşur (alttaki kutu)
        saldiriAsama = 1;
        saldiriSayac = oyuncuSonrasiBekleme;
        OyuncuKonussun(oyuncuCumlesi);
    }

    // Oyuncunun sözünü alttaki kutuda harf harf gösterir
    void OyuncuKonussun(string metin)
    {
        if (string.IsNullOrEmpty(metin)) return;

        if (oyuncuKutusu != null) oyuncuKutusu.SetActive(true);
        if (oyuncuYazisi != null)
        {
            oyuncuYazisi.gameObject.SetActive(true);
            Daktilo d = oyuncuYazisi.GetComponent<Daktilo>();
            if (d != null) d.Yaz(metin);
            else oyuncuYazisi.text = metin;
        }
        else
        {
            // Kutu kurulmadıysa ortadaki yazıda göster
            sonucYazisi.text = "<size=60%>" + metin + "</size>";
        }
    }

    bool OyuncuYaziyor
    {
        get
        {
            if (oyuncuYazisi == null) return false;
            Daktilo d = oyuncuYazisi.GetComponent<Daktilo>();
            return d != null && d.YaziyorMu;
        }
    }

    void OyuncuKutusunuKapat()
    {
        if (oyuncuYazisi != null)
        {
            Daktilo d = oyuncuYazisi.GetComponent<Daktilo>();
            if (d != null) d.Temizle();
            else oyuncuYazisi.text = "";
        }
        if (oyuncuKutusu != null) oyuncuKutusu.SetActive(false);
    }

    void SaldiriSahnesi()
    {
        if (saldiriAsama == 1)
        {
            // Oyuncu sözünü bitirsin, okunsun diye biraz beklensin
            if (OyuncuYaziyor)
            {
                saldiriSayac = oyuncuSonrasiBekleme;
                return;
            }
            saldiriSayac -= Time.deltaTime;
            if (saldiriSayac > 0f) return;

            // 2. adım: canavar döner, kızar ve konuşur
            OyuncuKutusunuKapat();
            sonucYazisi.text = "";
            saldiriAsama = 2;
            saldiriSayac = 1f;

            if (baloncuk != null) baloncuk.SetActive(true);
            Kiz();
            bekleyenCumle = string.IsNullOrEmpty(saldiriCanavarCumlesi) ? null : saldiriCanavarCumlesi;
            if (!gecisOynuyor) BekleyenCumleyiSoyle();
        }
        else if (saldiriAsama == 2)
        {
            // Canavar sözünü bitirince kısa bir sessizlik, sonra saldırı
            if (gecisOynuyor || bekleyenCumle != null || Yaziyor)
            {
                saldiriSayac = 1f;
                return;
            }
            saldiriSayac -= Time.deltaTime;
            if (saldiriSayac > 0f) return;

            saldiriAsama = 0;
            saldiriyor = false;
            mutluluk.Saldiri(saldiriMetni);
        }
    }

    // GunYoneticisi yeni gün başlayınca çağırır
    public void GunuBaslat()
    {
        Istatistik.Gun = gunYoneticisi.GunNumarasi;
        // Yeni oyunda güven sıfırdan başlar, sonraki günlerde kaldığı yerden devam eder
        if (gunYoneticisi.GunNumarasi <= 1)
        {
            guven = 100f;
            sorulanlar.Clear();
            PlayerPrefs.DeleteKey(SorulanlarAnahtari);
        }
        else
        {
            guven = PlayerPrefs.GetFloat(GuvenAnahtari, 100f);
            SorulanlariYukle();
        }
        GuvenYazisiniGuncelle();

        // Her gün torba yeniden dolar: günde canavar sayısı kadar müşteri gelirse hepsi farklı olur
        torba.Clear();
        seri = 0;

        YeniMusteri();
    }

    void Update()
    {
        // Tarif kitabı, poster ya da duraklatma menüsü açıkken sipariş etiketi üstlerine binmesin
        if (siparisEtiketi != null)
        {
            bool gizle = TarifKitabi.Acik || Poster.Acik || DuraklatmaMenusu.Acik;
            if (siparisEtiketi.enabled == gizle) siparisEtiketi.enabled = !gizle;
        }

        if (DuraklatmaMenusu.Acik) return;
        KapiUyarisiniGuncelle();

        // Saldırı sahnesi oynuyor: ağız oynar, başka hiçbir şey olmaz
        if (saldiriyor)
        {
            AgizOynat();
            SaldiriSahnesi();
            return;
        }

        if (kapidaBekliyor)
        {
            KapiBekle();
            return;
        }

        // Soru sorulurken ve tepki verilirken sabır azalmaz
        if (asama != Asama.Yok)
        {
            AgizOynat();
            SoruAsamasi();
            return;
        }

        if (!bekliyor) return;

        AgizOynat();
        SiparisEtiketiniKontrolEt();

        // Canavar siparişini söylerken (ve hemen sonrasında biraz) sabır azalmaz
        if (SiparisOkunuyorMu())
        {
            CubuguCiz();
            return;
        }

        kalanSabir -= Time.deltaTime;
        CubuguCiz();

        // Sabır azaldıkça ruh hali kötüleşir ve canavar bir şey söyler
        float oran = kalanSabir / sabirSuresi;
        if (oran < 0.33f) HaleGec(2, true);
        else if (oran < 0.66f) HaleGec(1, true);

        if (kalanSabir <= 0f)
        {
            kahveDoldurma.Durdur();
            Kiz();
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

    bool Yaziyor => daktilo != null && daktilo.YaziyorMu;

    // Geçiş bittiyse ve söylenecek cümle varsa söyle
    void BekleyenCumleyiSoyle()
    {
        if (bekleyenCumle != null && (bekliyor || asama != Asama.Yok || saldiriyor)) YaziGoster(bekleyenCumle);
        bekleyenCumle = null;
    }

    // Fare bu objenin üstünde mi (üst üste binen objeler olsa bile doğru çalışır)
    bool UstundeMi(GameObject obje, Vector2 nokta)
    {
        if (obje == null) return false;
        foreach (Collider2D c in Physics2D.OverlapPointAll(nokta))
        {
            if (c.gameObject == obje) return true;
        }
        return false;
    }

    // Konuşurken ağız çizimleri arasında rastgele geçiş yapar, yazı bitince ağız kapanır
    void AgizOynat()
    {
        if (gecisOynuyor || tur == null || halResmi == null) return;

        List<Sprite> kareler = tur.KonusmaKareleri(hal);
        if (kareler.Count == 0) return;

        if (!Yaziyor)
        {
            if (gorsel.sprite != halResmi) gorsel.sprite = halResmi;
            sonAgizKaresi = -1;
            return;
        }

        agizSayac -= Time.deltaTime;
        if (agizSayac > 0f) return;
        agizSayac = agizHizi;

        // 0 = ağız kapalı (normal resim), 1+ = ağız çizimleri. Aynı kare üst üste gelmesin.
        int toplam = kareler.Count + 1;
        int yeni = Random.Range(0, toplam);
        if (yeni == sonAgizKaresi) yeni = (yeni + 1) % toplam;
        sonAgizKaresi = yeni;

        gorsel.sprite = yeni == 0 ? halResmi : kareler[yeni - 1];
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
        if (fare != null && fare.leftButton.wasPressedThisFrame && !Poster.Engelli)
        {
            Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());
            if (UstundeMi(kapi.gameObject, nokta))
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
        if (tur.girisSesi != null) Sesler.Oynat(tur.girisSesi);

        gorsel.enabled = true;
        SabirGoster(true);
        if (baloncuk != null) baloncuk.SetActive(true);

        HalGoster(kizgin ? 2 : 0);
        if (kizgin) StartCoroutine(Titre());
        else StartCoroutine(GirisAnimasyonu());

        sonucYazisi.text = kizgin ? BeklettinMesaji : "";
        if (kizgin) Invoke(nameof(BeklettinYazisiniSil), 1.5f);

        kalanSabir = sabirSuresi;
        CubuguCiz();

        // Kızgın girmediyse bazen önce bir soru sorar
        aktifSoru = kizgin ? null : SoruSec();
        if (aktifSoru != null)
        {
            asama = Asama.Soru;
            butonlarGorunuyor = false;
            YaziGoster(CanavarTuru.KahveyiYerlestir(aktifSoru.soru, siparis));
            return;
        }

        SiparisVer();
    }

    // Canavar siparişini söyler, kahve yapmaya başlanabilir
    void SiparisVer()
    {
        asama = Asama.Yok;
        ButonlariGizle();

        // Güven değiştiyse bu müşterinin sabrı da hemen değişsin
        sabirSuresi = tur.sabirSuresi * gunYoneticisi.SabirCarpani * GuvenCarpani();
        kalanSabir = sabirSuresi;
        CubuguCiz();

        YaziGoster(tur.CumleSec(siparis));
        bekliyor = true;
        kahveDoldurma.YeniBardak(siparis);
        // Etiket hemen değil, canavar siparişini söyleyip bitirince çıkar
        if (siparisEtiketi != null) siparisEtiketi.text = "";
        etiketBekliyor = true;
        etiketSayac = etiketGecikmesi;
        siparisOkunuyor = true;
        okumaSayac = okumaPayi;
    }

    // ---------- SORU SİSTEMİ ----------

    // Bu canavarın henüz sorulmamış sorularından birini seçer (ya da soru sormaz)
    // Bu canavarın bu oyunda HİÇ sorulmamış sorularından birini seçer.
    // Her soru bir oyunda sadece bir kez sorulur (günler arası da hatırlanır).
    Soru SoruSec()
    {
        if (tur.sorular == null || tur.sorular.Length == 0) return null;
        if (Random.value > soruIhtimali) return null;

        List<int> kalan = new List<int>();
        for (int i = 0; i < tur.sorular.Length; i++)
        {
            Soru s = tur.sorular[i];
            if (s == null || string.IsNullOrEmpty(s.soru)) continue;
            if (sorulanlar.Contains(SoruAnahtari(i))) continue;
            kalan.Add(i);
        }
        if (kalan.Count == 0) return null; // Hepsi soruldu, artık soru yok

        int secilen = kalan[Random.Range(0, kalan.Count)];
        sorulanlar.Add(SoruAnahtari(secilen));
        SorulanlariKaydet();
        return tur.sorular[secilen];
    }

    string SoruAnahtari(int sira)
    {
        return tur.name + ":" + sira;
    }

    void SorulanlariKaydet()
    {
        PlayerPrefs.SetString(SorulanlarAnahtari, string.Join("|", sorulanlar));
        PlayerPrefs.Save();
    }

    void SorulanlariYukle()
    {
        sorulanlar.Clear();
        string kayit = PlayerPrefs.GetString(SorulanlarAnahtari, "");
        if (string.IsNullOrEmpty(kayit)) return;
        foreach (string parca in kayit.Split('|'))
        {
            if (!string.IsNullOrEmpty(parca)) sorulanlar.Add(parca);
        }
    }

    void SoruAsamasi()
    {
        if (asama == Asama.Soru)
        {
            // Soru yazılıp bitince butonlar çıkar
            if (!butonlarGorunuyor)
            {
                if (!Yaziyor) ButonlariGoster();
                return;
            }

            var fare = Mouse.current;
            if (fare == null) return;
            Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());

            bool ustunde1 = cevapButonu1 != null && UstundeMi(cevapButonu1.gameObject, nokta);
            bool ustunde2 = cevapButonu2 != null && UstundeMi(cevapButonu2.gameObject, nokta);
            if (cevapButonu1 != null) cevapButonu1.color = ustunde1 ? butonUzerindeRenk : butonNormalRenk;
            if (cevapButonu2 != null) cevapButonu2.color = ustunde2 ? butonUzerindeRenk : butonNormalRenk;

            if (fare.leftButton.wasPressedThisFrame && !Poster.Engelli && !TarifKitabi.Acik)
            {
                if (ustunde1) Cevapla(1);
                else if (ustunde2) Cevapla(2);
            }
        }
        else if (asama == Asama.Tepki)
        {
            // Geçiş ve tepki cümlesi bitince biraz bekle, sonra siparişi söyle
            if (gecisOynuyor || bekleyenCumle != null || Yaziyor)
            {
                tepkiSayac = tepkiSonrasiBekleme;
                return;
            }
            tepkiSayac -= Time.deltaTime;
            if (tepkiSayac <= 0f) SiparisVer();
        }
    }

    void Cevapla(int cevap)
    {
        Sesler.Tiklama();
        ButonlariGizle();

        bool iyi = cevap == aktifSoru.iyiCevap;

        // Güven ve mutluluk değişir
        GuvenDegistir(iyi ? iyiCevapGuven : kotuCevapGuven);
        mutluluk.Degistir(iyi ? iyiCevapMutluluk : kotuCevapMutluluk);
        if (mutluluk.OyunBitti)
        {
            asama = Asama.Yok;
            return;
        }

        sonucYazisi.text = iyi ? GuvenArttiMesaji : GuvenAzaldiMesaji;
        Tepki(iyi, iyi ? 2 : 3);
        CancelInvoke(nameof(GuvenMesajiniSil));
        Invoke(nameof(GuvenMesajiniSil), 1.5f);

        string tepki = CanavarTuru.KahveyiYerlestir(iyi ? aktifSoru.iyiTepki : aktifSoru.kotuTepki, siparis);
        asama = Asama.Tepki;
        tepkiSayac = tepkiSonrasiBekleme;

        if (iyi)
        {
            if (!string.IsNullOrEmpty(tepki)) YaziGoster(tepki);
        }
        else
        {
            // Kötü cevap: canavar nötr hale geçer, geçiş bitince tepkisini söyler
            HaleGec(1, false);
            bekleyenCumle = string.IsNullOrEmpty(tepki) ? null : tepki;
            if (!gecisOynuyor) BekleyenCumleyiSoyle();
        }
    }

    void GuvenMesajiniSil()
    {
        if (sonucYazisi.text == GuvenArttiMesaji || sonucYazisi.text == GuvenAzaldiMesaji) sonucYazisi.text = "";
    }

    void ButonlariGoster()
    {
        butonlarGorunuyor = true;
        if (cevapButonu1 != null)
        {
            cevapButonu1.gameObject.SetActive(true);
            cevapButonu1.color = butonNormalRenk;
        }
        if (cevapButonu2 != null)
        {
            cevapButonu2.gameObject.SetActive(true);
            cevapButonu2.color = butonNormalRenk;
        }
        if (cevapYazisi1 != null)
        {
            cevapYazisi1.gameObject.SetActive(true);
            cevapYazisi1.text = aktifSoru.cevap1;
        }
        if (cevapYazisi2 != null)
        {
            cevapYazisi2.gameObject.SetActive(true);
            cevapYazisi2.text = aktifSoru.cevap2;
        }
    }

    void ButonlariGizle()
    {
        butonlarGorunuyor = false;
        if (cevapButonu1 != null) cevapButonu1.gameObject.SetActive(false);
        if (cevapButonu2 != null) cevapButonu2.gameObject.SetActive(false);
        if (cevapYazisi1 != null) cevapYazisi1.gameObject.SetActive(false);
        if (cevapYazisi2 != null) cevapYazisi2.gameObject.SetActive(false);
    }

    // ---------- GÜVEN ----------

    void GuvenDegistir(float miktar)
    {
        guven = Mathf.Clamp(guven + miktar, 0f, 100f);
        GuvenYazisiniGuncelle();
    }

    // Güven 100 iken sabır tam, güven 0 iken sabır "enDusukSabirOrani" kadar
    float GuvenCarpani()
    {
        return Mathf.Lerp(enDusukSabirOrani, 1f, guven / 100f);
    }

    void GuvenYazisiniGuncelle()
    {
        if (guvenYazisi == null) return;
        guvenYazisi.text = "Güven %" + Mathf.RoundToInt(guven);
    }

    // ---------- KAHVE SONUCU ----------

    void BeklettinYazisiniSil()
    {
        if (sonucYazisi.text == BeklettinMesaji) sonucYazisi.text = "";
    }

    // Kahve makinesi servis edilince bunu çağırır
    // detay: neyin yanlış gittiği ("Süt eksik", "ÇİZGİYİ GEÇTİN!" gibi), boş olabilir
    public void KahveGeldi(string kalite, string detay = "")
    {
        if (!bekliyor) return;

        if (kalite == "MUKEMMEL")
        {
            HaleGec(0, false);
            Bitir("MÜKEMMEL!", kalite);
        }
        else if (kalite == "IYI")
        {
            Bitir("İYİ", kalite);
        }
        else if (kalite == "TASTI")
        {
            Kiz();
            Bitir("TAŞTI!" + KucukSatir(detay), kalite);
        }
        else if (kalite == "YANLIS")
        {
            Kiz();
            Bitir("YANLIŞ KAHVE!" + KucukSatir(detay), kalite);
        }
        else
        {
            // Kötü dökümde başlık neyin yanlış olduğunu söyler: "ÇİZGİYİ GEÇTİN!" ya da "AZ DOLDU..."
            Kiz();
            Bitir(string.IsNullOrEmpty(detay) ? "KÖTÜ..." : detay, kalite);
        }
    }

    // Sonuç yazısının altına küçük bir açıklama satırı
    static string KucukSatir(string detay)
    {
        return string.IsNullOrEmpty(detay) ? "" : "\n<size=55%>" + detay + "</size>";
    }

    // Sipariş cümlesi hâlâ yazılıyor mu (bitince kısa bir okuma payı daha verilir)
    bool SiparisOkunuyorMu()
    {
        if (!siparisOkunuyor) return false;
        if (Yaziyor || bekleyenCumle != null || gecisOynuyor)
        {
            okumaSayac = okumaPayi;
            return true;
        }
        okumaSayac -= Time.deltaTime;
        if (okumaSayac > 0f) return true;
        siparisOkunuyor = false;
        return false;
    }

    void Bitir(string mesaj, string sonuc)
    {
        bekliyor = false;
        bekleyenCumle = null;
        CancelInvoke(nameof(GuvenMesajiniSil));
        sonucYazisi.text = Renklendir(mesaj, sonuc);

        // İstatistik
        if (sonuc != "GEC") Istatistik.Servis++;
        if (sonuc == "MUKEMMEL") Istatistik.Mukemmel++;
        if (siparisEtiketi != null) siparisEtiketi.text = "";
        etiketBekliyor = false;
        siparisOkunuyor = false;

        // Mükemmel serisi: arka arkaya mükemmel kahve ekstra mutluluk verir
        float bonus = 0f;
        if (sonuc == "MUKEMMEL")
        {
            seri++;
            Istatistik.EnUzunSeri = Mathf.Max(Istatistik.EnUzunSeri, seri);
            if (seri >= 2)
            {
                bonus = Mathf.Min(seriBonusu * (seri - 1), enFazlaSeriBonusu);
                sonucYazisi.text += "\n<size=55%><color=" + SariRenk + ">SERİ x" + seri + "!</color></size>";
            }
        }
        else seri = 0;

        if (sonuc == "MUKEMMEL") Tepki(true, 4);
        else if (sonuc == "IYI") Tepki(true, 2);
        else Tepki(false, 4);
        YaziTemizle();
        if (baloncuk != null) baloncuk.SetActive(false);
        if (halResmi != null && !gecisOynuyor) gorsel.sprite = halResmi; // ağız açık kalmasın

        // Kafenin mutluluğunu değiştir
        mutluluk.SonucaGoreDegistir(sonuc, bonus);

        // Mutluluk bittiyse yeni müşteri gelmesin
        if (mutluluk.OyunBitti)
        {
            kahveDoldurma.Temizle();
            sonucYazisi.text = "";
            return;
        }

        // Canavar sonuç ekranda kaldıktan sonra yavaşça kaybolur
        StartCoroutine(CikisAnimasyonu(sonucBeklemeSuresi - cikisSuresi));

        // Günün müşterileri bitti mi
        bool memnun = sonuc == "MUKEMMEL" || sonuc == "IYI";
        bool gunBitti = gunYoneticisi.MusteriBitti(memnun);

        if (gunBitti) Invoke(nameof(GunuKapat), sonucBeklemeSuresi);
        else Invoke(nameof(YeniMusteri), sonucBeklemeSuresi);
    }

    // Gün bitti: ortalığı temizle, güveni kaydet, gün sonu ekranını aç
    void GunuKapat()
    {
        KorutinleriDurdur();
        gecisOynuyor = false;
        bekleyenCumle = null;
        asama = Asama.Yok;
        ButonlariGizle();
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        gorsel.enabled = false;
        SabirGoster(false);
        if (baloncuk != null) baloncuk.SetActive(false);
        sonucYazisi.text = "";
        kahveDoldurma.Temizle();
        if (muzik != null) muzik.Seviye(0);

        // Güven gün sonunda kaydedilir ("Devam Et" ile kaldığı yerden sürer)
        PlayerPrefs.SetFloat(GuvenAnahtari, guven);
        PlayerPrefs.Save();

        gunYoneticisi.GunSonuGoster();
    }

    // Torbadan sıradaki canavarı çeker.
    // Torba boşalınca tüm canavarlar tekrar konur ve karıştırılır.
    // Aynı canavar asla arka arkaya gelmez.
    CanavarTuru SiradakiCanavar()
    {
        // Torba boşsa doldur
        if (torba.Count == 0)
        {
            foreach (CanavarTuru c in canavarlar)
            {
                if (c != null) torba.Add(c);
            }

            // Karıştır (kartları deste gibi karıştırmak)
            for (int i = torba.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                CanavarTuru gecici = torba[i];
                torba[i] = torba[j];
                torba[j] = gecici;
            }

            // Yeni turun ilki, bir önceki müşteriyle aynıysa onu sona at
            if (torba.Count > 1 && torba[0] == sonCanavar)
            {
                CanavarTuru ilk = torba[0];
                torba.RemoveAt(0);
                torba.Add(ilk);
            }
        }

        if (torba.Count == 0) return null;

        CanavarTuru secilen = torba[0];
        torba.RemoveAt(0);
        sonCanavar = secilen;
        return secilen;
    }

    // Yeni canavar seçilir ve kapının önünde beklemeye başlar
    void YeniMusteri()
    {
        // Yarıda kalan animasyon ya da titreme varsa düzelt
        KorutinleriDurdur();
        gecisOynuyor = false;
        bekleyenCumle = null;
        asama = Asama.Yok;
        ButonlariGizle();
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        if (canavarlar == null || canavarlar.Length == 0)
        {
            Debug.LogError("Musteri: Canavarlar listesi bos! En az bir Canavar Turu karti ekle.");
            return;
        }

        // Torbadan sıradaki canavar, günün kahvelerinden rastgele bir kahve
        tur = SiradakiCanavar();
        if (tur == null)
        {
            Debug.LogError("Musteri: Canavarlar listesindeki kutular bos! Kartlari surukle.");
            return;
        }
        sabirSuresi = tur.sabirSuresi * gunYoneticisi.SabirCarpani * GuvenCarpani();
        HalGoster(0);
        siparis = gunYoneticisi.RastgeleKahve();
        Debug.Log(gunYoneticisi.GunNumarasi + ". gun: " + tur.canavarAdi + " kapida, istedigi: " + siparis + " | guven: " + guven);

        // Canavar henüz görünmüyor, kapıda bekliyor
        gorsel.enabled = false;
        SabirGoster(false);
        if (baloncuk != null) baloncuk.SetActive(false);
        YaziTemizle();
        sonucYazisi.text = "Kapıda biri var...";

        kapidaBekliyor = true;
        kapiSayac = kapiSabri;
    }

    // Kötü sonuç: canavar zaten kızgınsa bile kızgın sesi çalar ve titrer
    void Kiz()
    {
        if (hal == 2)
        {
            if (tur.kizginSesi != null) Sesler.Oynat(tur.kizginSesi);
            KorutinleriDurdur();
            gecisOynuyor = false;
            transform.position = asilKonum;
            transform.localScale = asilBoyut;
            StartCoroutine(Titre());
        }
        else
        {
            HaleGec(2, false);
        }
    }

    // Ruh halini değiştir: geçiş animasyonunu oynat ve titret.
    // konussun = true ise geçiş bitince canavar o hale uygun bir cümle söyler.
    void HaleGec(int yeniHal, bool konussun)
    {
        if (yeniHal == hal) return;

        int eskiHal = hal;
        if (yeniHal == 2 && tur.kizginSesi != null) Sesler.Oynat(tur.kizginSesi);
        KorutinleriDurdur();
        gecisOynuyor = false;
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        // Söylenecek cümleyi şimdiden seç, geçiş bitince söylenecek
        bekleyenCumle = konussun ? tur.HalCumlesiSec(yeniHal, siparis) : null;

        // Cümlede kahvenin adı geçmiyorsa altına küçük bir hatırlatma ekle (oyuncu siparişi unutmasın)
        if (bekleyenCumle != null)
        {
            string ad = CanavarTuru.KahveAdi(siparis);
            if (bekleyenCumle.IndexOf(ad, System.StringComparison.OrdinalIgnoreCase) < 0)
                bekleyenCumle += "\n<size=70%>(" + ad + ")</size>";
        }

        StartCoroutine(GecisOynat(eskiHal, yeniHal));
        StartCoroutine(Titre());
    }

    // Hal değişimi: geçiş kareleri varsa onları oynatır,
    // yoksa canavar siyaha kararır, yeni hale geçer ve siyahtan açılır.
    IEnumerator GecisOynat(int eskiHal, int yeniHal)
    {
        gecisOynuyor = true;
        hal = yeniHal; // Oyun mantığı açısından hal hemen değişir
        if (muzik != null) muzik.Seviye(hal);
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
            BekleyenCumleyiSoyle();
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
        BekleyenCumleyiSoyle();
    }

    // Hale uygun resmi göster, resim yoksa rengi değiştir
    void HalGoster(int yeniHal)
    {
        hal = yeniHal;
        if (muzik != null) muzik.Seviye(hal);

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
        titriyor = true;
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
        titriyor = false;
    }

    // Sipariş cümlesi yazılıp bitince (biraz bekleyip) bardağın üstüne sipariş etiketi çıkar
    void SiparisEtiketiniKontrolEt()
    {
        if (!etiketBekliyor || siparisEtiketi == null) return;
        if (Yaziyor) { etiketSayac = etiketGecikmesi; return; }
        etiketSayac -= Time.deltaTime;
        if (etiketSayac > 0f) return;

        siparisEtiketi.text = "Sipariş: " + CanavarTuru.KahveAdi(siparis);
        etiketBekliyor = false;
    }

    // Canavarın başının üstünden kalp ya da duman çıkar
    void Tepki(bool iyi, int adet)
    {
        if (tepkiEfekti == null || gorsel == null) return;
        Bounds b = gorsel.bounds;
        Vector3 bas = new Vector3(b.center.x, b.max.y - 0.25f, 0f);
        tepkiEfekti.Oynat(iyi, bas, adet);
    }

    // Canavar kapıda beklerken kapının üstündeki ünlem görünür ve zıplar
    void KapiUyarisiniGuncelle()
    {
        if (kapiUyarisi == null) return;
        bool goster = kapidaBekliyor && !mutluluk.OyunBitti;
        if (kapiUyarisi.enabled != goster) kapiUyarisi.enabled = goster;
        if (!goster) return;

        // Zıplama: piksel piksel (0.01 birim) yukarı aşağı
        float ziplama = Mathf.Abs(Mathf.Sin(Time.time * 5f)) * 0.08f;
        ziplama = Mathf.Round(ziplama * 100f) / 100f;
        kapiUyarisi.transform.position = kapiUyarisiKonum + Vector3.up * ziplama;
    }

    // Sabır barını (ve arkasındaki soluk kopyayı) gösterir ya da gizler
    void SabirGoster(bool goster)
    {
        sabirCubugu.gameObject.SetActive(goster);
        if (sabirArkaPlani != null) sabirArkaPlani.SetActive(goster);
    }

    // Bütün animasyonları durdurur ve bayraklarını sıfırlar
    void KorutinleriDurdur()
    {
        StopAllCoroutines();
        titriyor = false;
        animasyonOynuyor = false;
    }

    // Canavar dururken hafifçe nefes alır (ayakları yerde kalır, boyu hafifçe uzayıp kısalır)
    void LateUpdate()
    {
        if (gorsel == null || !gorsel.enabled || titriyor || animasyonOynuyor) return;
        float nefes = 1f + Mathf.Sin(Time.time * nefesHizi * 2f * Mathf.PI) * nefesMiktari;
        transform.localScale = new Vector3(asilBoyut.x, asilBoyut.y * nefes, asilBoyut.z);
    }

    // İçeri girerken: aşağıdan yukarı kayarak ve belirerek gelir
    IEnumerator GirisAnimasyonu()
    {
        animasyonOynuyor = true;
        transform.localScale = asilBoyut;
        Color renk = gorsel.color;
        float t = 0f;
        while (t < girisSuresi)
        {
            t += Time.deltaTime;
            float o = Mathf.Clamp01(t / girisSuresi);
            float yumusak = 1f - (1f - o) * (1f - o); // hızlı başla, yavaşça otur
            transform.position = asilKonum + Vector3.down * girisKaymasi * (1f - yumusak);
            Color c = renk;
            c.a = renk.a * yumusak;
            gorsel.color = c;
            yield return null;
        }
        transform.position = asilKonum;
        gorsel.color = renk;
        animasyonOynuyor = false;
    }

    // Giderken: biraz bekler, sonra aşağı kayarak kaybolur
    IEnumerator CikisAnimasyonu(float bekleme)
    {
        if (bekleme > 0f) yield return new WaitForSeconds(bekleme);

        animasyonOynuyor = true;
        transform.localScale = asilBoyut;
        Color renk = gorsel.color;
        float t = 0f;
        while (t < cikisSuresi)
        {
            t += Time.deltaTime;
            float o = Mathf.Clamp01(t / cikisSuresi);
            transform.position = asilKonum + Vector3.down * girisKaymasi * o * o;
            Color c = renk;
            c.a = renk.a * (1f - o);
            gorsel.color = c;
            yield return null;
        }
        gorsel.enabled = false;
        SabirGoster(false);
        transform.position = asilKonum;
        gorsel.color = renk;
        animasyonOynuyor = false;
    }

    // Sonuca göre yazıyı renklendirir: iyi yeşil, orta sarı, kötü kırmızı
    string Renklendir(string mesaj, string sonuc)
    {
        string renk;
        if (sonuc == "MUKEMMEL") renk = YesilRenk;
        else if (sonuc == "IYI") renk = SariRenk;
        else renk = KirmiziRenk;
        return "<color=" + renk + ">" + mesaj + "</color>";
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

        // Renk: dolu = yeşil, yarı = sarı, az = kırmızı. Çok azalınca yanıp söner.
        if (sabirGorsel != null)
        {
            Color renk = oran > 0.5f
                ? Color.Lerp(sabirOrtaRenk, sabirIyiRenk, (oran - 0.5f) * 2f)
                : Color.Lerp(sabirKotuRenk, sabirOrtaRenk, oran * 2f);
            if (oran < 0.25f && bekliyor)
            {
                float yanip = Mathf.PingPong(Time.time * 4f, 1f);
                renk = Color.Lerp(renk, Color.white, yanip * 0.6f);
            }
            sabirGorsel.color = renk;
        }

        // Yüzlü bar resmi: sabır çok azalınca yanıp söner
        if (sabirResmi != null)
        {
            float yanip = (oran < 0.25f && bekliyor) ? Mathf.PingPong(Time.time * 4f, 1f) : 0f;
            sabirResmi.color = new Color(1f, 1f, 1f, 1f - 0.5f * yanip);
        }
    }
}
