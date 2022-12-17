using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradesMenu : MonoBehaviour
{

    private Gun chosenGun;
    private AttachmentsScript attchiesScript;

    public TextMeshProUGUI selectedGunText;


    private void OnEnable()
    {
        chosenGun = GameManager.GM.GetCurrentGun();
        if (chosenGun != null) attchiesScript = chosenGun.GetComponent<AttachmentsScript>();

        if (chosenGun != null)
        {
            selectedGunText.text = chosenGun.gunName;
        }
        else
        {
            selectedGunText.text = "No equipped guns!";
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
