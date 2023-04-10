using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherit to all explosives like grenades, mines, abilities.
public class Explosive : MonoBehaviour
{
    [SerializeField] private float explosionRadius, explosionForce;
    [SerializeField] private int explosionDamage;
    [SerializeField] private Vector3 upVector = new Vector3(0f, 5f, 0f); // Helps explosives to throw targets in to the air a bit more.
    [SerializeField] bool activateRagdoll, canDismember;

    private List<Enemy> damagedEnemies = new List<Enemy>();

    void Detonate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position - upVector, explosionRadius);
            }

            Enemy enemy = nearbyObject.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                if (!damagedEnemies.Contains(enemy))
                {
                    float distance = Vector3.Distance(enemy.GetComponentInParent<Transform>().position, transform.position);
                    float locationalPercentage = 1f - (distance / explosionRadius); // eg. distance 2 and radius 8 = deal 75% damage: 1 - (2 / 8) = 0,75
                    float calculatedDamage = explosionDamage * locationalPercentage;
                    int roundedDamage = (int)calculatedDamage;
                    if (roundedDamage < 0) roundedDamage = 0;
                    enemy.TakeDamage(roundedDamage);
                    damagedEnemies.Add(enemy);

                    if (activateRagdoll) enemy.TurnOnRagdoll();

                    if (canDismember)
                    {
                        if (enemy.GetHealth() > 50)
                        {
                            LimbManager limbScript = enemy.GetComponent<LimbManager>();

                            if (UnityEngine.Random.Range(0, 4) == 1) limbScript.RemoveLimb(1); // LeftLowerLeg
                            if (UnityEngine.Random.Range(0, 4) == 1) limbScript.RemoveLimb(2); // LeftUpperLeg
                            if (UnityEngine.Random.Range(0, 4) == 1) limbScript.RemoveLimb(3); // RightLowerLeg
                            if (UnityEngine.Random.Range(0, 4) == 1) limbScript.RemoveLimb(4); // RightUpperLeg

                            if (UnityEngine.Random.Range(0, 4) == 1) limbScript.RemoveLimb(5); // RightArm     
                            if (UnityEngine.Random.Range(0, 4) == 1) limbScript.RemoveLimb(6); // RightShoulder
                            if (UnityEngine.Random.Range(0, 4) == 1) limbScript.RemoveLimb(7); // LeftArm     
                            if (UnityEngine.Random.Range(0, 4) == 1) limbScript.RemoveLimb(8); // LeftShoulder
                        }
                    }
                }
            }
        }

    }

}
