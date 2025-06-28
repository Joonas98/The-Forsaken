using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
	// This script is for handling the weapon panels of the HUD
	public GameObject weaponHolster;
	public Button upButton;
	public Button downButton;
	public Button sellButton;
	public GameObject[] buttons;

	[Header("Selection Indicator")]
	[Tooltip("UI Image to indicate selection state")]
	[SerializeField] private Image selectionIndicator;

	[Header("Indicator Colors")]
	[SerializeField] private Color inactiveColor;
	[SerializeField] private Color selectedColor;

	public TextMeshProUGUI indexText;

	private int currentIndex;
	private GameObject handledWeapon;

	private void Awake()
	{
		weaponHolster = GameObject.Find("WeaponHolster");
	}

	private void Start()
	{
		// Set the index in start because Awake is called too early
		// +1 because child indexing starts from 0 obviously
		indexText.text = (FindCurrentObjectChildIndex() + 1).ToString();

		// Initialize indicator color
		SetSelected(false);
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
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Call this to update the selection indicator's color.
	/// </summary>
	/// <param name="isSelected">Whether this panel is selected.</param>
	public void SetSelected(bool isSelected)
	{
		if (selectionIndicator != null)
		{
			selectionIndicator.color = isSelected ? selectedColor : inactiveColor;
		}
	}
}
