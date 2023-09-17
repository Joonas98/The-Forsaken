using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using SCPE;

public class Fire : MonoBehaviour
{
    public bool healingFire = false;
    public float damageInterval;
    public int damage;
    public float radius;

    public AudioSource audioSource;
    public AudioClip startSFX;
    public ParticleSystem ps;
    public Light fireLight;
    public PostProcessVolume ppVolume;

    private Colorize ppColorize;
    private float damageCounter;
    private bool stopped = false;
    private float stopIntensitySpeed = 1f; // When fire ends, how fast we lerp the ligh out
    private float lerpTimer = 0f;
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
        if (damageCounter <= 0)
        {
            damageCounter = damageInterval;
            damagedEnemies.Clear();
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider collider in colliders)
            {
				if (collider.gameObject.layer == 11)
				{
					Enemy enemyScript = collider.gameObject.GetComponentInParent<Enemy>();

					if (!damagedEnemies.Contains(enemyScript))
					{
						if (!healingFire)
						{
							enemyScript.TakeDamage(damage);
						}
						else
						{
							enemyScript.TakeDamage(damage * -1);
						}

						damagedEnemies.Add(enemyScript); // Add the enemy to the list of damaged enemies
					}
				}

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
        // fireLight.intensity /= 2f;
        Destroy(gameObject, 3f); // 3f to let the fire particles finish
    }

}
