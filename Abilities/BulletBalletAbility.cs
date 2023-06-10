using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/BulletBallet")]
public class BulletBalletAbility : Ability
{
    // This passive ability allows the player to shoot while running

    private void Awake()
    {
        Debug.Log("Added bullet ballet");
        // If we have a gun, reset the rotation
        if (GameManager.GM.GetCurrentGun() != null) GameManager.GM.GetCurrentGun().ResetRotation();
    }
}
