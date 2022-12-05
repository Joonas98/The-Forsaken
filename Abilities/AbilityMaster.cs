using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Globaali abilitydatan pitäjä
public class AbilityMaster : MonoBehaviour
{
    // Staattiset
    public static AbilityMaster instance = null;
    public static List<int> abilities = new List<int>();

    // Monot
    public GameObject player;
    public List<Ability> abilitiesList = new List<Ability>();

    public GameObject abilityPrefab;
    public Transform abilitiesParent;

    private List<int> ownedAbilities = new List<int>();

    private void Awake()
    {
        // Singleton logiikka
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

        if (abilitiesParent.transform.childCount == 1) abilityHolder.key = KeyCode.Z;
        if (abilitiesParent.transform.childCount == 2) abilityHolder.key = KeyCode.X;
        if (abilitiesParent.transform.childCount == 3) abilityHolder.key = KeyCode.C;
        if (abilitiesParent.transform.childCount == 4) abilityHolder.key = KeyCode.V;
    }

}
