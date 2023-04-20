using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Dash")]
public class DashAbility : Ability
{
    public float dashSpeed;

    public override void Activate(GameObject parent)
    {
        // Debug.Log("Dash activated");
        base.Activate(parent);
        PlayerMovement movementScript = parent.GetComponent<PlayerMovement>();
        movementScript.Run(false);
        movementScript.speed = movementScript.speed * dashSpeed;
        movementScript.canRun = false;
    }

    public override void BeginCooldown(GameObject parent)
    {
        // Debug.Log("Dash ended");
        base.BeginCooldown(parent);
        PlayerMovement movementScript = parent.GetComponent<PlayerMovement>();
        movementScript.speed = movementScript.ogSpeed;
        movementScript.canRun = true;
    }


}
