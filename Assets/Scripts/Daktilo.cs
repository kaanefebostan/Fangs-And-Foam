using System.Collections;
using UnityEngine;
using TMPro;

// Yazıyı harf harf gösterir (daktilo efekti). Yazının olduğu objeye takılır.
[RequireComponent(typeof(TMP_Text))]
public class Daktilo : MonoBehaviour
{
    public float harfSuresi = 0.035f;   // Her harf arası kaç saniye
    public int sesAraligi = 2;          // Kaç harfte bir konuşma sesi çalsın

    public bool YaziyorMu { get; private set; }

    TMP_Text yazi;
    Coroutine rutin;

    void Awake()
    {
        yazi = GetComponent<TMP_Text>();
    }

    // Yeni bir cümleyi harf harf yaz
    public void Yaz(string metin)
    {
        if (rutin != null) StopCoroutine(rutin);
        rutin = StartCoroutine(YazRutini(metin));
    }

    // Yazıyı sil, yazmayı durdur
    public void Temizle()
    {
        if (rutin != null) StopCoroutine(rutin);
        YaziyorMu = false;
        yazi.text = "";
        yazi.maxVisibleCharacters = 99999;
    }

    IEnumerator YazRutini(string metin)
    {
        YaziyorMu = true;
        yazi.text = metin;
        yazi.maxVisibleCharacters = 0;
        yazi.ForceMeshUpdate();

        int toplam = yazi.textInfo.characterCount;
        for (int i = 1; i <= toplam; i++)
        {
            yazi.maxVisibleCharacters = i;

            char harf = yazi.textInfo.characterInfo[i - 1].character;
            if (harf != ' ' && i % sesAraligi == 0) Sesler.Konusma();

            yield return new WaitForSeconds(harfSuresi);
        }

        yazi.maxVisibleCharacters = 99999;
        YaziyorMu = false;
    }
}