using UnityEngine;
using TMPro;

// Menüdeki tek bir buton. Box Collider 2D ile tıklanır.
// Yazı butonuysa (TextMeshPro): seçilince rengi değişir ve hafifçe büyür.
// Resim butonuysa: seçilince "parlak" resim görünür ve hafifçe yanıp söner.
public class MenuButonu : MonoBehaviour
{
    [Header("Resim butonu için (boş kalabilir)")]
    public SpriteRenderer parlakResim;   // Seçilince görünen parlak hali

    [Header("Yazı butonu için")]
    public Color normalRenk = new Color(0.96f, 0.9f, 0.78f);   // krem
    public Color seciliRenk = new Color(0.87f, 0.44f, 0.15f);  // turuncu
    public Color pasifRenk = new Color(0.4f, 0.37f, 0.35f);    // soluk gri
    public float buyume = 1.1f;          // Seçilince ne kadar büyür

    // Kullanılamıyorsa (örn. kayıt yokken "Devam Et") soluk görünür ve tıklanmaz
    public bool Kullanilabilir { get; set; } = true;

    TMP_Text yazi;
    Collider2D carpisma;
    Vector3 asilBoyut;
    bool hazir = false;
    bool secili = false;

    void Hazirla()
    {
        if (hazir) return;
        yazi = GetComponent<TMP_Text>();
        carpisma = GetComponent<Collider2D>();
        asilBoyut = transform.localScale;
        hazir = true;
    }

    // Seçili mi değil mi
    public void Vurgula(bool seciliMi)
    {
        Hazirla();
        secili = seciliMi && Kullanilabilir;

        if (yazi != null) yazi.color = !Kullanilabilir ? pasifRenk : (secili ? seciliRenk : normalRenk);
        if (parlakResim != null) parlakResim.enabled = secili;
        if (!secili) transform.localScale = asilBoyut;
    }

    // Fare bu butonun üstünde mi
    public bool FareUstunde(Vector2 nokta)
    {
        Hazirla();
        return Kullanilabilir && carpisma != null && carpisma.OverlapPoint(nokta);
    }

    void Update()
    {
        if (!secili) return;

        // Yazı butonu seçiliyken hafifçe büyür
        if (yazi != null) transform.localScale = asilBoyut * buyume;

        // Parlak resim hafifçe yanıp söner
        if (parlakResim != null)
        {
            float t = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 6f);
            parlakResim.color = new Color(1f, 1f, 1f, t);
        }
    }
}
