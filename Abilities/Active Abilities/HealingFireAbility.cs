using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/HealingFire")]
public class HealingFireAbility : Ability
{
    public float healingDuration, healingInterval;
    public int healingAmount;
    public GameObject healingFireBall;

    public override void Activate(GameObject parent)
    {
        // Debug.Log("Healing fire casted");
        base.Activate(parent);
        GameObject healingBall = Instantiate(healingFireBall, Camera.main.transform);
        HealingFireball ballScript = healingBall.GetComponent<HealingFireball>();
        ballScript.healAmount = healingAmount;
        ballScript.healDuration = healingDuration;
        ballScript.healInterval = healingInterval;
        if (audioSource != null && activateSFX != null) audioSource.PlayOneShot(activateSFX);
    }

    public override void BeginCooldown(GameObject parent)
    {
        // Debug.Log("Healing ball cooldown");
        base.BeginCooldown(parent);
    }

}
