using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

// Kafenin mutluluk çubuğu. 0 olunca jumpscare, sonra karanlık bir "oyun bitti" ekranı.
// Enter ile oyun yeniden başlar.
public class Mutluluk : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public Transform cubuk;             // İçteki renkli çubuk
    public TMP_Text oyunBittiYazisi;    // Ortadaki büyük yazı
    public SpriteRenderer karartma;     // Tüm ekranı kaplayan siyah kutu
    public SpriteRenderer bant;         // Yazının arkasındaki siyah şerit
    public KorkuEfektleri korku;        // Jumpscare'i oynatır (boş kalabilir)

    [Header("Ayarlar")]
    public float baslangic = 70f;       // Oyun başındaki mutluluk
    public float enFazla = 100f;        // En fazla mutluluk

    [Header("Sonuca göre değişim")]
    public float mukemmel = 15f;
    public float iyi = 5f;
    public float kotu = -15f;
    public float tasti = -15f;
    public float yanlis = -20f;
    public float gecKalma = -25f;

    [Header("Oyun sonu ekranı")]
    public string bitisMetni = "KAFE KAPANDI";
    public float belirmeSuresi = 2.5f;   // Ekranın kararıp yazının belirmesi kaç saniye sürer
    public AudioClip bitisSesi;          // İsteğe bağlı: gong / boom sesi

    [Header("Renkler")]
    public Color iyiRenk = new Color(0.2f, 0.9f, 0.2f);   // yeşil
    public Color ortaRenk = new Color(1f, 0.8f, 0.2f);    // sarı
    public Color kotuRenk = new Color(0.9f, 0.1f, 0.1f);  // kırmızı

    public float Deger { get; private set; }     // Şu anki mutluluk
    public bool OyunBitti { get; private set; }  // Oyun bitti mi
    public float Oran => Deger / enFazla;        // 0 ile 1 arası (efektler için)

    float cubukTamBoy;
    float cubukSolKenar;
    SpriteRenderer cubukGorsel;
    bool yenidenBaslayabilir = false;

    void Awake()
    {
        cubukTamBoy = cubuk.localScale.x;
        cubukSolKenar = cubuk.position.x - cubukTamBoy / 2f;
        cubukGorsel = cubuk.GetComponent<SpriteRenderer>();

        Deger = baslangic;
        if (oyunBittiYazisi != null) oyunBittiYazisi.text = "";
        SaydamlikAyarla(karartma, 0f);
        SaydamlikAyarla(bant, 0f);
        Ciz();
    }

    void Update()
    {
        // Oyun sonu ekranı tamamlandıysa Enter ile baştan başla
        var klavye = Keyboard.current;
        if (OyunBitti && yenidenBaslayabilir && klavye != null && klavye.enterKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // Müşteri, kahvenin sonucunu buraya söyler
    public void SonucaGoreDegistir(string sonuc)
    {
        if (sonuc == "MUKEMMEL") Degistir(mukemmel);
        else if (sonuc == "IYI") Degistir(iyi);
        else if (sonuc == "TASTI") Degistir(tasti);
        else if (sonuc == "YANLIS") Degistir(yanlis);
        else if (sonuc == "GEC") Degistir(gecKalma);
        else Degistir(kotu);
    }

    public void Degistir(float miktar)
    {
        if (OyunBitti) return;

        Deger = Mathf.Clamp(Deger + miktar, 0f, enFazla);
        Ciz();

        if (Deger <= 0f)
        {
            OyunBitti = true;
            StartCoroutine(OyunSonuEkrani());
        }
    }

    // Önce jumpscare, sonra ekran yavaşça kararır, şerit ve yazı belirir
    IEnumerator OyunSonuEkrani()
    {
        if (korku != null) yield return StartCoroutine(korku.Jumpscare());

        if (bitisSesi != null) AudioSource.PlayClipAtPoint(bitisSesi, Camera.main.transform.position);

        oyunBittiYazisi.text = bitisMetni;
        oyunBittiYazisi.alpha = 0f;
        Vector3 yaziBoyut = oyunBittiYazisi.transform.localScale;

        float t = 0f;
        while (t < belirmeSuresi)
        {
            t += Time.deltaTime;
            float oran = Mathf.Clamp01(t / belirmeSuresi);
            float yumusak = oran * oran * (3f - 2f * oran); // yavaş başla, yavaş bitir

            SaydamlikAyarla(karartma, 0.6f * yumusak);
            SaydamlikAyarla(bant, 0.85f * yumusak);
            oyunBittiYazisi.alpha = yumusak;
            oyunBittiYazisi.transform.localScale = yaziBoyut * Mathf.Lerp(0.92f, 1.05f, oran);
            yield return null;
        }

        yield return new WaitForSeconds(0.8f);
        oyunBittiYazisi.text = bitisMetni + "\n<size=35%>Enter: tekrar dene</size>";
        yenidenBaslayabilir = true;
    }

    void SaydamlikAyarla(SpriteRenderer kutu, float saydamlik)
    {
        if (kutu == null) return;
        Color c = kutu.color;
        c.a = saydamlik;
        kutu.color = c;
    }

    void Ciz()
    {
        // Çubuğun boyu
        Vector3 s = cubuk.localScale;
        s.x = cubukTamBoy * Oran;
        cubuk.localScale = s;

        Vector3 p = cubuk.position;
        p.x = cubukSolKenar + s.x / 2f;
        cubuk.position = p;

        // Çubuğun rengi: yüksekse yeşil, ortadaysa sarı, düşükse kırmızı
        if (Oran > 0.5f) cubukGorsel.color = Color.Lerp(ortaRenk, iyiRenk, (Oran - 0.5f) * 2f);
        else cubukGorsel.color = Color.Lerp(kotuRenk, ortaRenk, Oran * 2f);
    }
}