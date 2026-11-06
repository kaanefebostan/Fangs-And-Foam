using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Demo standı için yardımcılar:
// - F5: kaydı siler, oyunu menüye döndürür (yeni oyuncu için temiz başlangıç)
// - Shift + F5: rekorları da siler
// - Uzun süre kimse dokunmazsa oyun kendiliğinden menüye döner
// Sahnede boş bir objeye eklenir.
public class DemoAraclari : MonoBehaviour
{
    [Header("Kendiliğinden menüye dönüş")]
    public bool otomatikDonus = true;
    public float bosBeklemeSuresi = 60f;   // Kaç saniye dokunulmazsa menüye dönsün
    public bool donunceKaydiSil = true;    // Dönünce "Devam Et" kaydı da silinsin

    float bosSayac;

    void Update()
    {
        var klavye = Keyboard.current;
        var fare = Mouse.current;

        // F5: sıfırla
        if (klavye != null && klavye.f5Key.wasPressedThisFrame)
        {
            if (klavye.shiftKey.isPressed) Istatistik.RekorlariSil();
            Sifirla(true);
            return;
        }

        // Herhangi bir hareket var mı
        bool hareket = false;
        if (klavye != null && klavye.anyKey.wasPressedThisFrame) hareket = true;
        if (fare != null && (fare.leftButton.wasPressedThisFrame || fare.rightButton.wasPressedThisFrame
                             || fare.delta.ReadValue().sqrMagnitude > 0.5f)) hareket = true;

        // Menüdeyken saymaya gerek yok
        if (hareket || MenuYoneticisi.Acik)
        {
            bosSayac = 0f;
            return;
        }

        if (!otomatikDonus) return;
        bosSayac += Time.unscaledDeltaTime;
        if (bosSayac >= bosBeklemeSuresi) Sifirla(donunceKaydiSil);
    }

    void Sifirla(bool kaydiSil)
    {
        if (kaydiSil)
        {
            PlayerPrefs.DeleteKey(GunYoneticisi.KayitAnahtari);
            PlayerPrefs.DeleteKey("kafeGuveni");
            PlayerPrefs.DeleteKey("sorulanSorular");
            PlayerPrefs.Save();
        }
        Time.timeScale = 1f;   // Duraklatma açıksa zaman dursun kalmasın
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
