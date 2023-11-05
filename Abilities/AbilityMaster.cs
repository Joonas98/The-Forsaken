using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

// Global holder for ability data
public class AbilityMaster : MonoBehaviour
{
	// Statics
	public static AbilityMaster instance = null;
	public static List<int> abilities = new List<int>(); // IDs of owned abilities

	// Monos
	public GameObject player;

	[Tooltip("This original instances of abilities")]
	public List<Ability> abilitiesList = new List<Ability>();
	public List<KeyCode> keycodesList = new List<KeyCode>();

	public GameObject abilityPrefab;
	public Transform abilitiesParent;

	private List<int> ownedAbilities = new List<int>();
	private int hotkeyIndex = 0;

	private void Awake()
	{
		if (instance == null)
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		abilities.Clear();
	}

	public void AddAbility(int abilityNumber)
	{
		if (abilities.Contains(abilityNumber)) return;
		//  ownedAbilities.Add(abilityNumber);
		abilities.Add(abilityNumber);

		AbilityHolder abilityHolder = player.AddComponent<AbilityHolder>();
		abilityHolder.ability = abilitiesList[abilityNumber];
		GameObject newAbility = Instantiate(abilityPrefab);
		newAbility.transform.SetParent(abilitiesParent, false);

		Image[] abilityImages = newAbility.GetComponentsInChildren<Image>();
		foreach (Image img in abilityImages)
		{
			img.sprite = abilitiesList[abilityNumber].picture;
		}
		abilityImages[0].sprite = null;

		abilityHolder.abilityImage = abilityImages[2];
		abilityHolder.backgroundImage = abilityImages[0];

		// Assign hotkeys to active abilities and display the hotkey
		TextMeshProUGUI hotkeyText = newAbility.GetComponentInChildren<TextMeshProUGUI>();
		if (abilitiesList[abilityNumber].GetPassiveType() == false) // If not passive ability
		{
			if (hotkeyIndex <= keycodesList.Count)
			{
				abilityHolder.key = keycodesList[hotkeyIndex];
				hotkeyText.text = keycodesList[hotkeyIndex].ToString();
				hotkeyIndex++;
			}
			else
			{
				Debug.Log("Out of ability hotkeys");
			}

			// Set the new ability as the first child under abilitiesParent
			newAbility.transform.SetAsFirstSibling();
		}
		else
		{
			hotkeyText.text = ""; // No hotkey on passives
								  // Set the new ability as the last child under abilitiesParent
			newAbility.transform.SetAsLastSibling();
		}
	}


	public string GetAbilityDescription(int abilityNumber)
	{
		if (abilitiesList[abilityNumber] != null)
		{
			return abilitiesList[abilityNumber].abilityDescription;
		}
		else
		{
			Debug.Log("Ability number: " + abilityNumber + " was not found!");
			return null;
		}
	}
}
