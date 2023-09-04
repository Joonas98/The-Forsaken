using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UpgradesMenu : MonoBehaviour
{
    public TextMeshProUGUI selectedWeaponText;
    public GameObject scopesGroup, muzzlesGroup, gripsGroup;

    private Gun chosenGun;
    private AttachmentsScript attchiesScript;

    private void OnEnable()
    {
        chosenGun = GameManager.GM.GetCurrentGun();
        if (chosenGun != null) attchiesScript = chosenGun.GetComponent<AttachmentsScript>();

        if (chosenGun != null) selectedWeaponText.text = chosenGun.weaponName;
        else selectedWeaponText.text = "No equipped guns!";

        EnableAllAttachmentButtons();
        FilterAttachments();
    }

    public void MenuEquipScope(int scopeIndex)
    {
        attchiesScript.EquipScope(scopeIndex);
    }

    public void MenuEquipMuzzle(int muzzleIndex)
    {
        attchiesScript.EquipMuzzle(muzzleIndex);
    }

    public void MenuEquipGrip(int gripIndex)
    {
        attchiesScript.EquipGrip(gripIndex);
    }

    // Called OnEnable to enable all attachment buttons, then we disable unwanted depending on the gun
    public void EnableAllAttachmentButtons()
    {
        scopesGroup.SetActive(true);
        muzzlesGroup.SetActive(true);
        gripsGroup.SetActive(true);

        for (int i = 0; i < scopesGroup.transform.childCount; i++)
        {
            Transform child = scopesGroup.transform.GetChild(i);
            child.gameObject.SetActive(true);
        }

        for (int i = 0; i < muzzlesGroup.transform.childCount; i++)
        {
            Transform child = muzzlesGroup.transform.GetChild(i);
            child.gameObject.SetActive(true);
        }

        for (int i = 0; i < gripsGroup.transform.childCount; i++)
        {
            Transform child = gripsGroup.transform.GetChild(i);
            child.gameObject.SetActive(true);
        }
    }

    // All attachments are not available for all guns
    public void FilterAttachments()
    {
        if (attchiesScript == null) return;
        FilterScopes();
        FilterMuzzles();
        FilterGrips();
    }

    private void FilterScopes()
    {
        if (attchiesScript.scopes.Length == 0)
        {
            scopesGroup.SetActive(false);
        }
        else
        {
            for (int i = 0; i < attchiesScript.unavailableScopes.Length; i++)
            {
                int scopeIndex = attchiesScript.unavailableScopes[i];
                if (scopeIndex >= 0 && scopeIndex < scopesGroup.transform.childCount)
                {
                    scopesGroup.transform.GetChild(scopeIndex).gameObject.SetActive(false);
                }
            }
        }
    }

    private void FilterMuzzles()
    {
        if (attchiesScript.muzzleDevices.Length == 0) muzzlesGroup.SetActive(false);
    }

    private void FilterGrips()
    {
        if (attchiesScript.grips.Length == 0) gripsGroup.SetActive(false);
    }

}
