using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Barbedwire : MonoBehaviour
{

    public float slowMultiplier;

    private Enemy enemyScript;
    private Enemy enemyScriptExit;
    private List<Enemy> collidedEnemies = new List<Enemy>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Torso"))
        {
            enemyScript = other.GetComponentInParent<Enemy>();
            if (!collidedEnemies.Contains(enemyScript) && enemyScript != null)
            {
                enemyScript.SlowDown(slowMultiplier);

                if (!collidedEnemies.Contains(enemyScript))
                    collidedEnemies.Add(enemyScript);

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Torso"))
        {
            enemyScriptExit = other.GetComponentInParent<Enemy>();

            if (collidedEnemies.Contains(enemyScriptExit))
            {
                enemyScriptExit.RestoreMovementSpeed();
                collidedEnemies.Remove(enemyScriptExit);
            }
        }
    }
}
