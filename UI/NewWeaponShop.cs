using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewWeaponShop : MonoBehaviour
{
	public static NewWeaponShop instance;
	// 5.10.2023 new script to handle weapon purchasing
	[Header("General functionality")]
	public Transform equipTrans; // The position and rotation of new weapons

	[Header("Weapon list (top right)")]
	public GameObject weaponPanelPrefab; // Weapon panels in the top right corner
	public Transform weaponPanelParent; // The parent of weapon panels

	private WeaponPanel weaponPanelScript;

	[Header("References")]
	public Transform[] weaponButtonParents; // Parents of each category that holds the weapon buttons
	public WeaponButton[] weaponButtons; // Every WeaponButton reference

	public Michsky.MUIP.ModalWindowManager modalManager; // Used for confirmation of purchase
	public Michsky.MUIP.ButtonManager confirmPurchaseButton;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void OnValidate()
	{
		// Create a list to accumulate WeaponButton components
		List<WeaponButton> buttonList = new List<WeaponButton>();

		// Iterate through weaponButtonParents
		foreach (var parent in weaponButtonParents)
		{
			// Get WeaponButton components from children of each parent
			var buttons = parent.GetComponentsInChildren<WeaponButton>(true);
			buttonList.AddRange(buttons); // Add them to the list
		}

		// Convert the list to an array
		weaponButtons = buttonList.ToArray();
	}

	// Main purchase function
	public void PurchaseWeapon(GameObject purchasedWeapon)
	{
		// Instantiate and set parent to the purchased weapon
		GameObject newWeapon = Instantiate(purchasedWeapon, equipTrans.position, equipTrans.transform.rotation); // Set the correct position and rotation
		newWeapon.transform.parent = WeaponSwitcher.instance.transform; // Set the correct parent

		// Add UI elements of the new weapon
		AddWeaponToPanel(newWeapon.GetComponent<Weapon>());
		NewAttachmentShop.instance.AddOwnedWeaponButton(newWeapon);
	}

	// Add weapon panel to top right corner of HUD
	public void AddWeaponToPanel(Weapon weaponScript)
	{
		// Instantiate and set parent
		GameObject newPanel = Instantiate(weaponPanelPrefab, weaponPanelParent.position, Quaternion.identity);
		newPanel.transform.SetParent(weaponPanelParent, false);

		// Reference the panel script
		weaponPanelScript = newPanel.GetComponent<WeaponPanel>();

		// Get and set the text and image
		TextMeshProUGUI newPanelName = newPanel.GetComponentInChildren<TextMeshProUGUI>();
		Image newPanelImage = newPanel.transform.GetChild(1).GetComponentInChildren<Image>();

		newPanelName.text = weaponScript.weaponName;
		newPanelImage.sprite = weaponScript.weaponSprite;
	}

	// Update all buttons to have correct color, e.g. green if weapon can be afforded, blue for owned etc.
	public void UpdateWeaponButtonColors()
	{
		foreach (WeaponButton button in weaponButtons)
		{
			button.UpdateButtonColor();
		}
	}

	public void UpdateConfirmationModalWindow(GameObject weaponPrefab)
	{
		// Get the script reference to access name and price
		Weapon weaponScript = weaponPrefab.GetComponent<Weapon>();

		// Update modal window UI
		modalManager.descriptionText = "You are about to purchase: " + weaponScript.weaponName + "<br> For the price of: " + weaponScript.weaponPrice.ToString() + "€";
		modalManager.UpdateUI();

		// Clear listeners to avoid 
		confirmPurchaseButton.onClick.RemoveAllListeners();

		// Add the default confirm functionality
		confirmPurchaseButton.onClick.AddListener(() => modalManager.Close());

		// Update confirmation button listener
		confirmPurchaseButton.onClick.AddListener(() => PurchaseWeapon(weaponPrefab));

		// Update all weapon button colors on confirmed purchase
		confirmPurchaseButton.onClick.AddListener(() => UpdateWeaponButtonColors());
	}

}
