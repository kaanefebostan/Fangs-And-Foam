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

    [Header("Kafenin güveni")]
    public TMP_Text guvenYazisi;            // "Güven %85" yazısı (boş kalabilir)
    public float iyiCevapGuven = 5f;        // İyi cevapta güven artışı
    public float kotuCevapGuven = -15f;     // Kötü cevapta güven azalışı
    [Range(0.1f, 1f)] public float enDusukSabirOrani = 0.5f; // Güven 0 olunca sabır bu orana iner

    const string GuvenAnahtari = "kafeGuveni";

    SpriteRenderer gorsel;
    Daktilo daktilo;
    Vector3 asilKonum;
    Vector3 asilBoyut;
    int hal = 0;           // 0 = mutlu, 1 = nötr, 2 = kızgın
    CanavarTuru tur;       // Şu anki canavar
    Sprite halResmi;       // Şu anki halin normal (ağzı kapalı) resmi
    bool gecisOynuyor = false;
    float sabirSuresi = 20f;
    float agizSayac;
    int sonAgizKaresi = -1;
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
    Dictionary<CanavarTuru, List<int>> kalanSorular = new Dictionary<CanavarTuru, List<int>>();

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
        ButonlariGizle();
        OyuncuKutusunuKapat();
        YaziTemizle();
        sonucYazisi.text = "";

        guven = PlayerPrefs.GetFloat(GuvenAnahtari, 100f);
        GuvenYazisiniGuncelle();
    }

    // Müşteri içeride kahve bekliyor mu (korku efektleri için)
    public bool IcerideBekliyor => bekliyor;

    // Canavar içeride mi (soru sorarken, beklerken ya da tepki verirken)
    public bool IcerideMi => gorsel != null && gorsel.enabled && !kapidaBekliyor;

    // Şu an gelen canavar (jumpscare için)
    public CanavarTuru SuankiCanavar => tur;

    // Kafenin güveni (0 - 100)
    public float Guven => guven;

    // Jumpscare başlarken müşteriyi, sabır çubuğunu ve baloncuğu gizler
    public void JumpscareIcinGizle()
    {
        CancelInvoke();
        saldiriyor = false;
        saldiriAsama = 0;
        OyuncuKutusunuKapat();
        StopAllCoroutines();
        gecisOynuyor = false;
        bekleyenCumle = null;
        asama = Asama.Yok;
        ButonlariGizle();
        transform.position = asilKonum;
        transform.localScale = asilBoyut;
        gorsel.enabled = false;
        sabirCubugu.gameObject.SetActive(false);
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
            HaleGec(2, false);
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
        // Yeni oyunda güven sıfırdan başlar, sonraki günlerde kaldığı yerden devam eder
        if (gunYoneticisi.GunNumarasi <= 1) guven = 100f;
        else guven = PlayerPrefs.GetFloat(GuvenAnahtari, 100f);
        GuvenYazisiniGuncelle();

        YeniMusteri();
    }

    void Update()
    {
        if (DuraklatmaMenusu.Acik) return;

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

        kalanSabir -= Time.deltaTime;
        CubuguCiz();

        // Sabır azaldıkça ruh hali kötüleşir ve canavar bir şey söyler
        float oran = kalanSabir / sabirSuresi;
        if (oran < 0.33f) HaleGec(2, true);
        else if (oran < 0.66f) HaleGec(1, true);

        if (kalanSabir <= 0f)
        {
            kahveDoldurma.Durdur();
            HaleGec(2, false);
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
        sabirCubugu.gameObject.SetActive(true);
        if (baloncuk != null) baloncuk.SetActive(true);

        HalGoster(kizgin ? 2 : 0);
        if (kizgin) StartCoroutine(Titre());

        sonucYazisi.text = kizgin ? "ÇOK BEKLETTİN!" : "";
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
    }

    // ---------- SORU SİSTEMİ ----------

    // Bu canavarın henüz sorulmamış sorularından birini seçer (ya da soru sormaz)
    Soru SoruSec()
    {
        if (tur.sorular == null || tur.sorular.Length == 0) return null;
        if (Random.value > soruIhtimali) return null;

        if (!kalanSorular.TryGetValue(tur, out List<int> kalan))
        {
            kalan = new List<int>();
            kalanSorular[tur] = kalan;
        }

        // Hepsi sorulduysa baştan doldur
        if (kalan.Count == 0)
        {
            for (int i = 0; i < tur.sorular.Length; i++)
            {
                Soru s = tur.sorular[i];
                if (s != null && !string.IsNullOrEmpty(s.soru)) kalan.Add(i);
            }
        }
        if (kalan.Count == 0) return null;

        int sira = Random.Range(0, kalan.Count);
        int secilen = kalan[sira];
        kalan.RemoveAt(sira);
        return tur.sorular[secilen];
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

        sonucYazisi.text = iyi ? "GÜVEN ARTTI!" : "GÜVEN AZALDI!";
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
        if (sonucYazisi.text == "GÜVEN ARTTI!" || sonucYazisi.text == "GÜVEN AZALDI!") sonucYazisi.text = "";
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
        if (sonucYazisi.text == "ÇOK BEKLETTİN!") sonucYazisi.text = "";
    }

    // Kahve makinesi servis edilince bunu çağırır
    public void KahveGeldi(string kalite)
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
            HaleGec(2, false);
            Bitir("TAŞTI!", kalite);
        }
        else if (kalite == "YANLIS")
        {
            HaleGec(2, false);
            Bitir("YANLIŞ KAHVE!", kalite);
        }
        else
        {
            HaleGec(2, false);
            Bitir("KÖTÜ...", kalite);
        }
    }

    void Bitir(string mesaj, string sonuc)
    {
        bekliyor = false;
        bekleyenCumle = null;
        CancelInvoke(nameof(GuvenMesajiniSil));
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

    // Gün bitti: ortalığı temizle, güveni kaydet, gün sonu ekranını aç
    void GunuKapat()
    {
        StopAllCoroutines();
        gecisOynuyor = false;
        bekleyenCumle = null;
        asama = Asama.Yok;
        ButonlariGizle();
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        gorsel.enabled = false;
        sabirCubugu.gameObject.SetActive(false);
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
        StopAllCoroutines();
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
        sabirCubugu.gameObject.SetActive(false);
        if (baloncuk != null) baloncuk.SetActive(false);
        YaziTemizle();
        sonucYazisi.text = "Kapıda biri var...";

        kapidaBekliyor = true;
        kapiSayac = kapiSabri;
    }

    // Ruh halini değiştir: geçiş animasyonunu oynat ve titret.
    // konussun = true ise geçiş bitince canavar o hale uygun bir cümle söyler.
    void HaleGec(int yeniHal, bool konussun)
    {
        if (yeniHal == hal) return;

        int eskiHal = hal;
        if (yeniHal == 2 && tur.kizginSesi != null) Sesler.Oynat(tur.kizginSesi);
        StopAllCoroutines();
        gecisOynuyor = false;
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        // Söylenecek cümleyi şimdiden seç, geçiş bitince söylenecek
        bekleyenCumle = konussun ? tur.HalCumlesiSec(yeniHal, siparis) : null;

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
