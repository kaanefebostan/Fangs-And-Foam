using UnityEngine;
using UnityEngine.InputSystem;

// Fare üstüne gelince objenin etrafında ince renkli bir çerçeve çıkar, üstünde de adı yazar.
// Rehber, sıradaki tıklanacak objeyi göstermek için çerçeveyi yanıp söndürebilir (Isaretli).
// Objede Sprite Renderer ve bir Collider 2D (Box / Polygon) olmalı.
[RequireComponent(typeof(SpriteRenderer))]
public class SecilebilirVurgu : MonoBehaviour
{
    [Header("İsim")]
    [Tooltip("Üstüne gelince yazacak isim. Boş bırakılırsa malzemenin adı yazar.")]
    public string isim = "";
    public bool isimGoster = true;

    [Header("Çerçeve")]
    public Color cerceveRengi = new Color(0.91f, 0.15f, 0.17f);   // #E8262B kırmızı
    public int kalinlik = 1;              // Kaç piksel kalınlığında
    public float pikselBirim = 100f;      // Pixels Per Unit (resimlerin ayarıyla aynı)
    public Material cerceveMalzemesi;     // "Cerceve" malzemesi (GUI/Text Shader). Boş kalabilir.

    // Rehber bunu açınca çerçeve fare üstünde olmasa da yanıp söner
    public bool Isaretli { get; set; }

    SpriteRenderer gorsel;
    Collider2D carpisma;
    SpriteRenderer[] parcalar;

    static readonly Vector2[] yonler =
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1)
    };

    void Start()
    {
        gorsel = GetComponent<SpriteRenderer>();
        carpisma = GetComponent<Collider2D>();

        // İsim yazılmadıysa malzemenin adını kullan
        if (string.IsNullOrEmpty(isim))
        {
            Malzeme m = GetComponent<Malzeme>();
            if (m != null) isim = KahveDoldurma.GorunenAd(m.malzemeAdi);
        }

        // Malzeme verilmediyse düz renk çizen hazır gölgelendiriciyi dene
        Material malzeme = cerceveMalzemesi;
        if (malzeme == null)
        {
            Shader s = Shader.Find("GUI/Text Shader");
            malzeme = s != null ? new Material(s) : gorsel.sharedMaterial;
        }

        // Objenin arkasına, her yöne 1 piksel kaydırılmış düz renkli kopyalar koyulur.
        // Kenardan taşan kısımları çerçeve gibi görünür.
        parcalar = new SpriteRenderer[yonler.Length];
        for (int i = 0; i < yonler.Length; i++)
        {
            GameObject g = new GameObject("Cerceve");
            g.transform.SetParent(transform, false);
            SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = malzeme;
            sr.sortingLayerID = gorsel.sortingLayerID;
            sr.sortingOrder = gorsel.sortingOrder - 1;
            sr.maskInteraction = gorsel.maskInteraction;
            sr.enabled = false;
            parcalar[i] = sr;
        }
    }

    void Update()
    {
        if (parcalar == null) return;

        bool engel = TarifKitabi.Acik || DuraklatmaMenusu.Acik || Poster.Engelli || MenuYoneticisi.Acik;

        bool ustunde = false;
        var fare = Mouse.current;
        if (!engel && fare != null && carpisma != null && carpisma.enabled && gorsel.enabled && Camera.main != null)
        {
            Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());
            ustunde = carpisma.OverlapPoint(nokta);
        }

        float saydamlik = 0f;
        if (ustunde) saydamlik = 1f;
        else if (Isaretli && !engel) saydamlik = 0.3f + 0.7f * Mathf.PingPong(Time.unscaledTime * 2.5f, 1f);
        Ciz(saydamlik);

        // İsim yazısı
        if (IsimEtiketi.Ornek != null)
        {
            if (ustunde && isimGoster && !string.IsNullOrEmpty(isim))
                IsimEtiketi.Ornek.Goster(this, isim, new Vector3(gorsel.bounds.center.x, gorsel.bounds.max.y, 0f));
            else
                IsimEtiketi.Ornek.Gizle(this);
        }
    }

    void Ciz(float saydamlik)
    {
        bool goster = saydamlik > 0.01f && gorsel.enabled && gorsel.sprite != null;

        Vector3 olcek = transform.lossyScale;
        float px = kalinlik / pikselBirim;
        float ox = px / Mathf.Max(0.0001f, Mathf.Abs(olcek.x));
        float oy = px / Mathf.Max(0.0001f, Mathf.Abs(olcek.y));

        Color renk = cerceveRengi;
        renk.a = saydamlik * gorsel.color.a;

        for (int i = 0; i < parcalar.Length; i++)
        {
            SpriteRenderer sr = parcalar[i];
            if (sr.enabled != goster) sr.enabled = goster;
            if (!goster) continue;

            // Obje resmini değiştirirse (kapı açılır, buton parlar) çerçeve de değişsin
            sr.sprite = gorsel.sprite;
            sr.flipX = gorsel.flipX;
            sr.flipY = gorsel.flipY;
            sr.sortingLayerID = gorsel.sortingLayerID;
            sr.sortingOrder = gorsel.sortingOrder - 1;
            sr.color = renk;
            sr.transform.localPosition = new Vector3(yonler[i].x * ox, yonler[i].y * oy, 0f);
        }
    }

    void OnDisable()
    {
        if (parcalar != null)
            foreach (SpriteRenderer sr in parcalar) if (sr != null) sr.enabled = false;
        if (IsimEtiketi.Ornek != null) IsimEtiketi.Ornek.Gizle(this);
    }
}
