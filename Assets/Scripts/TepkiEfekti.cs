using System.Collections;
using UnityEngine;

// Canavarın başının üstünden tepki efekti çıkarır:
// memnunsa kalpler, kızgınsa duman pufları yükselip kaybolur.
// Boş bir objeye eklenir; Musteri kodu "Oynat" ile çağırır.
public class TepkiEfekti : MonoBehaviour
{
    [Header("Resimler")]
    public Sprite[] kalpler;             // Kalp resimleri
    public Sprite[] dumanlar;            // Duman pufu resimleri
    public int siralama = 25;            // Order in Layer (canavarın önünde)
    public Material isiksizMateryal;     // Isiksiz materyal (boş kalabilir)

    [Header("Hareket")]
    public float omur = 1.3f;            // Bir parça kaç saniye yaşar
    public float yukselme = 0.5f;        // Ömrü boyunca ne kadar yükselir
    public float dagilma = 0.35f;        // Başın ortasından ne kadar sağa-sola dağılır
    public float arka = 0.12f;           // Parçalar arası kaç saniye

    // iyi = true: kalp, false: duman. adet: kaç parça
    public void Oynat(bool iyi, Vector3 konum, int adet)
    {
        Sprite[] liste = iyi ? kalpler : dumanlar;
        if (liste == null || liste.Length == 0) return;

        for (int i = 0; i < adet; i++)
        {
            Sprite s = liste[Random.Range(0, liste.Length)];
            StartCoroutine(Parca(s, konum, i * arka, iyi));
        }
    }

    IEnumerator Parca(Sprite resim, Vector3 merkez, float gecikme, bool kalp)
    {
        if (gecikme > 0f) yield return new WaitForSeconds(gecikme);

        GameObject o = new GameObject(kalp ? "Kalp" : "Duman");
        o.transform.SetParent(transform, false);
        SpriteRenderer sr = o.AddComponent<SpriteRenderer>();
        sr.sprite = resim;
        sr.sortingOrder = siralama;
        if (isiksizMateryal != null) sr.sharedMaterial = isiksizMateryal;

        Vector3 baslangic = merkez + new Vector3(Random.Range(-dagilma, dagilma), Random.Range(-0.05f, 0.05f), 0f);
        float faz = Random.Range(0f, 6.28f);
        float t = 0f;
        while (t < omur)
        {
            t += Time.deltaTime;
            float o2 = Mathf.Clamp01(t / omur);

            // Kalpler sağa sola salınarak, dumanlar düz ve yavaşlayarak yükselir
            float x = kalp ? Mathf.Sin(t * 5f + faz) * 0.05f : 0f;
            float y = kalp ? yukselme * o2 : yukselme * (1f - (1f - o2) * (1f - o2));
            Vector3 p = baslangic + new Vector3(x, y, 0f);
            p.x = Mathf.Round(p.x * 100f) / 100f;   // piksel piksel
            p.y = Mathf.Round(p.y * 100f) / 100f;
            o.transform.position = p;

            Color c = Color.white;
            c.a = o2 < 0.6f ? 1f : 1f - (o2 - 0.6f) / 0.4f;   // sonda kaybolur
            sr.color = c;
            yield return null;
        }
        Destroy(o);
    }
}
