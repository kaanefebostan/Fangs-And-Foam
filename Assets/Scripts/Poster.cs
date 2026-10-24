using UnityEngine;
using UnityEngine.InputSystem;

// Duvardaki "aranıyor" posteri.
// Tıklanınca büyük hali açılır; herhangi bir yere tıklayınca kapanır.
// AMA ikizler içerideyken tıklanırsa ikizler saldırır ve oyun biter.
[RequireComponent(typeof(BoxCollider2D))]
public class Poster : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public Musteri musteri;          // Müşteri objesi
    public Mutluluk mutluluk;        // Mutluluk çubuğu (oyun bitti mi diye bakmak için)
    public GameObject buyukPoster;   // Ortada açılan büyük poster (arka plan karartmasıyla birlikte)

    [Header("Saldıracak canavar")]
    public CanavarTuru ikizler;      // İkizlerin canavar kartı

    [Header("Saldırı sahnesi")]
    [TextArea] public string oyuncuCumlesi = "Bir dakika... Bu siz misiniz?";
    [TextArea] public string ikizlerinCumlesi = "Gördü bizi...\nSusturalım o zaman.";
    public string saldiriMetni = "SENİ GÖRDÜLER";

    [Header("Ayarlar")]
    public AudioClip tiklamaSesi;    // Poster açılınca çalar (boş kalabilir)

    // Poster açık mı (açıkken kahve yapılamaz, kapıya tıklanamaz)
    public static bool Acik { get; private set; }
    static int kapanisKaresi = -1;

    // Açıkken ya da bu karede kapandıysa diğer tıklamalar çalışmasın
    public static bool Engelli => Acik || Time.frameCount == kapanisKaresi;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Sifirla()
    {
        Acik = false;
        kapanisKaresi = -1;
    }

    void Start()
    {
        Acik = false;
        if (buyukPoster != null) buyukPoster.SetActive(false);
    }

    void Update()
    {
        if (DuraklatmaMenusu.Acik || MenuYoneticisi.Acik) return;
        if (mutluluk != null && mutluluk.OyunBitti)
        {
            if (Acik) Kapat();
            return;
        }

        var fare = Mouse.current;
        if (fare == null || !fare.leftButton.wasPressedThisFrame) return;

        // Açıksa: nereye tıklanırsa tıklansın kapat
        if (Acik)
        {
            Kapat();
            return;
        }

        if (TarifKitabi.Acik) return;

        // Postere mi tıklandı
        Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());
        bool bunaTiklandi = false;
        foreach (Collider2D c in Physics2D.OverlapPointAll(nokta))
        {
            if (c.gameObject == gameObject) bunaTiklandi = true;
        }
        if (!bunaTiklandi) return;

        // İkizler içerideyse: saldırı!
        if (musteri != null && ikizler != null && musteri.IcerideMi && musteri.SuankiCanavar == ikizler)
        {
            musteri.Saldir(oyuncuCumlesi, ikizlerinCumlesi, saldiriMetni);
            return;
        }

        Ac();
    }

    void Ac()
    {
        Acik = true;
        if (tiklamaSesi != null) Sesler.Oynat(tiklamaSesi);
        if (buyukPoster != null) buyukPoster.SetActive(true);
    }

    void Kapat()
    {
        Acik = false;
        kapanisKaresi = Time.frameCount;
        if (buyukPoster != null) buyukPoster.SetActive(false);
    }
}
