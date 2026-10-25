using System.Collections.Generic;
using UnityEngine;

// Bardakta kahve varken üstünden küçük beyaz buhar pikselleri yükselir.
// Boş bir objeye eklenir; "Kahve Doldurma" kutusuna makine objesini sürükle.
public class Buhar : MonoBehaviour
{
    public KahveDoldurma kahveDoldurma;

    [Header("Görünüm")]
    public Color renk = new Color(1f, 1f, 1f, 0.45f);
    public float pikselBoyu = 0.02f;     // Bir buhar parçasının boyu (0.02 = 2 piksel)
    public int siralama = 12;            // Order in Layer (bardağın önünde)
    public Material isiksizMateryal;     // Isiksiz materyal (boş kalabilir)

    [Header("Hareket")]
    public float aralik = 0.18f;         // Kaç saniyede bir yeni parça çıkar
    public float omur = 1.6f;            // Bir parça kaç saniye yaşar
    public float yukselme = 0.35f;       // Ömrü boyunca ne kadar yükselir
    public float genislik = 0.1f;        // Bardağın ortasından ne kadar sağa-sola dağılır

    class Parca
    {
        public SpriteRenderer sr;
        public Vector3 baslangic;
        public float yas;
        public float faz;
    }

    readonly List<Parca> parcalar = new List<Parca>();
    readonly Queue<SpriteRenderer> havuz = new Queue<SpriteRenderer>();
    Sprite kare;
    float sayac;

    void Start()
    {
        kare = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    void Update()
    {
        if (kahveDoldurma == null) return;

        // Kahve varsa yeni parça çıkar
        if (kahveDoldurma.KahveVar)
        {
            sayac -= Time.deltaTime;
            if (sayac <= 0f)
            {
                sayac = aralik;
                ParcaCikar();
            }
        }

        // Parçalar yükselir, sağa sola salınır ve kaybolur
        for (int i = parcalar.Count - 1; i >= 0; i--)
        {
            Parca p = parcalar[i];
            p.yas += Time.deltaTime;
            float o = p.yas / omur;
            if (o >= 1f)
            {
                p.sr.gameObject.SetActive(false);
                havuz.Enqueue(p.sr);
                parcalar.RemoveAt(i);
                continue;
            }

            float x = Mathf.Sin(p.yas * 3f + p.faz) * 0.03f;
            Vector3 konum = p.baslangic + new Vector3(x, yukselme * o, 0f);
            // Piksel piksel hareket etsin
            konum.x = Mathf.Round(konum.x * 100f) / 100f;
            konum.y = Mathf.Round(konum.y * 100f) / 100f;
            p.sr.transform.position = konum;

            Color c = renk;
            c.a = renk.a * (1f - o);
            p.sr.color = c;
        }
    }

    void ParcaCikar()
    {
        SpriteRenderer sr;
        if (havuz.Count > 0)
        {
            sr = havuz.Dequeue();
            sr.gameObject.SetActive(true);
        }
        else
        {
            GameObject o = new GameObject("BuharParcasi");
            o.transform.SetParent(transform, false);
            sr = o.AddComponent<SpriteRenderer>();
            sr.sprite = kare;
            sr.sortingOrder = siralama;
            if (isiksizMateryal != null) sr.sharedMaterial = isiksizMateryal;
            o.transform.localScale = Vector3.one * pikselBoyu;
        }

        float x = kahveDoldurma.BardakX + Random.Range(-genislik, genislik);
        float y = kahveDoldurma.BardakUstY;
        parcalar.Add(new Parca
        {
            sr = sr,
            baslangic = new Vector3(x, y, 0f),
            yas = 0f,
            faz = Random.Range(0f, 6.28f)
        });
    }
}
