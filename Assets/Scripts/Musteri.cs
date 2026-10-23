using UnityEngine;
using TMPro;

// Müşteriyi yönetir: sipariş verir, sabrı azalır, kahveye tepki verir.
public class Musteri : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public KahveDoldurma kahveDoldurma; // Kahve makinesi
    public Transform sabirCubugu;       // Yeşil sabır çubuğu
    public TMP_Text siparisYazisi;      // Müşterinin üstündeki yazı
    public TMP_Text sonucYazisi;        // Ortadaki büyük sonuç yazısı

    [Header("Ayarlar")]
    public float sabirSuresi = 20f;          // Müşteri kaç saniye bekler
    public float sonucBeklemeSuresi = 2f;    // Sonuç ekranda kaç saniye kalır
    public string[] siparisler = { "Espresso", "Americano", "Latte", "Pumpkin Spice Latte" };

    float kalanSabir;
    bool bekliyor = false;
    float cubukTamBoy;
    float cubukSolKenar;
    string siparis;

    void Start()
    {
        cubukTamBoy = sabirCubugu.localScale.x;
        cubukSolKenar = sabirCubugu.position.x - cubukTamBoy / 2f;
        YeniMusteri();
    }

    void Update()
    {
        if (!bekliyor) return;

        kalanSabir -= Time.deltaTime;
        CubuguCiz();

        if (kalanSabir <= 0f)
        {
            kahveDoldurma.Durdur();
            Bitir("GEC KALDIN!");
        }
    }

    // Kahve makinesi servis edilince bunu çağırır
    public void KahveGeldi(string kalite)
    {
        if (!bekliyor) return;

        if (kalite == "MUKEMMEL") Bitir("MUKEMMEL!");
        else if (kalite == "IYI") Bitir("IYI");
        else if (kalite == "TASTI") Bitir("TASTI!");
        else if (kalite == "YANLIS") Bitir("YANLIS KAHVE!");
        else Bitir("KOTU...");
    }

    void Bitir(string mesaj)
    {
        bekliyor = false;
        sonucYazisi.text = mesaj;
        Invoke(nameof(YeniMusteri), sonucBeklemeSuresi);
    }

    void YeniMusteri()
    {
        // Listeden rastgele bir kahve iste
        siparis = siparisler[Random.Range(0, siparisler.Length)];

        kalanSabir = sabirSuresi;
        bekliyor = true;
        siparisYazisi.text = siparis;
        sonucYazisi.text = "";
        CubuguCiz();
        kahveDoldurma.YeniBardak(siparis);
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