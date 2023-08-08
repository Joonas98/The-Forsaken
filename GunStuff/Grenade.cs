using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private bool isIncendiary, isImpact, isMine, activateRagdoll;

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
        if (hasExploded) return;

        meshRenderer.enabled = false;
        Destroy(gameObject, 3f);

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
            if (enemy != null)
            {
                if (!damagedEnemies.Contains(enemy))
                {
                    float distance = Vector3.Distance(enemy.GetComponentInParent<Transform>().position, transform.position);
                    float locationalPercentage = 1f - (distance / explosionRadius); // eg. distance 2 and radius 8 = deal 75% damage: 1 - (2 / 8) = 0,75
                    float calculatedDamage = explosionDamage * locationalPercentage;
                    int roundedDamage = (int)calculatedDamage;
                    if (roundedDamage < 0) roundedDamage = 0;
                    enemy.TakeDamage(roundedDamage);
                    damagedEnemies.Add(enemy);

                    if (activateRagdoll) enemy.TurnOnRagdoll();

                    LimbManager limbScript = enemy.GetComponent<LimbManager>();
                    for (int i = 0; i < 9; i++)
                    {
                        enemy.DamageLimb(i, roundedDamage);
                        if (enemy.GetHealth(i) < 0) limbScript.RemoveLimb(i);
                    }
                }
            }

            // Grenades blow up other grenades
            Grenade grenade = nearbyObject.GetComponentInParent<Grenade>();
            if (grenade != null)
            {
                if (!grenade.isIncendiary) grenade.Explode();
                else grenade.ExplodeIncendiary();
            }
        }
    }

    void ExplodeIncendiary()
    {
        GameObject fireObject = Instantiate(firePrefab, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity);
        Fire fireScript = fireObject.GetComponent<Fire>();
        fireScript.InitializeFire(fireDuration);
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
