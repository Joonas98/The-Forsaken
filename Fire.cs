using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{

    public float damageInterval;
    public int damage;
    [HideInInspector] public float radius; // radius = localScale.x / 2

    public ParticleSystem firePS;

    private Enemy enemyScript;
    private Enemy enemyScriptExit;
    private List<Enemy> collidedEnemies = new List<Enemy>();

    private float damageCounter;

    private void Awake()
    {
        damageCounter = damageInterval;

        radius = transform.localScale.x / 2;

        // Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        // foreach (Collider collider in colliders)
        // {
        //     if (collider.CompareTag("Torso") && collider.gameObject.layer == 2)
        //     {
        //         Enemy enemyScriptStart = collider.gameObject.GetComponentInParent<Enemy>();
        //         if (!collidedEnemies.Contains(enemyScriptStart) && enemyScriptStart != null)
        //         {
        //             if (!collidedEnemies.Contains(enemyScriptStart))
        //                 collidedEnemies.Add(enemyScriptStart);
        //             print(enemyScriptStart);
        //         }
        //     }
        // }
    }

    private void Update()
    {
        CalculateDamageIntervals();
    }

    private void CalculateDamageIntervals() // Ajastin joka vahingoittaa kaikkia vihollisia listassa
    {
        if (damageCounter <= 0)
        {
            damageCounter = damageInterval;
            // foreach (Enemy enemyscript in collidedEnemies)
            // {
            //     if (enemyScript != null) enemyScript.TakeDamage(damage);
            // }

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Torso") && collider.gameObject.layer == 2)
                {
                    Enemy enemyScriptStart = collider.gameObject.GetComponentInParent<Enemy>();
                    enemyScriptStart.TakeDamage(damage);
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
