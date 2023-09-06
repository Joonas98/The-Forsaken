using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShooting : MonoBehaviour
{
    public Transform firePoint;
    public LayerMask targetLayer;
    public float fireRate;
    public float shootingRange;
    public int damage;

    [Header("FX & Stuff")]
    public ParticleSystem shootingFX;
    public GameObject casingGO;
    public Transform casingTrans;
    public Light muzzleFlashLight;

    [Header("Audio & Animation")]
    public AudioClip shootSound;
    public AudioSource audioSource;
    public Animator animator;

    private float nextFireTime;

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            if (IsEnemyInFront())
            {
                Shoot();
            }
            nextFireTime = Time.time + fireRate;
        }
        MuzzleLight();
    }

    bool IsEnemyInFront()
    {
        // Create a ray from the firePoint position in the forward direction
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, shootingRange, targetLayer))
        {
            // Check if the hit object has an Enemy component
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();

            if (enemy != null && !enemy.isDead)
            {
                return true;
            }
        }

        return false;
    }

    void Shoot()
    {
        // Create a ray from the firePoint position in the forward direction
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, shootingRange, targetLayer))
        {
            // Check if the hit object has an Enemy component
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                shootingFX.Play();
                audioSource.PlayOneShot(shootSound);
                animator.Play("TurretShoot");
                DropCasing();
            }
        }
    }

    private void DropCasing()
    {
        GameObject newCasing = Instantiate(casingGO, casingTrans.position, transform.rotation * Quaternion.Euler(-90f, 0f, 0f));
        Rigidbody newCasingRB;

        if (newCasing.GetComponent<Rigidbody>() != null)
        {
            newCasingRB = newCasing.GetComponent<Rigidbody>();
        }
        else
        {
            newCasingRB = newCasing.GetComponentInChildren<Rigidbody>();
        }
        newCasingRB.AddForce(transform.up * 1f + transform.right);
        Destroy(newCasing, 1f);
    }

    private void MuzzleLight()
    {
        // Light from muzzle flash that is not too expensive and looks nice enough
        if (muzzleFlashLight == null) return;
        bool isEmitting = false;
        if (shootingFX != null) isEmitting = shootingFX.isEmitting;
        muzzleFlashLight.enabled = isEmitting;
    }

}
