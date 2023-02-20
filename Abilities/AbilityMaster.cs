using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Global holder for ability data
public class AbilityMaster : MonoBehaviour
{
    // Statics
    public static AbilityMaster instance = null;
    public static List<int> abilities = new List<int>();

    // Monos
    public GameObject player;
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

        keycodesList.Add(KeyCode.Z);
        keycodesList.Add(KeyCode.X);
        keycodesList.Add(KeyCode.C);
        keycodesList.Add(KeyCode.V);
    }

    public void AddAbility(int abilityNumber)
    {
        if (ownedAbilities.Contains(abilityNumber)) return;
        ownedAbilities.Add(abilityNumber);
        abilities.Add(abilityNumber);

        AbilityHolder abilityHolder = player.AddComponent<AbilityHolder>();
        abilityHolder.ability = abilitiesList[abilityNumber];
        GameObject newAbility = Instantiate(abilityPrefab);
        newAbility.transform.SetParent(abilitiesParent);

        Image[] abilityImages = newAbility.GetComponentsInChildren<Image>();
        foreach (Image img in abilityImages)
        {
            img.sprite = abilitiesList[abilityNumber].picture;
        }
        abilityImages[0].sprite = null;

        abilityHolder.abilityImage = abilityImages[2];
        abilityHolder.backgroundImage = abilityImages[0];

        TextMeshProUGUI hotkeyText = newAbility.GetComponentInChildren<TextMeshProUGUI>();

        // Assign hotkeys to active abilities and display the hotkey
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
        }
        else
        {
            hotkeyText.text = ""; // No hotkey on passives
        }

    }

}
