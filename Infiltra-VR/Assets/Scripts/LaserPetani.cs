using UnityEngine;

public class LaserPetani : MonoBehaviour
{
    public PapanPintar papanHologram; 
    public LayerMask cumaTanah; 
    public bool lagiPegangAlat = false; 

    void Update()
    {
        if (lagiPegangAlat == false)
        {
            papanHologram.gameObject.SetActive(false);
            return; 
        }

        Ray sinarLaser = new Ray(transform.position, transform.forward);
        RaycastHit titikYangKena;

        // Bikin garis laser MERAH di layar Unity (cuma kelihatan di tab Scene pas Play) biar ketahuan lasernya nembak ke mana!
        Debug.DrawRay(transform.position, transform.forward * 10f, Color.red);

        if (Physics.Raycast(sinarLaser, out titikYangKena, 10f, cumaTanah))
        {
            papanHologram.gameObject.SetActive(true);
            papanHologram.PindahKeTitik(titikYangKena.point);
        }
        else
        {
            papanHologram.gameObject.SetActive(false);
        }
    }

    // Fungsi ini HARUS dipanggil sama XR Grab Cangkul/Bibit
    public void AlatDiambil()
    {
        lagiPegangAlat = true;
        Debug.Log("SAKLAR LASER ON: Tangan VR sedang menggenggam alat!");
    }

    public void AlatDilepas()
    {
        lagiPegangAlat = false;
        Debug.Log("SAKLAR LASER OFF: Alat dilepaskan dari tangan!");
    }
}