using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Bir canavar türünün ayar kartı. Kod bilmeden Inspector'dan doldurulur.
// Oluşturmak için: Project panelinde sağ tık > Create > Kafe > Canavar Turu
[CreateAssetMenu(fileName = "YeniCanavar", menuName = "Kafe/Canavar Turu")]
public class CanavarTuru : ScriptableObject
{
    public string canavarAdi = "Canavar";

    [Header("Resimler (ağız kapalı hali)")]
    public Sprite mutluResim;
    public Sprite notrResim;
    public Sprite kizginResim;
    public Sprite jumpscareResmi;   // Boşsa kızgın resim kullanılır

    [Header("Konuşma: her hal için ağız çizimleri (sıra önemli değil, boş kalabilir)")]
    public Sprite[] mutluKonusma;
    public Sprite[] notrKonusma;
    public Sprite[] kizginKonusma;

    [Header("Eski tek ağız resimleri (yukarıdaki listeler boşsa kullanılır)")]
    public Sprite mutluAgizAcik;
    public Sprite notrAgizAcik;
    public Sprite kizginAgizAcik;

    [Header("Geçiş animasyonları (kareleri sırayla sürükle, boş kalabilir)")]
    public Sprite[] mutludanNotre;     // Mutlu -> nötr geçiş kareleri
    public Sprite[] notrdenKizgina;    // Nötr -> kızgın geçiş kareleri
    public float kareSuresi = 0.08f;   // Her kare kaç saniye görünür
    public float siyahGecisSuresi = 0.25f; // Kare yoksa: siyaha kararma ve açılma süresi

    [Header("Resim yokken kullanılacak renkler")]
    public Color mutluRenk = new Color(0.55f, 0f, 1f);     // mor
    public Color notrRenk = new Color(1f, 0.8f, 0.2f);     // sarı
    public Color kizginRenk = new Color(0.9f, 0.1f, 0.1f); // kırmızı

    [Header("Huy")]
    public float sabirSuresi = 20f;   // Kaç saniye bekler

    [Header("Karakter sesleri (boş kalabilir)")]
    public AudioClip girisSesi;    // İçeri girince çalar
    public AudioClip kizginSesi;   // Kızgın hale geçince çalar

    [Header("Diyaloglar: her kahve için birkaç cümle ({kahve} yazılan yere kahvenin adı gelir)")]
    [TextArea] public string[] espressoCumleleri;
    [TextArea] public string[] americanoCumleleri;
    [TextArea] public string[] latteCumleleri;
    [TextArea] public string[] pslCumleleri;

    [Header("Hal değişince söylenecek cümleler (boş kalabilir, {kahve} ya da {KAHVE} kullanılabilir)")]
    [TextArea] public string[] notreGecinceCumleleri;    // Sabrı azalmaya başlayınca
    [TextArea] public string[] kizginaGecinceCumleleri;  // Sabrı bitmek üzereyken

    static readonly CultureInfo turkce = new CultureInfo("tr-TR");

    // Kahvenin oyunda görünen adı
    public static string KahveAdi(string kahve)
    {
        if (kahve == "Espresso") return "Espresso";
        if (kahve == "Americano") return "Americano";
        if (kahve == "Latte") return "Latte";
        return "Balkabaklı Latte";
    }

    // Cümledeki {kahve} ve {KAHVE} yerine kahvenin adını koyar
    static string KahveyiYerlestir(string cumle, string kahve)
    {
        if (string.IsNullOrEmpty(cumle)) return cumle;
        string ad = KahveAdi(kahve);
        cumle = cumle.Replace("{KAHVE}", ad.ToUpper(turkce));
        cumle = cumle.Replace("{kahve}", ad);
        return cumle;
    }

    // İstenen kahve için rastgele bir cümle seçer. Cümle yoksa kahvenin adını döndürür.
    public string CumleSec(string kahve)
    {
        string[] liste;
        if (kahve == "Espresso") liste = espressoCumleleri;
        else if (kahve == "Americano") liste = americanoCumleleri;
        else if (kahve == "Latte") liste = latteCumleleri;
        else liste = pslCumleleri;

        if (liste == null || liste.Length == 0) return KahveAdi(kahve);
        return KahveyiYerlestir(liste[Random.Range(0, liste.Length)], kahve);
    }

    // Hal değişince söylenecek cümleyi seçer (1 = nötr, 2 = kızgın).
    // Liste boşsa null döner, o zaman canavar bir şey söylemez.
    public string HalCumlesiSec(int hal, string kahve)
    {
        string[] liste = null;
        if (hal == 1) liste = notreGecinceCumleleri;
        else if (hal == 2) liste = kizginaGecinceCumleleri;

        if (liste == null || liste.Length == 0) return null;
        return KahveyiYerlestir(liste[Random.Range(0, liste.Length)], kahve);
    }

    // Hale göre konuşma (ağız) çizimlerini verir. Liste boşsa eski tek resmi kullanır.
    public List<Sprite> KonusmaKareleri(int hal)
    {
        Sprite[] liste = hal == 0 ? mutluKonusma : (hal == 1 ? notrKonusma : kizginKonusma);
        List<Sprite> sonuc = new List<Sprite>();
        if (liste != null)
        {
            foreach (Sprite s in liste)
            {
                if (s != null) sonuc.Add(s);
            }
        }

        if (sonuc.Count == 0)
        {
            Sprite tek = hal == 0 ? mutluAgizAcik : (hal == 1 ? notrAgizAcik : kizginAgizAcik);
            if (tek != null) sonuc.Add(tek);
        }
        return sonuc;
    }

    // Bir halden diğerine geçerken oynatılacak kareleri sırasıyla verir.
    // Kötüleşirken düz, iyileşirken tersten oynatır.
    public List<Sprite> GecisKareleri(int eskiHal, int yeniHal)
    {
        List<Sprite> kareler = new List<Sprite>();

        if (eskiHal < yeniHal)
        {
            if (eskiHal <= 0 && yeniHal >= 1 && mutludanNotre != null) kareler.AddRange(mutludanNotre);
            if (eskiHal <= 1 && yeniHal >= 2 && notrdenKizgina != null) kareler.AddRange(notrdenKizgina);
        }
        else
        {
            if (eskiHal >= 2 && yeniHal <= 1 && notrdenKizgina != null)
            {
                List<Sprite> ters = new List<Sprite>(notrdenKizgina);
                ters.Reverse();
                kareler.AddRange(ters);
            }
            if (eskiHal >= 1 && yeniHal <= 0 && mutludanNotre != null)
            {
                List<Sprite> ters = new List<Sprite>(mutludanNotre);
                ters.Reverse();
                kareler.AddRange(ters);
            }
        }

        return kareler;
    }
}