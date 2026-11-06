using UnityEngine;
using TMPro;

// Fare bir objenin üstüne gelince objenin adını gösteren tek yazı.
// Sahnede bir tane "Text - TextMeshPro" objesine eklenir. SecilebilirVurgu bunu kullanır.
[RequireComponent(typeof(TMP_Text))]
public class IsimEtiketi : MonoBehaviour
{
    public static IsimEtiketi Ornek;

    public float yukari = 0.1f;   // Objenin üst kenarından ne kadar yukarıda dursun

    TMP_Text yazi;
    SecilebilirVurgu sahip;   // Şu an kimin adı yazıyor

    void Awake()
    {
        Ornek = this;
        yazi = GetComponent<TMP_Text>();
        yazi.text = "";
    }

    public void Goster(SecilebilirVurgu kim, string metin, Vector3 ustNokta)
    {
        sahip = kim;
        if (yazi.text != metin) yazi.text = metin;

        // Ekranın dışına taşmasın (ekran x: -3.2..3.2, y: -1.8..1.8)
        Vector3 p = ustNokta + Vector3.up * yukari;
        p.x = Mathf.Clamp(p.x, -2.8f, 2.8f);
        p.y = Mathf.Clamp(p.y, -1.6f, 1.65f);
        p.z = transform.position.z;
        transform.position = p;
    }

    public void Gizle(SecilebilirVurgu kim)
    {
        if (sahip != kim) return;   // Başka bir objenin adı yazıyorsa dokunma
        sahip = null;
        if (yazi.text != "") yazi.text = "";
    }
}
