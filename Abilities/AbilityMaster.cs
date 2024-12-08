using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityMaster : MonoBehaviour
{
	public static AbilityMaster instance = null;
	public GameObject player;
	public GameObject abilityHUDPrefab;
	public Transform activeParent, automaticParent, passiveParent; // Each type has their own HUD lists

	[Tooltip("Hotkeys assigned to Active abilities in order")]
	public List<KeyCode> keycodesList = new List<KeyCode>();
	private int hotkeyIndex = 0;

	// Set of owned abilities - using a HashSet for O(1) lookups
	public static HashSet<string> ownedAbilities = new HashSet<string>();

	private void Awake()
	{
		// Singleton pattern
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
		// Clear owned abilities at the start if desired
		ownedAbilities.Clear();
	}

	/// <summary>
	/// Adds a new ability if not already owned.
	/// Creates and configures its UI representation.
	/// Assigns a hotkey if it's an active ability.
	/// </summary>
	/// <param name="ability">The Ability ScriptableObject reference</param>
	public void AddAbility(Ability ability)
	{
		if (ownedAbilities.Contains(ability.name))
			return; // Already owned

		ownedAbilities.Add(ability.name);

		// Add AbilityHolder to the player so the ability can function at runtime
		AbilityHolder abilityHolder = player.AddComponent<AbilityHolder>();
		abilityHolder.ability = ability;

		GameObject newAbilityHUD;

		switch (ability.abilityType)
		{
			case Ability.AbilityType.Active:
				newAbilityHUD = Instantiate(abilityHUDPrefab, activeParent, false);
				break;
			case Ability.AbilityType.Automatic:
				newAbilityHUD = Instantiate(abilityHUDPrefab, automaticParent, false);
				break;
			case Ability.AbilityType.Passive:
				newAbilityHUD = Instantiate(abilityHUDPrefab, passiveParent, false);
				break;

			default:
				newAbilityHUD = Instantiate(abilityHUDPrefab, activeParent, false);
				Debug.LogError("Instantiated ability type not recognized");
				break;
		}

		// Update HUD images with the ability's sprite
		Image[] abilityImages = newAbilityHUD.GetComponentsInChildren<Image>();
		foreach (Image img in abilityImages)
		{
			img.sprite = ability.picture;
		}

		// Assign references in AbilityHolder
		abilityHolder.abilityImage = abilityImages.Length > 1 ? abilityImages[1] : null;
		//abilityHolder.backgroundImage = abilityImages.Length > 0 ? abilityImages[0] : null;

		// Find hotkey text and set accordingly
		TextMeshProUGUI hotkeyText = newAbilityHUD.GetComponentInChildren<TextMeshProUGUI>();
		if (hotkeyText != null)
		{
			if (ability.abilityType == Ability.AbilityType.Active)
			{
				// Assign next available hotkey if we have one
				if (hotkeyIndex < keycodesList.Count)
				{
					abilityHolder.key = keycodesList[hotkeyIndex];
					hotkeyText.text = keycodesList[hotkeyIndex].ToString();
					hotkeyIndex++;
				}
				else
				{
					Debug.Log("Out of ability hotkeys");
					hotkeyText.text = "";
				}

				// Active abilities: put them at the front
				newAbilityHUD.transform.SetAsFirstSibling();
			}
			else
			{
				// Passive and ActivePassive do not get hotkeys
				hotkeyText.text = "";

				// Non-active abilities: put them at the end
				newAbilityHUD.transform.SetAsLastSibling();
			}
		}
	}

	public string GetAbilityDescription(Ability ability)
	{
		if (ownedAbilities.Contains(ability.name))
		{
			return ability.abilityDescription;
		}
		else
		{
			Debug.Log($"Ability {ability.name} is not owned!");
			return null;
		}
	}

	public bool HasAbility(string name)
	{
		return ownedAbilities.Contains(name);
	}

	public void DebugTesting()
	{
		Debug.Log("Called method");
	}

}
