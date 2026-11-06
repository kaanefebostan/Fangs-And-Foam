using UnityEngine;

// Bir oyun boyunca tutulan istatistikler ve kayıtlı rekorlar.
// Oyun sonunda özet ve "YENİ REKOR!" yazısı buradan gelir.
public static class Istatistik
{
    public static int Servis;       // Servis edilen kahve
    public static int Mukemmel;     // Mükemmel kahve
    public static int EnUzunSeri;   // En uzun mükemmel serisi
    public static int Gun;          // Ulaşılan gün

    const string RekorGunAnahtari = "rekorGun";
    const string RekorSeriAnahtari = "rekorSeri";
    const string RekorMukemmelAnahtari = "rekorMukemmel";

    static bool kaydedildi = false;

    public static void Sifirla()
    {
        Servis = 0;
        Mukemmel = 0;
        EnUzunSeri = 0;
        Gun = 1;
        kaydedildi = false;
    }

    // Oyun sonu özeti; rekor kırıldıysa kaydeder. kazandi: 5 günü bitirdiyse true
    public static string OzetVeRekor(bool kazandi)
    {
        int rekorGun = PlayerPrefs.GetInt(RekorGunAnahtari, 0);
        int rekorSeri = PlayerPrefs.GetInt(RekorSeriAnahtari, 0);
        int rekorMukemmel = PlayerPrefs.GetInt(RekorMukemmelAnahtari, 0);

        // Kazandıysa son günü de bitirmiş sayılır
        int gunPuani = kazandi ? Gun + 1 : Gun;

        bool yeniRekor = gunPuani > rekorGun || EnUzunSeri > rekorSeri || Mukemmel > rekorMukemmel;

        if (!kaydedildi)
        {
            if (gunPuani > rekorGun) PlayerPrefs.SetInt(RekorGunAnahtari, gunPuani);
            if (EnUzunSeri > rekorSeri) PlayerPrefs.SetInt(RekorSeriAnahtari, EnUzunSeri);
            if (Mukemmel > rekorMukemmel) PlayerPrefs.SetInt(RekorMukemmelAnahtari, Mukemmel);
            PlayerPrefs.Save();
            kaydedildi = true;
        }

        string metin = (kazandi ? "Bütün günler tamamlandı" : Gun + ". günde kapandı")
                       + "\nServis edilen kahve: " + Servis
                       + "\nMükemmel: " + Mukemmel
                       + "\nEn uzun seri: x" + EnUzunSeri;
        if (yeniRekor) metin += "\n<color=#FFD34D>YENİ REKOR!</color>";
        return metin;
    }

    // Tek satırlık kısa özet (dar ekranlar için). Rekor kırıldıysa ikinci satırda yazar.
    public static string KisaOzet(bool kazandi)
    {
        string uzun = OzetVeRekor(kazandi);   // rekoru kaydetsin
        bool yeniRekor = uzun.Contains("YENİ REKOR");
        string metin = (kazandi ? "" : Gun + ". gün  -  ")
                       + "Servis: " + Servis + "  -  Mükemmel: " + Mukemmel + "  -  Seri: x" + EnUzunSeri;
        if (yeniRekor) metin += "\n<color=#FFD34D>YENİ REKOR!</color>";
        return metin;
    }

    // Menüde gösterilecek rekor satırı (hiç oynanmadıysa boş)
    public static string RekorMetni()
    {
        int gun = PlayerPrefs.GetInt(RekorGunAnahtari, 0);
        if (gun == 0) return "";
        int seri = PlayerPrefs.GetInt(RekorSeriAnahtari, 0);
        int mukemmel = PlayerPrefs.GetInt(RekorMukemmelAnahtari, 0);
        string gunMetni = gun > 5 ? "5 gün tamam" : gun + ". gün";
        return "REKOR:  " + gunMetni + "  -  " + mukemmel + " mükemmel  -  seri x" + seri;
    }

    // Rekorları siler (F5 ile değil, Shift+F5 ile)
    public static void RekorlariSil()
    {
        PlayerPrefs.DeleteKey(RekorGunAnahtari);
        PlayerPrefs.DeleteKey(RekorSeriAnahtari);
        PlayerPrefs.DeleteKey(RekorMukemmelAnahtari);
        PlayerPrefs.Save();
    }
}
