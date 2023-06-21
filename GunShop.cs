using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunShop : MonoBehaviour
{

    public GameObject[] weaponPrefabs;
    public GameObject[] buttons;

    public GameObject WeaponHolster;

    public Transform equipTrans;

    private GameObject selectedWeapon;
    private Gun selectedGunScript;
    private int selectedGunIndex;

    private string selectedGunName = "Knife";
    private Sprite selectedGunSprite;

    public Sprite defaultGunSprite;

    [SerializeField] private TextMeshProUGUI gunNameText, firemodeText, pelletCountText, spreadText;
    [SerializeField] private TextMeshProUGUI penetrationText, damageText, headshotMultiplierText, RPMText;
    [SerializeField] private TextMeshProUGUI magazineSizeText, reloadTimeText, aimingSpeedText, zoomAmountText;
    [SerializeField] private TextMeshProUGUI recoilText, stationaryAccuracyText;

    public GameObject weaponListGO;
    public WeaponList weaponListScript;

    public GameObject ownedGunsList;

    private bool managingWeapons = false;

    [Tooltip("Audio")]
    public AudioSource audioSource;
    public AudioClip openShopSound, closeShopSound;
    public AudioClip buyWeaponSound;

    void Awake()
    {
        audioSource.ignoreListenerPause = true;
        SelectWeapon(0);
    }

    private void OnEnable()
    {
        audioSource.PlayOneShot(openShopSound);
    }

    private void OnDisable()
    {
        audioSource.PlayOneShot(closeShopSound);
    }

    public void SelectWeapon(int GunNumber)
    {
        selectedGunIndex = GunNumber;
        selectedWeapon = weaponPrefabs[GunNumber];
        selectedGunScript = selectedWeapon.GetComponentInChildren<Gun>(true);

        gunNameText.text = selectedGunScript.gunName.ToString();
        if (selectedGunScript.semiAutomatic == true)
        {
            firemodeText.text = "Semi-automatic";
        }
        else
        {
            firemodeText.text = "Fully Automatic";
        }
        // pelletCountText.text = selectedGunScript.pelletCount.ToString() + " with " + selectedGunScript.shotgunDeviation.ToString() + " deviation";
        spreadText.text = selectedGunScript.spread.ToString();
        penetrationText.text = selectedGunScript.penetration.ToString();
        damageText.text = selectedGunScript.damage.ToString();
        headshotMultiplierText.text = selectedGunScript.headshotMultiplier.ToString();
        RPMText.text = selectedGunScript.RPM.ToString();
        magazineSizeText.text = selectedGunScript.magazineSize.ToString();
        reloadTimeText.text = selectedGunScript.reloadTime.ToString();
        aimingSpeedText.text = selectedGunScript.aimSpeed.ToString();
        zoomAmountText.text = selectedGunScript.zoomAmount.ToString();
        recoilText.text = "X" + selectedGunScript.recoilX.ToString() + ", Y" + selectedGunScript.recoilY.ToString() + ", Z" + selectedGunScript.recoilZ.ToString() + ", Snappiness " +
            selectedGunScript.snappiness.ToString() + ", Return " + selectedGunScript.returnSpeed.ToString();
        // stationaryAccuracyText.text = selectedGunScript.stationaryAccuracy.ToString();

        selectedGunName = selectedGunScript.gunName;
        selectedGunSprite = selectedGunScript.gunSprite;

    }

    public void BuyGunButton()
    {
        AddWeapon(selectedGunIndex);
        buttons[selectedGunIndex].transform.SetParent(ownedGunsList.transform);
        audioSource.PlayOneShot(buyWeaponSound);
    }

    public void ManageWeaponsButton()
    {
        if (!managingWeapons)
        {
            managingWeapons = true;
        }
        else
        {
            managingWeapons = false;
        }
    }

    public void AddWeapon(int GunNumber) // Lis‰‰ ase WeaponHolsteriin ja aseen paneeli
    {
        GameObject newWeapon = Instantiate(weaponPrefabs[GunNumber], equipTrans.position, equipTrans.transform.rotation);
        newWeapon.transform.parent = WeaponHolster.transform;
        newWeapon.SetActive(false);


        if (WeaponHolster.transform.childCount == 1)
        {
            newWeapon.SetActive(true);
        }

        weaponListScript.AddWeaponToPanel(selectedGunName, selectedGunSprite);

    }

}
