using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimsonCastParticle : MonoBehaviour
{
	// Find enemies that got hit by the particle and apply crimson debuff on them

	private List<ParticleCollisionEvent> collisionEvents;
	private ParticleSystem ps;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		collisionEvents = new List<ParticleCollisionEvent>();
	}

	private void OnParticleCollision(GameObject other)
	{
		ps.GetCollisionEvents(other, collisionEvents);

		// Can't hit player to prevent the ability exploding right away
		if (!other.CompareTag("Player"))
		{
			Destroy(gameObject, 1f);
		}
	}
}
