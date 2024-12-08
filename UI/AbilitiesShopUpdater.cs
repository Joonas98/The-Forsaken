using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AbilitiesShopUpdater : MonoBehaviour
{
	public GameObject abilityShopButtonPrefab;
	public AbilityMaster abilityMaster;

	[Header("Ability Folders")]
	public string activeAbilitiesFolder = "Assets/Abilities/Active";
	public string activePassiveAbilitiesFolder = "Assets/Abilities/ActivePassive";
	public string passiveAbilitiesFolder = "Assets/Abilities/Passive";

	[Header("UI Containers")]
	public Transform activeAbilitiesContainer;
	public Transform activePassiveAbilitiesContainer;
	public Transform passiveAbilitiesContainer;

	private void Start()
	{
		UpdateShop();
	}

	public void UpdateShop()
	{
#if UNITY_EDITOR
		// Load abilities from each folder
		var activeAbilities = LoadAbilitiesFromFolder(activeAbilitiesFolder);
		var activePassiveAbilities = LoadAbilitiesFromFolder(activePassiveAbilitiesFolder);
		var passiveAbilities = LoadAbilitiesFromFolder(passiveAbilitiesFolder);

		// Clear existing buttons
		ClearChildren(activeAbilitiesContainer);
		ClearChildren(activePassiveAbilitiesContainer);
		ClearChildren(passiveAbilitiesContainer);

		// Create buttons for each ability type
		CreateAbilityButtons(activeAbilities, activeAbilitiesContainer);
		CreateAbilityButtons(activePassiveAbilities, activePassiveAbilitiesContainer);
		CreateAbilityButtons(passiveAbilities, passiveAbilitiesContainer);
#endif
	}

#if UNITY_EDITOR
	private System.Collections.Generic.List<Ability> LoadAbilitiesFromFolder(string folderPath)
	{
		var abilities = new System.Collections.Generic.List<Ability>();
		var guids = AssetDatabase.FindAssets("t:Ability", new[] { folderPath });

		foreach (var guid in guids)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(guid);
			var ability = AssetDatabase.LoadAssetAtPath<Ability>(assetPath);
			if (ability != null)
				abilities.Add(ability);
		}

		return abilities;
	}
#endif

	private void CreateAbilityButtons(System.Collections.Generic.List<Ability> abilities, Transform container)
	{
		foreach (var ability in abilities)
		{
			GameObject buttonGO = Instantiate(abilityShopButtonPrefab, container);
			buttonGO.name = ability.name;

			// Set the name text if present
			Transform nameTransform = buttonGO.transform.Find("AbilityName");
			if (nameTransform != null)
			{
				var nameText = nameTransform.GetComponent<TMPro.TextMeshProUGUI>();
				if (nameText != null)
				{
					nameText.text = ability.name;
				}
			}

			// Set the image if present
			var imageComponent = buttonGO.GetComponent<Image>();
			if (imageComponent != null && ability.picture != null)
			{
				imageComponent.sprite = ability.picture;
			}

			// Assign the onClick listener, to add the functionality of adding abilities
			var button = buttonGO.GetComponent<Button>();

			if (button != null)
			{
				button.onClick.AddListener(() =>
				{
					abilityMaster.AddAbility(ability);
				});
			}
		}
	}

	private void ClearChildren(Transform parent)
	{
		if (parent == null) return;
		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			DestroyImmediate(parent.GetChild(i).gameObject);
		}
	}
}

// Custom editor to show the "Update Shop" button in the inspector
#if UNITY_EDITOR
[CustomEditor(typeof(AbilitiesShopUpdater))]
public class AbilitiesShopUpdaterEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		AbilitiesShopUpdater updater = (AbilitiesShopUpdater)target;

		if (GUILayout.Button("Update Shop"))
		{
			updater.UpdateShop();
			Debug.Log("Updated ability shop");
		}
	}
}
#endif
