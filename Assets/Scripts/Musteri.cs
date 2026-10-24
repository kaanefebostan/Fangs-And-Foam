using System.Collections;
using UnityEngine;
using TMPro;

// Müşteriyi yönetir: rastgele bir canavar gelir, sipariş cümlesini söyler,
// sabrı azalır, ruh hali değişir, kahveye tepki verir, mutluluğu etkiler.
public class Musteri : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public KahveDoldurma kahveDoldurma; // Kahve makinesi
    public Mutluluk mutluluk;           // Kafenin mutluluk çubuğu
    public Transform sabirCubugu;       // Yeşil sabır çubuğu
    public TMP_Text siparisYazisi;      // Müşterinin söylediği cümle
    public TMP_Text sonucYazisi;        // Ortadaki büyük sonuç yazısı

    [Header("Canavarlar (Canavar Turu kartlarını buraya sürükle)")]
    public CanavarTuru[] canavarlar;

    [Header("Ayarlar")]
    public float sonucBeklemeSuresi = 2f;    // Sonuç ekranda kaç saniye kalır
    public string[] siparisler = { "Espresso", "Americano", "Latte", "Pumpkin Spice Latte" };

    [Header("Geçiş efekti")]
    public float titremeSuresi = 0.3f;   // Kaç saniye titrer
    public float titremeGucu = 0.06f;    // Ne kadar sert titrer

    SpriteRenderer gorsel;
    Vector3 asilKonum;
    Vector3 asilBoyut;
    int hal = 0;           // 0 = mutlu, 1 = nötr, 2 = kızgın
    CanavarTuru tur;       // Şu anki canavar
    float sabirSuresi = 20f;

    float kalanSabir;
    bool bekliyor = false;
    float cubukTamBoy;
    float cubukSolKenar;
    string siparis;

    void Start()
    {
        gorsel = GetComponent<SpriteRenderer>();
        asilKonum = transform.position;
        asilBoyut = transform.localScale;

        cubukTamBoy = sabirCubugu.localScale.x;
        cubukSolKenar = sabirCubugu.position.x - cubukTamBoy / 2f;
        YeniMusteri();
    }

    void Update()
    {
        if (!bekliyor) return;

        kalanSabir -= Time.deltaTime;
        CubuguCiz();

        // Sabır azaldıkça ruh hali kötüleşir
        float oran = kalanSabir / sabirSuresi;
        if (oran < 0.33f) HaleGec(2);
        else if (oran < 0.66f) HaleGec(1);

        if (kalanSabir <= 0f)
        {
            kahveDoldurma.Durdur();
            HaleGec(2);
            Bitir("GEC KALDIN!", "GEC");
        }
    }

    // Kahve makinesi servis edilince bunu çağırır
    public void KahveGeldi(string kalite)
    {
        if (!bekliyor) return;

        if (kalite == "MUKEMMEL")
        {
            HaleGec(0);
            Bitir("MUKEMMEL!", kalite);
        }
        else if (kalite == "IYI")
        {
            Bitir("IYI", kalite);
        }
        else if (kalite == "TASTI")
        {
            HaleGec(2);
            Bitir("TASTI!", kalite);
        }
        else if (kalite == "YANLIS")
        {
            HaleGec(2);
            Bitir("YANLIS KAHVE!", kalite);
        }
        else
        {
            HaleGec(2);
            Bitir("KOTU...", kalite);
        }
    }

    void Bitir(string mesaj, string sonuc)
    {
        bekliyor = false;
        sonucYazisi.text = mesaj;

        // Kafenin mutluluğunu değiştir
        mutluluk.SonucaGoreDegistir(sonuc);

        // Mutluluk bittiyse yeni müşteri gelmesin
        if (mutluluk.OyunBitti)
        {
            kahveDoldurma.Durdur();
            siparisYazisi.text = "";
            sonucYazisi.text = "";
            return;
        }

        Invoke(nameof(YeniMusteri), sonucBeklemeSuresi);
    }

    void YeniMusteri()
    {
        // Titreme yarıda kaldıysa düzelt
        StopAllCoroutines();
        transform.position = asilKonum;
        transform.localScale = asilBoyut;

        if (canavarlar == null || canavarlar.Length == 0)
        {
            Debug.LogError("Musteri: Canavarlar listesi bos! En az bir Canavar Turu karti ekle.");
            return;
        }

        // Rastgele bir canavar gelsin
        tur = canavarlar[Random.Range(0, canavarlar.Length)];
        sabirSuresi = tur.sabirSuresi;
        HalGoster(0); // Yeni müşteri mutlu gelir

        // Rastgele bir kahve istesin, kendi cümlesiyle söylesin
        siparis = siparisler[Random.Range(0, siparisler.Length)];
        siparisYazisi.text = tur.CumleSec(siparis);
        Debug.Log(tur.canavarAdi + " geldi, istedigi: " + siparis);

        kalanSabir = sabirSuresi;
        bekliyor = true;
        sonucYazisi.text = "";
        CubuguCiz();
        kahveDoldurma.YeniBardak(siparis);
    }

    // Ruh halini değiştir ve titret (aynı haldeyse bir şey yapma)
    void HaleGec(int yeniHal)
    {
        if (yeniHal == hal) return;
        HalGoster(yeniHal);
        StopAllCoroutines();
        StartCoroutine(Titre());
    }

    // Hale uygun resmi göster, resim yoksa rengi değiştir
    void HalGoster(int yeniHal)
    {
        hal = yeniHal;

        Sprite resim;
        Color renk;
        if (hal == 0) { resim = tur.mutluResim; renk = tur.mutluRenk; }
        else if (hal == 1) { resim = tur.notrResim; renk = tur.notrRenk; }
        else { resim = tur.kizginResim; renk = tur.kizginRenk; }

        if (resim != null)
        {
            gorsel.sprite = resim;
            gorsel.color = Color.white;
        }
        else
        {
            gorsel.color = renk;
        }
    }

    // Kısa süre titreyip büyüyerek geçişi hissettirir
    IEnumerator Titre()
    {
        float t = 0f;
        while (t < titremeSuresi)
        {
            t += Time.deltaTime;
            float kalan = 1f - t / titremeSuresi; // giderek azalır
            transform.position = asilKonum + (Vector3)(Random.insideUnitCircle * titremeGucu * kalan);
            transform.localScale = asilBoyut * (1f + 0.08f * kalan);
            yield return null;
        }
        transform.position = asilKonum;
        transform.localScale = asilBoyut;
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