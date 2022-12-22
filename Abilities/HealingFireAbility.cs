using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class HealingFireAbility : Ability
{
    public float healingDuration, healingInterval;
    public int healingAmount;
    public GameObject healingFireBall;

    public override void Activate(GameObject parent)
    {
        // Debug.Log("Healing fire casted");
        GameObject healingBall = Instantiate(healingFireBall, Camera.main.transform);
        HealingFireball ballScript = healingBall.GetComponent<HealingFireball>();
        ballScript.healAmount = healingAmount;
        ballScript.healDuration = healingDuration;
        ballScript.healInterval = healingInterval;
        audioSource.PlayOneShot(activateSFX);
    }

    public override void BeginCooldown(GameObject parent)
    {
        // Debug.Log("Dash ended");
    }

}
