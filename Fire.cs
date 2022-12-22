using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public bool healingFire = false;
    public float damageInterval;
    public int damage;
    [HideInInspector] public float radius; // radius = localScale.x / 2

    public AudioSource audioSource;
    public AudioClip startSFX;

    private float damageCounter;

    private void Awake()
    {
        damageCounter = damageInterval;
        radius = transform.localScale.x / 2;
        if (audioSource != null && startSFX != null) audioSource.PlayOneShot(startSFX);
    }

    private void Update()
    {
        CalculateDamageIntervals();
    }

    private void CalculateDamageIntervals()
    {
        if (damageCounter <= 0)
        {
            damageCounter = damageInterval;

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Torso") && collider.gameObject.layer == 2)
                {
                    Enemy enemyScriptStart = collider.gameObject.GetComponentInParent<Enemy>();

                    if (!healingFire)
                    {
                        enemyScriptStart.TakeDamage(damage);
                    }
                    else
                    {
                        enemyScriptStart.TakeDamage(damage * -1);
                    }
                }

                if (collider.CompareTag("Player"))
                {
                    Player playerScript = collider.gameObject.GetComponentInParent<Player>();
                    if (playerScript == null) return;

                    if (!healingFire)
                    {
                        playerScript.TakeDamage(damage, 0.1f);
                    }
                    else
                    {
                        playerScript.Heal(damage);
                    }
                }

            }

        }
        else
        {
            damageCounter -= Time.deltaTime;
        }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Torso") && other.gameObject.layer == 2)
    //     {
    //         enemyScript = other.GetComponentInParent<Enemy>();
    //         if (!collidedEnemies.Contains(enemyScript) && enemyScript != null)
    //         {
    //             if (!collidedEnemies.Contains(enemyScript))
    //                 collidedEnemies.Add(enemyScript);
    //
    //         }
    //     }
    // }
    //
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Torso") && other.gameObject.layer == 2)
    //     {
    //         enemyScriptExit = other.GetComponentInParent<Enemy>();
    //
    //         if (collidedEnemies.Contains(enemyScriptExit))
    //         {
    //             collidedEnemies.Remove(enemyScriptExit);
    //         }
    //     }
    // }

    public void SetDuration(float duration)
    {
        Destroy(gameObject, duration);
    }

}
