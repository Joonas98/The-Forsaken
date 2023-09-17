using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Dash")]
public class DashAbility : Ability
{
    public float speedMultiplier;
    public float speedDuration;

    public override void Activate(GameObject parent)
    {
        // Debug.Log("Dash activated");
        base.Activate(parent);
        PlayerMovement movementScript = parent.GetComponent<PlayerMovement>();
        movementScript.ApplySpeedEffect(speedMultiplier, speedDuration);
    }

    public override void BeginCooldown(GameObject parent)
    {
        // Debug.Log("Dash ended");
        base.BeginCooldown(parent);
    }
}
