using System.Collections;
using UnityEngine;

// E ile servis edince: dolu bardağın bir kopyası tezgahta canavara doğru kayar ve kaybolur.
// Gerçek bardak o an boşalır, böylece "kahveyi verdik" hissi olur.
// Boş bir objeye eklenir; KahveDoldurma kodu "Oynat" ile çağırır.
public class ServisAnimasyonu : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public SpriteRenderer bardakResmi;   // Bardak objesi
    public SpriteMask bardakMaske;       // BardakMaske objesi (bardağın iç şekli)
    public Musteri musteri;              // Bardak canavara doğru gider

    [Header("Ayarlar")]
    public float kaymaSuresi = 0.45f;    // Canavara kadar kaç saniyede gider
    public float kaybolmaSuresi = 0.25f; // Vardıktan sonra kaç saniyede kaybolur
    public float ziplama = 0.12f;        // Giderken ne kadar havalanır
    public float canavaraUzaklik = 0.6f; // Canavarın ortasından ne kadar önde durur

    public void Oynat(Color siviRengi)
    {
        if (bardakResmi == null || musteri == null) return;
        StartCoroutine(Animasyon(siviRengi));
    }

    IEnumerator Animasyon(Color siviRengi)
    {
        // Bardağın kopyasını yap
        GameObject kok = new GameObject("ServisBardagi");
        kok.transform.position = bardakResmi.transform.position;

        SpriteRenderer cam = new GameObject("Cam").AddComponent<SpriteRenderer>();
        cam.transform.SetParent(kok.transform, false);
        cam.sprite = bardakResmi.sprite;
        cam.sharedMaterial = bardakResmi.sharedMaterial;
        cam.sortingOrder = bardakResmi.sortingOrder;
        cam.transform.localScale = bardakResmi.transform.lossyScale;

        SpriteRenderer ic = null;
        if (bardakMaske != null && bardakMaske.sprite != null)
        {
            ic = new GameObject("Ic").AddComponent<SpriteRenderer>();
            ic.transform.SetParent(kok.transform, false);
            ic.transform.position = bardakMaske.transform.position;
            ic.transform.localScale = bardakMaske.transform.lossyScale;
            ic.sprite = bardakMaske.sprite;
            ic.color = siviRengi;
            ic.sortingOrder = bardakResmi.sortingOrder - 1;
        }

        // Tezgah üstünde canavara doğru kay, hafifçe havalan
        Vector3 baslangic = kok.transform.position;
        Vector3 hedef = new Vector3(musteri.transform.position.x + canavaraUzaklik, baslangic.y, baslangic.z);
        float t = 0f;
        while (t < kaymaSuresi)
        {
            t += Time.deltaTime;
            float o = Mathf.Clamp01(t / kaymaSuresi);
            float yumusak = o * o * (3f - 2f * o);
            Vector3 p = Vector3.Lerp(baslangic, hedef, yumusak) + Vector3.up * Mathf.Sin(o * Mathf.PI) * ziplama;
            p.x = Mathf.Round(p.x * 100f) / 100f;   // piksel piksel
            p.y = Mathf.Round(p.y * 100f) / 100f;
            kok.transform.position = p;
            yield return null;
        }

        // Kaybol
        t = 0f;
        Color camRenk = cam.color;
        Color icRenk = ic != null ? ic.color : Color.white;
        while (t < kaybolmaSuresi)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / kaybolmaSuresi);
            camRenk.a = a;
            cam.color = camRenk;
            if (ic != null)
            {
                Color c = icRenk;
                c.a = icRenk.a * a;
                ic.color = c;
            }
            yield return null;
        }
        Destroy(kok);
    }
}
