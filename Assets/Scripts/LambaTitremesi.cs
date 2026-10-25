using UnityEngine;
using UnityEngine.Rendering.Universal;

// Lamba ışığı hafifçe nefes alır, ara sıra kısa süre yanıp söner.
// Bir Light 2D objesine eklenir.
public class LambaTitremesi : MonoBehaviour
{
    [Header("Ara sıra yanıp sönme")]
    public float enAzBekleme = 5f;      // İki yanıp sönme arası en az kaç saniye
    public float enFazlaBekleme = 12f;  // İki yanıp sönme arası en fazla kaç saniye
    public float titremeSuresi = 0.4f;  // Yanıp sönme kaç saniye sürer

    [Header("Sürekli hafif salınım")]
    [Range(0f, 0.5f)] public float salinim = 0.08f; // 0 = sabit, 0.08 = hafif canlı

    Light2D isik;
    float normalYogunluk;
    float sayac;
    float titremeKalan;

    void Start()
    {
        isik = GetComponent<Light2D>();
        if (isik != null) normalYogunluk = isik.intensity;
        sayac = Random.Range(enAzBekleme, enFazlaBekleme);
    }

    void Update()
    {
        if (isik == null) return;

        // Sıradaki yanıp sönmeye kadar say
        sayac -= Time.deltaTime;
        if (sayac <= 0f)
        {
            titremeKalan = titremeSuresi;
            sayac = Random.Range(enAzBekleme, enFazlaBekleme);
        }

        // Hafif, düzensiz nefes alma
        float carpan = 1f + (Mathf.PerlinNoise(Time.time * 2f, 0f) - 0.5f) * 2f * salinim;

        // Yanıp sönme anı: rastgele kararıp açılır
        if (titremeKalan > 0f)
        {
            titremeKalan -= Time.deltaTime;
            if (Random.value < 0.5f) carpan *= 0.25f;
        }

        isik.intensity = normalYogunluk * carpan;
    }
}
