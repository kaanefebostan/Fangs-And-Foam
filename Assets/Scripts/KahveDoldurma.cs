using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Kahve hazırlama sistemi.
// Makineye / şişeye TIKLA: malzemeyi seç.
// BOSLUK basılı tut: seçili malzemeyi dök (kahve, süt, su), kırmızı çizgide bırak.
// Şurup seçiliyken BARDAGA TIKLA: şurup eklenir (bardak sarı yanıp sönerek bunu hatırlatır).
// E: servis et.  R: bardağı dök, baştan başla.
public class KahveDoldurma : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public Transform kahve;          // Eski Kahve kutusu: katmanlar bunun kopyası olarak oluşur
    public Transform hedefCizgi;     // Kırmızı hedef çizgisi
    public Transform bardak;         // Bardak (şurup eklemek için tıklanır)
    public SpriteRenderer bardakCerceve; // Bardağın arkasındaki sarı çerçeve (boş kalabilir)
    public Musteri musteri;          // Kahveyi bekleyen müşteri
    public TMP_Text bardakYazisi;    // Bardakta ne olduğunu gösteren yazı
    public ServisAnimasyonu servisAnimasyonu; // Servis edince bardak canavara kayar (boş kalabilir)

    [Header("Ayarlar")]
    public float dolmaHizi = 0.5f;   // Saniyede bardağın ne kadarı dolar
    public float bardakAlt = -0.8f;  // Bardağın alt kenarının yüksekliği
    public float bardakBoyu = 0.5f;  // Bardağın yüksekliği

    [Header("Bardağa tıkla vurgusu")]
    public Color vurguRengi = new Color(1f, 0.85f, 0.1f); // Sarı
    public float vurguHizi = 4f;                          // Ne kadar hızlı yanıp söner

    // Dökülen malzemelerin isimleri
    static readonly string[] dokulenler = { "Kahve", "Sut", "Su" };

    float doluluk;            // 0 = boş, 1 = ağzına kadar, 1'den fazlası taşmış
    bool aktif;               // Bekleyen müşteri var mı
    bool dokuyor;             // Şu an dökülüyor mu
    float katmanBaslangic;    // Şu anki katmanın başladığı seviye
    Transform suankiKatman;   // Şu an dökülen katman
    string sonDokulen;        // En son dökülen malzeme (aynısı tekrar dökülürse aynı katman devam eder)
    Malzeme secili;           // Seçili malzeme
    string siparis;           // Müşterinin istediği kahve
    int dokulecekSayisi;      // Tarifte kaç dökülen malzeme var
    float sonHedef;           // Bardağın en son dolması gereken seviye

    SpriteRenderer bardakGorsel;
    Color bardakNormalRenk;

    List<string> icindekiler = new List<string>();   // Bardağa konanlar
    List<float> farklar = new List<float>();          // Her dökümün hedeften ne kadar saptığı
    List<float> yonler = new List<float>();           // + ise çizgiyi geçmiş, - ise az doldurmuş
    string uyari = "";                                // Bardak yazısının altındaki kısa uyarı ("Bardak dolu!")
    List<GameObject> katmanlar = new List<GameObject>();

    // Dökme animasyonu için bilgiler
    public bool Dokuyor => dokuyor;
    public Malzeme Secili => secili;
    public float SiviUstY => bardakAlt + Mathf.Min(doluluk, 1f) * bardakBoyu;
    public float BardakX => bardak.position.x;
    public float BardakUstY => bardakAlt + bardakBoyu;
    public bool KahveVar => icindekiler.Contains("Kahve");   // Buhar efekti için

    void Awake()
    {
        // Eski Kahve kutusunu gizle, sadece kopyalamak için kullanacağız
        kahve.gameObject.SetActive(false);

        bardakGorsel = bardak.GetComponent<SpriteRenderer>();
        if (bardakGorsel != null) bardakNormalRenk = bardakGorsel.color;
        if (bardakCerceve != null) bardakCerceve.gameObject.SetActive(false);
    }

    void Update()
    {
        BardakVurgusu();

        var fare = Mouse.current;
        var klavye = Keyboard.current;
        if (!aktif || fare == null || klavye == null || DuraklatmaMenusu.Acik) return;

        // Tarif kitabı ya da poster açıkken kahve yapılamaz
        if (TarifKitabi.Acik || Poster.Engelli)
        {
            if (dokuyor) DokmeyiBitir();
            return;
        }

        // Fareyle bir şeye tıklandı mı
        if (fare.leftButton.wasPressedThisFrame) TiklamayaBak(fare);

        // R: bardağı dök
        if (klavye.rKey.wasPressedThisFrame)
        {
            BardagiBosalt();
            return;
        }

        // E: servis et
        if (klavye.eKey.wasPressedThisFrame && icindekiler.Count > 0)
        {
            ServisEt();
            return;
        }

        // Boşluğa basıldığı an dökmeye başla
        if (klavye.spaceKey.wasPressedThisFrame) DokmeyeBasla();

        if (dokuyor)
        {
            if (klavye.spaceKey.isPressed)
            {
                doluluk += dolmaHizi * Time.deltaTime;
                if (doluluk >= 1.15f) doluluk = 1.15f;
                KatmaniCiz();
                if (doluluk >= 1.15f) DokmeyiBitir(); // Çok taştı, dur
            }
            else
            {
                DokmeyiBitir(); // Boşluk bırakıldı
            }
        }
    }

    // Şurup gibi "bardağa tıklanarak eklenen" bir şey seçiliyse bardak sarı yanıp söner
    void BardakVurgusu()
    {
        bool vurgula = aktif && secili != null && !secili.dokulur && !TarifKitabi.Acik;
        float t = Mathf.PingPong(Time.time * vurguHizi, 1f);

        if (bardakGorsel != null)
            bardakGorsel.color = vurgula ? Color.Lerp(bardakNormalRenk, vurguRengi, t * 0.6f) : bardakNormalRenk;

        if (bardakCerceve != null)
        {
            bardakCerceve.gameObject.SetActive(vurgula);
            if (vurgula)
            {
                Color c = vurguRengi;
                c.a = 0.3f + 0.7f * t;
                bardakCerceve.color = c;
            }
        }
    }

    // Müşteri gelince çağırır
    public void YeniBardak(string yeniSiparis)
    {
        siparis = yeniSiparis;
        aktif = true;

        // Bu tarifte kaç malzeme dökülecek
        dokulecekSayisi = 0;
        foreach (string m in Tarif(siparis))
        {
            if (System.Array.IndexOf(dokulenler, m) >= 0) dokulecekSayisi++;
        }

        BardagiBosalt();
    }

    // Müşteri giderse çağırır
    public void Durdur()
    {
        aktif = false;
        dokuyor = false;
        Sesler.DokmeDur();
        YaziyiSil();
    }

    // Müşteri gidince "Seçili: ..." yazısı ekranda kalmasın
    void YaziyiSil()
    {
        secili = null;
        if (bardakYazisi != null) bardakYazisi.text = "";
    }

    // Gün bitince bardağı ve yazıyı temizler
    public void Temizle()
    {
        aktif = false;
        BardagiBosalt();
        secili = null;
        if (bardakYazisi != null) bardakYazisi.text = "";
        hedefCizgi.gameObject.SetActive(false);
    }

    void TiklamayaBak(Mouse fare)
    {
        if (dokuyor) return;

        Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());
        Collider2D tiklanan = Physics2D.OverlapPoint(nokta);
        if (tiklanan == null) return;
        Sesler.Tiklama();

        // Bardağa tıklandıysa ve şurup gibi dökülmeyen bir şey seçiliyse: ekle
        if (tiklanan.transform == bardak)
        {
            if (secili != null && !secili.dokulur)
            {
                // Aynı şey (örn. şurup) iki kez eklenmesin
                if (!icindekiler.Contains(secili.malzemeAdi)) icindekiler.Add(secili.malzemeAdi);
                sonDokulen = null;   // Bundan sonraki döküm yeni katman olsun
                secili = null;   // Eklendi, vurgu söner (iki kere eklenmesin)
                YaziyiGuncelle();
            }
            return;
        }

        // Malzemeye tıklandıysa: seç
        Malzeme m = tiklanan.GetComponent<Malzeme>();
        if (m == null) return;

        secili = m;
        YaziyiGuncelle();
    }

    void DokmeyeBasla()
    {
        // Seçili bir şey yoksa ya da seçili şey dökülmüyorsa (şurup) dökme
        if (dokuyor || secili == null || !secili.dokulur) return;

        // Bardak dolu: yeni malzeme dökülemez, oyuncuya söyle
        if (doluluk >= 1f)
        {
            uyari = "<color=#E8762B>Bardak dolu! R: boşalt</color>";
            YaziyiGuncelle();
            return;
        }

        dokuyor = true;
        Sesler.DokmeBasla();

        // Aynı malzeme arka arkaya tekrar dökülüyorsa: yeni malzeme sayılmaz, aynı katman uzar.
        // Önceki dökümün puanı silinir, bu döküm bitince yeniden hesaplanır.
        if (sonDokulen == secili.malzemeAdi && suankiKatman != null && farklar.Count > 0)
        {
            farklar.RemoveAt(farklar.Count - 1);
            yonler.RemoveAt(yonler.Count - 1);
            HedefCizgiyiGuncelle();
            KatmaniCiz();
            YaziyiGuncelle();
            return;
        }

        katmanBaslangic = doluluk;
        sonDokulen = secili.malzemeAdi;

        // Yeni bir renkli katman oluştur
        GameObject katman = Instantiate(kahve.gameObject);
        katman.SetActive(true);
        katman.GetComponent<SpriteRenderer>().color = secili.renk;
        katmanlar.Add(katman);
        suankiKatman = katman.transform;

        icindekiler.Add(secili.malzemeAdi);
        KatmaniCiz();
        YaziyiGuncelle();
    }

    void DokmeyiBitir()
    {
        dokuyor = false;
        Sesler.DokmeDur();

        // Bu döküm hedefe ne kadar yakın
        float hedef = HedefSeviye(farklar.Count + 1);
        float fark = doluluk > 1f ? 99f : Mathf.Abs(doluluk - hedef);
        farklar.Add(fark);
        yonler.Add(doluluk - hedef);

        HedefCizgiyiGuncelle();
    }

    void BardagiBosalt()
    {
        foreach (GameObject k in katmanlar) Destroy(k);
        katmanlar.Clear();
        icindekiler.Clear();
        farklar.Clear();
        yonler.Clear();
        uyari = "";
        sonDokulen = null;
        suankiKatman = null;
        doluluk = 0f;
        dokuyor = false;
        Sesler.DokmeDur();
        sonHedef = Random.Range(0.7f, 0.9f); // Her bardakta hedef biraz değişir

        HedefCizgiyiGuncelle();
        YaziyiGuncelle();
    }

    // Kaçıncı dökümde bardak nereye kadar dolmalı
    float HedefSeviye(int kacinci)
    {
        if (dokulecekSayisi == 0 || kacinci > dokulecekSayisi) return sonHedef;
        return sonHedef * kacinci / dokulecekSayisi;
    }

    void HedefCizgiyiGuncelle()
    {
        // Bütün dökümler bitse de çizgi son hedefte kalır (üstünü tamamlamak isteyen görebilsin)
        bool gorunsun = dokulecekSayisi > 0;
        hedefCizgi.gameObject.SetActive(gorunsun);
        if (!gorunsun) return;

        int sonraki = Mathf.Min(farklar.Count + 1, dokulecekSayisi);
        Vector3 p = hedefCizgi.position;
        p.y = bardakAlt + HedefSeviye(sonraki) * bardakBoyu;
        hedefCizgi.position = p;
    }

    void KatmaniCiz()
    {
        float ust = Mathf.Min(doluluk, 1f);
        float yukseklik = Mathf.Max(0f, ust - katmanBaslangic) * bardakBoyu;

        Vector3 s = suankiKatman.localScale;
        s.y = yukseklik;
        suankiKatman.localScale = s;

        Vector3 p = suankiKatman.position;
        p.y = bardakAlt + katmanBaslangic * bardakBoyu + yukseklik / 2f;
        suankiKatman.position = p;
    }

    void YaziyiGuncelle()
    {
        if (bardakYazisi == null) return;
        string seciliAd = secili != null ? GorunenAd(secili.malzemeAdi) : "-";
        string ipucu = (secili != null && !secili.dokulur) ? "\n<size=80%>Bardağa tıkla!</size>" : "";
        if (!string.IsNullOrEmpty(uyari)) ipucu += "\n<size=80%>" + uyari + "</size>";
        List<string> adlar = new List<string>();
        foreach (string m in icindekiler) adlar.Add(GorunenAd(m));
        bardakYazisi.text = "Seçili: " + seciliAd + "\n" + string.Join(" + ", adlar) + ipucu;
    }

    void ServisEt()
    {
        // Dökerken E'ye basıldıysa: önce dökmeyi bitir, o döküm de puana sayılsın
        if (dokuyor) DokmeyiBitir();

        aktif = false;
        dokuyor = false;
        Sesler.DokmeDur();
        Sesler.Zil();

        // En kötü döküm kaliteyi belirler
        float enKotu = 0f;
        int enKotuSira = -1;
        for (int i = 0; i < farklar.Count; i++)
        {
            if (farklar[i] > enKotu) { enKotu = farklar[i]; enKotuSira = i; }
        }

        string kalite;
        string detay = "";
        if (enKotu >= 99f)
        {
            // Taştıysa önce bu söylenir (taşınca diğer malzeme de eklenemiyor olabilir)
            kalite = "TASTI";
            if (!TarifTutuyor()) detay = EksikFazla();
        }
        else if (!TarifTutuyor())
        {
            kalite = "YANLIS";
            detay = EksikFazla();
        }
        else if (enKotu < 0.04f) kalite = "MUKEMMEL";
        else if (enKotu < 0.12f) kalite = "IYI";
        else
        {
            kalite = "KOTU";
            bool gecti = enKotuSira >= 0 && enKotuSira < yonler.Count && yonler[enKotuSira] > 0f;
            detay = gecti ? "ÇİZGİYİ GEÇTİN!" : "AZ DOLDU...";
        }

        Debug.Log("Siparis: " + siparis + " | Bardakta: " + string.Join(" + ", icindekiler) + " | Sonuc: " + kalite);

        // Bardağın kopyası canavara gider, gerçek bardak boşalır
        if (servisAnimasyonu != null)
        {
            servisAnimasyonu.Oynat(SiviRengi());
            BardagiBosalt();
            hedefCizgi.gameObject.SetActive(false);
        }

        YaziyiSil();
        musteri.KahveGeldi(kalite, detay);
    }

    // Bardaktaki sıvının ortalama rengi (servis animasyonu için)
    Color SiviRengi()
    {
        Color toplam = Color.clear;
        int sayi = 0;
        foreach (GameObject k in katmanlar)
        {
            if (k == null) continue;
            SpriteRenderer sr = k.GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            toplam += sr.color;
            sayi++;
        }
        if (sayi == 0) return new Color(0.35f, 0.2f, 0.1f, 1f);
        Color ortalama = toplam / sayi;
        ortalama.a = 1f;
        return ortalama;
    }

    // Bardaktakiler tarifle aynı mı (sıra önemli değil)
    bool TarifTutuyor()
    {
        List<string> tarif = new List<string>(Tarif(siparis));
        // Aynı malzeme birden fazla kez eklendiyse bir kez sayılır
        List<string> bardakIci = new List<string>();
        foreach (string m in icindekiler)
        {
            if (!bardakIci.Contains(m)) bardakIci.Add(m);
        }
        tarif.Sort();
        bardakIci.Sort();

        if (tarif.Count != bardakIci.Count) return false;
        for (int i = 0; i < tarif.Count; i++)
        {
            if (tarif[i] != bardakIci[i]) return false;
        }
        return true;
    }

    // Tarife göre bardakta ne eksik / ne fazla: "Süt eksik", "Su fazla" gibi
    string EksikFazla()
    {
        List<string> tarif = new List<string>(Tarif(siparis));
        List<string> eksik = new List<string>();
        List<string> fazla = new List<string>();
        foreach (string m in tarif)
            if (!icindekiler.Contains(m)) eksik.Add(GorunenAd(m));
        foreach (string m in icindekiler)
            if (!tarif.Contains(m) && !fazla.Contains(GorunenAd(m))) fazla.Add(GorunenAd(m));

        List<string> parcalar = new List<string>();
        if (eksik.Count > 0) parcalar.Add(string.Join(", ", eksik) + " eksik");
        if (fazla.Count > 0) parcalar.Add(string.Join(", ", fazla) + " fazla");
        return string.Join(" - ", parcalar);
    }

    // Kodda Türkçe harfsiz yazılan malzeme adlarının ekranda görünen hali
    public static string GorunenAd(string ad)
    {
        switch (ad)
        {
            case "Sut": return "Süt";
            case "Surup": return "Şurup";
            case "Seker": return "Şeker";
        }
        return ad;
    }

    // TARIFLER: yeni kahve eklemek için buraya bir satır ekle
    static string[] Tarif(string kahveAdi)
    {
        switch (kahveAdi)
        {
            case "Espresso": return new[] { "Kahve" };
            case "Americano": return new[] { "Kahve", "Su" };
            case "Latte": return new[] { "Kahve", "Sut" };
            case "Pumpkin Spice Latte": return new[] { "Kahve", "Sut", "Surup" };
            case "Türk Kahvesi": return new[] { "Kahve", "Su", "Seker" };
        }
        return new string[0];
    }
}