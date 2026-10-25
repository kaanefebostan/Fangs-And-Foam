using UnityEngine;
using UnityEngine.InputSystem;

// Fare üstüne gelince resim "parlak" haliyle değişir, fare gidince eski haline döner.
// Objede Sprite Renderer ve Box Collider 2D olmalı.
public class UzerineGelinceParla : MonoBehaviour
{
    public Sprite parlakResim;   // Fare üstündeyken görünecek resim

    SpriteRenderer gorsel;
    Sprite normalResim;
    Collider2D carpisma;

    void Start()
    {
        gorsel = GetComponent<SpriteRenderer>();
        carpisma = GetComponent<Collider2D>();
        if (gorsel != null) normalResim = gorsel.sprite;
    }

    void Update()
    {
        var fare = Mouse.current;
        if (fare == null || gorsel == null || carpisma == null || Camera.main == null) return;

        Vector2 nokta = Camera.main.ScreenToWorldPoint(fare.position.ReadValue());
        bool ustunde = carpisma.OverlapPoint(nokta);

        Sprite hedef = (ustunde && parlakResim != null) ? parlakResim : normalResim;
        if (gorsel.sprite != hedef) gorsel.sprite = hedef;
    }

    void OnDisable()
    {
        // Kitap kapanınca parlak halde kalmasın
        if (gorsel != null && normalResim != null) gorsel.sprite = normalResim;
    }
}
