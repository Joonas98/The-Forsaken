using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingFireball : MonoBehaviour
{
    // Basically get the first intersection where particleSystem collides
    // and then spawn the healing fire prefab there

    public GameObject healFirePrefab;
    public float healDuration, healInterval;
    public int healAmount;

    private List<ParticleCollisionEvent> collisionEvents;
    private ParticleSystem ps;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    public void SpawnHealingFire(Vector3 pos)
    {
        GameObject fireObject = Instantiate(healFirePrefab, new Vector3(pos.x, pos.y + 0.5f, pos.z), Quaternion.identity);
        Fire fireScript = fireObject.GetComponent<Fire>();
        fireScript.healingFire = true;
        fireScript.InitializeFire(healDuration);
        fireScript.damage = healAmount;
        fireScript.damageInterval = healInterval;
        Destroy(gameObject);
    }

    private void OnParticleCollision(GameObject other)
    {
        ps.GetCollisionEvents(other, collisionEvents);

        // Can't hit player to prevent the ability exploding right away
        if (!other.CompareTag("Player"))
        {
            SpawnHealingFire(collisionEvents[0].intersection);
        }
    }

}
