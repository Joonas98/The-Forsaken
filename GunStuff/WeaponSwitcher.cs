using System.Collections;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
	public int selectedWeapon;
	public static bool canSwitchWeapon = true;
	public Weapon currentWeapon;

	// Parents for UI weapon panels
	// Remember that the actual objects are under this class' GO
	public Transform storeParent, hudParent;

	public static WeaponSwitcher instance; // Singleton reference

	private WeaponPanel handledPanel;
	private float unequipTime;
	private Gun currentGun;

	private void Awake()
	{
		// Singleton
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
	}

	void Update()
	{
		int previousSelectedWeapon = selectedWeapon;

		if (!canSwitchWeapon) return;

		#region Wheel Selection
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			if (selectedWeapon >= transform.childCount - 1)
			{
				selectedWeapon = 0;
			}
			else
			{
				selectedWeapon++;
			}
		}

		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			if (selectedWeapon <= 0)
			{
				selectedWeapon = transform.childCount - 1;
			}
			else
			{
				selectedWeapon--;
			}
		}
		#endregion

		#region Numbers Selection
		if (Input.GetKeyDown(KeyCode.Alpha1) && transform.childCount >= 1)
		{
			selectedWeapon = 0;
		}

		if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
		{
			selectedWeapon = 1;
		}

		if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
		{
			selectedWeapon = 2;
		}

		if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4)
		{
			selectedWeapon = 3;
		}

		if (Input.GetKeyDown(KeyCode.Alpha5) && transform.childCount >= 5)
		{
			selectedWeapon = 4;
		}

		if (Input.GetKeyDown(KeyCode.Alpha6) && transform.childCount >= 6)
		{
			selectedWeapon = 5;
		}

		if (Input.GetKeyDown(KeyCode.Alpha7) && transform.childCount >= 7)
		{
			selectedWeapon = 6;
		}

		if (Input.GetKeyDown(KeyCode.Alpha8) && transform.childCount >= 8)
		{
			selectedWeapon = 7;
		}

		if (Input.GetKeyDown(KeyCode.Alpha9) && transform.childCount >= 9)
		{
			selectedWeapon = 8;
		}
		#endregion

		if (previousSelectedWeapon != selectedWeapon)
		{
			if (currentWeapon != null)
			{
				StartCoroutine(UnequipTimer());
			}
			else
			{
				SelectWeapon();
			}
		}
	}

	public void SelectWeapon()
	{
		int i = 0;
		foreach (Transform weapon in transform)
		{
			if (i == selectedWeapon)
				weapon.gameObject.SetActive(true);
			else
				weapon.gameObject.SetActive(false);
			i++;
		}

		currentWeapon = gameObject.GetComponentInChildren<Weapon>();
		currentGun = gameObject.GetComponentInChildren<Gun>();

		if (currentWeapon != null)
		{
			unequipTime = currentWeapon.unequipTime;
			GameManager.GM.currentWeapon = currentWeapon;
		}

		if (currentGun != null)
		{
			GameManager.GM.currentGun = currentGun;
		}
		else
		{
			GameManager.GM.currentGun = null;
		}

		GameManager.GM.currentWeaponIndex = selectedWeapon;

		// Handle highlight for HUD
		if (handledPanel != null) handledPanel.highlightObject.SetActive(false);
		handledPanel = hudParent.transform.GetChild(selectedWeapon).GetComponent<WeaponPanel>();
		handledPanel.highlightObject.SetActive(true);
	}

	public static void CanSwitch(bool boolean)
	{
		canSwitchWeapon = boolean;
	}

	public void MoveWeaponLeft(GameObject weapon)
	{
		int currentIndex = weapon.transform.GetSiblingIndex();
		if (currentIndex > 0)
		{
			// Move it left in the Weapon Switcher
			weapon.transform.SetSiblingIndex(currentIndex - 1);

			// Also move the corresponding weapon in the Store UI and HUD UI
			storeParent.GetChild(currentIndex).SetSiblingIndex(currentIndex - 1);
			hudParent.GetChild(currentIndex).SetSiblingIndex(currentIndex - 1);
		}
	}

	public void MoveWeaponRight(GameObject weapon)
	{
		int currentIndex = weapon.transform.GetSiblingIndex();
		int maxIndex = transform.childCount - 1;
		if (currentIndex < maxIndex)
		{
			weapon.transform.SetSiblingIndex(currentIndex + 1);

			storeParent.GetChild(currentIndex).SetSiblingIndex(currentIndex + 1);
			hudParent.GetChild(currentIndex).SetSiblingIndex(currentIndex + 1);
		}
	}

	IEnumerator UnequipTimer()
	{
		currentWeapon.UnequipWeapon();
		yield return new WaitForSeconds(unequipTime + 0.01f);
		SelectWeapon();
		canSwitchWeapon = true;
	}
}
