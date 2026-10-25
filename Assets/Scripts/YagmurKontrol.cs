using UnityEngine;

// Yağmur sesi sadece müşteri içeride kahve beklerken duyulur.
// Müşteri gelince yavaşça açılır, gidince yavaşça kısılır. Menüde ve duraklatmada sessizdir.
// Yağmur sesinin olduğu Audio Source objesine eklenir.
[RequireComponent(typeof(AudioSource))]
public class YagmurKontrol : MonoBehaviour
{
    public Musteri musteri;
    [Range(0f, 1f)] public float sesSeviyesi = 0.3f;  // Yağmur açıkken sesi
    public float gecisSuresi = 1.5f;                  // Açılıp kısılma kaç saniye sürer

    AudioSource kaynak;

    void Start()
    {
        kaynak = GetComponent<AudioSource>();
        kaynak.loop = true;
        kaynak.volume = 0f;
        if (!kaynak.isPlaying) kaynak.Play();
    }

    void Update()
    {
        bool cal = musteri != null && musteri.IcerideBekliyor && !DuraklatmaMenusu.Acik;
        float hedef = cal ? sesSeviyesi : 0f;
        float adim = sesSeviyesi / Mathf.Max(0.01f, gecisSuresi) * Time.unscaledDeltaTime;
        kaynak.volume = Mathf.MoveTowards(kaynak.volume, hedef, adim);
    }
}
