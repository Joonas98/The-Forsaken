using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{

    [SerializeField] private bool isIncendiary;
    [SerializeField] private bool isImpact;
    [SerializeField] private bool isMine;

    public GameObject firePrefab;
    [SerializeField] private float fireDuration, fireInterval;
    [SerializeField] private int fireDamage;

    [SerializeField] private float explosionDelay;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;
    [SerializeField] private int explosionDamage;
    [SerializeField] private Vector3 upVector = new Vector3(0f, 5f, 0f);
    [HideInInspector] private float countdown;
    [HideInInspector] private bool hasExploded = false;
    [HideInInspector] private bool isArmed = false;

    public GameObject explosionEffect;
    public GameObject explosionEffect2;
    public GameObject collisionSphere;

    public AudioSource audioSource;
    public AudioClip explosionSound;
    public AudioClip pinSound;

    public MeshRenderer meshRenderer;

    private List<Enemy> damagedEnemies = new List<Enemy>();

    void Start()
    {
        countdown = explosionDelay;
        StartCoroutine(ArmingDelay(countdown));
    }

    private void Awake()
    {
        audioSource.PlayOneShot(pinSound);
    }

    void Update()
    {
        if (!isImpact && !isMine)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f && !hasExploded)
            {
                if (!isIncendiary && !isMine)
                {
                    Explode();
                }
                else
                {
                    ExplodeIncendiary();
                }
            }
        }
    }

    public void Explode()
    {
        if (!hasExploded)
        {
            audioSource.PlayOneShot(explosionSound);
            hasExploded = true;
            Instantiate(explosionEffect, transform.position, Quaternion.LookRotation(Vector3.down));
            if (explosionEffect2 != null) Instantiate(explosionEffect2, transform.position, Quaternion.LookRotation(Vector3.up));

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

            foreach (Collider nearbyObject in colliders)
            {
                Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position - upVector, explosionRadius);
                }

                Enemy enemy = nearbyObject.GetComponentInParent<Enemy>();
                if (enemy != null && !enemy.isDead)
                {
                    if (!damagedEnemies.Contains(enemy))
                    {
                        float distance = Vector3.Distance(enemy.GetComponentInParent<Transform>().position, transform.position);
                        float locationalPercentage = 1f - (distance / explosionRadius); // esim. distance 2 ja radius 8 eli tee 75% damagesta: 1 - (2 / 8) = 0,75
                        float calculatedDamage = explosionDamage * locationalPercentage;
                        int roundedDamage = (int)calculatedDamage;
                        enemy.TakeDamage(roundedDamage);
                        damagedEnemies.Add(enemy);

                        if (enemy.GetHealth() > 50)
                        {
                            LimbManager limbScript = enemy.GetComponent<LimbManager>();

                            if (UnityEngine.Random.Range(0, 2) == 1)
                            {
                                limbScript.RemoveLimb(1); // LeftLowerLeg
                                if (!enemy.isCrawling)
                                    enemy.StartCrawling();
                            }

                            if (UnityEngine.Random.Range(0, 2) == 1)
                            {
                                limbScript.RemoveLimb(2); // LeftUpperLeg
                                if (!enemy.isCrawling)
                                    enemy.StartCrawling();
                            }

                            if (UnityEngine.Random.Range(0, 2) == 1)
                            {
                                limbScript.RemoveLimb(3); // RightLowerLeg
                                if (!enemy.isCrawling)
                                    enemy.StartCrawling();
                            }

                            if (UnityEngine.Random.Range(0, 2) == 1)
                            {
                                limbScript.RemoveLimb(4); // RightUpperLeg
                                if (!enemy.isCrawling)
                                    enemy.StartCrawling();
                            }

                            if (UnityEngine.Random.Range(0, 2) == 1) limbScript.RemoveLimb(5); // RightArm     
                            if (UnityEngine.Random.Range(0, 2) == 1) limbScript.RemoveLimb(6); // RightShoulder
                            if (UnityEngine.Random.Range(0, 2) == 1) limbScript.RemoveLimb(7); // LeftArm     
                            if (UnityEngine.Random.Range(0, 2) == 1) limbScript.RemoveLimb(8); // LeftShoulder
                        }
                    }
                }

                // Grenades blow up other grenades
                Grenade grenade = nearbyObject.GetComponentInParent<Grenade>();
                if (grenade != null)
                {
                    if (!grenade.isIncendiary)
                    {
                        grenade.Explode();
                    }
                    else
                    {
                        grenade.ExplodeIncendiary();
                    }

                }
            }
        }
        meshRenderer.enabled = false;
        Destroy(gameObject, 3f);
    }

    void ExplodeIncendiary()
    {
        GameObject fireObject = Instantiate(firePrefab, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity);
        Fire fireScript = fireObject.GetComponent<Fire>();
        fireScript.SetDuration(fireDuration);
        fireScript.damage = fireDamage;
        fireScript.damageInterval = fireInterval;
        meshRenderer.enabled = false;
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") && isArmed == true)
        {
            if (!isIncendiary && isImpact)
            {
                Explode();
            }
            else if (isIncendiary && isImpact)
            {
                ExplodeIncendiary();
            }
        }
    }

    IEnumerator ArmingDelay(float r)
    {
        yield return new WaitForSeconds(r);
        isArmed = true;
    }

}
