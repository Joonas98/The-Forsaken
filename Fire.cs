using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using SCPE;
using static Enemy;

public class Fire : MonoBehaviour
{
	public bool healingFire = false;
	public float damageInterval;
	public int damage;
	public float radius;
	public float debuffDurationPerTick; // If >0, apply fire debuff each tick

	public AudioSource audioSource;
	public AudioClip startSFX;
	public ParticleSystem ps;
	public Light fireLight;
	public PostProcessVolume ppVolume;

	private Colorize ppColorize;
	private float damageCounter;
	private bool stopped = false;
	private float stopIntensitySpeed = 1f; // When fire ends, how fast we lerp the ligh out
	private List<Enemy> damagedEnemies = new List<Enemy>();

	private void Awake()
	{
		damageCounter = damageInterval;

		ParticleSystem.ShapeModule sm = ps.shape;
		sm.radius = radius;
		if (audioSource != null && startSFX != null) audioSource.PlayOneShot(startSFX);

		if (ppVolume == null) ppVolume = GetComponentInChildren<PostProcessVolume>();
	}

	private void Start()
	{
		ppVolume.profile.TryGetSettings(out ppColorize);
	}

	private void Update()
	{
		CalculateDamageIntervals();
	}

	private void CalculateDamageIntervals()
	{
		// Stopped it used to let particle systems finish before destroying this gameobject
		if (stopped)
		{
			fireLight.intensity = Mathf.Lerp(fireLight.intensity, 0f, stopIntensitySpeed * Time.deltaTime);
			ppColorize.intensity.value = Mathf.Lerp(ppColorize.intensity.value, 0f, stopIntensitySpeed * Time.deltaTime);
			return;
		}
		if (damageCounter <= 0) // Time for damage tick
		{
			damageCounter = damageInterval;
			damagedEnemies.Clear();
			Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
			foreach (Collider collider in colliders)
			{
				// Found object is enemy
				if (collider.gameObject.layer == 11)
				{
					// Reference enemy script
					Enemy enemyScript = collider.gameObject.GetComponentInParent<Enemy>();
					if (enemyScript == null) return;

					// Reference debuff manager, if we want to set them on fire
					DebuffManager debuffManager;
					debuffManager = enemyScript.GetComponent<DebuffManager>();

					// Make sure we don't damage the same enemy multiple times
					if (!damagedEnemies.Contains(enemyScript))
					{
						if (!healingFire)
						{
							// Direct damage from the fire
							enemyScript.TakeDamage(damage, 0, DamageType.Fire);

							// Apply fire debuff
							if (debuffDurationPerTick > 0)
							{
								debuffManager.ApplyDebuff(DebuffManager.Debuffs.Fire, debuffDurationPerTick);
							}
						}
						else
						{
							// Todo: heal function to enemies
							enemyScript.Heal(damage);
						}

						damagedEnemies.Add(enemyScript); // Add the enemy to the list of damaged enemies
					}
				}

				// Found object the player
				if (collider.CompareTag("Player"))
				{
					Player playerScript = collider.gameObject.GetComponentInParent<Player>();
					if (playerScript == null) return;

					if (!healingFire)
					{
						playerScript.TakeDamage(damage, 0f);
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

	public void InitializeFire(float duration)
	{
		// Debug.Log("Fire initialized");
		Invoke(nameof(StopFire), duration);
	}

	public void StopFire()
	{
		// Debug.Log("Stopping fire");
		stopped = true;
		ps.Stop();
		Destroy(gameObject, 3f); // 3f to let the fire particles finish
	}

}
