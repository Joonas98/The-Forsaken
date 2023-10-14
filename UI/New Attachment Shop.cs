using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NewAttachmentShop : MonoBehaviour
{
	// Script to handle attachment purchasing and owned guns
	public static NewAttachmentShop instance;

	[Header("References")]
	public GameObject selectedWeapon; // Currently selected weapon for attachment / upgrade customization
	public GameObject ownedWeaponButtonPrefab;
	public Transform ownedWeaponButtonsParent;
	public TextMeshProUGUI attachmentsPageTitle;
	public Michsky.MUIP.WindowManager attachmentsWindowManager;

	private AttachmentsScript attachmentsScript;

	[Header("Attachment list")]
	public GameObject attachmentButtonPrefab;
	public Transform scopesParent;
	public Transform muzzlesParent;
	public Transform gripsParent;

	private List<Transform> scopeAttachments = new List<Transform>();
	private List<Transform> muzzleAttachments = new List<Transform>();
	private List<Transform> gripAttachments = new List<Transform>();
	private bool recreatingButtons; // Add a flag to track whether buttons are being recreated

	[Header("Colors")]
	public Color defaultColor;
	public Color highlightColor;

	enum AttachmentType { Scope, Muzzle, Grip }

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
			return;
		}
	}

	public void AddOwnedWeaponButton(GameObject weapon)
	{
		// Instantiate and set parent
		GameObject newButton = Instantiate(ownedWeaponButtonPrefab, ownedWeaponButtonsParent.position, Quaternion.identity); // Set position and rotation
		newButton.transform.SetParent(ownedWeaponButtonsParent, false); // Set the correct parent

		// Get the components
		OwnedWeaponButton ownedWeaponButtonScript = newButton.GetComponent<OwnedWeaponButton>();
		ownedWeaponButtonScript.weaponObject = weapon;
		ownedWeaponButtonScript.weaponScript = weapon.GetComponent<Weapon>();
	}

	// Reset button colors to default and highlight the selected
	public void ChangeSelection(OwnedWeaponButton newSelection)
	{
		// Highlight the selected OwnedWeaponButton and reset rest
		foreach (OwnedWeaponButton ownedWeaponButton in ownedWeaponButtonsParent.GetComponentsInChildren<OwnedWeaponButton>())
		{
			ownedWeaponButton.border.color = ownedWeaponButton.defaultColor;
		}
		newSelection.border.color = newSelection.selectedColor;

		// Get reference to the new selection's attachmentsScript and update everything
		attachmentsScript = selectedWeapon.GetComponent<AttachmentsScript>();
		UpdateAttachmentButtons();

		// Update the highlights of selected attachments
		if (attachmentsScript != null) StartCoroutine(UpdateSelectionHighlights());
	}

	// Update the attachment buttons based on the selected weapon
	public void UpdateAttachmentButtons()
	{
		// Update the attachment script to the current weapon
		attachmentsScript = selectedWeapon.GetComponent<AttachmentsScript>();

		// Delete previous buttons
		ClearAttachmentButtons(scopesParent);
		ClearAttachmentButtons(muzzlesParent);
		ClearAttachmentButtons(gripsParent);

		// Empty lists that hold references to attachments
		scopeAttachments.Clear();
		muzzleAttachments.Clear();
		gripAttachments.Clear();

		// Exit after clearing because no attachmentsScript = no attachments for selected weapon
		if (attachmentsScript == null) return;

		// List the attachments the selected weapon has
		CollectAttachmentTransforms(attachmentsScript.scopesHolder, scopeAttachments);
		CollectAttachmentTransforms(attachmentsScript.muzzlesHolder, muzzleAttachments);
		CollectAttachmentTransforms(attachmentsScript.gripsHolder, gripAttachments);

		// Create unequip buttons if there are attachments of that type
		AddUnequipButton(scopeAttachments, scopesParent, AttachmentType.Scope);
		AddUnequipButton(muzzleAttachments, muzzlesParent, AttachmentType.Muzzle);
		AddUnequipButton(gripAttachments, gripsParent, AttachmentType.Grip);

		// Create the button gameobjects
		InstantiateAttachmentButtons(scopeAttachments, scopesParent);
		InstantiateAttachmentButtons(muzzleAttachments, muzzlesParent);
		InstantiateAttachmentButtons(gripAttachments, gripsParent);

		// Create listeners with one frame delay so previous buttons are deleted
		StartCoroutine(AttachListenersToButtonsDelayed());
		if (attachmentsScript.scopes.Length > 0) ChangeHighlight(scopesParent.GetChild(attachmentsScript.currentScope + 1).gameObject, scopesParent);
		if (attachmentsScript.muzzleDevices.Length > 0) ChangeHighlight(muzzlesParent.GetChild(attachmentsScript.currentMuzzle + 1).gameObject, muzzlesParent);
		if (attachmentsScript.grips.Length > 0) ChangeHighlight(gripsParent.GetChild(attachmentsScript.currentGrip + 1).gameObject, gripsParent);
	}

	// List the attachments the selected weapon has
	private void CollectAttachmentTransforms(Transform attachmentHolder, List<Transform> attachmentList)
	{
		if (attachmentHolder == null)
		{
			return;
		}

		foreach (Transform attachment in attachmentHolder)
		{
			attachmentList.Add(attachment);
		}
	}

	// Delete previous buttons
	private void ClearAttachmentButtons(Transform parent)
	{
		foreach (Transform child in parent)
		{
			Destroy(child.gameObject);
		}
	}

	// Create the button gameobjects
	private void InstantiateAttachmentButtons(List<Transform> attachments, Transform parent)
	{
		foreach (Transform attachment in attachments)
		{
			GameObject attachmentButton = Instantiate(attachmentButtonPrefab, parent);

			// Recursively search for Text component in children
			TextMeshProUGUI textComponent = FindTextComponentInChildren(attachmentButton.transform);

			if (textComponent != null)
			{
				// Set the text of the Text component to the attachment's name
				textComponent.text = attachment.name;
			}
		}
	}

	// Add a button for unequipping attachments
	private void AddUnequipButton(List<Transform> attachments, Transform parent, AttachmentType attachmentType)
	{
		if (attachments.Count > 0)
		{
			GameObject unequipButton = Instantiate(attachmentButtonPrefab, parent);
			TextMeshProUGUI textComponent = FindTextComponentInChildren(unequipButton.transform);

			if (textComponent != null)
			{
				textComponent.text = "Unequip " + attachmentType.ToString(); // Set button text
			}

			// Attach a listener to unequip the attachment of this type
			Button buttonComponent = unequipButton.GetComponent<Button>();
			buttonComponent.onClick.RemoveAllListeners(); // Clear existing listeners

			buttonComponent.onClick.AddListener(() =>
			{
				// Determine the attachment type based on the parent
				switch (parent)
				{
					case var _ when parent == scopesParent:
						attachmentsScript.UnequipScope();
						break;
					case var _ when parent == muzzlesParent:
						attachmentsScript.UnequipMuzzle();
						break;
					case var _ when parent == gripsParent:
						attachmentsScript.UnequipGrip();
						break;
					default:
						Debug.LogError("Invalid attachment type.");
						break;
				}
			});
		}
	}

	// Create listeners
	private void AttachListenersToButtons(AttachmentType attachmentType)
	{
		Transform parent = null;
		switch (attachmentType)
		{
			case AttachmentType.Scope:
				parent = scopesParent;
				break;
			case AttachmentType.Muzzle:
				parent = muzzlesParent;
				break;
			case AttachmentType.Grip:
				parent = gripsParent;
				break;
			default:
				Debug.LogError("Invalid attachment type.");
				return;
		}

		if (parent.childCount > 0)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				GameObject attachmentButton = parent.GetChild(i).gameObject;
				Button buttonComponent = attachmentButton.GetComponent<Button>();
				int currentAttachmentIndex = i - 1; // Offset the index by -1 for unequipping

				// Attach the listener based on the attachment type
				buttonComponent.onClick.RemoveAllListeners(); // Clear existing listeners

				// Add listener to update color
				buttonComponent.onClick.AddListener(() => ChangeHighlight(buttonComponent.gameObject, parent));

				switch (attachmentType)
				{
					case AttachmentType.Scope:
						buttonComponent.onClick.AddListener(() => attachmentsScript.EquipScope(currentAttachmentIndex));
						break;
					case AttachmentType.Muzzle:
						buttonComponent.onClick.AddListener(() => attachmentsScript.EquipMuzzle(currentAttachmentIndex));
						break;
					case AttachmentType.Grip:
						buttonComponent.onClick.AddListener(() => attachmentsScript.EquipGrip(currentAttachmentIndex));
						break;
					default:
						Debug.LogError("Invalid attachment type.");
						break;
				}
			}
		}
	}

	// Figuring this out took so many hours
	// Listeners need to be attached in the next frame, because deletion of previous buttons is not done fast enough
	private IEnumerator AttachListenersToButtonsDelayed()
	{
		yield return new WaitForEndOfFrame();
		AttachListenersToButtons(AttachmentType.Scope);
		AttachListenersToButtons(AttachmentType.Muzzle);
		AttachListenersToButtons(AttachmentType.Grip);
	}

	private IEnumerator UpdateSelectionHighlights()
	{
		yield return new WaitForEndOfFrame();
		if (attachmentsScript.scopes.Length > 0) ChangeHighlight(scopesParent.GetChild(attachmentsScript.currentScope + 1).gameObject, scopesParent);
		if (attachmentsScript.muzzleDevices.Length > 0) ChangeHighlight(muzzlesParent.GetChild(attachmentsScript.currentMuzzle + 1).gameObject, muzzlesParent);
		if (attachmentsScript.grips.Length > 0) ChangeHighlight(gripsParent.GetChild(attachmentsScript.currentGrip + 1).gameObject, gripsParent);
	}

	// Helper method to change button's image color
	private void ChangeHighlight(GameObject button, Transform parent)
	{
		// Reset all button colors of the category
		foreach (Transform child in parent)
		{
			Transform first = child.transform.GetChild(0);
			first.GetComponent<Image>().color = defaultColor;
		}

		// Highlight
		Transform firstChild = button.transform.GetChild(0);
		Image buttonImage = firstChild.GetComponent<Image>();
		if (buttonImage != null)
		{
			buttonImage.color = highlightColor;
		}
	}

	private TextMeshProUGUI FindTextComponentInChildren(Transform parent)
	{
		TextMeshProUGUI textComponent = parent.GetComponentInChildren<TextMeshProUGUI>();

		if (textComponent != null)
		{
			return textComponent;
		}

		foreach (Transform child in parent)
		{
			textComponent = FindTextComponentInChildren(child);
			if (textComponent != null)
			{
				return textComponent;
			}
		}

		return null;
	}

}
