using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

// Bir günün ayarları (Inspector'dan değiştirilebilir)
[System.Serializable]
public class GunAyari
{
    public string[] kahveler = { "Espresso" };   // Bu gün sipariş edilebilecek kahveler
    public int musteriSayisi = 5;                // Bu gün kaç müşteri gelecek
    public float sabirCarpani = 1f;              // 1.2 = %20 daha sabırlı, 0.8 = %20 daha sabırsız
    public string gunNotu = "";                  // Gün başında gösterilecek not
}

// Günleri yönetir: gün başı ekranı, müşteri sayımı, gün sonu ekranı, kazanma, kayıt.
public class GunYoneticisi : MonoBehaviour
{
    public const string KayitAnahtari = "kayitliGun";   // Devam Et için kaydedilen gün

    [Header("Sahnedeki objeler")]
    public Musteri musteri;
    public Mutluluk mutluluk;
    public TMP_Text gunYazisi;       // Gün başı / gün sonu yazısı
    public SpriteRenderer gunPaneli; // Yazının arkasındaki karanlık ekran

    [Header("Başlangıç")]
    public bool menuyuBekle = true;  // İşaretliyse oyun ana menüden başlatılır

    [Header("Günler")]
    public GunAyari[] gunler =
    {
        new GunAyari { kahveler = new[] { "Espresso" }, musteriSayisi = 5, sabirCarpani = 1.3f,
                       gunNotu = "Bugünün menüsü: Espresso" },
        new GunAyari { kahveler = new[] { "Espresso", "Americano" }, musteriSayisi = 5, sabirCarpani = 1.15f,
                       gunNotu = "Yeni tarif: Americano" },
        new GunAyari { kahveler = new[] { "Espresso", "Americano", "Latte" }, musteriSayisi = 5, sabirCarpani = 1f,
                       gunNotu = "Yeni tarif: Latte" },
        new GunAyari { kahveler = new[] { "Espresso", "Americano", "Latte", "Pumpkin Spice Latte" }, musteriSayisi = 5, sabirCarpani = 1f,
                       gunNotu = "Yeni tarif: Pumpkin Spice Latte" },
        new GunAyari { kahveler = new[] { "Espresso", "Americano", "Latte", "Pumpkin Spice Latte" }, musteriSayisi = 6, sabirCarpani = 0.75f,
                       gunNotu = "Son gece. Müşteriler sabırsız..." },
    };

    int gun = 0;              // Şu anki gün (0 = 1. gün)
    int bitenMusteri = 0;
    int memnunMusteri = 0;
    bool enterBekliyor = false;
    bool kazandi = false;
    int acilisKaresi = -1;    // Menüdeki Enter, gün ekranını hemen geçmesin diye

    public int GunNumarasi => gun + 1;
    public float SabirCarpani => gunler[gun].sabirCarpani;

    // Gün numarasını kelime olarak verir (rakamlar piksel fontta karışmasın)
    string GunAdi()
    {
        string[] adlar = { "BİRİNCİ", "İKİNCİ", "ÜÇÜNCÜ", "DÖRDÜNCÜ", "BEŞİNCİ", "ALTINCI", "YEDİNCİ" };
        return gun < adlar.Length ? adlar[gun] : (gun + 1) + ".";
    }

    // Bu kahve bugüne kadar açıldı mı (tarif kitabı için)
    public bool KahveAcikMi(string kahve)
    {
        for (int i = 0; i <= gun && i < gunler.Length; i++)
        {
            if (System.Array.IndexOf(gunler[i].kahveler, kahve) >= 0) return true;
        }
        return false;
    }

    void Start()
    {
        gunYazisi.gameObject.SetActive(false);
        if (gunPaneli != null) gunPaneli.gameObject.SetActive(false);

        // Menü yoksa oyun direkt başlasın
        if (!menuyuBekle) OyunuBaslat(0);
    }

    // Menü "Yeni Oyun" ya da "Devam Et" deyince çağırır
    public void OyunuBaslat(int baslangicGunu)
    {
        gun = Mathf.Clamp(baslangicGunu, 0, gunler.Length - 1);
        acilisKaresi = Time.frameCount;
        PaneliAc(GunAdi() + " GÜN\n<size=60%>" + gunler[gun].gunNotu + "\n\nEnter: başla</size>");
    }

    void Update()
    {
        if (!enterBekliyor || mutluluk.OyunBitti || Time.frameCount == acilisKaresi) return;

        var klavye = Keyboard.current;
        if (klavye != null && klavye.enterKey.wasPressedThisFrame)
        {
            if (kazandi)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }

            // Yeni günü başlat
            PaneliKapat();
            bitenMusteri = 0;
            memnunMusteri = 0;
            musteri.GunuBaslat();
        }
    }

    // Müşteri bu günün kahvelerinden rastgele birini ister
    public string RastgeleKahve()
    {
        string[] liste = gunler[gun].kahveler;
        return liste[Random.Range(0, liste.Length)];
    }

    // Her müşteri bitince çağrılır. Gün bittiyse true döner.
    public bool MusteriBitti(bool memnunMu)
    {
        bitenMusteri++;
        if (memnunMu) memnunMusteri++;
        return bitenMusteri >= gunler[gun].musteriSayisi;
    }

    // Gün sonu ekranı (ya da son günse kazanma ekranı)
    public void GunSonuGoster()
    {
        string ozet = GunAdi() + " GÜN BİTTİ\n<size=60%>Memnun müşteri: " + memnunMusteri + " / " + bitenMusteri + "\n\n";

        gun++;
        if (gun >= gunler.Length)
        {
            kazandi = true;
            PlayerPrefs.DeleteKey(KayitAnahtari);   // Oyun bitti, kayda gerek yok
            PlayerPrefs.Save();
            PaneliAc("KAFE HAYATTA KALDI\n<size=60%>...şimdilik\n\nEnter: menüye dön</size>");
            return;
        }

        // Devam Et için hangi günde kaldığını kaydet
        PlayerPrefs.SetInt(KayitAnahtari, gun);
        PlayerPrefs.Save();

        PaneliAc(ozet + GunAdi() + " GÜN: " + gunler[gun].gunNotu + "\n\nEnter: devam</size>");
    }

    void PaneliAc(string metin)
    {
        gunYazisi.text = metin;
        gunYazisi.gameObject.SetActive(true);
        if (gunPaneli != null) gunPaneli.gameObject.SetActive(true);
        enterBekliyor = true;
    }

    void PaneliKapat()
    {
        gunYazisi.gameObject.SetActive(false);
        if (gunPaneli != null) gunPaneli.gameObject.SetActive(false);
        enterBekliyor = false;
    }
}