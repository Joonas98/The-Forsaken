using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponButton : MonoBehaviour
{
	public GameObject weaponPrefab;
	public Weapon weaponScript;
	public Image buttonImage;
	public TextMeshProUGUI nameText, priceText;

	private void OnValidate()
	{
		if (weaponPrefab == null) return;
		weaponScript = weaponPrefab.GetComponent<Weapon>();
		if (weaponScript == null) return;
		buttonImage.sprite = weaponScript.weaponSprite;
		nameText.text = weaponScript.weaponName;
		priceText.text = weaponScript.weaponPrice.ToString() + "€";
		gameObject.name = weaponScript.weaponName + " button";
	}
}
