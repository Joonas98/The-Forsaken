using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeThrow : MonoBehaviour
{

    [SerializeField] private float throwForce;
    [SerializeField] private float throwForceImpact;

    public GameObject grenadePrefab;
    public GameObject grenadePrefab2;
    public GameObject grenadePrefab3;

    public PlayerInventory inventoryScript;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && Time.timeScale > 0)
        {
            ThrowGrenade();
        }

        if (Input.GetKeyDown(KeyCode.H) && Time.timeScale > 0)
        {
            ThrowGrenade2();
        }

        if (Input.GetKeyDown(KeyCode.J) && Time.timeScale > 0)
        {
            ThrowGrenade3();
        }

    }

    // DEFAULT GRENADE
    public void ThrowGrenade()
    {
        if (inventoryScript.GetGrenadeCount(0) <= 0) return;
        GameObject grenade = Instantiate(grenadePrefab, transform.position, transform.rotation);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * throwForce);

        inventoryScript.HandleGrenades(0, -1);
    }

    // IMPACT GRENADE
    public void ThrowGrenade2()
    {
        if (inventoryScript.GetGrenadeCount(1) <= 0) return;
        GameObject grenade = Instantiate(grenadePrefab2, transform.position, transform.rotation);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * throwForceImpact);

        inventoryScript.HandleGrenades(1, -1);
    }

    // INCENDIARY GRENADE
    public void ThrowGrenade3()
    {
        if (inventoryScript.GetGrenadeCount(2) <= 0) return;
        GameObject grenade = Instantiate(grenadePrefab3, transform.position, transform.rotation);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * throwForceImpact);

        inventoryScript.HandleGrenades(2, -1);
    }

}
