using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ViperVenom")]
public class ViperVenomAbility : Ability
{
    // Each bullet deals 5% of enemy's max HP as dmg

    public override void InitializeAbility()
    {
        base.InitializeAbility();
        // Refresh current gun stats
        // 4.7.2023 Added, not working because this code does not execute
        if (GameManager.GM.currentGun != null) GameManager.GM.currentGun.RefreshGun();
        Debug.Log("Gun refreshed: " + GameManager.GM.currentGun);
    }

}
