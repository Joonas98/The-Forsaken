using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradesMenu : MonoBehaviour
{
    public TextMeshProUGUI selectedWeaponText;

    private Gun chosenGun;
    private AttachmentsScript attchiesScript;

    private void OnEnable()
    {
        chosenGun = GameManager.GM.GetCurrentGun();
        if (chosenGun != null) attchiesScript = chosenGun.GetComponent<AttachmentsScript>();

        if (chosenGun != null)
        {
            selectedWeaponText.text = chosenGun.weaponName;
        }
        else
        {
            selectedWeaponText.text = "No equipped guns!";
        }
    }

    public void MenuEquipScope(int scopeIndex)
    {
        attchiesScript.EquipScope(scopeIndex);
    }

    public void MenuEquipMuzzle(int muzzleIndex)
    {
        attchiesScript.EquipSilencer(muzzleIndex);
    }

    public void MenuEquipGrip(int gripIndex)
    {
        attchiesScript.EquipGrip(gripIndex);
    }

}
