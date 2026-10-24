using UnityEngine;

// Oyundaki ses efektlerini tek yerden çalar.
public class Sesler : MonoBehaviour
{
    public static Sesler Ornek;   // Sahnedeki tek Sesler objesi

    [Header("Ses efektleri (Audio klasöründen sürükle)")]
    public AudioClip tiklama;
    public AudioClip dokme;
    public AudioClip kapi;
    public AudioClip zil;
    public AudioClip posterTiklama;
    public AudioClip konusma;     // Harf harf yazarken çalan kısa ses

    [Header("Ayarlar")]
    [Range(0f, 1f)] public float sesSeviyesi = 0.8f;

    AudioSource tekSeferlik;   // Kısa sesler için
    AudioSource dokmeKaynagi;  // Dökme sesi için (döngülü)

    void Awake()
    {
        Ornek = this;
        tekSeferlik = gameObject.AddComponent<AudioSource>();
        tekSeferlik.playOnAwake = false;

        dokmeKaynagi = gameObject.AddComponent<AudioSource>();
        dokmeKaynagi.playOnAwake = false;
        dokmeKaynagi.loop = true;
    }

    static void Cal(AudioClip ses)
    {
        if (Ornek == null || ses == null) return;
        Ornek.tekSeferlik.PlayOneShot(ses, Ornek.sesSeviyesi);
    }

    public static void Tiklama() { if (Ornek != null) Cal(Ornek.tiklama); }
    public static void Kapi() { if (Ornek != null) Cal(Ornek.kapi); }
    public static void Zil() { if (Ornek != null) Cal(Ornek.zil); }
    public static void PosterTiklama() { if (Ornek != null) Cal(Ornek.posterTiklama); }
    public static void Konusma() { if (Ornek != null) Cal(Ornek.konusma); }

    public static void DokmeBasla()
    {
        if (Ornek == null || Ornek.dokme == null) return;
        Ornek.dokmeKaynagi.clip = Ornek.dokme;
        Ornek.dokmeKaynagi.volume = Ornek.sesSeviyesi;
        Ornek.dokmeKaynagi.Play();
    }

    public static void DokmeDur()
    {
        if (Ornek == null) return;
        Ornek.dokmeKaynagi.Stop();
    }
}