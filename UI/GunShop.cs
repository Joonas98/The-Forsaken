using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 5.10.2023 OBSOTELE OLD SYSTEM
public class GunShop : MonoBehaviour
{
	public Transform equipTrans;
	public WeaponList weaponListScript;
	public GameObject weaponHolster, weaponListGO, ownedGunsList;
	public GameObject[] weaponPrefabs;
	public Transform buttonsParent;
	public Sprite defaultGunSprite;

	[SerializeField] private TextMeshProUGUI gunNameText, firemodeText, pelletCountText, spreadText;
	[SerializeField] private TextMeshProUGUI penetrationText, damageText, headshotMultiplierText, RPMText;
	[SerializeField] private TextMeshProUGUI magazineSizeText, reloadTimeText, aimingSpeedText, zoomAmountText;
	[SerializeField] private TextMeshProUGUI recoilText, stationaryAccuracyText;

	private GameObject selectedWeaponGO;
	private Weapon selectedWeaponScript;
	private Gun selectedGunScript;
	private int selectedGunIndex;
	private GameObject[] buttons;

	[Tooltip("Audio")]
	public AudioSource audioSource;
	public AudioClip openShopSound, closeShopSound;
	public AudioClip buyWeaponSound;

	private void OnValidate()
	{
		buttons = new GameObject[buttonsParent.childCount];
		for (int i = 0; i < buttonsParent.childCount; i++)
		{
			buttons[i] = buttonsParent.GetChild(i).gameObject;
		}
	}

	void Awake()
	{
		audioSource.ignoreListenerPause = true;
	}

	private void OnEnable()
	{
		audioSource.PlayOneShot(openShopSound);
	}

	private void OnDisable()
	{
		audioSource.PlayOneShot(closeShopSound);
	}

	public void SelectWeapon(int weaponIndex)
	{
		selectedGunScript = null;
		selectedWeaponScript = null;

		selectedGunIndex = weaponIndex;
		selectedWeaponGO = weaponPrefabs[weaponIndex];
		selectedGunScript = selectedWeaponGO.GetComponentInChildren<Gun>(true);
		selectedWeaponScript = selectedWeaponGO.GetComponentInChildren<Weapon>(true);

		UpdateWeaponInfo();
	}

	public void BuyGunButton()
	{
		if (selectedWeaponGO)
			AddWeapon(selectedGunIndex);
		buttons[selectedGunIndex].transform.SetParent(ownedGunsList.transform);
		audioSource.PlayOneShot(buyWeaponSound);
	}

	public void AddWeapon(int GunNumber) // Add weapon to WeaponHolster and the (UI) weapon panel 
	{
		GameObject newWeapon = Instantiate(weaponPrefabs[GunNumber], equipTrans.position, equipTrans.transform.rotation);
		newWeapon.transform.parent = weaponHolster.transform;
		newWeapon.SetActive(false);

		if (weaponHolster.transform.childCount == 1)
		{
			newWeapon.SetActive(true);
		}

		weaponListScript.AddWeaponToPanel(selectedWeaponScript.weaponName, selectedWeaponScript.weaponSprite);
	}

	private void UpdateWeaponInfo()
	{
		gunNameText.text = selectedWeaponGO.GetComponent<Weapon>().weaponName;
		if (selectedGunScript == null) return; // Melee weapon

		if (selectedGunScript.semiAutomatic) firemodeText.text = "Semi-automatic";
		else firemodeText.text = "Fully Automatic";

		spreadText.text = selectedGunScript.aimSpread.ToString();
		penetrationText.text = selectedGunScript.penetration.ToString();
		damageText.text = selectedGunScript.damage.ToString();
		headshotMultiplierText.text = selectedGunScript.headshotMultiplier.ToString();
		RPMText.text = selectedGunScript.RPM.ToString();
		magazineSizeText.text = selectedGunScript.magazineSize.ToString();
		reloadTimeText.text = selectedGunScript.reloadTime.ToString();
		aimingSpeedText.text = selectedGunScript.aimSpeed.ToString();
		zoomAmountText.text = selectedGunScript.zoomAmount.ToString();
		recoilText.text = "X" + selectedGunScript.recoil.x.ToString() + ", Y" + selectedGunScript.recoil.y.ToString() + ", Z" + selectedGunScript.recoil.z.ToString() + ", Snappiness " +
			selectedGunScript.snappiness.ToString() + ", Return " + selectedGunScript.returnSpeed.ToString();
	}

}
