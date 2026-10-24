using System.Collections;
using UnityEngine;
using TMPro;

// Obje her açıldığında (içindekilerle birlikte) yumuşakça belirir, isteğe bağlı hafifçe büyüyerek.
public class Belirme : MonoBehaviour
{
    public float sure = 0.25f;          // Kaç saniyede belirir
    public bool buyuyerek = false;      // İşaretliyse küçükten büyüyerek gelir
    public float baslangicBoyutu = 0.85f;

    SpriteRenderer[] spriteler;
    float[] spriteAlfa;
    TMP_Text[] yazilar;
    float[] yaziAlfa;
    Vector3 asilBoyut;
    bool hazir = false;

    void Hazirla()
    {
        if (hazir) return;

        spriteler = GetComponentsInChildren<SpriteRenderer>(true);
        spriteAlfa = new float[spriteler.Length];
        for (int i = 0; i < spriteler.Length; i++) spriteAlfa[i] = spriteler[i].color.a;

        yazilar = GetComponentsInChildren<TMP_Text>(true);
        yaziAlfa = new float[yazilar.Length];
        for (int i = 0; i < yazilar.Length; i++) yaziAlfa[i] = yazilar[i].alpha;

        asilBoyut = transform.localScale;
        hazir = true;
    }

    void OnEnable()
    {
        Hazirla();
        StopAllCoroutines();
        StartCoroutine(Belir());
    }

    void OnDisable()
    {
        // Bir sonraki açılış için eski haline getir
        if (!hazir) return;
        Uygula(1f);
        transform.localScale = asilBoyut;
    }

    IEnumerator Belir()
    {
        float t = 0f;
        while (t < sure)
        {
            t += Time.unscaledDeltaTime;   // Oyun duraklatılmışken de çalışsın
            float oran = Mathf.Clamp01(t / sure);
            float yumusak = 1f - (1f - oran) * (1f - oran);
            Uygula(yumusak);
            if (buyuyerek) transform.localScale = asilBoyut * Mathf.Lerp(baslangicBoyutu, 1f, yumusak);
            yield return null;
        }
        Uygula(1f);
        transform.localScale = asilBoyut;
    }

    void Uygula(float oran)
    {
        for (int i = 0; i < spriteler.Length; i++)
        {
            if (spriteler[i] == null) continue;
            Color c = spriteler[i].color;
            c.a = spriteAlfa[i] * oran;
            spriteler[i].color = c;
        }
        for (int i = 0; i < yazilar.Length; i++)
        {
            if (yazilar[i] == null) continue;
            yazilar[i].alpha = yaziAlfa[i] * oran;
        }
    }
}