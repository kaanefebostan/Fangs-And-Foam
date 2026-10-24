using UnityEngine;
using UnityEngine.InputSystem;

// Fareyle üzerine gelince obje birkaç piksel yukarı kalkar (tıklanabilir olduğunu gösterir).
[RequireComponent(typeof(Collider2D))]
public class UzerineGelince : MonoBehaviour
{
    public float kalkma = 0.02f;   // Ne kadar kalksın (0.01 = 1 piksel)
    public float hiz = 15f;        // Ne kadar hızlı kalksın

    Collider2D alan;
    Vector3 asilKonum;

    void Start()
    {
        alan = GetComponent<Collider2D>();
        asilKonum = transform.position;
    }

    void Update()
    {
        bool uzerinde = false;
        var fare = Mouse.current;
        bool engel = MenuYoneticisi.Acik || DuraklatmaMenusu.Acik || TarifKitabi.Acik;

        if (fare != null && !engel && Camera.main != null)
        {
            Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());
            uzerinde = alan.OverlapPoint(nokta - (Vector2)(transform.position - asilKonum));
        }

        Vector3 hedef = asilKonum + (uzerinde ? Vector3.up * kalkma : Vector3.zero);
        transform.position = Vector3.Lerp(transform.position, hedef, Time.unscaledDeltaTime * hiz);
    }
}