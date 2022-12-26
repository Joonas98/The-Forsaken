using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DashAbility : Ability
{
    public float dashSpeed;

    public override void Activate(GameObject parent)
    {
        // Debug.Log("Dash activated");
        PlayerMovement movementScript = parent.GetComponent<PlayerMovement>();
        movementScript.Run(false);
        movementScript.speed = movementScript.speed * dashSpeed;
        movementScript.canRun = false;
        if (audioSource != null && activateSFX != null) audioSource.PlayOneShot(activateSFX);
    }

    public override void BeginCooldown(GameObject parent)
    {
        // Debug.Log("Dash ended");
        PlayerMovement movementScript = parent.GetComponent<PlayerMovement>();
        movementScript.speed = movementScript.ogSpeed;
        movementScript.canRun = true;
        if (audioSource != null && activateSFX != null) audioSource.PlayOneShot(endSFX);
    }


}
