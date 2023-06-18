using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/UkkoBlast")]
public class UkkoBlastAbility : Ability
{
    public GameObject boltFX;
    public int totalEnemyCount, damage;
    public float maxDistance = 30f;

    private List<Enemy> affectedEnemies, damagedEnemies;
    private GameObject closestObject; // Temp helper to find closest enemy

    public override void Activate(GameObject parent)
    {
        // Debug.Log("Electric bolt casted");
        base.Activate(parent);

        // First create the list of affected enemies
        affectedEnemies = new List<Enemy>(totalEnemyCount - 1);
        damagedEnemies = new List<Enemy>(totalEnemyCount - 1);
        if (FindClosestEnemy(parent, affectedEnemies) == null) // No enemies in range
        {
            Debug.Log("No enemies found for Ukko Blast");
            return;
        }

        affectedEnemies.Add(FindClosestEnemy(parent, affectedEnemies));

        for (int i = 0; i < totalEnemyCount - 1; i++)
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
            if (x != null && !damagedEnemies.Contains(x))
            {
                x.TakeDamage(damage);
                damagedEnemies.Add(x);
                x.StartCoroutine(x.ApplyDebuff(Enemy.debuffs.ShockBlue, 2f));
            }
        }

        Destroy(electricFX, activeTime);
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
