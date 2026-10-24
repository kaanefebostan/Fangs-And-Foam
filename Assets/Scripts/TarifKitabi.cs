using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Tarif kitabının bir sayfası (Inspector'dan değiştirilebilir)
[System.Serializable]
public class TarifSayfasi
{
    public string kahveAdi = "";             // Boşsa sayfa hep açık. Doluysa o kahve açılınca görünür.
    [TextArea(4, 10)] public string metin = "";
}

// Tezgahtaki kitaba tıklayınca açılan tarif kitabı.
// Oklara tıkla ya da A / D (ok tuşları): sayfa çevir. ESC ya da kitaba tekrar tıklamak: kapat.
public class TarifKitabi : MonoBehaviour
{
    public static bool Acik { get; private set; }   // Diğer kodlar kitap açık mı diye bakar

    [Header("Sahnedeki objeler")]
    public GameObject kitapPaneli;       // Açılan büyük kitap (içindekilerle birlikte)
    public Collider2D kitapIkonu;        // Tezgahtaki küçük kitap
    public TMP_Text solSayfa;
    public TMP_Text sagSayfa;
    public Collider2D solOk;             // Önceki sayfa oku
    public Collider2D sagOk;             // Sonraki sayfa oku
    public GunYoneticisi gunYoneticisi;

    [Header("Sayfalar")]
    public TarifSayfasi[] sayfalar =
    {
        new TarifSayfasi { kahveAdi = "", metin =
            "<b>NASIL YAPILIR</b>\n\n<size=80%>Kapı yanıp sönünce tıkla\nMalzemeye tıkla\nBOŞLUK basılı tut\nKırmızı çizgide bırak\nE: servis   R: dök\nA / D: sayfa   ESC: kapat</size>" },
        new TarifSayfasi { kahveAdi = "Espresso", metin =
            "<b>ESPRESSO</b>\n\nKahve\n\n<size=80%>Tek döküm.</size>" },
        new TarifSayfasi { kahveAdi = "Americano", metin =
            "<b>AMERICANO</b>\n\nKahve\nSu" },
        new TarifSayfasi { kahveAdi = "Latte", metin =
            "<b>LATTE</b>\n\nKahve\nSüt" },
        new TarifSayfasi { kahveAdi = "Pumpkin Spice Latte", metin =
            "<b>PUMPKIN SPICE LATTE</b>\n\nKahve\nSüt\nŞurup\n\n<size=80%>Şuruba, sonra bardağa tıkla.</size>" },
        new TarifSayfasi { kahveAdi = "", metin =
            "<size=80%>...ve kim olursa olsun,\nmüşteriyi asla bekletme.</size>" },
    };
    [TextArea] public string kilitliMetin = "???\n\n<size=70%>Bu tarifi henüz bilmiyorsun...</size>";

    int acikCift = 0;   // Hangi iki sayfa açık (0 = 1. ve 2. sayfa)

    int ToplamCift => (sayfalar.Length + 1) / 2;

    void Start()
    {
        Kapat();
    }

    void Update()
    {
        if (MenuYoneticisi.Acik) return;
        var fare = Mouse.current;
        var klavye = Keyboard.current;

        if (fare != null && fare.leftButton.wasPressedThisFrame)
        {
            Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());

            // Küçük kitaba tıklandı mı: aç / kapat
            if (kitapIkonu.OverlapPoint(nokta))
            {
                if (Acik) Kapat();
                else Ac();
                return;
            }

            // Kitap açıksa oklara tıklandı mı
            if (Acik)
            {
                if (OkTiklandi(sagOk, nokta)) { SayfaCevir(1); return; }
                if (OkTiklandi(solOk, nokta)) { SayfaCevir(-1); return; }
            }
        }

        if (!Acik || klavye == null) return;

        if (klavye.escapeKey.wasPressedThisFrame)
        {
            Kapat();
            return;
        }
        if (klavye.dKey.wasPressedThisFrame || klavye.rightArrowKey.wasPressedThisFrame) SayfaCevir(1);
        if (klavye.aKey.wasPressedThisFrame || klavye.leftArrowKey.wasPressedThisFrame) SayfaCevir(-1);
    }

    bool OkTiklandi(Collider2D ok, Vector2 nokta)
    {
        return ok != null && ok.gameObject.activeInHierarchy && ok.OverlapPoint(nokta);
    }

    void Ac()
    {
        Acik = true;
        kitapPaneli.SetActive(true);
        Sesler.Tiklama();
        Goster();
    }

    void Kapat()
    {
        Acik = false;
        kitapPaneli.SetActive(false);
    }

    void SayfaCevir(int yon)
    {
        int yeni = Mathf.Clamp(acikCift + yon, 0, ToplamCift - 1);
        if (yeni == acikCift) return;

        acikCift = yeni;
        Sesler.Tiklama();
        Goster();
    }

    void Goster()
    {
        solSayfa.text = SayfaMetni(acikCift * 2);
        sagSayfa.text = SayfaMetni(acikCift * 2 + 1);

        // İlk sayfada sol ok, son sayfada sağ ok gizlensin
        if (solOk != null) solOk.gameObject.SetActive(acikCift > 0);
        if (sagOk != null) sagOk.gameObject.SetActive(acikCift < ToplamCift - 1);
    }

    // Sayfa açık mı kilitli mi bakar
    string SayfaMetni(int i)
    {
        if (i >= sayfalar.Length) return "";
        TarifSayfasi sayfa = sayfalar[i];

        bool kilitli = !string.IsNullOrEmpty(sayfa.kahveAdi)
                       && gunYoneticisi != null
                       && !gunYoneticisi.KahveAcikMi(sayfa.kahveAdi);

        return kilitli ? kilitliMetin : sayfa.metin;
    }
}