using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SelfExplosion")]
public class SelfExplosionAbility : Ability
{
    public int damage;
    public float radius, force;
    public ParticleSystem explosionEffect;
    public AudioClip explosionSound;
    private List<Enemy> damagedEnemies = new List<Enemy>();
    [SerializeField] private Vector3 upVector = new Vector3(0f, 5f, 0f);

    public override void Activate(GameObject parent)
    {
        // Debug.Log("Explosion activated");
        base.Activate(parent);

        if (audioSource != null && explosionSound != null) audioSource.PlayOneShot(explosionSound);
        ParticleSystem xplosion = Instantiate(explosionEffect, new Vector3(parent.transform.position.x, parent.transform.position.y - 1, parent.transform.position.z), Quaternion.LookRotation(Vector3.down));
        Destroy(xplosion.gameObject, 3f);

        Collider[] colliders = Physics.OverlapSphere(parent.transform.position, radius);

        parent.GetComponent<Player>().TakeDamage(damage);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(force, parent.transform.position - upVector, radius);
            }

            Enemy enemy = nearbyObject.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                if (!damagedEnemies.Contains(enemy))
                {
                    // float distance = Vector3.Distance(enemy.GetComponentInParent<Transform>().position, parent.transform.position);
                    // float locationalPercentage = 1f - (distance / radius); // esim. distance 2 ja radius 8 eli tee 75% damagesta: 1 - (2 / 8) = 0,75
                    // float calculatedDamage = damage * locationalPercentage;
                    // int roundedDamage = (int)calculatedDamage;
                    enemy.TakeDamage(damage);
                    damagedEnemies.Add(enemy);

                    /*
                    if (enemy.GetHealth() > 50)
                    {
                        LimbManager limbScript = enemy.GetComponent<LimbManager>();

                        if (UnityEngine.Random.Range(0, 2) == 1)
                        {
                            limbScript.RemoveLimb(1); // LeftLowerLeg
                            if (!enemy.isCrawling)
                                enemy.StartCrawling();
                        }

                        if (UnityEngine.Random.Range(0, 2) == 1)
                        {
                            limbScript.RemoveLimb(2); // LeftUpperLeg
                            if (!enemy.isCrawling)
                                enemy.StartCrawling();
                        }

                        if (UnityEngine.Random.Range(0, 2) == 1)
                        {
                            limbScript.RemoveLimb(3); // RightLowerLeg
                            if (!enemy.isCrawling)
                                enemy.StartCrawling();
                        }

                        if (UnityEngine.Random.Range(0, 2) == 1)
                        {
                            limbScript.RemoveLimb(4); // RightUpperLeg
                            if (!enemy.isCrawling)
                                enemy.StartCrawling();
                        }

                        if (UnityEngine.Random.Range(0, 2) == 1) limbScript.RemoveLimb(5); // RightArm     
                        if (UnityEngine.Random.Range(0, 2) == 1) limbScript.RemoveLimb(6); // RightShoulder
                        if (UnityEngine.Random.Range(0, 2) == 1) limbScript.RemoveLimb(7); // LeftArm     
                        if (UnityEngine.Random.Range(0, 2) == 1) limbScript.RemoveLimb(8); // LeftShoulder
                }
                    */
                }
            }
        }
    }

    public override void BeginCooldown(GameObject parent)
    {
        // Debug.Log("Explosion ended");
        base.BeginCooldown(parent);
        damagedEnemies.Clear();
    }
}
