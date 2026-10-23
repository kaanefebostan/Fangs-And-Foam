using UnityEngine;

// Tezgahtaki bir malzeme şişesi. Oyuncu tıklayınca KahveDoldurma bunu okur.
// Bu kodu bir objeye takınca tıklanabilmesi için Box Collider 2D otomatik eklenir.
[RequireComponent(typeof(BoxCollider2D))]
public class Malzeme : MonoBehaviour
{
    public string malzemeAdi = "Kahve";   // Birebir şunlardan biri: Kahve, Sut, Su, Surup
    public bool dokulur = true;            // Kahve, Sut, Su için işaretli; Surup için işaretsiz
    public Color renk = new Color(0.3f, 0.15f, 0.05f); // Bardakta görünecek renk
}