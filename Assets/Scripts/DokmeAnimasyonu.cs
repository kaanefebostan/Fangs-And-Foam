using System.Collections;
using UnityEngine;

// Dökerken şişe bardağın üstüne gelir, eğilir, ağzından bardağa sıvı akar.
// Bırakınca şişe yerine döner.
public class DokmeAnimasyonu : MonoBehaviour
{
    [Header("Sahnedeki objeler")]
    public KahveDoldurma kahveDoldurma;
    public SpriteRenderer akis;          // İnce sıvı akışı (bir kare)
    public ParticleSystem damlalar;      // Bardakta sıçrayan damlalar (boş kalabilir)

    [Header("Ayarlar")]
    public Vector2 bardagaGoreKonum = new Vector2(0.3f, 0.35f); // Şişe bardağın neresinde dursun
    public float aci = 110f;             // Ne kadar eğilsin
    public float agizUzakligi = 0.25f;   // Şişenin ortasından ağzına uzaklık
    public float hiz = 12f;              // Ne kadar hızlı gidip gelsin
    public float akisKalinligi = 0.03f;  // Akışın kalınlığı (0.01 = 1 piksel)
    public int tasinirkenSira = 7;       // Taşınırken Order in Layer (bardağın önünde)

    Malzeme tasinan;
    SpriteRenderer tasinanGorsel;
    UzerineGelince tasinanUzerine;
    Vector3 evKonum;
    Quaternion evDonus;
    int evSira;

    // Yerine dönmekte olan şişe
    Malzeme donen;
    SpriteRenderer donenGorsel;
    UzerineGelince donenUzerine;
    Vector3 donenKonum;
    Quaternion donenDonus;
    int donenSira;

    void Start()
    {
        if (akis != null) akis.gameObject.SetActive(false);
    }

    void Update()
    {
        // Şu an bardağa taşınması gereken şişe hangisi
        Malzeme m = kahveDoldurma.Dokuyor ? kahveDoldurma.Secili : null;
        if (m != null && !m.bardagaTasinsin) m = null;

        if (tasinan != null && tasinan != m) Birak();
        if (m != null && tasinan == null) Al(m);

        if (tasinan == null)
        {
            if (akis != null) akis.gameObject.SetActive(false);
            if (damlalar != null && damlalar.isPlaying) damlalar.Stop();
            return;
        }

        // Şişeyi bardağın üstüne götür ve eğ
        Vector3 hedef = new Vector3(kahveDoldurma.BardakX + bardagaGoreKonum.x,
                                    kahveDoldurma.BardakUstY + bardagaGoreKonum.y,
                                    evKonum.z);
        Quaternion hedefDonus = Quaternion.Euler(0f, 0f, aci);
        tasinan.transform.position = Vector3.Lerp(tasinan.transform.position, hedef, Time.deltaTime * hiz);
        tasinan.transform.rotation = Quaternion.Slerp(tasinan.transform.rotation, hedefDonus, Time.deltaTime * hiz);

        // Şişe yeterince eğildiyse akış başlasın
        bool egildi = Quaternion.Angle(tasinan.transform.rotation, hedefDonus) < 20f;
        AkisiCiz(egildi);
    }

    void AkisiCiz(bool goster)
    {
        Vector3 agiz = tasinan.transform.position + tasinan.transform.rotation * new Vector3(0f, agizUzakligi, 0f);
        float siviUst = kahveDoldurma.SiviUstY;
        float boy = agiz.y - siviUst;

        if (akis != null)
        {
            akis.gameObject.SetActive(goster && boy > 0f);
            if (goster && boy > 0f)
            {
                akis.transform.position = new Vector3(agiz.x, (agiz.y + siviUst) / 2f, 0f);
                akis.transform.localScale = new Vector3(akisKalinligi, boy, 1f);
                Color c = tasinan.renk;
                c.a = 1f;
                akis.color = c;
            }
        }

        if (damlalar != null)
        {
            if (goster)
            {
                damlalar.transform.position = new Vector3(agiz.x, siviUst, 0f);
                var ana = damlalar.main;
                Color c = tasinan.renk;
                c.a = 1f;
                ana.startColor = c;
                if (!damlalar.isPlaying) damlalar.Play();
            }
            else if (damlalar.isPlaying)
            {
                damlalar.Stop();
            }
        }
    }

    void Al(Malzeme m)
    {
        // Yerine dönmekte olan bir şişe varsa onu hemen yerine koy
        StopAllCoroutines();
        if (donen != null) DoneniYerlestir();

        tasinan = m;
        tasinanGorsel = m.GetComponent<SpriteRenderer>();
        tasinanUzerine = m.GetComponent<UzerineGelince>();
        if (tasinanUzerine != null) tasinanUzerine.enabled = false;   // Taşırken kalkma efekti karışmasın

        evKonum = m.transform.position;
        evDonus = m.transform.rotation;
        evSira = tasinanGorsel != null ? tasinanGorsel.sortingOrder : 0;
        if (tasinanGorsel != null) tasinanGorsel.sortingOrder = tasinirkenSira;
    }

    void Birak()
    {
        donen = tasinan;
        donenGorsel = tasinanGorsel;
        donenUzerine = tasinanUzerine;
        donenKonum = evKonum;
        donenDonus = evDonus;
        donenSira = evSira;
        tasinan = null;

        StopAllCoroutines();
        StartCoroutine(GeriDon());
    }

    IEnumerator GeriDon()
    {
        float t = 0f;
        while (t < 0.35f && donen != null)
        {
            t += Time.deltaTime;
            donen.transform.position = Vector3.Lerp(donen.transform.position, donenKonum, Time.deltaTime * hiz);
            donen.transform.rotation = Quaternion.Slerp(donen.transform.rotation, donenDonus, Time.deltaTime * hiz);
            yield return null;
        }
        if (donen != null) DoneniYerlestir();
    }

    void DoneniYerlestir()
    {
        donen.transform.position = donenKonum;
        donen.transform.rotation = donenDonus;
        if (donenGorsel != null) donenGorsel.sortingOrder = donenSira;
        if (donenUzerine != null) donenUzerine.enabled = true;
        donen = null;
    }
}