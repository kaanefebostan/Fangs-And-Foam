using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Oyun açılınca önce içerik uyarısı, sonra ana menü (Yeni Oyun / Devam Et / Çıkış).
public class MenuYoneticisi : MonoBehaviour
{
    public static bool Acik { get; private set; }   // Diğer kodlar menü açık mı diye bakar
    static bool uyariGosterildi = false;            // Uyarı oyun açılınca bir kez gösterilir

    // Editörde her ▶'ye basışta sıfırlansın (Unity bazen bu değerleri hatırlıyor)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Sifirla()
    {
        uyariGosterildi = false;
        Acik = false;
    }

    [Header("Sahnedeki objeler")]
    public GunYoneticisi gunYoneticisi;
    public GameObject menuPaneli;     // Menünün tamamı (arka plan dahil)
    public GameObject uyariGrubu;     // Uyarı ekranı
    public GameObject menuGrubu;      // Başlık ve butonlar
    public TMP_Text uyariYazisi;
    public TMP_Text yeniOyunButonu;
    public TMP_Text devamButonu;
    public TMP_Text cikisButonu;

    [Header("Uyarı")]
    [TextArea(6, 12)]
    public string uyariMetni =
        "<b>UYARI</b>\n\n" +
        "Bu oyun ani korkutmalar (jumpscare), yanıp sönen ışıklar, ekran sarsıntısı " +
        "ve bazı oyuncular için rahatsız edici olabilecek görüntüler içermektedir.\n\n" +
        "Işığa duyarlı epilepsi ya da benzeri bir rahatsızlığınız varsa oynamanız önerilmez.";
    public string devamIpucu = "\n\n<size=70%>Devam etmek için tıkla ya da Enter'a bas</size>";
    public float uyariEnAzSure = 2f;   // Uyarı en az kaç saniye ekranda kalır

    [Header("Renkler")]
    public Color normalRenk = new Color(0.85f, 0.8f, 0.72f);
    public Color seciliRenk = new Color(0.9f, 0.15f, 0.15f);
    public Color pasifRenk = new Color(0.35f, 0.33f, 0.3f);

    static readonly string[] butonAdlari = { "Yeni Oyun", "Devam Et", "Çıkış" };

    TMP_Text[] butonlar;
    bool uyaridaMi;
    bool ipucuGosterildi;
    float uyariSayac;
    int secili = 0;     // 0 = Yeni Oyun, 1 = Devam Et, 2 = Çıkış
    bool kayitVar;

    void Start()
    {
        Acik = true;
        menuPaneli.SetActive(true);
        butonlar = new[] { yeniOyunButonu, devamButonu, cikisButonu };
        kayitVar = PlayerPrefs.GetInt(GunYoneticisi.KayitAnahtari, 0) > 0;

        if (uyariGosterildi) MenuyuGoster();
        else UyariyiGoster();
    }

    void UyariyiGoster()
    {
        uyaridaMi = true;
        uyariSayac = 0f;
        ipucuGosterildi = false;
        uyariYazisi.text = uyariMetni;
        uyariGrubu.SetActive(true);
        menuGrubu.SetActive(false);
    }

    void MenuyuGoster()
    {
        uyaridaMi = false;
        uyariGosterildi = true;
        uyariGrubu.SetActive(false);
        menuGrubu.SetActive(true);
        secili = kayitVar ? 1 : 0;   // Kayıt varsa "Devam Et" seçili gelsin
        Renkler();
    }

    void Update()
    {
        if (!Acik) return;

        var klavye = Keyboard.current;
        var fare = Mouse.current;
        bool enter = klavye != null && (klavye.enterKey.wasPressedThisFrame || klavye.spaceKey.wasPressedThisFrame);
        bool tik = fare != null && fare.leftButton.wasPressedThisFrame;

        // Uyarı ekranı: en az birkaç saniye kalsın, sonra tıkla/Enter ile geç
        if (uyaridaMi)
        {
            uyariSayac += Time.deltaTime;
            if (uyariSayac >= uyariEnAzSure)
            {
                if (!ipucuGosterildi)
                {
                    uyariYazisi.text = uyariMetni + devamIpucu;
                    ipucuGosterildi = true;
                }
                if (enter || tik) MenuyuGoster();
            }
            return;
        }

        // Klavye ile seçim
        if (klavye != null)
        {
            if (klavye.downArrowKey.wasPressedThisFrame || klavye.sKey.wasPressedThisFrame) SecimiKaydir(1);
            if (klavye.upArrowKey.wasPressedThisFrame || klavye.wKey.wasPressedThisFrame) SecimiKaydir(-1);
        }

        // Fare ile üzerine gelme ve tıklama
        if (fare != null)
        {
            Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());
            for (int i = 0; i < butonlar.Length; i++)
            {
                Collider2D c = butonlar[i].GetComponent<Collider2D>();
                if (c == null || !Kullanilabilir(i) || !c.OverlapPoint(nokta)) continue;

                if (secili != i)
                {
                    secili = i;
                    Sesler.Tiklama();
                    Renkler();
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

    bool Kullanilabilir(int i)
    {
        return i != 1 || kayitVar;   // Kayıt yoksa "Devam Et" kullanılamaz
    }

    void SecimiKaydir(int yon)
    {
        int yeni = secili;
        for (int adim = 0; adim < butonlar.Length; adim++)
        {
            yeni = (yeni + yon + butonlar.Length) % butonlar.Length;
            if (Kullanilabilir(yeni)) break;
        }
        secili = yeni;
        Sesler.Tiklama();
        Renkler();
    }

    void Renkler()
    {
        for (int i = 0; i < butonlar.Length; i++)
        {
            bool seciliMi = i == secili && Kullanilabilir(i);
            butonlar[i].text = (seciliMi ? "> " : "") + butonAdlari[i] + (seciliMi ? " <" : "");
            butonlar[i].color = !Kullanilabilir(i) ? pasifRenk : (seciliMi ? seciliRenk : normalRenk);
        }
    }

    void Sec()
    {
        Sesler.Tiklama();
        if (secili == 0)
        {
            PlayerPrefs.DeleteKey(GunYoneticisi.KayitAnahtari);
            Kapat();
            gunYoneticisi.OyunuBaslat(0);
        }
        else if (secili == 1)
        {
            Kapat();
            gunYoneticisi.OyunuBaslat(PlayerPrefs.GetInt(GunYoneticisi.KayitAnahtari, 0));
        }
        else
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;   // Editörde oyunu durdurur
#endif
        }
    }

    void Kapat()
    {
        Acik = false;
        menuPaneli.SetActive(false);
    }
}