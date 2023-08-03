using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeThrow : MonoBehaviour
{
    // Variables
    public float throwForce;
    public float throwForceImpact;
    public int selectedGrenade = 0; // 0 = normal, 1 = impact, 2 = incendiary

    public GameObject normalGrenadePrefab, impactGrenadePrefab, incendiaryGrenadePrefab;
    public GameObject selectionMenu;
    public PlayerInventory inventoryScript;

    private bool selectingGrenade = false;

    private void Update()
    {
        //  if (Input.GetKeyDown(KeyCode.G) && Time.timeScale > 0)
        //  {
        //      ThrowGrenade();
        //  }
        HandleInputs();
    }

    private void HandleInputs()
    {
        if (Time.timeScale < 0) return; // Game paused

        if (Input.GetKey(KeyCode.H))
        {
            if (!selectingGrenade)
            {
                selectionMenu.SetActive(true);
                selectingGrenade = true;
                MouseLook.instance.canRotate = false;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            if (selectingGrenade)
            {
                selectionMenu.SetActive(false);
                selectingGrenade = false;
                MouseLook.instance.canRotate = true;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ThrowGrenade();
        }
    }

    public void ThrowGrenade()
    {
        if (inventoryScript.GetGrenadeCount(selectedGrenade) <= 0) return;
        GameObject newGrenade;
        switch (selectedGrenade)
        {
            case 0:
                newGrenade = Instantiate(normalGrenadePrefab, transform.position, transform.rotation);
                break;

            case 1:
                newGrenade = Instantiate(impactGrenadePrefab, transform.position, transform.rotation);
                break;

            case 2:
                newGrenade = Instantiate(incendiaryGrenadePrefab, transform.position, transform.rotation);
                break;

            case 3:
                newGrenade = Instantiate(normalGrenadePrefab, transform.position, transform.rotation);
                break;

            default:
                newGrenade = Instantiate(normalGrenadePrefab, transform.position, transform.rotation);
                break;
        }

        Rigidbody rb = newGrenade.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * throwForce);

        inventoryScript.HandleGrenades(selectedGrenade, -1);
    }

    // DEFAULT GRENADE
    // public void ThrowGrenade()
    // {
    //     if (inventoryScript.GetGrenadeCount(0) <= 0) return;
    //     GameObject grenade = Instantiate(grenadePrefab, transform.position, transform.rotation);
    //
    //     Rigidbody rb = grenade.GetComponent<Rigidbody>();
    //     rb.AddForce(transform.forward * throwForce);
    //
    //     inventoryScript.HandleGrenades(0, -1);
    // }

    // IMPACT GRENADE
    //  public void ThrowGrenade2()
    //  {
    //      if (inventoryScript.GetGrenadeCount(1) <= 0) return;
    //      GameObject grenade = Instantiate(grenadePrefab2, transform.position, transform.rotation);
    //
    //      Rigidbody rb = grenade.GetComponent<Rigidbody>();
    //      rb.AddForce(transform.forward * throwForceImpact);
    //
    //      inventoryScript.HandleGrenades(1, -1);
    //  }
    //
    //  // INCENDIARY GRENADE
    //  public void ThrowGrenade3()
    //  {
    //      if (inventoryScript.GetGrenadeCount(2) <= 0) return;
    //      GameObject grenade = Instantiate(grenadePrefab3, transform.position, transform.rotation);
    //
    //      Rigidbody rb = grenade.GetComponent<Rigidbody>();
    //      rb.AddForce(transform.forward * throwForceImpact);
    //
    //      inventoryScript.HandleGrenades(2, -1);
    //  }

    public void SelectGrenade(int index)
    {
        selectedGrenade = index;

        // Later add UI indicator of selected grenade from here
    }

}
