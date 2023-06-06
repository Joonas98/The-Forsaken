using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private float damageCounter;
    private bool stopped = false;
    private float lightCountDown;

    private void Awake()
    {
        damageCounter = damageInterval;

        ParticleSystem.ShapeModule sm = ps.shape;
        sm.radius = radius;
        if (audioSource != null && startSFX != null) audioSource.PlayOneShot(startSFX);
    }

    private void Update()
    {
        CalculateDamageIntervals();
    }

    private void CalculateDamageIntervals()
    {
        if (stopped) return; // Stopped it used to let particle systems finish before destroying this gameobject
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
        Debug.Log("Fire initialized");
        Invoke("StopFire", duration);
    }

    public void StopFire()
    {
        Debug.Log("Stopping fire");
        stopped = true;
        ps.Stop();
        fireLight.intensity /= 2f;
        Destroy(gameObject, 3f); // 3f to let the fire particles finish
    }

}
