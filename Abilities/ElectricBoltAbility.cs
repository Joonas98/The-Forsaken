using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ElectricBolt")]
public class ElectricBoltAbility : Ability
{
    public GameObject boltFX;
    public int totalEnemyCount, damage;
    public float maxDistance = 30f;

    public List<Enemy> affectedEnemies;
    private GameObject closestObject; // Temp helper to find closest enemy

    public override void Activate(GameObject parent)
    {
        // Debug.Log("Electric bolt casted");
        base.Activate(parent);

        // First create the list of affected enemies
        affectedEnemies = new List<Enemy>(totalEnemyCount);
        if (FindClosestEnemy(parent, affectedEnemies) == null) // No enemies in range
        {
            Debug.Log("No enemies found for electric bolt");
            return;
        }

        affectedEnemies.Add(FindClosestEnemy(parent, affectedEnemies));

        for (int i = 0; i < totalEnemyCount; i++)
        {
            // Find chain of enemies, each enemy can appear in the list only once
            affectedEnemies.Add(FindClosestEnemy(affectedEnemies[i].gameObject, affectedEnemies).GetComponent<Enemy>());
            // Debug.Log("Found enemy");
        }

        // Create effects and deal damage etc.
        GameObject electricFX = Instantiate(boltFX);
        electricFX.transform.SetParent(GameManager.GM.playerGO.transform);
        DigitalRuby.ThunderAndLightning.LightningBoltPathScript boltScript = electricFX.GetComponent<DigitalRuby.ThunderAndLightning.LightningBoltPathScript>();
        boltScript.Camera = Camera.main;
        boltScript.LightningPath[0] = GameManager.GM.playerGO; // The FX starts from player

        foreach (Enemy obj in affectedEnemies)
        {
            if (!boltScript.LightningPath.Contains(obj.modelRoot)) boltScript.LightningPath.Add(obj.modelRoot);
        }

        foreach (Enemy x in affectedEnemies)
        {
            if (x != null)
            {
                x.TakeDamage(damage);
                x.StartCoroutine(x.ApplyDebuff(Enemy.debuffs.ShockBlue, 2f));
            }
        }

        Destroy(electricFX, activeTime);

        #region
        // If FX uses line renderer
        // LineRenderer lr = electricFX.GetComponent<LineRenderer>();
        // // lr.transform.SetParent(parent.transform);
        //
        // lr.positionCount = affectedEnemies.Count;
        // lr.SetPosition(0, parent.transform.position);
        // for (int i = 1; i < affectedEnemies.Count; i++)
        // {
        //     lr.SetPosition(i, affectedEnemies[i].transform.position);
        // }
        #endregion
    }

    public override void BeginCooldown(GameObject parent)
    {
        // Debug.Log("Electric bolt cooldown");
        base.BeginCooldown(parent);
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
