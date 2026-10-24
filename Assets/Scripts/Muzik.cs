using UnityEngine;

// İki müziği aynı anda çalar, aralarında yumuşak geçiş yapar.
// Normal müzik sesli başlar, karanlık müzik sessiz başlar.
public class Muzik : MonoBehaviour
{
    [Header("Müzikler (Audio klasöründen sürükle)")]
    public AudioClip normalMuzik;
    public AudioClip karanlikMuzik;

    [Header("Ayarlar")]
    [Range(0f, 1f)] public float sesSeviyesi = 0.6f;  // Müziğin genel sesi
    public float gecisSuresi = 1.5f;                  // Geçiş kaç saniye sürer

    AudioSource normal;
    AudioSource karanlik;
    bool karanlikMi = false;

    void Awake()
    {
        // İki ayrı hoparlör oluştur, ikisini de aynı anda başlat
        normal = gameObject.AddComponent<AudioSource>();
        karanlik = gameObject.AddComponent<AudioSource>();
        Hazirla(normal, normalMuzik, sesSeviyesi);
        Hazirla(karanlik, karanlikMuzik, 0f);
    }

    void Hazirla(AudioSource kaynak, AudioClip muzik, float ses)
    {
        kaynak.clip = muzik;
        kaynak.loop = true;
        kaynak.volume = ses;
        kaynak.playOnAwake = false;
        if (muzik != null) kaynak.Play();
    }

    void Update()
    {
        // Sesleri yavaşça hedefe doğru kaydır
        float hedefNormal = karanlikMi ? 0f : sesSeviyesi;
        float hedefKaranlik = karanlikMi ? sesSeviyesi : 0f;
        float adim = sesSeviyesi / gecisSuresi * Time.deltaTime;

        normal.volume = Mathf.MoveTowards(normal.volume, hedefNormal, adim);
        karanlik.volume = Mathf.MoveTowards(karanlik.volume, hedefKaranlik, adim);
    }

    // Müşteri çağırır: true = karanlık müziğe geç, false = normale dön
    public void KaranlikOlsun(bool evet)
    {
        karanlikMi = evet;
    }
}