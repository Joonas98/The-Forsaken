using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponButton : MonoBehaviour
{
	[Header("States")]
	public bool isOwned;

	[Header("Weapon prefab and reference")]
	public GameObject weaponPrefab;
	public Weapon weaponScript;

	[Header("Colors")]
	public Color unavailableColor; // Can't afford, red
	public Color availableColor; // Can afford, green
	public Color ownedColor; // Already owned, light blueish?

	[Header("UI references")]
	public Image buttonImage;
	public Image weaponImage;
	public TextMeshProUGUI nameText, priceText;
	public Button button;

	private void OnValidate()
	{
		// Update weapon button information automatically
		if (weaponPrefab == null) return;
		weaponScript = weaponPrefab.GetComponent<Weapon>();
		if (weaponScript == null) return;

		// If we have references correctly, set the image, name and price
		weaponImage.sprite = weaponScript.weaponSprite;
		nameText.text = weaponScript.weaponName;
		priceText.text = weaponScript.weaponPrice.ToString() + "€";

		// Change the button gameobject name according to the weapon
		gameObject.name = weaponScript.weaponName + " button";

		// Automatize the button event call
		if (button != null)
		{
			// Remove any existing listeners to avoid duplicates
			button.onClick.RemoveAllListeners();

			// Add a new listener that calls NewWeaponShop.instance.PurchaseWeapon(weaponPrefab)
			button.onClick.AddListener(() => NewWeaponShop.instance.UpdateConfirmationModalWindow(weaponPrefab));

			// Open the modal window
			button.onClick.AddListener(() => NewWeaponShop.instance.modalManager.Open());
		}
	}

	// Update color of the button
	public void UpdateButtonColor()
	{
		if (isOwned)
		{
			buttonImage.color = ownedColor;
		}
		else if (GameManager.GM.money >= weaponScript.weaponPrice)
		{
			buttonImage.color = availableColor;
		}
		else
		{
			buttonImage.color = unavailableColor;
		}
	}

	// Update information to the modal window, that is used to confirm or cancel this purchase
	private void UpdateConfirmationModalWindow(GameObject weaponPrefab)
	{

	}

}
