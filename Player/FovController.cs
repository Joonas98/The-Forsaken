using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FovController : MonoBehaviour
{
    [HideInInspector] public static FovController Instance { get; private set; }
    public PlayerMovement playerMovement;
    public Camera mainCamera;
    public Camera weaponCamera; // Camera that render's weapons
    public float fovDefault, fovSprint, fovLerpSpeed; // Default values accordingly: 60f, 90f, 5f 
    [HideInInspector] public float fovAim; // Varies by weapon

    private bool isRunning = false;
    private bool isAiming = false;
    private float fovCurrent, fovTarget;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            // DontDestroyOnLoad(gameObject); Persistance has to be in a root object, but it isn't needed for this script
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(Instance);
        }
    }

    private void Start()
    {
        fovCurrent = fovDefault;
        fovTarget = fovDefault;
    }

    private void Update()
    {
        isRunning = playerMovement.isRunning;
        isAiming = GameManager.GM.currentGunAiming;

        // Update FOV based on sprinting and aiming states
        if (isRunning)
        {
            fovTarget = fovSprint;
        }
        else if (isAiming)
        {
            fovTarget = fovAim;
        }
        else
        {
            fovTarget = fovDefault;
        }

        // Smoothly lerp the camera FOV and update camera
        fovCurrent = Mathf.Lerp(fovCurrent, fovTarget, fovLerpSpeed * Time.deltaTime);
        mainCamera.fieldOfView = fovCurrent;
        weaponCamera.fieldOfView = fovCurrent;
    }
}
