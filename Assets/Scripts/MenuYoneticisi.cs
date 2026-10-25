using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Oyun açılınca önce içerik uyarısı, sonra ana menü (PLAY / SETTINGS / EXIT).
// PLAY: "Devam Et / Yeni Oyun / Geri" penceresi açar.
// SETTINGS: ses ayarı ve kontroller.
// Menü resmi, butonlar ve pencereler oyun başlarken KOD TARAFINDAN kurulur;
// Inspector'dan sadece resimleri ve fontu vermek yeterli.
public class MenuYoneticisi : MonoBehaviour
{
    public static bool Acik { get; private set; }   // Diğer kodlar menü açık mı diye bakar
    static bool uyariGosterildi = false;            // Uyarı oyun açılınca bir kez gösterilir

    public const string SesAnahtari = "sesSeviyesi";

    // Editörde her ▶'ye basışta sıfırlansın (Unity bazen bu değerleri hatırlıyor)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Sifirla()
    {
        uyariGosterildi = false;
        Acik = false;
    }

    // Oyun açılır açılmaz kayıtlı ses seviyesini uygula
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void SesiUygula()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(SesAnahtari, 1f);
    }

    [Header("Sahnedeki objeler")]
    public GunYoneticisi gunYoneticisi;
    public GameObject menuPaneli;     // Menünün tamamı
    public GameObject uyariGrubu;     // Uyarı ekranı
    public GameObject menuGrubu;      // Ana menü (kod bunun içine kurar)
    public TMP_Text uyariYazisi;

    [Header("Menü resimleri")]
    public Sprite menuResmi;          // menu_arkaplan
    public Sprite playParlak;         // menu_play_parlak
    public Sprite ayarlarParlak;      // menu_settings_parlak
    public Sprite cikisParlak;        // menu_exit_parlak
    public Sprite pencereResmi;       // panel
    public TMP_FontAsset font;        // Pixelify Sans SDF
    public Material isiksizMateryal;  // Sprite-Unlit-Default (ışık titrese de menü etkilenmesin)
    public int siralama = 200;        // Menünün Order in Layer'ı (her şeyin önünde olsun)

    [Header("Yazı renkleri")]
    public Color kremRenk = new Color(0.96f, 0.9f, 0.78f);
    public Color turuncuRenk = new Color(0.87f, 0.44f, 0.15f);

    [Header("Uyarı")]
    [TextArea(6, 12)]
    public string uyariMetni =
        "<b>UYARI</b>\n\n" +
        "Bu oyun ani korkutmalar (jumpscare), yanıp sönen ışıklar, ekran sarsıntısı " +
        "ve bazı oyuncular için rahatsız edici olabilecek görüntüler içermektedir.\n\n" +
        "Işığa duyarlı epilepsi ya da benzeri bir rahatsızlığınız varsa oynamanız önerilmez.";
    public string devamIpucu = "\n\n<size=70%>Devam etmek için tıkla ya da Enter'a bas</size>";
    public float uyariEnAzSure = 2f;   // Uyarı en az kaç saniye ekranda kalır

    [Header("Ekip")]
    public string ekipMetni = "Berra Naz Saka  -  Ahmet Sait Araz  -  Kaan Efe Bostan  -  Ali Emre Çamalan";

    [Header("Kontroller")]
    [TextArea(6, 14)]
    public string kontrollerMetni =
        "Sol tık: malzeme seç, kapıya tıkla\n" +
        "SPACE (basılı tut): dök\n" +
        "Şurup: şişeye, sonra bardağa tıkla\n" +
        "E: servis et     R: bardağı boşalt\n" +
        "A / D: tarif kitabı sayfası\n" +
        "ESC: duraklat";

    // Kodun kurduğu butonlar
    MenuButonu playButonu, ayarlarButonu, cikisButonu;
    MenuButonu devamButonu, yeniOyunButonu, oyunGeriButonu;
    MenuButonu sesAzaltButonu, sesArttirButonu, ayarlarGeriButonu;
    GameObject oyunPenceresi, ayarlarPenceresi;
    TextMeshPro sesYazisi;

    enum Ekran { Uyari, Ana, Oyun, Ayarlar }
    Ekran ekran;
    MenuButonu[] butonlar = new MenuButonu[0];   // Şu anki ekrandaki butonlar
    int secili;
    float uyariSayac;
    bool ipucuGosterildi;
    bool kayitVar;

    void Start()
    {
        Acik = true;
        menuPaneli.SetActive(true);
        kayitVar = PlayerPrefs.GetInt(GunYoneticisi.KayitAnahtari, 0) > 0;
        SesiUygula();

        MenuyuKur();

        if (uyariGosterildi) AnaMenuyuGoster();
        else UyariyiGoster();
    }

    // ================= MENÜYÜ KUR =================

    void MenuyuKur()
    {
        // Her şey kameranın ortasına göre yerleşir
        Vector3 kamera = Camera.main.transform.position;
        Transform kok = new GameObject("KurulanMenu").transform;
        kok.SetParent(menuGrubu.transform, false);
        kok.position = new Vector3(kamera.x, kamera.y, 0f);

        // Arka plan resmi (başlık ve PLAY / SETTINGS / EXIT çizili)
        ResimEkle(YeniObje("MenuResmi", kok, Vector2.zero), menuResmi, siralama);

        // Ana menü butonları: tıklama alanları çizimdeki tahtaların üstünde
        playButonu = ResimButonu("PlayButonu", kok, playParlak, new Vector2(0.03f, 0.04f), new Vector2(1.95f, 0.72f));
        ayarlarButonu = ResimButonu("SettingsButonu", kok, ayarlarParlak, new Vector2(0f, -0.67f), new Vector2(1.66f, 0.42f));
        cikisButonu = ResimButonu("ExitButonu", kok, cikisParlak, new Vector2(-0.02f, -1.24f), new Vector2(1.05f, 0.44f));

        // Ekibin isimleri, menünün en altında küçük yazı
        TextMeshPro ekip = YaziEkle(kok, "EkipYazisi", ekipMetni, new Vector2(0f, -1.7f), 1.0f, kremRenk, new Vector2(6.2f, 0.2f), TextAlignmentOptions.Center);
        ekip.sortingOrder = siralama + 1;
        ekip.alpha = 0.8f;

        // PLAY penceresi
        oyunPenceresi = PencereKur("OyunPenceresi", kok, "OYNA");
        devamButonu = YaziButonu(oyunPenceresi.transform, "DEVAM ET", new Vector2(0f, 0.35f), 2.4f, new Vector2(2.6f, 0.4f));
        yeniOyunButonu = YaziButonu(oyunPenceresi.transform, "YENİ OYUN", new Vector2(0f, -0.2f), 2.4f, new Vector2(2.6f, 0.4f));
        oyunGeriButonu = YaziButonu(oyunPenceresi.transform, "GERİ", new Vector2(0f, -1.0f), 2.0f, new Vector2(1.4f, 0.35f));

        // SETTINGS penceresi
        ayarlarPenceresi = PencereKur("AyarlarPenceresi", kok, "AYARLAR");
        Transform ap = ayarlarPenceresi.transform;
        YaziEkle(ap, "SesEtiketi", "SES", new Vector2(-0.85f, 0.6f), 2.2f, kremRenk, new Vector2(1.2f, 0.4f), TextAlignmentOptions.Center);
        sesAzaltButonu = YaziButonu(ap, "-", new Vector2(0.15f, 0.6f), 2.6f, new Vector2(0.4f, 0.4f));
        sesYazisi = YaziEkle(ap, "SesYazisi", "%100", new Vector2(0.7f, 0.6f), 2.2f, kremRenk, new Vector2(0.8f, 0.4f), TextAlignmentOptions.Center);
        sesArttirButonu = YaziButonu(ap, "+", new Vector2(1.25f, 0.6f), 2.6f, new Vector2(0.4f, 0.4f));
        YaziEkle(ap, "KontrollerBaslik", "KONTROLLER", new Vector2(0f, 0.15f), 2.0f, turuncuRenk, new Vector2(2.8f, 0.35f), TextAlignmentOptions.Center);
        YaziEkle(ap, "KontrollerYazisi", kontrollerMetni, new Vector2(0f, -0.48f), 1.3f, kremRenk, new Vector2(2.8f, 1.0f), TextAlignmentOptions.Center);
        ayarlarGeriButonu = YaziButonu(ap, "GERİ", new Vector2(0f, -1.1f), 2.0f, new Vector2(1.4f, 0.35f));

        oyunPenceresi.SetActive(false);
        ayarlarPenceresi.SetActive(false);
    }

    GameObject YeniObje(string ad, Transform ebeveyn, Vector2 konum)
    {
        GameObject o = new GameObject(ad);
        o.transform.SetParent(ebeveyn, false);
        o.transform.localPosition = new Vector3(konum.x, konum.y, 0f);
        return o;
    }

    SpriteRenderer ResimEkle(GameObject o, Sprite resim, int sira)
    {
        SpriteRenderer sr = o.AddComponent<SpriteRenderer>();
        sr.sprite = resim;
        sr.sortingOrder = sira;
        if (isiksizMateryal != null) sr.sharedMaterial = isiksizMateryal;
        return sr;
    }

    TextMeshPro YaziEkle(Transform ebeveyn, string ad, string metin, Vector2 konum, float boyut, Color renk, Vector2 alan, TextAlignmentOptions hiza)
    {
        GameObject o = YeniObje(ad, ebeveyn, konum);
        TextMeshPro t = o.AddComponent<TextMeshPro>();
        if (font != null) t.font = font;
        t.text = metin;
        t.fontSize = boyut;
        t.color = renk;
        t.alignment = hiza;
        t.rectTransform.sizeDelta = alan;
        t.sortingOrder = siralama + 5;
        return t;
    }

    MenuButonu YaziButonu(Transform ebeveyn, string metin, Vector2 konum, float boyut, Vector2 tiklamaAlani)
    {
        TextMeshPro t = YaziEkle(ebeveyn, metin + "Butonu", metin, konum, boyut, kremRenk, tiklamaAlani, TextAlignmentOptions.Center);
        BoxCollider2D c = t.gameObject.AddComponent<BoxCollider2D>();
        c.size = tiklamaAlani;
        MenuButonu b = t.gameObject.AddComponent<MenuButonu>();
        b.normalRenk = kremRenk;
        b.seciliRenk = turuncuRenk;
        return b;
    }

    MenuButonu ResimButonu(string ad, Transform ebeveyn, Sprite parlak, Vector2 alanMerkezi, Vector2 alanBoyu)
    {
        GameObject o = YeniObje(ad, ebeveyn, Vector2.zero);
        BoxCollider2D c = o.AddComponent<BoxCollider2D>();
        c.offset = alanMerkezi;
        c.size = alanBoyu;

        // Parlak hali: menü resmiyle tam üst üste durur, sadece seçilince görünür
        GameObject p = YeniObje(ad + "Parlak", o.transform, Vector2.zero);
        SpriteRenderer sr = ResimEkle(p, parlak, siralama + 1);
        sr.enabled = false;

        MenuButonu b = o.AddComponent<MenuButonu>();
        b.parlakResim = sr;
        b.buyume = 1f;
        return b;
    }

    GameObject PencereKur(string ad, Transform ebeveyn, string baslik)
    {
        GameObject pencere = YeniObje(ad, ebeveyn, Vector2.zero);

        // Arkadaki menüyü karartan yarı saydam siyah
        Sprite kare = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        GameObject karartma = YeniObje("Karartma", pencere.transform, Vector2.zero);
        SpriteRenderer kr = ResimEkle(karartma, kare, siralama + 2);
        kr.color = new Color(0f, 0f, 0f, 0.6f);
        karartma.transform.localScale = new Vector3(30f, 20f, 1f);

        // Çerçeveli pencere
        ResimEkle(YeniObje("Cerceve", pencere.transform, Vector2.zero), pencereResmi, siralama + 3);

        // Başlık
        YaziEkle(pencere.transform, "Baslik", baslik, new Vector2(0f, 1.05f), 2.8f, turuncuRenk, new Vector2(2.8f, 0.45f), TextAlignmentOptions.Center);
        return pencere;
    }

    // ================= EKRANLAR =================

    void UyariyiGoster()
    {
        ekran = Ekran.Uyari;
        uyariSayac = 0f;
        ipucuGosterildi = false;
        uyariYazisi.text = uyariMetni;
        uyariGrubu.SetActive(true);
        menuGrubu.SetActive(false);
        butonlar = new MenuButonu[0];
    }

    void AnaMenuyuGoster()
    {
        ekran = Ekran.Ana;
        uyariGosterildi = true;
        uyariGrubu.SetActive(false);
        menuGrubu.SetActive(true);
        PencereleriKapat();
        EkranButonlari(new[] { playButonu, ayarlarButonu, cikisButonu }, 0);
    }

    void OyunPenceresiniAc()
    {
        ekran = Ekran.Oyun;
        PencereleriKapat();
        oyunPenceresi.SetActive(true);

        devamButonu.Kullanilabilir = kayitVar;
        // Kayıt varsa "Devam Et", yoksa "Yeni Oyun" seçili gelsin
        EkranButonlari(new[] { devamButonu, yeniOyunButonu, oyunGeriButonu }, kayitVar ? 0 : 1);
    }

    void AyarlariAc()
    {
        ekran = Ekran.Ayarlar;
        PencereleriKapat();
        ayarlarPenceresi.SetActive(true);
        SesYazisiniGuncelle();
        EkranButonlari(new[] { sesAzaltButonu, sesArttirButonu, ayarlarGeriButonu }, 2);
    }

    void PencereleriKapat()
    {
        oyunPenceresi.SetActive(false);
        ayarlarPenceresi.SetActive(false);

        // Ana menü butonlarının parlaklığı sönsün
        playButonu.Vurgula(false);
        ayarlarButonu.Vurgula(false);
        cikisButonu.Vurgula(false);
    }

    // Bu ekranın butonlarını ayarlar ve birini seçili yapar
    void EkranButonlari(MenuButonu[] yeniButonlar, int ilkSecili)
    {
        butonlar = yeniButonlar;
        secili = ilkSecili;
        if (!butonlar[secili].Kullanilabilir) SecimiKaydir(1, false);
        Vurgula();
    }

    // ================= GİRİŞ =================

    void Update()
    {
        if (!Acik) return;

        var klavye = Keyboard.current;
        var fare = Mouse.current;
        bool enter = klavye != null && (klavye.enterKey.wasPressedThisFrame || klavye.spaceKey.wasPressedThisFrame);
        bool tik = fare != null && fare.leftButton.wasPressedThisFrame;
        bool esc = klavye != null && klavye.escapeKey.wasPressedThisFrame;

        // Uyarı ekranı: en az birkaç saniye kalsın, sonra tıkla/Enter ile geç
        if (ekran == Ekran.Uyari)
        {
            uyariSayac += Time.unscaledDeltaTime;
            if (uyariSayac >= uyariEnAzSure)
            {
                if (!ipucuGosterildi)
                {
                    uyariYazisi.text = uyariMetni + devamIpucu;
                    ipucuGosterildi = true;
                }
                if (enter || tik) AnaMenuyuGoster();
            }
            return;
        }

        // ESC: açık pencereyi kapat, ana menüye dön
        if (esc && ekran != Ekran.Ana)
        {
            Sesler.Tiklama();
            AnaMenuyuGoster();
            return;
        }

        // Klavye ile seçim
        if (klavye != null)
        {
            if (klavye.downArrowKey.wasPressedThisFrame || klavye.sKey.wasPressedThisFrame) SecimiKaydir(1, true);
            if (klavye.upArrowKey.wasPressedThisFrame || klavye.wKey.wasPressedThisFrame) SecimiKaydir(-1, true);

            // Ayarlarda sol/sağ ok ya da A/D ile ses
            if (ekran == Ekran.Ayarlar)
            {
                if (klavye.leftArrowKey.wasPressedThisFrame || klavye.aKey.wasPressedThisFrame) SesDegistir(-0.1f);
                if (klavye.rightArrowKey.wasPressedThisFrame || klavye.dKey.wasPressedThisFrame) SesDegistir(0.1f);
            }
        }

        // Fare ile üzerine gelme ve tıklama
        if (fare != null)
        {
            Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());
            for (int i = 0; i < butonlar.Length; i++)
            {
                if (butonlar[i] == null || !butonlar[i].FareUstunde(nokta)) continue;

                if (secili != i)
                {
                    secili = i;
                    Sesler.Tiklama();
                    Vurgula();
                }
                if (tik)
                {
                    Sec();
                    return;
                }
            }
        }

        if (enter) Sec();
    }

    void SecimiKaydir(int yon, bool sesli)
    {
        int yeni = secili;
        for (int adim = 0; adim < butonlar.Length; adim++)
        {
            yeni = (yeni + yon + butonlar.Length) % butonlar.Length;
            if (butonlar[yeni] != null && butonlar[yeni].Kullanilabilir) break;
        }
        secili = yeni;
        if (sesli) Sesler.Tiklama();
        Vurgula();
    }

    void Vurgula()
    {
        for (int i = 0; i < butonlar.Length; i++)
        {
            if (butonlar[i] != null) butonlar[i].Vurgula(i == secili);
        }
    }

    // ================= SEÇİM =================

    void Sec()
    {
        Sesler.Tiklama();

        if (ekran == Ekran.Ana)
        {
            if (secili == 0) OyunPenceresiniAc();
            else if (secili == 1) AyarlariAc();
            else Cik();
        }
        else if (ekran == Ekran.Oyun)
        {
            if (secili == 0 && kayitVar)
            {
                Kapat();
                gunYoneticisi.OyunuBaslat(PlayerPrefs.GetInt(GunYoneticisi.KayitAnahtari, 0));
            }
            else if (secili == 1)
            {
                PlayerPrefs.DeleteKey(GunYoneticisi.KayitAnahtari);
                Kapat();
                gunYoneticisi.OyunuBaslat(0);
            }
            else if (secili == 2)
            {
                AnaMenuyuGoster();
            }
        }
        else if (ekran == Ekran.Ayarlar)
        {
            if (secili == 0) SesDegistir(-0.1f);
            else if (secili == 1) SesDegistir(0.1f);
            else AnaMenuyuGoster();
        }
    }

    void Cik()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // Editörde oyunu durdurur
#endif
    }

    void Kapat()
    {
        Acik = false;
        menuPaneli.SetActive(false);
    }

    // ================= SES =================

    void SesDegistir(float miktar)
    {
        float ses = Mathf.Clamp01(Mathf.Round((AudioListener.volume + miktar) * 10f) / 10f);
        AudioListener.volume = ses;
        PlayerPrefs.SetFloat(SesAnahtari, ses);
        PlayerPrefs.Save();
        SesYazisiniGuncelle();
    }

    void SesYazisiniGuncelle()
    {
        if (sesYazisi != null) sesYazisi.text = "%" + Mathf.RoundToInt(AudioListener.volume * 100f);
    }
}
