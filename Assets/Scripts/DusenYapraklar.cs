using System.Collections.Generic;
using UnityEngine;

// Ekranın üstünden sonbahar yaprakları süzülerek düşer.
// Menünün içine (MenuGrubu'nun altına) boş bir objeye eklenir; menü kapanınca yapraklar da kaybolur.
public class DusenYapraklar : MonoBehaviour
{
    public Sprite[] yapraklar;           // Yaprak resimleri
    public int siralama = 201;           // Order in Layer (menü resminin hemen önü)
    public Material isiksizMateryal;     // Isiksiz materyal (boş kalabilir)

    [Header("Ayarlar")]
    public float aralik = 0.7f;          // Kaç saniyede bir yeni yaprak
    public float enAzHiz = 0.25f;        // Düşme hızı (birim/saniye)
    public float enFazlaHiz = 0.45f;
    public float salinim = 0.25f;        // Sağa sola ne kadar savrulur
    public float genislik = 3.3f;        // Ekranın yarı genişliği
    public float yukseklik = 1.9f;       // Ekranın yarı yüksekliği

    class Yaprak
    {
        public SpriteRenderer sr;
        public float x, y, hiz, faz;
    }

    readonly List<Yaprak> aktifler = new List<Yaprak>();
    readonly Queue<SpriteRenderer> havuz = new Queue<SpriteRenderer>();
    float sayac;

    void OnEnable()
    {
        // Menü açılınca ekranda birkaç yaprak hazır olsun
        for (int i = 0; i < 5; i++) YaprakCikar(Random.Range(-yukseklik, yukseklik));
    }

    void OnDisable()
    {
        foreach (Yaprak y in aktifler)
        {
            if (y.sr != null)
            {
                y.sr.gameObject.SetActive(false);
                havuz.Enqueue(y.sr);
            }
        }
        aktifler.Clear();
    }

    void Update()
    {
        if (yapraklar == null || yapraklar.Length == 0) return;

        // Menüde zaman durmuş olabilir, o yüzden gerçek zamanı kullan
        float dt = Time.unscaledDeltaTime;

        sayac -= dt;
        if (sayac <= 0f)
        {
            sayac = aralik;
            YaprakCikar(yukseklik + 0.1f);
        }

        Vector3 merkez = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        for (int i = aktifler.Count - 1; i >= 0; i--)
        {
            Yaprak y = aktifler[i];
            y.y -= y.hiz * dt;
            y.faz += dt;

            if (y.y < -yukseklik - 0.2f)
            {
                y.sr.gameObject.SetActive(false);
                havuz.Enqueue(y.sr);
                aktifler.RemoveAt(i);
                continue;
            }

            float savrulma = Mathf.Sin(y.faz * 1.5f) * salinim;
            Vector3 konum = new Vector3(merkez.x + y.x + savrulma, merkez.y + y.y, 0f);
            konum.x = Mathf.Round(konum.x * 100f) / 100f;   // piksel piksel
            konum.y = Mathf.Round(konum.y * 100f) / 100f;
            y.sr.transform.position = konum;

            // Savrulurken yaprak döner gibi görünsün (sağa-sola çevrilir)
            y.sr.flipX = Mathf.Cos(y.faz * 1.5f) < 0f;
        }
    }

    void YaprakCikar(float baslangicY)
    {
        if (yapraklar == null || yapraklar.Length == 0) return;

        SpriteRenderer sr;
        if (havuz.Count > 0)
        {
            sr = havuz.Dequeue();
            sr.gameObject.SetActive(true);
        }
        else
        {
            GameObject o = new GameObject("Yaprak");
            o.transform.SetParent(transform, false);
            sr = o.AddComponent<SpriteRenderer>();
            sr.sortingOrder = siralama;
            if (isiksizMateryal != null) sr.sharedMaterial = isiksizMateryal;
        }
        sr.sprite = yapraklar[Random.Range(0, yapraklar.Length)];

        aktifler.Add(new Yaprak
        {
            sr = sr,
            x = Random.Range(-genislik, genislik),
            y = baslangicY,
            hiz = Random.Range(enAzHiz, enFazlaHiz),
            faz = Random.Range(0f, 6.28f)
        });
    }
}
