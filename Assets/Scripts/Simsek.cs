using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Pencereden ara sıra şimşek çakar, biraz sonra gök gürler.
// Bir Light 2D objesine eklenir (ışığın Intensity değeri 0 olmalı, kod açıp kapatır).
public class Simsek : MonoBehaviour
{
    [Header("Zamanlama")]
    public float enAzBekleme = 20f;     // İki şimşek arası en az kaç saniye
    public float enFazlaBekleme = 40f;  // İki şimşek arası en fazla kaç saniye
    public float ilkBekleme = 8f;       // Oyun açılınca ilk şimşek en erken ne zaman

    [Header("Işık")]
    public float enParlak = 2f;         // Çakma anındaki parlaklık

    [Header("Gök gürültüsü")]
    public AudioClip gokGurultusu;      // Gök gürültüsü sesi (boş kalabilir)
    [Range(0f, 1f)] public float sesSeviyesi = 0.5f;
    public float sesGecikmesiEnAz = 0.4f;   // Şimşekten kaç saniye sonra gürlesin
    public float sesGecikmesiEnFazla = 1.5f;

    Light2D isik;
    AudioSource kaynak;

    void Start()
    {
        isik = GetComponent<Light2D>();
        if (isik != null) isik.intensity = 0f;

        kaynak = gameObject.AddComponent<AudioSource>();
        kaynak.playOnAwake = false;

        StartCoroutine(Dongu());
    }

    IEnumerator Dongu()
    {
        yield return new WaitForSeconds(ilkBekleme + Random.Range(0f, 5f));
        while (true)
        {
            yield return StartCoroutine(Cak());
            yield return new WaitForSeconds(Random.Range(enAzBekleme, enFazlaBekleme));
        }
    }

    IEnumerator Cak()
    {
        if (isik != null)
        {
            // Çak - sön - tekrar çak - yavaşça sön
            isik.intensity = enParlak;
            yield return new WaitForSeconds(0.07f);
            isik.intensity = 0f;
            yield return new WaitForSeconds(0.08f);
            isik.intensity = enParlak * 0.8f;

            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                isik.intensity = Mathf.Lerp(enParlak * 0.8f, 0f, t / 0.5f);
                yield return null;
            }
            isik.intensity = 0f;
        }

        // Biraz sonra gök gürlesin
        if (gokGurultusu != null)
        {
            yield return new WaitForSeconds(Random.Range(sesGecikmesiEnAz, sesGecikmesiEnFazla));
            kaynak.pitch = Random.Range(0.85f, 1.1f);   // Her seferinde biraz farklı duyulsun
            kaynak.PlayOneShot(gokGurultusu, sesSeviyesi);
        }
    }
}
