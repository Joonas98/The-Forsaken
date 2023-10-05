using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponList : MonoBehaviour
{
	// This script handles the list of weapons in the top right corner
	public GameObject weaponPanel;

	private GameObject weaponsList;
	private WeaponPanel weaponPanelScript;

	private void Start()
	{
		// weaponsList = GameObject.Find("WeaponsPanel");
		// weaponsList = gameObject;
	}

	public void AddWeaponToPanel(string text, Sprite image)
	{
		GameObject newPanel = Instantiate(weaponPanel, gameObject.transform.position, Quaternion.identity);
		newPanel.transform.SetParent(gameObject.transform, false);

		weaponPanelScript = newPanel.GetComponent<WeaponPanel>();

		if (gameObject.transform.childCount > 1)
			weaponPanelScript.EnableButtons();

		TextMeshProUGUI newPanelText = newPanel.GetComponentInChildren<TextMeshProUGUI>();

		Image newPanelImage = newPanel.transform.GetChild(1).GetComponentInChildren<Image>();

		newPanelText.text = text;
		newPanelImage.sprite = image;
	}


}
