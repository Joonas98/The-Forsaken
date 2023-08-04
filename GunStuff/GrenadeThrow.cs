using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrenadeThrow : MonoBehaviour
{
    // Variables
    public float throwForce;
    public float throwForceImpact;
    public float grenadeSelectionTimeSlow; // Slow time down when selecting grenade
    [HideInInspector] public int selectedGrenade = 0; // 0 = normal, 1 = impact, 2 = incendiary

    public GameObject normalGrenadePrefab, impactGrenadePrefab, incendiaryGrenadePrefab;
    public Image[] grenadePanels;
    public Color defaultColor, highlightColor;
    public GameObject selectionMenu;
    public PlayerInventory inventoryScript;
    public static GrenadeThrow instance;
    public bool selectingGrenade = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance);
        }
        SelectGrenade(0);
    }

    private void Update()
    {
        HandleInputs();
    }

    private void HandleInputs()
    {
        if (Time.timeScale <= 0) return; // Game paused

        if (Input.GetKey(KeyCode.H))
        {
            if (!selectingGrenade)
            {
                selectionMenu.SetActive(true);
                selectingGrenade = true;
                MouseLook.instance.canRotate = false;
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = grenadeSelectionTimeSlow;
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
                Time.timeScale = 1f;
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

    public void SelectGrenade(int index)
    {
        grenadePanels[selectedGrenade].color = defaultColor; // Previous selection to default color
        grenadePanels[index].color = highlightColor; // Highlight new selection
        selectedGrenade = index; // Update selectedGrenade variable for other uses
    }
}
