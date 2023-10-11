using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OwnedWeaponButton : MonoBehaviour
{
	[Header("References")]
	public GameObject weaponObject;
	public Weapon weaponScript;

	public Image border;
	public Image weaponImage;
	public TextMeshProUGUI nameText;

	[Header("UI Stuff")]
	public Color defaultColor;
	public Color selectedColor;

	private void Start()
	{
		// Set correct image, sprite and color
		weaponImage.sprite = weaponScript.weaponSprite;
		nameText.text = weaponScript.weaponName;
		border.color = defaultColor;
	}

	// Called from the button this script is held in
	public void SelectWeapon()
	{
		NewAttachmentShop.instance.selectedWeapon = weaponObject;
		NewAttachmentShop.instance.attachmentsPageTitle.text = "Choose attachments for " + weaponScript.weaponName;
		NewAttachmentShop.instance.ChangeSelection(this);
	}

}
