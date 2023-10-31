using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponPanel : MonoBehaviour
{
	// This script is for handling the weapon panels of the HUD
	public GameObject weaponHolster;
	public Button upButton;
	public Button downButton;
	public Button sellButton;
	public GameObject[] buttons;
	public GameObject highlightObject;
	public TextMeshProUGUI indexText;

	private int currentIndex;
	private GameObject handledWeapon;

	private void Awake()
	{
		weaponHolster = GameObject.Find("WeaponHolster");
	}

	private void Start()
	{
		// Set the index in start because awake is called too early
		// +1 because child indexing starts from 0 obviously
		indexText.text = (FindCurrentObjectChildIndex() + 1).ToString();
	}

	public void MoveWeaponUp()
	{
		currentIndex = transform.GetSiblingIndex();
		transform.SetSiblingIndex(currentIndex - 1);
		handledWeapon = weaponHolster.transform.GetChild(currentIndex).gameObject;
		handledWeapon.transform.SetSiblingIndex(currentIndex - 1);
	}

	public void MoveWeaponDown()
	{
		currentIndex = transform.GetSiblingIndex();
		transform.SetSiblingIndex(currentIndex + 1);
		handledWeapon = weaponHolster.transform.GetChild(currentIndex).gameObject;
		handledWeapon.transform.SetSiblingIndex(currentIndex + 1);
	}

	public void SellWeapon()
	{
		currentIndex = transform.GetSiblingIndex();
		handledWeapon = weaponHolster.transform.GetChild(currentIndex).gameObject;
		Destroy(handledWeapon);
		Destroy(gameObject);

		// If the weapon to be sold is currently equippped, equip knife
		if (GameManager.GM.currentWeaponIndex == currentIndex)
		{
			WeaponSwitcher.instance.selectedWeapon = 0;
			WeaponSwitcher.instance.SelectWeapon();
		}
	}

	public void EnableButtons()
	{
		foreach (GameObject go in buttons)
		{
			go.SetActive(true);
		}
	}

	public void DisableButtons()
	{
		foreach (GameObject go in buttons)
		{
			go.SetActive(false);
		}
	}

	// Used for the index text. First weapon gets 1, second 2 and so on
	public int FindCurrentObjectChildIndex()
	{
		if (transform.parent != null)
		{
			for (int i = 0; i < transform.parent.childCount; i++)
			{
				if (transform.parent.GetChild(i).gameObject == gameObject)
				{
					return i; // Return the index of the current GameObject within its parent's children
				}
			}
		}

		// If the current object is not a child or doesn't have a parent, return -1.
		return -1;
	}
}
