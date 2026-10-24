using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

// ESC ile açılan duraklatma menüsü: Devam Et / Ana Menü / Çıkış.
// Açıkken oyun durur (zaman ve sesler).
public class DuraklatmaMenusu : MonoBehaviour
{
    public static bool Acik { get; private set; }   // Diğer kodlar oyun duraklatıldı mı diye bakar

    // Editörde her ▶'ye basışta sıfırlansın
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Sifirla()
    {
        Acik = false;
    }

    [Header("Sahnedeki objeler")]
    public GameObject panel;          // Duraklatma ekranının tamamı
    public Mutluluk mutluluk;         // Oyun bittiyse ESC menüsü açılmasın
    public TMP_Text devamButonu;
    public TMP_Text anaMenuButonu;
    public TMP_Text cikisButonu;

    [Header("Renkler")]
    public Color normalRenk = new Color(0.85f, 0.8f, 0.72f);
    public Color seciliRenk = new Color(0.9f, 0.15f, 0.15f);

    static readonly string[] butonAdlari = { "Devam Et", "Ana Menü", "Çıkış" };

    TMP_Text[] butonlar;
    int secili = 0;

    void Start()
    {
        butonlar = new[] { devamButonu, anaMenuButonu, cikisButonu };
        Acik = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        panel.SetActive(false);
    }

    void Update()
    {
        var klavye = Keyboard.current;
        var fare = Mouse.current;
        if (klavye == null) return;

        // Kapalıyken: ESC'ye basılırsa aç (menüde, kitap açıkken ve oyun bitince açılmaz)
        if (!Acik)
        {
            bool engel = MenuYoneticisi.Acik
                         || TarifKitabi.Acik
                         || TarifKitabi.KapanisKaresi == Time.frameCount
                         || (mutluluk != null && mutluluk.OyunBitti);
            if (klavye.escapeKey.wasPressedThisFrame && !engel) Ac();
            return;
        }

        // Açıkken: ESC ile kapat
        if (klavye.escapeKey.wasPressedThisFrame)
        {
            Kapat();
            return;
        }

        // Klavye ile seçim
        if (klavye.downArrowKey.wasPressedThisFrame || klavye.sKey.wasPressedThisFrame) SecimiKaydir(1);
        if (klavye.upArrowKey.wasPressedThisFrame || klavye.wKey.wasPressedThisFrame) SecimiKaydir(-1);

        // Fare ile üzerine gelme ve tıklama.
        // Zaman durduğu için tıklama alanı (collider) yerine yazının kendi kutusuna bakıyoruz.
        if (fare != null && Camera.main != null)
        {
            Vector2 ekranNoktasi = fare.position.ReadValue();
            for (int i = 0; i < butonlar.Length; i++)
            {
                RectTransform alan = butonlar[i].rectTransform;
                if (!RectTransformUtility.RectangleContainsScreenPoint(alan, ekranNoktasi, Camera.main)) continue;

                if (secili != i)
                {
                    secili = i;
                    Renkler();
                }
                if (fare.leftButton.wasPressedThisFrame)
                {
                    Sec();
                    return;
                }
            }
        }

        if (klavye.enterKey.wasPressedThisFrame || klavye.spaceKey.wasPressedThisFrame) Sec();
    }

    void Ac()
    {
        Acik = true;
        panel.SetActive(true);
        Time.timeScale = 0f;          // Oyun durur
        AudioListener.pause = true;   // Sesler durur
        secili = 0;
        Renkler();
    }

    void Kapat()
    {
        Acik = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void SecimiKaydir(int yon)
    {
        secili = (secili + yon + butonlar.Length) % butonlar.Length;
        Renkler();
    }

    void Renkler()
    {
        for (int i = 0; i < butonlar.Length; i++)
        {
            bool seciliMi = i == secili;
            butonlar[i].text = (seciliMi ? "> " : "") + butonAdlari[i] + (seciliMi ? " <" : "");
            butonlar[i].color = seciliMi ? seciliRenk : normalRenk;
        }
    }

    void Sec()
    {
        if (secili == 0)
        {
            Kapat();
        }
        else if (secili == 1)
        {
            Kapat();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // Ana menüye döner
        }
        else
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}