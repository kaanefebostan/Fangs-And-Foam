using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Mutluluk azalınca ekranın kenarları kalp atışı gibi kırmızı yanıp söner.
// Global Volume objesine eklenir (içinde Vignette efekti olmalı).
[RequireComponent(typeof(Volume))]
public class KirmiziNabiz : MonoBehaviour
{
    public Mutluluk mutluluk;

    [Header("Ayarlar")]
    [Range(0f, 1f)] public float baslangic = 0.3f;   // Mutluluk bu oranın altına inince başlar
    public Color nabizRengi = new Color(0.6f, 0f, 0f);
    public float enFazlaEkGuc = 0.2f;                // Vignette ne kadar güçlenir
    public float enYavasHiz = 1f;                    // Kalp atışı (saniyede) mutluluk eşikteyken
    public float enHizliHiz = 2f;                    // Mutluluk sıfıra yakınken

    Vignette vignette;
    Color normalRenk;
    float normalGuc;
    float evre;

    void Start()
    {
        Volume volume = GetComponent<Volume>();
        // .profile bir kopya oluşturur, böylece asıl ayar dosyası bozulmaz
        if (volume.profile != null && volume.profile.TryGet(out vignette))
        {
            vignette.color.overrideState = true;
            vignette.intensity.overrideState = true;
            normalRenk = vignette.color.value;
            normalGuc = vignette.intensity.value;
        }
    }

    void Update()
    {
        if (vignette == null || mutluluk == null) return;

        float oran = mutluluk.Oran;
        if (oran >= baslangic || mutluluk.OyunBitti)
        {
            // Normale dön
            vignette.color.value = Color.Lerp(vignette.color.value, normalRenk, Time.unscaledDeltaTime * 3f);
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, normalGuc, Time.unscaledDeltaTime * 3f);
            return;
        }

        // Mutluluk azaldıkça daha güçlü ve daha hızlı atar
        float tehlike = Mathf.Clamp01((baslangic - oran) / baslangic);
        float hiz = Mathf.Lerp(enYavasHiz, enHizliHiz, tehlike);
        evre = (evre + Time.deltaTime * hiz) % 1f;

        // Kalp atışı: "tuk-tuk ... tuk-tuk"
        float atis = Mathf.Max(Darbe(evre, 0f), Darbe(evre, 0.22f) * 0.7f);
        float guc = (0.35f + 0.65f * tehlike) * atis;

        vignette.color.value = Color.Lerp(normalRenk, nabizRengi, 0.5f + 0.5f * tehlike);
        vignette.intensity.value = normalGuc + enFazlaEkGuc * guc;
    }

    // Belli bir anda kısa bir darbe (0 ile 1 arası)
    float Darbe(float t, float an)
    {
        float fark = t - an;
        if (fark < 0f || fark > 0.18f) return 0f;
        return Mathf.Sin(fark / 0.18f * Mathf.PI);
    }
}
