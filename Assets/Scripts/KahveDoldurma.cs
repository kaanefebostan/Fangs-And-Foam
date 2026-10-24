using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Kahve hazırlama sistemi.
// Makineye / şişeye TIKLA: malzemeyi seç.
// BOSLUK basılı tut: seçili malzemeyi dök (kahve, süt, su), kırmızı çizgide bırak.
// Şurup seçiliyken BARDAGA TIKLA: şurup eklenir.
// E: servis et.  R: bardağı dök, baştan başla.
public class KahveDoldurma : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public Transform kahve;          // Eski Kahve kutusu: katmanlar bunun kopyası olarak oluşur
    public Transform hedefCizgi;     // Kırmızı hedef çizgisi
    public Transform bardak;         // Bardak (şurup eklemek için tıklanır)
    public Musteri musteri;          // Kahveyi bekleyen müşteri
    public TMP_Text bardakYazisi;    // Bardakta ne olduğunu gösteren yazı

    [Header("Ayarlar")]
    public float dolmaHizi = 0.5f;   // Saniyede bardağın ne kadarı dolar
    public float bardakAlt = -0.8f;  // Bardağın alt kenarının yüksekliği
    public float bardakBoyu = 0.5f;  // Bardağın yüksekliği

    // Dökülen malzemelerin isimleri
    static readonly string[] dokulenler = { "Kahve", "Sut", "Su" };

    float doluluk;            // 0 = boş, 1 = ağzına kadar, 1'den fazlası taşmış
    bool aktif;               // Bekleyen müşteri var mı
    bool dokuyor;             // Şu an dökülüyor mu
    float katmanBaslangic;    // Şu anki katmanın başladığı seviye
    Transform suankiKatman;   // Şu an dökülen katman
    Malzeme secili;           // Seçili malzeme
    string siparis;           // Müşterinin istediği kahve
    int dokulecekSayisi;      // Tarifte kaç dökülen malzeme var
    float sonHedef;           // Bardağın en son dolması gereken seviye

    List<string> icindekiler = new List<string>();   // Bardağa konanlar
    List<float> farklar = new List<float>();          // Her dökümün hedeften ne kadar saptığı
    List<GameObject> katmanlar = new List<GameObject>();

    void Awake()
    {
        // Eski Kahve kutusunu gizle, sadece kopyalamak için kullanacağız
        kahve.gameObject.SetActive(false);
    }

    void Update()
    {
        var fare = Mouse.current;
        var klavye = Keyboard.current;
        if (!aktif || fare == null || klavye == null) return;

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
                icindekiler.Add(secili.malzemeAdi);
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
        if (dokuyor || secili == null || !secili.dokulur || doluluk >= 1f) return;

        dokuyor = true;
        Sesler.DokmeBasla();
        katmanBaslangic = doluluk;

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

        HedefCizgiyiGuncelle();
    }

    void BardagiBosalt()
    {
        foreach (GameObject k in katmanlar) Destroy(k);
        katmanlar.Clear();
        icindekiler.Clear();
        farklar.Clear();
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
        int sonraki = farklar.Count + 1;
        bool gorunsun = sonraki <= dokulecekSayisi;
        hedefCizgi.gameObject.SetActive(gorunsun);
        if (!gorunsun) return;

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
        string seciliAd = secili != null ? secili.malzemeAdi : "-";
        bardakYazisi.text = "Seçili: " + seciliAd + "\n" + string.Join(" + ", icindekiler);
    }

    void ServisEt()
    {
        aktif = false;
        dokuyor = false;
        Sesler.DokmeDur();
        Sesler.Zil();

        string kalite;
        if (!TarifTutuyor())
        {
            kalite = "YANLIS";
        }
        else
        {
            // En kötü döküm kaliteyi belirler
            float enKotu = 0f;
            foreach (float f in farklar) enKotu = Mathf.Max(enKotu, f);

            if (enKotu >= 99f) kalite = "TASTI";
            else if (enKotu < 0.04f) kalite = "MUKEMMEL";
            else if (enKotu < 0.12f) kalite = "IYI";
            else kalite = "KOTU";
        }

        Debug.Log("Siparis: " + siparis + " | Bardakta: " + string.Join(" + ", icindekiler) + " | Sonuc: " + kalite);
        musteri.KahveGeldi(kalite);
    }

    // Bardaktakiler tarifle aynı mı (sıra önemli değil)
    bool TarifTutuyor()
    {
        List<string> tarif = new List<string>(Tarif(siparis));
        List<string> bardakIci = new List<string>(icindekiler);
        tarif.Sort();
        bardakIci.Sort();

        if (tarif.Count != bardakIci.Count) return false;
        for (int i = 0; i < tarif.Count; i++)
        {
            if (tarif[i] != bardakIci[i]) return false;
        }
        return true;
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
        }
        return new string[0];
    }
}