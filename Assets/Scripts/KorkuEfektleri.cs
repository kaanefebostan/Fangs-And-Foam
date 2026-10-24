using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Ekran kenarından çıkan bir uzuv (Inspector'dan ayarlanır)
[System.Serializable]
public class Uzuv
{
    public Transform obje;                              // Sahnedeki uzuv objesi
    [Range(0f, 1f)] public float esik = 0.5f;           // Mutluluk bu oranın altına inince çıkar
    public Vector2 gizliKayma = new Vector2(-1.5f, 0f); // Gizliyken ekranın ne kadar dışında durur
    [HideInInspector] public Vector3 gorunurKonum;
}

// Bir şey kötü gidince: ışık kısa süre titreyip kızıllaşır, ekran sarsılır, sonra normale döner.
// Mutluluk düştükçe uzuvlar çıkar. Mutluluk bitince jumpscare oynatır.
public class KorkuEfektleri : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public Mutluluk mutluluk;
    public Musteri musteri;              // Sürekli sallanma sadece müşteri içerideyken
    public Light2D isik;                 // Global Light 2D
    public SpriteRenderer musteriGorsel; // Jumpscare'de bu canavarın yüzü kullanılır
    public SpriteRenderer jumpscareResmi;

    [Header("Işık")]
    public float normalIsik = 1f;
    public Color normalRenk = Color.white;
    public Color korkuRengi = new Color(1f, 0.45f, 0.45f);
    public float isikTitremeSuresi = 1f;       // Bir şey kötü gidince ışık kaç saniye titrer
    public bool mutlulukDustukceKarar = false; // İşaretlersen mutluluk düştükçe ortam yavaşça kararır
    public float enKaranlikIsik = 0.6f;        // (sadece yukarıdaki işaretliyse)

    [Header("Ekran sallanması")]
    public float hataSarsintisi = 0.003f;   // Her 1 mutluluk kaybında ne kadar sarsılır
    public float surekliSarsinti = 0.012f;  // Mutluluk çok düşükken, müşteri içerideyken
    [Range(0f, 1f)] public float titremeBaslangici = 0.25f; // Sürekli sallanma bu oranın altında başlar

    [Header("Uzuvlar")]
    public Uzuv[] uzuvlar;
    public float uzuvHizi = 3f;

    [Header("Jumpscare")]
    public Sprite jumpscareOzelResim;    // Boşsa müşterinin o anki resmi kullanılır
    public float jumpscareSuresi = 1.2f;
    [Range(0f, 1f)] public float yuzYuksekligi = 0.8f; // Yüz resmin neresinde (0 = alt, 1 = üst)

    Transform kamera;
    Vector3 kameraKonum;
    float oncekiDeger;
    float darbe;
    float isikTitremeKalan;
    bool jumpscareOynuyor = false;

    void Start()
    {
        kamera = Camera.main.transform;
        kameraKonum = kamera.position;
        oncekiDeger = mutluluk.Deger;
        if (jumpscareResmi != null) jumpscareResmi.gameObject.SetActive(false);

        // Uzuvların görünür yerini hatırla, sonra ekran dışına sakla
        foreach (Uzuv u in uzuvlar)
        {
            if (u.obje == null) continue;
            u.gorunurKonum = u.obje.position;
            u.obje.position = u.gorunurKonum + (Vector3)u.gizliKayma;
        }
    }

    // Dışarıdan da çağrılabilir: ışık titrer, ekran sarsılır
    public void Korkut()
    {
        isikTitremeKalan = isikTitremeSuresi;
        darbe = Mathf.Max(darbe, 0.05f);
    }

    void Update()
    {
        if (jumpscareOynuyor) return;
        float oran = mutluluk.Oran;

        // Mutluluk düştüyse: ekran sarsılsın, ışık titresin
        if (mutluluk.Deger < oncekiDeger)
        {
            darbe = Mathf.Max(darbe, (oncekiDeger - mutluluk.Deger) * hataSarsintisi);
            isikTitremeKalan = isikTitremeSuresi;
        }
        oncekiDeger = mutluluk.Deger;
        darbe = Mathf.MoveTowards(darbe, 0f, Time.deltaTime * 0.3f);
        isikTitremeKalan -= Time.deltaTime;

        IsigiAyarla(oran);

        // Sürekli sallanma: sadece müşteri içerideyken ve oyun bitmemişken
        bool musteriIceride = musteri == null || musteri.IcerideBekliyor;
        float surekli = 0f;
        if (musteriIceride && oran < titremeBaslangici)
            surekli = surekliSarsinti * (1f - oran / titremeBaslangici);

        float guc = darbe + surekli;
        if (mutluluk.OyunBitti) guc = 0f;
        kamera.position = kameraKonum + (Vector3)(Random.insideUnitCircle * guc);

        // Uzuvlar: eşiğin altındaysa çıkar ve kıpırdar, değilse saklanır
        foreach (Uzuv u in uzuvlar)
        {
            if (u.obje == null) continue;
            bool goster = oran < u.esik;
            Vector3 hedef = u.gorunurKonum;
            if (goster) hedef += (Vector3)(u.gizliKayma.normalized * Mathf.Sin(Time.time * 2f + u.esik * 10f) * 0.05f);
            else hedef += (Vector3)u.gizliKayma;
            u.obje.position = Vector3.Lerp(u.obje.position, hedef, Time.deltaTime * uzuvHizi);
        }
    }

    void IsigiAyarla(float oran)
    {
        if (isik == null) return;

        // Normal hal
        float yogunluk = normalIsik;
        Color renk = normalRenk;

        // İsteğe bağlı: mutluluk düştükçe yavaşça karar
        if (mutlulukDustukceKarar)
        {
            float korku = Mathf.Clamp01((0.6f - oran) / 0.6f);
            yogunluk = Mathf.Lerp(normalIsik, enKaranlikIsik, korku);
            renk = Color.Lerp(normalRenk, korkuRengi, korku * 0.5f);
        }

        // Bir şey kötü gittiyse kısa süre titre ve kızıllaş, sonra normale dön
        if (isikTitremeKalan > 0f && !mutluluk.OyunBitti)
        {
            renk = korkuRengi;
            if (Random.value < 0.35f) yogunluk *= 0.25f;
        }

        isik.intensity = yogunluk;
        isik.color = renk;
    }

    // Mutluluk bitince Mutluluk kodu bunu çağırır
    public IEnumerator Jumpscare()
    {
        jumpscareOynuyor = true;
        Sesler.Jumpscare();

        Sprite resim = jumpscareOzelResim;
        if (resim == null && musteriGorsel != null) resim = musteriGorsel.sprite;

        if (jumpscareResmi != null && resim != null)
        {
            jumpscareResmi.sprite = resim;
            jumpscareResmi.color = Color.white;
            jumpscareResmi.gameObject.SetActive(true);
        }

        float t = 0f;
        while (t < jumpscareSuresi)
        {
            t += Time.deltaTime;

            // Yüz çok hızlı ekrana yaklaşır
            float yaklasma = Mathf.Clamp01(t / 0.12f);
            float boyut = Mathf.Lerp(1.4f, 2.2f, yaklasma);
            if (jumpscareResmi != null && jumpscareResmi.sprite != null) YuzuOrtala(boyut);

            // Sert sarsıntı ve yanıp sönen kızıl ışık
            kamera.position = kameraKonum + (Vector3)(Random.insideUnitCircle * 0.12f);
            if (isik != null)
            {
                isik.color = korkuRengi;
                isik.intensity = Random.value < 0.5f ? 1.4f : 0.6f;
            }
            yield return null;
        }

        if (jumpscareResmi != null) jumpscareResmi.gameObject.SetActive(false);
        kamera.position = kameraKonum;

        // Işık normale dönsün
        if (isik != null)
        {
            isik.intensity = normalIsik;
            isik.color = normalRenk;
        }
        jumpscareOynuyor = false;
    }

    // Resmi büyütür ve canavarın yüzünü ekranın ortasına getirir.
    // Resmin boş (şeffaf) kısımlarını saymaz, sadece çizili kısma bakar.
    void YuzuOrtala(float boyut)
    {
        Sprite s = jumpscareResmi.sprite;
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (Vector2 v in s.vertices)
        {
            if (v.x < minX) minX = v.x;
            if (v.x > maxX) maxX = v.x;
            if (v.y < minY) minY = v.y;
            if (v.y > maxY) maxY = v.y;
        }

        float yuzX = (minX + maxX) / 2f;
        float yuzY = minY + (maxY - minY) * yuzYuksekligi;

        Vector3 merkez = new Vector3(kameraKonum.x, kameraKonum.y, 0f);
        jumpscareResmi.transform.localScale = Vector3.one * boyut;
        jumpscareResmi.transform.position = merkez - new Vector3(yuzX * boyut, yuzY * boyut, 0f);
    }
}