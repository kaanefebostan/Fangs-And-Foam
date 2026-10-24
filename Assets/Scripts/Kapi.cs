using System.Collections;
using UnityEngine;

// Kapının açılıp kapanma animasyonu.
// Müşteri içeri girerken AcVeKapat() çağrılır.
public class Kapi : MonoBehaviour
{
    [Header("Resimler (gelince buraya sürükle)")]
    public Sprite kapaliResim;
    public Sprite acikResim;
    public Sprite[] acilmaKareleri;      // Kapalıdan açığa doğru, sırayla (boş kalabilir)

    [Header("Ayarlar")]
    public float kareSuresi = 0.06f;     // Her kare kaç saniye görünür
    public float acikKalmaSuresi = 1f;   // Kapı kaç saniye açık kalır

    SpriteRenderer gorsel;
    Color normalRenk;

    void Awake()
    {
        gorsel = GetComponent<SpriteRenderer>();
        normalRenk = gorsel.color;
        if (kapaliResim != null) gorsel.sprite = kapaliResim;
    }

    public void AcVeKapat()
    {
        StopAllCoroutines();
        StartCoroutine(AcKapat());
    }

    IEnumerator AcKapat()
    {
        // Açılma kareleri
        if (acilmaKareleri != null)
        {
            foreach (Sprite kare in acilmaKareleri)
            {
                if (kare == null) continue;
                gorsel.sprite = kare;
                yield return new WaitForSeconds(kareSuresi);
            }
        }

        // Açık hal (resim yoksa siyah boşluk)
        if (acikResim != null) gorsel.sprite = acikResim;
        else gorsel.color = Color.black;

        yield return new WaitForSeconds(acikKalmaSuresi);

        // Kapanma: aynı kareler tersten
        gorsel.color = normalRenk;
        if (acilmaKareleri != null)
        {
            for (int i = acilmaKareleri.Length - 1; i >= 0; i--)
            {
                if (acilmaKareleri[i] == null) continue;
                gorsel.sprite = acilmaKareleri[i];
                yield return new WaitForSeconds(kareSuresi);
            }
        }

        if (kapaliResim != null) gorsel.sprite = kapaliResim;
    }
}