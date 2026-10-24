using UnityEngine;
using TMPro;

// Yazı her değiştiğinde büyük çıkıp hafifçe sekerek yerine oturur.
// İsteğe bağlı: sonuca göre renk (mükemmel altın, iyi yeşil, kötü kırmızı).
[RequireComponent(typeof(TMP_Text))]
public class YaziEfekti : MonoBehaviour
{
    public float sure = 0.4f;          // Efekt kaç saniye sürer
    public float buyukluk = 1.35f;     // İlk çıkışta ne kadar büyük
    public bool otomatikRenk = true;   // Sonuca göre renk değişsin mi

    [Header("Renkler")]
    public Color mukemmelRenk = new Color(1f, 0.82f, 0.2f);   // altın
    public Color iyiRenk = new Color(0.55f, 0.95f, 0.45f);    // yeşil
    public Color kotuRenk = new Color(0.95f, 0.25f, 0.2f);    // kırmızı

    TMP_Text yazi;
    string onceki = "";
    Vector3 asilBoyut;
    Color asilRenk;
    float t = 99f;

    void Awake()
    {
        yazi = GetComponent<TMP_Text>();
        asilBoyut = transform.localScale;
        asilRenk = yazi.color;
    }

    void LateUpdate()
    {
        // Yazı değişti mi
        if (yazi.text != onceki)
        {
            onceki = yazi.text;
            if (!string.IsNullOrEmpty(onceki))
            {
                t = 0f;
                if (otomatikRenk) yazi.color = RenkSec(onceki);
            }
        }

        if (t < sure)
        {
            t += Time.unscaledDeltaTime;
            float oran = Mathf.Clamp01(t / sure);
            // Büyük başla, sekerek normale dön
            float boyut = 1f + (buyukluk - 1f) * Mathf.Exp(-6f * oran) * Mathf.Cos(oran * 10f);
            transform.localScale = oran >= 1f ? asilBoyut : asilBoyut * boyut;
        }
    }

    Color RenkSec(string m)
    {
        if (m.Contains("MÜKEMMEL")) return mukemmelRenk;
        if (m.Contains("İYİ")) return iyiRenk;
        if (m.Contains("KÖTÜ") || m.Contains("YANLIŞ") || m.Contains("TAŞTI") ||
            m.Contains("GEÇ") || m.Contains("BEKLETTİN")) return kotuRenk;
        return asilRenk;
    }
}