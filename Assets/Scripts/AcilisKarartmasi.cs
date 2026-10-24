using System.Collections;
using UnityEngine;

// Sahne her açıldığında ekran siyahtan yavaşça açılır.
[RequireComponent(typeof(SpriteRenderer))]
public class AcilisKarartmasi : MonoBehaviour
{
    public float sure = 0.8f;

    IEnumerator Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;

        float t = 0f;
        while (t < sure)
        {
            t += Time.unscaledDeltaTime;
            c.a = 1f - Mathf.Clamp01(t / sure);
            sr.color = c;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}