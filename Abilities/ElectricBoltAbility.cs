using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ElectricBoltAbility : Ability
{
    public GameObject boltFX;
    public int totalEnemyCount, damage;
    public float maxDistance = 30f;

    public List<Enemy> affectedEnemies = new List<Enemy>();
    private GameObject closestObject; // Temp helper to find closest enemy

    public override void Activate(GameObject parent)
    {
        // Debug.Log("Electric bolt casted");
        base.Activate(parent);

        // First create the list of affected enemies
        if (FindClosestEnemy(parent, affectedEnemies) == null) // No enemies in range
        {
            Debug.Log("No enemies found for electric bolt");
            return;
        }

        affectedEnemies.Add(FindClosestEnemy(parent, affectedEnemies));
        for (int i = 0; i <= totalEnemyCount - 2; i++)
        {
            affectedEnemies.Add(FindClosestEnemy(affectedEnemies[i].gameObject, affectedEnemies));
        }

        // Create effects and deal damage etc.
        GameObject electricFX = Instantiate(boltFX);
        LineRenderer lr = electricFX.GetComponent<LineRenderer>();
        // lr.transform.SetParent(parent.transform);

        lr.positionCount = affectedEnemies.Count;
        lr.SetPosition(0, parent.transform.position);
        for (int i = 1; i < affectedEnemies.Count; i++)
        {
            lr.SetPosition(i, affectedEnemies[i].transform.position);
        }

        foreach (Enemy x in affectedEnemies)
        {
            x.TakeDamage(damage);
        }

        Debug.Log(affectedEnemies.Count);

        Destroy(lr, activeTime);
    }

    public override void BeginCooldown(GameObject parent)
    {
        // Debug.Log("Electric bolt cooldown");
        base.BeginCooldown(parent);
        affectedEnemies.Clear();
    }

    private Enemy FindClosestEnemy(GameObject searcherObject, List<Enemy> enemiesToIgnore)
    {
        float closestDistance = Mathf.Infinity;

        foreach (GameObject obj in GameManager.GM.enemiesAliveGos)
        {
            if (!enemiesToIgnore.Contains(obj.GetComponent<Enemy>()))
            {
                float distance = Vector3.Distance(searcherObject.transform.position, obj.transform.position);

                if (distance < closestDistance && distance <= maxDistance)
                {
                    closestDistance = distance;
                    closestObject = obj;
                }
            }
        }

        if (closestObject != null)
        {
            // Debug.Log("Closest enemy is " + closestObject.name);
            return closestObject.GetComponent<Enemy>();
        }
        else
        {
            // Debug.Log("No enemies found within " + maxDistance + " units.");
            return null;
        }
    }
}
