using UnityEngine;

// Birden fazla müziği aynı anda çalar ama sadece biri duyulur.
// Seviye 0 = normal (1. müzik). Seviye arttıkça daha karanlık müziğe yumuşak geçiş yapar.
// Jumpscare olunca Sustur() ile bütün müzik bir anda kesilir.
public class Muzik : MonoBehaviour
{
    public static Muzik Ornek { get; private set; }   // Diğer kodlar müziğe buradan ulaşır

    [Header("Müzikler: en normalden en karanlığa (srk1, srk2, srk3)")]
    public AudioClip[] muzikler;

    [Header("Menü müziği (boş kalabilir)")]
    public AudioClip menuMuzigi;          // Ana menü açıkken çalar, oyun başlayınca yavaşça oyun müziğine geçer
    [Range(0f, 1f)] public float menuSesSeviyesi = 0.25f;

    [Header("Eski alanlar (liste boşsa bunlar kullanılır)")]
    public AudioClip normalMuzik;
    public AudioClip karanlikMuzik;

    [Header("Ayarlar")]
    [Range(0f, 1f)] public float sesSeviyesi = 0.25f;  // Müziğin genel sesi
    public float gecisSuresi = 1.5f;                   // Geçiş kaç saniye sürer

    AudioSource[] kaynaklar;
    AudioSource menuKaynak;
    int seviye = 0;
    bool susturuldu = false;

    void Awake()
    {
        Ornek = this;

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
            // Menü müziği varsa her şey sessiz başlar, sonra doğru müzik yavaşça açılır
            k.volume = (i == 0 && menuMuzigi == null) ? sesSeviyesi : 0f;
            if (muzikler[i] != null) k.Play();
            kaynaklar[i] = k;
        }

        // Menü müziği ayrı bir kaynakta çalar
        if (menuMuzigi != null)
        {
            menuKaynak = gameObject.AddComponent<AudioSource>();
            menuKaynak.clip = menuMuzigi;
            menuKaynak.loop = true;
            menuKaynak.playOnAwake = false;
            menuKaynak.volume = 0f;
            menuKaynak.Play();
        }
    }

    void Update()
    {
        if (kaynaklar == null) return;

        // Susturulduysa hepsi sessiz kalır
        if (susturuldu)
        {
            foreach (AudioSource k in kaynaklar) k.volume = 0f;
            if (menuKaynak != null) menuKaynak.volume = 0f;
            return;
        }

        // Menü açıkken menü müziği çalar, oyun müziği susar (menü müziği yoksa oyun müziği çalar)
        bool menude = menuKaynak != null && MenuYoneticisi.Acik;

        // Menüde zaman durmuş olabilir, o yüzden gerçek zamanı kullan
        float dt = Time.unscaledDeltaTime;
        if (menuKaynak != null)
        {
            float menuHedef = menude ? menuSesSeviyesi : 0f;
            menuKaynak.volume = Mathf.MoveTowards(menuKaynak.volume, menuHedef, menuSesSeviyesi / gecisSuresi * dt);
        }

        // Seçili müziğin sesi açılır, diğerleri yavaşça kısılır
        float adim = sesSeviyesi / gecisSuresi * dt;
        for (int i = 0; i < kaynaklar.Length; i++)
        {
            float hedef = (!menude && i == seviye) ? sesSeviyesi : 0f;
            kaynaklar[i].volume = Mathf.MoveTowards(kaynaklar[i].volume, hedef, adim);
        }
    }

    // 0 = normal, 1 = biraz karanlık, 2 = en karanlık
    public void Seviye(int yeniSeviye)
    {
        if (kaynaklar == null || kaynaklar.Length == 0) return;
        seviye = Mathf.Clamp(yeniSeviye, 0, kaynaklar.Length - 1);
    }

    // Bütün müziği bir anda keser (jumpscare için). Sahne yeniden yüklenince müzik geri gelir.
    public void Sustur()
    {
        susturuldu = true;
        if (kaynaklar == null) return;
        foreach (AudioSource k in kaynaklar) k.volume = 0f;
    }

    // Eski kodlar için: true = en karanlık, false = normal
    public void KaranlikOlsun(bool evet)
    {
        if (kaynaklar == null || kaynaklar.Length == 0) return;
        Seviye(evet ? kaynaklar.Length - 1 : 0);
    }
}
