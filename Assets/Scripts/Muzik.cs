using UnityEngine;

// Birden fazla müziği aynı anda çalar ama sadece biri duyulur.
// Seviye 0 = normal (1. müzik). Seviye arttıkça daha karanlık müziğe yumuşak geçiş yapar.
public class Muzik : MonoBehaviour
{
    [Header("Müzikler: en normalden en karanlığa (srk1, srk2, srk3)")]
    public AudioClip[] muzikler;

    [Header("Eski alanlar (liste boşsa bunlar kullanılır)")]
    public AudioClip normalMuzik;
    public AudioClip karanlikMuzik;

    [Header("Ayarlar")]
    [Range(0f, 1f)] public float sesSeviyesi = 0.25f;  // Müziğin genel sesi
    public float gecisSuresi = 1.5f;                   // Geçiş kaç saniye sürer

    AudioSource[] kaynaklar;
    int seviye = 0;

    void Awake()
    {
        // Liste boşsa eski iki müziği kullan
        if (muzikler == null || muzikler.Length == 0)
            muzikler = new[] { normalMuzik, karanlikMuzik };

        // Hepsini aynı anda başlat, sadece ilki duyulsun
        kaynaklar = new AudioSource[muzikler.Length];
        for (int i = 0; i < muzikler.Length; i++)
        {
            AudioSource k = gameObject.AddComponent<AudioSource>();
            k.clip = muzikler[i];
            k.loop = true;
            k.playOnAwake = false;
            k.volume = i == 0 ? sesSeviyesi : 0f;
            if (muzikler[i] != null) k.Play();
            kaynaklar[i] = k;
        }
    }

    void Update()
    {
        if (kaynaklar == null) return;

        // Seçili müziğin sesi açılır, diğerleri yavaşça kısılır
        float adim = sesSeviyesi / gecisSuresi * Time.deltaTime;
        for (int i = 0; i < kaynaklar.Length; i++)
        {
            float hedef = i == seviye ? sesSeviyesi : 0f;
            kaynaklar[i].volume = Mathf.MoveTowards(kaynaklar[i].volume, hedef, adim);
        }
    }

    // 0 = normal, 1 = biraz karanlık, 2 = en karanlık
    public void Seviye(int yeniSeviye)
    {
        if (kaynaklar == null || kaynaklar.Length == 0) return;
        seviye = Mathf.Clamp(yeniSeviye, 0, kaynaklar.Length - 1);
    }

    // Eski kodlar için: true = en karanlık, false = normal
    public void KaranlikOlsun(bool evet)
    {
        if (kaynaklar == null || kaynaklar.Length == 0) return;
        Seviye(evet ? kaynaklar.Length - 1 : 0);
    }
}