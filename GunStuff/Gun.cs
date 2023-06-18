using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class Gun : Weapon
{
    [Header("Gun Settings")]
    public Sprite gunSprite;
    public string gunName;
    public bool semiAutomatic;
    public int pelletCount, penetration, damage, MagazineSize;
    public float hipSpread, spread, headshotMultiplier, RPM, ReloadTime, knockbackPower, range;
    [Tooltip("Should be more than 1. High = faster")] [SerializeField] public float aimSpeed;
    [Tooltip("Should be 0-1. Low = more zoom")] [SerializeField] public float zoomAmount;
    public int ammoType; //0 = .22 LR, 1 = HK 4.6x30mm, 2 = .357 Magnum, 3 = .45 ACP, 4 = 12 Gauge, 5 = 5.45x39, 6 = 5.56 NATO, 7 = 7.62 NATO, 8 = .50 BMG

    [Header("Recoil Settings")]
    [Tooltip("Up and down")] [SerializeField] public float recoilX;
    [Tooltip("Left and right")] [SerializeField] public float recoilY;
    [Tooltip("Tilt")] [SerializeField] public float recoilZ;
    [SerializeField] public float snappiness, returnSpeed;
    [Tooltip("Recoil multiplier midair")] public float rec1;
    [Tooltip("Grounded, moving, not aiming")] public float rec2;
    [Tooltip("Grounded, moving, aiming")] public float rec3;
    [Tooltip("Grounded, not moving, not aiming")] public float rec4;
    [Tooltip("Grounded, not moving, aiming")] public float rec5;
    [Tooltip("Recoil multiplier if nothing previous matches")] public float rec6;

    [Header("Visual Recoil")]
    [Tooltip("Vire up")] public float vireX = -2;
    [Tooltip("Vire left and right")] public float vireY = 2;
    [Tooltip("Vire rotation")] public float vireZ = 7;
    [Tooltip("Recoil kicking towards player")] public float vireKick = 0.2f;
    public float vireSnap = 5;
    public float vireReturn = 8;

    [Header("Effects")]
    public ParticleSystem MuzzleFlash;
    public ParticleSystem BloodFX;
    [HideInInspector] public ParticleSystem HitFX;
    public ParticleSystem GroundFX;
    public LineRenderer LR;

    public bool dropCasings;
    public GameObject casingGO;
    private float casingDespawnTime = 15f;
    private float laserTime = 0.05f;

    private string reloadAnimationName, shootAnimationName;
    private bool playedAimSound = false;
    private bool playedUnaimSound = true;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound, aimSound, unaimSound;
    public AudioClip zoomScopeInSound, zoomScopeOutSound;
    public AudioClip actionSound, dryFireSound; // Pump shotgun, bolt action etc.
    public float actionDelay = 0f; // Seconds to wait before playing action sound

    public AudioMixer audioMixer;

    [Header("Other Things")]
    [Tooltip("What layers the gun can hit")] public LayerMask targetLayers;
    public GameObject gunTip;
    public Transform casingTransform;
    public AnimationClip reloadAnimation;
    public string overrideReloadName;

    [SerializeField] private Animator animator;
    public GameObject aimingSpot;
    [SerializeField] private bool hasShootAnimation;

    private GameObject reloadSymbol;
    private Recoil recoilScript;
    private CanvasManager canvasManagerScript;
    private TextMeshProUGUI magazineText;
    private float shotCounter, fireRate;
    private int CurrentMagazine;
    [HideInInspector] public bool isFiring = false;
    private bool hasFired = false;
    private float defaultFov = 60f;

    [HideInInspector] public GameObject ImpactEffect;
    [HideInInspector] public ParticleSystem PS;
    [HideInInspector] public ParticleSystemRenderer rend;
    [HideInInspector] public Vector3 equipVector;
    [HideInInspector] public Camera mainCamera, weaponCam;
    [HideInInspector] public string magString, totalAmmoString;
    [HideInInspector] public bool isReloading = false, isAiming = false, unequipping = false;
    [HideInInspector] public bool canAim; // True in update unless mid air etc.
    [HideInInspector] public float maxZoom, minZoom;
    [HideInInspector] public int shotsLeft;

    [HideInInspector] public BulletHoles bulletHoleScript;
    // public GameObject damagePopupText;

    private GameObject CrosshairContents;
    private Crosshair crosshairScript;
    private PlayerMovement playerMovementScript;
    private PlayerInventory inventoryScript;

    [HideInInspector] public Camera scopeCam = null;
    private float sprintLerp, unsprintLerp; // Timers to handle lerping

    //OG THINGS
    [HideInInspector] public float RPMOG;

    private AudioClip shootSoundOG;
    private GameObject gunTipOG;
    [HideInInspector] public GameObject aimingSpotOG;
    private ParticleSystem muzzleFlashOG;
    private float aimSpeedOG;

    private float recoilXOG, recoilYOG, recoilZOG;
    private VisualRecoil vire;

    private void Awake()
    {
        vire = GameObject.Find("ViRe").GetComponent<VisualRecoil>();

        if (bulletHoleScript == null)
            bulletHoleScript = GetComponent<BulletHoles>();

        defaultFov = Camera.main.fieldOfView;
        recoilScript = GetComponentInParent<Recoil>();
        mainCamera = Camera.main;
        weaponCam = GameObject.Find("WeaponCamera").GetComponent<Camera>();
        magazineText = GameObject.Find("MagazineNumbers").GetComponent<TextMeshProUGUI>();
        GameObject CrosshairCanvas = GameObject.Find("CrossHairCanvas");
        crosshairScript = CrosshairCanvas.GetComponent<Crosshair>();
        reloadSymbol = CrosshairCanvas.transform.GetChild(0).gameObject;
        canvasManagerScript = GameObject.Find("Canvases").GetComponent<CanvasManager>();
        animator = GetComponent<Animator>();
        inventoryScript = GameObject.Find("Player").GetComponent<PlayerInventory>();

        HandleAnimationStrings();

        aimSpeedOG = aimSpeed;
        shootSoundOG = shootSound;
        muzzleFlashOG = MuzzleFlash;
        gunTipOG = gunTip;
        aimingSpotOG = aimingSpot;
        recoilXOG = recoilX;
        recoilYOG = recoilY;
        recoilZOG = recoilZ;
        RPMOG = RPM;

        equipTrans = GameObject.Find("EquipTrans").transform;
    }

    private void Start()
    {
        if (weaponSpot == null)
            weaponSpot = GameObject.Find("WeaponSpot");

        CrosshairContents = GameObject.Find("CrosshairPanel");
        playerMovementScript = GameObject.Find("Player").GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        shotsLeft = pelletCount;
        CurrentMagazine = MagazineSize;
        magString = CurrentMagazine.ToString() + " / " + MagazineSize.ToString();
        magazineText.text = magString;

        UpdateFirerate();
        HandleAnimationStrings();
        UpdateRecoil();
    }

    private void OnEnable()
    {
        SetFOV(defaultFov); // Avoid bugs

        // Handle ammo UI 
        magString = CurrentMagazine.ToString() + " / " + MagazineSize.ToString();
        magazineText.text = magString;
        inventoryScript.UpdateTotalAmmoText(ammoType);

        UpdateRecoil(); // Recoil is a singleton, update when taking weapon out
        EquipWeapon(); // Animations etc. when equpping weapon
    }

    private void Update()
    {
        HandleShooting();
        HandleAiming();
        HandleReloading();
        HandleCrosshair();
        HandleScopeZoom();
        HandleSwitchingLerps();
        HandleSprinting();
        recoilScript.aiming = isAiming;
        vire.aiming = isAiming;
    }

    public void HandleAiming()
    {
        if (playerMovementScript.isRunning || !playerMovementScript.isGrounded)
        {
            canAim = false;
        }
        else
        {
            canAim = true;
        }

        if (isAiming == true && playedAimSound == false)
        {
            playedAimSound = true;
            audioSource.PlayOneShot(aimSound);
        }

        // Actual aiming
        if (Input.GetButton("Fire2") && canAim && equipped && !isReloading && Time.timeScale > 0)
        {
            isAiming = true;
            playedUnaimSound = false;
            WeaponSwayAndBob.instance.disableSwayBob = true;
            CrosshairContents.SetActive(false);
            WeaponSwitcher.canSwitch(false);

            transform.position = Vector3.Lerp(transform.position, transform.parent.transform.position + (transform.position - aimingSpot.transform.position), (aimSpeed * 2f) * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(0, 180, 0);

            SetFOV(Mathf.Lerp(Camera.main.fieldOfView, zoomAmount * defaultFov, aimSpeed * Time.deltaTime));
            SetFOV(Mathf.Lerp(weaponCam.fieldOfView, zoomAmount * defaultFov, aimSpeed * Time.deltaTime));
        }
        else
        {
            isAiming = false;
            if (playedUnaimSound == false)
            {
                playedUnaimSound = true;
                audioSource.PlayOneShot(unaimSound);
            }

            playedAimSound = false;
            WeaponSwayAndBob.instance.disableSwayBob = false;

            if (equipped == true && unequipping == false)
                transform.position = Vector3.Lerp(transform.position, weaponSpot.transform.position, (aimSpeed * 2f) * Time.deltaTime);

            SetFOV(Mathf.Lerp(Camera.main.fieldOfView, defaultFov, aimSpeed * Time.deltaTime));
            SetFOV(Mathf.Lerp(weaponCam.fieldOfView, defaultFov, aimSpeed * Time.deltaTime));
            CrosshairContents.SetActive(true);

            if (Time.timeScale > 0 && equipped && !isReloading)
                WeaponSwitcher.canSwitch(true);
        }

    }

    private void HandleAnimationStrings()
    {
        shootAnimationName = hasShootAnimation ? "Shoot " + gunName : "";
        reloadAnimationName = (overrideReloadName == "") ? "Reload " + gunName : overrideReloadName;
    }

    public void HandleReloading()
    {
        // Reloading
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && CurrentMagazine != MagazineSize && Time.timeScale > 0 && inventoryScript.GetAmmoCount(ammoType) > 0)
        {
            if (animator == null) animator = gameObject.GetComponentInChildren<Animator>();
            isReloading = true;

            // Reset rotation
            ResetRotation();

            // Adjust reload speed to animation and sound
            animator.SetFloat("ReloadSpeedMultiplier", reloadAnimation.length / ReloadTime);
            audioMixer.SetFloat("WeaponsPitch", reloadAnimation.length / ReloadTime);

            WeaponSwitcher.canSwitch(false);
            reloadSymbol.SetActive(true);
            shotCounter = ReloadTime;
            audioSource.PlayOneShot(reloadSound);

            if (animator != null && reloadAnimationName != "")
                animator.Play(reloadAnimationName);

            // Handle ammo correctly
            if (inventoryScript.GetAmmoCount(ammoType) >= MagazineSize)
            {
                // Debug.Log("Full reload");
                StartCoroutine(WaitReloadTime(ReloadTime, MagazineSize));
                inventoryScript.HandleAmmo(ammoType, CurrentMagazine - MagazineSize);
            }
            else if (inventoryScript.GetAmmoCount(ammoType) + CurrentMagazine >= MagazineSize)
            {
                // Debug.Log("Stock + clip >= full mag");
                StartCoroutine(WaitReloadTime(ReloadTime, MagazineSize));
                inventoryScript.HandleAmmo(ammoType, CurrentMagazine - MagazineSize);
            }
            else if (inventoryScript.GetAmmoCount(ammoType) < MagazineSize)
            {
                // Debug.Log("Stock + clip < full mag");
                StartCoroutine(WaitReloadTime(ReloadTime, inventoryScript.GetAmmoCount(ammoType) + CurrentMagazine));
                inventoryScript.HandleAmmo(ammoType, inventoryScript.GetAmmoCount(ammoType) * -1);
            }
        }
    }

    public void HandleShooting()
    {
        // Can't shoot when running
        if (playerMovementScript.isRunning && !AbilityMaster.abilities.Contains(7))
        {
            isFiring = false;
            return;
        }

        // Shooting
        if (Input.GetButtonDown("Fire1") && Time.timeScale > 0)
        {
            isFiring = true;
        }
        else if (Input.GetButtonUp("Fire1") || !Input.GetButton("Fire1"))
        {
            isFiring = false;
            hasFired = false;
        }

        // Fully automatic weapons
        if (isFiring == true && semiAutomatic == false)
        {
            shotCounter -= Time.deltaTime;

            if (shotCounter <= 0 && CurrentMagazine > 0) //Shooting
            {
                shotCounter = fireRate;
                Shoot(pelletCount);
                vire.Recoil();
                --CurrentMagazine;
                magString = CurrentMagazine.ToString() + " / " + MagazineSize.ToString();
                magazineText.text = magString;
            }
            else if (shotCounter <= 0 && CurrentMagazine <= 0)
            {
                audioSource.PlayOneShot(dryFireSound);
                isFiring = false;
            }
        }
        // Semi automatic weapons
        else if (isFiring == true && semiAutomatic == true && hasFired == false)
        {
            shotCounter -= Time.deltaTime;
            hasFired = true;

            if (shotCounter <= 0 && CurrentMagazine > 0) //Shooting
            {
                shotCounter = fireRate;
                Shoot(pelletCount);
                vire.Recoil();
                --CurrentMagazine;
                magString = CurrentMagazine.ToString() + " / " + MagazineSize.ToString();
                magazineText.text = magString;
            }
            else if (shotCounter <= 0 && CurrentMagazine <= 0)
            {
                audioSource.PlayOneShot(dryFireSound);
                isFiring = false;
            }
        }
        else
        {
            shotCounter -= Time.deltaTime;
        }
    }

    // Mostly to lerp weapons
    public void HandleSprinting()
    {
        // No rotating if reloading or we have bullet ballet ability
        if (isReloading || AbilityMaster.abilities.Contains(7)) return;

        if (!playerMovementScript.isRunning || !equipped)
        {
            // Return to default gun rotation
            unsprintLerp += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, 180, 0), unsprintLerp * 50f * Time.deltaTime);
            sprintLerp = 0f;
        }
        else
        {
            // Move to running rotation
            sprintLerp += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(50, 180, 0), sprintLerp * 50f * Time.deltaTime);
            unsprintLerp = 0f;
        }
    }

    public void HandleCrosshair()
    {
        crosshairScript.AdjustCrosshair(spread);
    }

    // Mouse wheel changes scope zoom
    private void HandleScopeZoom()
    {
        if (scopeCam != null)
        {
            if (isAiming && Input.GetAxis("Mouse ScrollWheel") < 0f && Time.timeScale > 0)
            {
                scopeCam.fieldOfView += 1;
                scopeCam.fieldOfView = scopeCam.fieldOfView * 1.1f;

                if (scopeCam.fieldOfView > minZoom)
                {
                    scopeCam.fieldOfView = minZoom;
                }
                else
                {
                    audioSource.PlayOneShot(zoomScopeInSound);
                }
            }

            if (isAiming && Input.GetAxis("Mouse ScrollWheel") > 0f && Time.timeScale > 0)
            {
                scopeCam.fieldOfView -= 1;
                scopeCam.fieldOfView = scopeCam.fieldOfView * 0.9f;

                if (scopeCam.fieldOfView < maxZoom)
                {
                    scopeCam.fieldOfView = maxZoom;
                }
                else
                {
                    audioSource.PlayOneShot(zoomScopeOutSound);
                }
            }

            // if (scopeCam.fieldOfView < maxZoom)
            // {
            //     scopeCam.fieldOfView = maxZoom;
            // }
            //
            // if (scopeCam.fieldOfView > minZoom)
            // {
            //     scopeCam.fieldOfView = minZoom;
            // }
        }
    }

    // Handle lerps for switching weapons
    // public void HandleSwitchingLerps()
    // {
    //     base.HandleSwitchingLerps();
    //     // Take gun out
    //     if (canAim2 == false && !unequipping && equipLerp <= equipTime)
    //     {
    //         equipLerp += Time.deltaTime;
    //         transform.position = Vector3.Lerp(equipTrans.position, weaponSpot.transform.position, equipLerp / equipTime);
    //         transform.rotation = Quaternion.Lerp(Quaternion.Euler(equipRotX, equipRotY, equipRotZ), weaponSpot.transform.rotation, equipLerp / equipTime);
    //     }
    //
    //     // Put gun away
    //     if (unequipping && unequipLerp <= unequipTime)
    //     {
    //         unequipLerp += Time.deltaTime;
    //         transform.position = Vector3.Lerp(weaponSpot.transform.position, equipTrans.position, unequipLerp / unequipTime);
    //         transform.rotation = Quaternion.Lerp(weaponSpot.transform.rotation, Quaternion.Euler(equipRotX, equipRotY, equipRotZ), unequipLerp / unequipTime);
    //     }
    // }

    // Handle impact, eg. hit enemies
    public void HandleImpact(RaycastHit hit)
    {
        // Push rigidbodies
        if (hit.collider.GetComponent<Rigidbody>() != null)
        {
            Rigidbody targetRB = hit.collider.gameObject.GetComponent<Rigidbody>();
            targetRB.AddForce(((mainCamera.transform.position - hit.transform.position) * -1) * knockbackPower, ForceMode.Impulse);
        }

        // Destroyable stuff like windows or boxes etc.
        if (hit.collider.CompareTag("Destroyable"))
        {
            Destroy(hit.collider.gameObject);
        }

        // Handle enemies
        Enemy enemy = hit.collider.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            LimbManager limbScript = hit.collider.gameObject.GetComponentInParent<LimbManager>();
            EnemyImpactFX(hit);

            if (enemy.GetHealth() > 0) // Hitmarker
            {
                if (hit.collider.tag == "Head")
                {
                    canvasManagerScript.Hitmarker(hit.point, true);
                }
                else
                {
                    canvasManagerScript.Hitmarker(hit.point, false);
                }
            }

            if (hit.collider.tag == "Head")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(Mathf.RoundToInt(damage * headshotMultiplier), 5);
                }
                else
                {
                    enemy.TakeDamage(Mathf.RoundToInt(damage * headshotMultiplier));
                }

                if (limbScript != null && enemy.GetHealth() <= 0) limbScript.RemoveLimb(0);
            }

            // LEGS
            else if (hit.collider.tag == "UpperLegL")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(damage, 5);
                }
                else
                {
                    enemy.TakeDamage(damage);
                }

                if (limbScript != null && enemy.GetHealth() <= 50)
                {
                    limbScript.RemoveLimb(2);
                }

            }

            else if (hit.collider.tag == "LowerLegL")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(damage, 5);
                }
                else
                {
                    enemy.TakeDamage(damage);
                }

                if (limbScript != null && enemy.GetHealth() <= 50)
                {
                    limbScript.RemoveLimb(1);
                }
            }

            else if (hit.collider.tag == "UpperLegR")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(damage, 5);
                }
                else
                {
                    enemy.TakeDamage(damage);
                }

                if (limbScript != null && enemy.GetHealth() <= 50)
                {
                    limbScript.RemoveLimb(4);
                }
            }

            else if (hit.collider.tag == "LowerLegR")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(damage, 5);
                }
                else
                {
                    enemy.TakeDamage(damage);
                }

                if (limbScript != null && enemy.GetHealth() <= 50)
                {
                    limbScript.RemoveLimb(3);
                }
            }

            else if (hit.collider.tag == "ArmL")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(damage, 5);
                }
                else
                {
                    enemy.TakeDamage(damage);
                }

                if (limbScript != null && enemy.GetHealth() <= 50)
                {
                    limbScript.RemoveLimb(7);
                }
            }

            else if (hit.collider.tag == "ShoulderL")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(damage, 5);
                }
                else
                {
                    enemy.TakeDamage(damage);
                }

                if (limbScript != null && enemy.GetHealth() <= 50)
                {
                    limbScript.RemoveLimb(8);
                }
            }

            else if (hit.collider.tag == "ArmR")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(damage, 5);
                }
                else
                {
                    enemy.TakeDamage(damage);
                }

                if (limbScript != null && enemy.GetHealth() <= 50)
                {
                    limbScript.RemoveLimb(5);
                }
            }

            else if (hit.collider.tag == "ShoulderR")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(damage, 5);
                }
                else
                {
                    enemy.TakeDamage(damage);
                }

                if (limbScript != null && enemy.GetHealth() <= 50)
                {
                    limbScript.RemoveLimb(6);
                }
            }

            // OTHERS
            else if (hit.collider.tag == "Torso")
            {
                if (AbilityMaster.abilities.Contains(2))
                {
                    enemy.TakeDamagePercentage(damage, 5);
                }
                else
                {
                    enemy.TakeDamage(damage);
                }

                if (AbilityMaster.abilities.Contains(6))
                {
                    if (UnityEngine.Random.value < 0.25f) // 25% chance
                    {
                        enemy.TurnOnRagdoll();
                        audioSource.PlayOneShot(AbilityMaster.instance.abilitiesList[6].activateSFX);
                        enemy.Invoke("TurnOffRagdoll", 1f);
                    }
                }
            }

        }
        else
        {
            GroundImpactFX(hit);
            bulletHoleScript.AddBulletHole(hit);
        }
    }

    // Main shooting function
    public void Shoot(int pelletCount)
    {
        MuzzleFlash.Play();
        int penetrationLeft = penetration;
        recoilScript.RecoilFire();
        audioSource.PlayOneShot(shootSound);
        Invoke("PlayActionSound", actionDelay);

        // Casing creation
        if (dropCasings)
        {
            GameObject newCasing = Instantiate(casingGO, casingTransform.position, transform.rotation * Quaternion.Euler(-90f, 0f, 0f));
            Rigidbody newCasingRB = null;

            if (newCasing.GetComponent<Rigidbody>() != null)
            {
                newCasingRB = newCasing.GetComponent<Rigidbody>();
            }
            else
            {
                newCasingRB = newCasing.GetComponentInChildren<Rigidbody>();
            }
            newCasingRB.AddForce(transform.up * 1f + transform.right * -1f);
            Destroy(newCasing.gameObject, casingDespawnTime);
        }

        if (animator != null && shootAnimationName != "")
        {
            animator.Play(shootAnimationName);
            // animator.SetTrigger("Shoot");
        }

        #region vanha systeemi
        /* if (pelletCount == 1)
         {
             Vector3 forwardVector = Vector3.forward;
             float deviation = UnityEngine.Random.Range(0f, spread);
             float angle = UnityEngine.Random.Range(0f, 360f);
             forwardVector = Quaternion.AngleAxis(deviation, Vector3.up) * forwardVector;
             forwardVector = Quaternion.AngleAxis(angle, Vector3.forward) * forwardVector;

             if (!shootFromBarrel) forwardVector = mainCamera.transform.rotation * forwardVector;
             else forwardVector = gunTip.transform.rotation * forwardVector;

             RaycastHit[] hitPointsList;

             if (!isAiming) hitPointsList = Physics.RaycastAll(mainCamera.transform.position, forwardVector, Mathf.Infinity);
             else hitPointsList = Physics.RaycastAll(aimingSpot.transform.position, -aimingSpot.transform.forward + forwardVector, Mathf.Infinity);

             System.Array.Sort(hitPointsList, (x, y) => x.distance.CompareTo(y.distance));

             if (hitPointsList.Length == 0)
             {
                 DrawLaser(gunTip.transform.position, forwardVector * 5000);
             }
             else
             {
                 DrawLaser(gunTip.transform.position, hitPointsList[0].point);
             }

             List<Enemy> hitEnemies = new List<Enemy>();
             List<RaycastHit> hitpointsInEnemies = new List<RaycastHit>();
             foreach (RaycastHit hitPoint in hitPointsList)
             {
                 // Debug.Log(hitPoint.collider.gameObject.name);
                 Enemy enemyInstance = hitPoint.collider.gameObject.GetComponentInParent<Enemy>();

                 if (enemyInstance != null && !hitEnemies.Contains(enemyInstance))
                 {
                     hitEnemies.Add(enemyInstance);
                     hitpointsInEnemies.Add(hitPoint);
                 }
                 else if (enemyInstance == null)
                 {
                     HandleImpact(hitPoint);
                 }
             }

             if (penetration == 0 && hitpointsInEnemies.Count > 0)
             {
                 HandleImpact(hitpointsInEnemies[0]);
             }
             else if (penetration > 0)
             {
                 int penLeft = penetration + 1;

                 foreach (RaycastHit hit in hitpointsInEnemies)
                 {
                     if (penLeft > 0) HandleImpact(hit);
                     --penLeft;
                 }
             }

             // hitEnemies.Clear();
             // hitpointsInEnemies.Clear();
             // Array.Clear(hitPointsList, 0, hitPointsList.Length);
         }
         else if (pelletCount > 1) // Shotguns or multiple bullets otherwise
         {
             int pelletsLeft = pelletCount;

             for (int i = pelletsLeft; i > 0; i--)
             {
                 Vector3 forwardVector = Vector3.forward;
                 float deviation = UnityEngine.Random.Range(0f, spread);
                 float angle = UnityEngine.Random.Range(0f, 360f);
                 forwardVector = Quaternion.AngleAxis(deviation, Vector3.up) * forwardVector;
                 forwardVector = Quaternion.AngleAxis(angle, Vector3.forward) * forwardVector;

                 if (!shootFromBarrel) forwardVector = mainCamera.transform.rotation * forwardVector;
                 else forwardVector = gunTip.transform.rotation * forwardVector;

                 Vector3 forwardVectorAim = -aimingSpot.transform.forward;
                 float deviationAim = UnityEngine.Random.Range(0f, spread);
                 float angleAim = UnityEngine.Random.Range(0f, 360f);
                 forwardVectorAim = Quaternion.AngleAxis(deviationAim, Vector3.up) * forwardVectorAim;
                 forwardVectorAim = Quaternion.AngleAxis(angleAim, Vector3.forward) * forwardVectorAim;

                 if (!shootFromBarrel) forwardVectorAim = mainCamera.transform.rotation * forwardVectorAim;
                 else forwardVectorAim = gunTip.transform.rotation * forwardVectorAim;

                 RaycastHit[] hitPointsList;
                 // if (!shootFromBarrel) hitPointsList = Physics.RaycastAll(mainCamera.transform.position, forwardVector, Mathf.Infinity);
                 // else hitPointsList = Physics.RaycastAll(gunTip.transform.position, forwardVector, Mathf.Infinity);

                 if (!isAiming) hitPointsList = Physics.RaycastAll(mainCamera.transform.position, forwardVector, Mathf.Infinity);
                 // else hitPointsList = Physics.RaycastAll(aimingSpot.transform.position, forwardVector, Mathf.Infinity);
                 else hitPointsList = Physics.RaycastAll(aimingSpot.transform.position, forwardVectorAim, Mathf.Infinity);

                 System.Array.Sort(hitPointsList, (x, y) => x.distance.CompareTo(y.distance));

                 if (hitPointsList.Length == 0)
                 {
                     DrawLaser(gunTip.transform.position, forwardVector * 5000);
                 }
                 else
                 {
                     DrawLaser(gunTip.transform.position, hitPointsList[0].point);
                 }

                 List<Enemy> hitEnemies = new List<Enemy>();
                 List<RaycastHit> hitpointsInEnemies = new List<RaycastHit>();
                 foreach (RaycastHit hitPoint in hitPointsList)
                 {
                     // Debug.Log(hitPoint.collider.gameObject.name);
                     Enemy enemyInstance = hitPoint.collider.gameObject.GetComponentInParent<Enemy>();

                     if (enemyInstance != null && !hitEnemies.Contains(enemyInstance))
                     {
                         hitEnemies.Add(enemyInstance);
                         hitpointsInEnemies.Add(hitPoint);
                     }
                     else if (enemyInstance == null)
                     {
                         HandleImpact(hitPoint);
                     }
                 }

                 if (penetration == 0 && hitpointsInEnemies.Count > 0)
                 {
                     HandleImpact(hitpointsInEnemies[0]);
                 }
                 else if (penetration > 0)
                 {
                     int penLeft = penetration + 1;

                     foreach (RaycastHit hit in hitpointsInEnemies)
                     {
                         if (penLeft > 0) HandleImpact(hit);
                         --penLeft;
                     }
                 }
             }

         } */
        #endregion

        int pelletsLeft = pelletCount;

        for (int i = pelletsLeft; i > 0; i--)
        {
            float deviation;
            if (isAiming)
            {
                deviation = UnityEngine.Random.Range(0f, spread);
            }
            else
            {
                deviation = UnityEngine.Random.Range(0f, hipSpread);
            }

            Vector3 forwardVector = Vector3.forward;
            float angle = UnityEngine.Random.Range(0f, 360f);
            forwardVector = Quaternion.AngleAxis(deviation, Vector3.up) * forwardVector;
            forwardVector = Quaternion.AngleAxis(angle, Vector3.forward) * forwardVector;

            forwardVector = mainCamera.transform.rotation * forwardVector;

            RaycastHit[] hitPointsList;

            if (!isAiming) hitPointsList = Physics.RaycastAll(mainCamera.transform.position, forwardVector, Mathf.Infinity, targetLayers);
            else hitPointsList = Physics.RaycastAll(aimingSpot.transform.position, forwardVector, Mathf.Infinity, targetLayers);

            System.Array.Sort(hitPointsList, (x, y) => x.distance.CompareTo(y.distance));

            if (hitPointsList.Length == 0)
            {
                DrawLaser(gunTip.transform.position, forwardVector * 5000);
            }
            else
            {
                DrawLaser(gunTip.transform.position, hitPointsList[0].point);
            }

            List<Enemy> hitEnemies = new List<Enemy>();
            List<RaycastHit> hitpointsInEnemies = new List<RaycastHit>();
            foreach (RaycastHit hitPoint in hitPointsList)
            {
                // Debug.Log(hitPoint.collider.gameObject.name);
                Enemy enemyInstance = hitPoint.collider.gameObject.GetComponentInParent<Enemy>();

                if (enemyInstance != null && !hitEnemies.Contains(enemyInstance))
                {
                    hitEnemies.Add(enemyInstance);
                    hitpointsInEnemies.Add(hitPoint);
                }
                else if (enemyInstance == null)
                {
                    HandleImpact(hitPoint);
                }
            }

            if (penetration == 0 && hitpointsInEnemies.Count > 0)
            {
                HandleImpact(hitpointsInEnemies[0]);
            }
            else if (penetration > 0)
            {
                int penLeft = penetration + 1;

                foreach (RaycastHit hit in hitpointsInEnemies)
                {
                    if (penLeft > 0) HandleImpact(hit);
                    --penLeft;
                }
            }
        }
    }

    // Blood effect at enemies
    public void EnemyImpactFX(RaycastHit hit)
    {
        if (BloodFX != null)
        {
            ParticleSystem bloodFXGO = Instantiate(BloodFX, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(bloodFXGO.gameObject, 2f);
        }

        if (HitFX != null)
        {
            ParticleSystem hitFXGO = Instantiate(HitFX, hit.point, Quaternion.identity);
            Destroy(hitFXGO, 2f);
        }
    }

    // Ground impacts
    public void GroundImpactFX(RaycastHit hit)
    {
        ParticleSystem groundFXGO = Instantiate(GroundFX, hit.point, Quaternion.identity);
        Destroy(groundFXGO, 2f);
    }

    // Linerenderer between muzzle and hit point
    public void DrawLaser(Vector3 v0, Vector3 v1)
    {
        // TODO: object pooling to optimize
        if (LR == null) return;
        var InstantiatedLaser = Instantiate(LR);

        LineRenderer lr = InstantiatedLaser.GetComponent<LineRenderer>();

        // var Effects = lr.GetComponentsInChildren<ParticleSystem>();
        // foreach (var AllPs in Effects)
        // {
        //     if (!AllPs.isPlaying) AllPs.Play();
        //     Debug.Log("Playing effect: " + AllPs);
        // }

        lr.SetPosition(0, v0);
        lr.SetPosition(1, v1);
        Destroy(InstantiatedLaser.gameObject, laserTime);
    }

    private void SetFOV(float fov)
    {
        Camera.main.fieldOfView = fov;
        weaponCam.fieldOfView = fov;
    }

    public override void EquipWeapon()
    {
        base.EquipWeapon();
        shotCounter = equipTime;
    }

    public override void UnequipWeapon()
    {
        base.UnequipWeapon();
        shotCounter = unequipTime + 0.01f;
    }

    public void UpdateFirerate()
    {
        fireRate = (RPM / 60);
        fireRate = 1 / fireRate;
    }

    // Update recoil script, ViRe and recoil
    private void UpdateRecoil()
    {
        recoilScript.SetRecoilX(recoilX * -1);
        recoilScript.SetRecoilY(recoilY);
        recoilScript.SetRecoilZ(recoilZ);
        recoilScript.SetSnappiness(snappiness);
        recoilScript.SetReturnSpeed(returnSpeed);
        recoilScript.rec1 = rec1;
        recoilScript.rec2 = rec2;
        recoilScript.rec3 = rec3;
        recoilScript.rec4 = rec4;
        recoilScript.rec5 = rec5;
        recoilScript.rec6 = rec6;

        vire.SetVireX(vireX);
        vire.SetVireY(vireY);
        vire.SetVireZ(vireZ);
        vire.SetVireKickback(vireKick);
        vire.SetVireSnappiness(vireSnap);
        vire.SetVireReturn(vireReturn);
    }

    // Reloading delay etc.
    IEnumerator WaitReloadTime(float r, int ammoAmount)
    {
        yield return new WaitForSeconds(r + 0.05f);
        CurrentMagazine = ammoAmount;
        magString = CurrentMagazine.ToString() + " / " + MagazineSize.ToString();
        magazineText.text = magString;
        WeaponSwitcher.canSwitch(true);
        audioMixer.SetFloat("WeaponsPitch", 1f);
        isReloading = false;
        reloadSymbol.SetActive(false);
    }


    // Invoked after action delay
    public void PlayActionSound()
    {
        if (actionSound == null) return;
        audioSource.PlayOneShot(actionSound);
    }

    // Adjust values from other scripts
    #region Adjust Values 

    public void AdjustDamage(int amount)
    {
        damage = damage + amount;
    }

    public void AdjustReloadtime(float amount)
    {
        ReloadTime = ReloadTime + amount;
    }

    public void AdjustRecoil(float xAmount, float yAmount, float zAmount)
    {
        recoilX = recoilXOG + xAmount;
        recoilY = recoilYOG + yAmount;
        recoilZ = recoilZOG + zAmount;

        if (recoilX < 0) recoilX = 0;
        if (recoilY < 0) recoilY = 0;
        if (recoilZ < 0) recoilZ = 0;
        UpdateRecoil();
    }

    public void AdjustAimspeed(float amount)
    {
        aimSpeed = aimSpeed + amount;
        if (aimSpeed < 1f)
        {
            aimSpeed = 1f;
        }
    }

    #endregion

    // Reset to original values
    #region Reset Values
    public void ResetAimingSpot()
    {
        aimingSpot = aimingSpotOG;
    }

    public void ResetAimSpeed()
    {
        aimSpeed = aimSpeedOG;
    }

    public void ResetGunTip()
    {
        gunTip = gunTipOG;
        MuzzleFlash = muzzleFlashOG;
        shootSound = shootSoundOG;
    }

    public void ResetFOV()
    {
        Camera.main.fieldOfView = defaultFov;
        weaponCam.fieldOfView = defaultFov;
    }

    public void ResetRecoils()
    {
        recoilX = recoilXOG;
        recoilY = recoilYOG;
        recoilZ = recoilZOG;
        UpdateRecoil();
    }

    public void ResetRotation()
    {
        transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    #endregion

}
