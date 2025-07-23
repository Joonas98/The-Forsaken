// WeaponSwitcher.cs
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
	public int selectedWeapon;
	public static bool canSwitchWeapon = true;
	public Weapon currentWeapon;

	public Transform storeParent, hudParent;

	public static WeaponSwitcher instance;

	private WeaponPanel handledPanel;
	private float unequipTime;
	private Gun currentGun;

	private void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);

		selectedWeapon = -1;
	}

	private void Start()
	{
		// Equip the first weapon on start (if any)
		if (transform.childCount > 0)
			ActivateAndEquip(selectedWeapon);
	}

	private void Update()
	{
		// Pause check
		if (Time.timeScale == 0) return;

		// No switching while aiming
		if (GameManager.GM.currentGun != null && GameManager.GM.currentGun.isAiming) return;

		int prev = selectedWeapon;
		#region Wheel Selection
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			selectedWeapon = (selectedWeapon + 1) % transform.childCount;
		else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			selectedWeapon = (selectedWeapon - 1 + transform.childCount) % transform.childCount;
		#endregion

		#region Number Keys
		int max = Mathf.Min(transform.childCount, 9);
		for (int i = 0; i < max; i++)
			if (Input.GetKeyDown(KeyCode.Alpha1 + i))
				selectedWeapon = i;
		#endregion

		if (prev != selectedWeapon)
			SwitchTo(selectedWeapon);
	}

	/// <summary>
	/// Stop any current equip, deactivate old, then equip the new.
	/// </summary>
	public void SwitchTo(int index)
	{
		if (index < 0 || index >= transform.childCount) return;

		// 1) Cancel any in-progress equip on the old weapon
		if (currentWeapon != null)
			currentWeapon.CancelEquip();

		// 2) Instantly deactivate the old
		if (currentWeapon != null)
			currentWeapon.gameObject.SetActive(false);

		// 3) Activate & equip the new
		ActivateAndEquip(index);
	}

	private void ActivateAndEquip(int index)
	{
		canSwitchWeapon = false;

		// Turn on only that weapon
		for (int i = 0; i < transform.childCount; i++)
			transform.GetChild(i).gameObject.SetActive(i == index);

		// Cache it
		currentWeapon = transform.GetChild(index).GetComponent<Weapon>();

		// Update GameManager / HUD
		GameManager.GM.currentWeapon = currentWeapon;
		GameManager.GM.currentWeaponIndex = index;

		currentGun = currentWeapon as Gun;
		if (currentGun != null)
		{
			GameManager.GM.currentGun = currentGun;
			GameManager.GM.meleeEquipped = false;
			AmmoHUD.Instance.UpdateAmmoHUD(currentGun.currentMagazine, currentGun.magazineSize);
		}
		else
		{
			GameManager.GM.currentGun = null;
			GameManager.GM.meleeEquipped = true;
		}

		// Highlight HUD
		if (handledPanel != null) handledPanel.SetSelected(false);
		if (hudParent != null && index < hudParent.childCount)
		{
			handledPanel = hudParent.GetChild(index).GetComponent<WeaponPanel>();
			if (handledPanel != null) handledPanel.SetSelected(true);
		}

		// 4) Start equip on the new
		currentWeapon.EquipWeapon();
	}
}
