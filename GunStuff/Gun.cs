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
    public bool semiAutomatic;
    public int pelletCount, penetration, damage, magazineSize;
    public float hipSpread, spread, headshotMultiplier, RPM, reloadTime, knockbackPower, range;
    [Tooltip("Should be more than 1. High = faster")] [SerializeField] public float aimSpeed;
    [Tooltip("Should be 0-1. Low = more zoom")] [SerializeField] public float zoomAmount;
    //0 = .22 LR, 1 = HK 4.6x30mm, 2 = .357 Magnum, 3 = .45 ACP, 4 = 12 Gauge, 5 = 5.45x39, 6 = 5.56 NATO, 7 = 7.62 NATO, 8 = .50 BMG
    public int ammoType; // Todo: change ammotype to enum?

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
    public ParticleSystem muzzleFlash;
    public ParticleSystem bloodFX, hitFX, groundFX;
    public LineRenderer LR;
    public bool dropCasings;
    public GameObject casingGO;
    public float casingDespawnTime = 1f;

    private float laserTime = 0.05f;
    private string reloadAnimationName, shootAnimationName;
    private bool playedAimSound = false;
    private bool playedUnaimSound = true;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public AudioClip shootSound;
    public AudioClip reloadSound, aimSound, unaimSound;
    public AudioClip zoomScopeInSound, zoomScopeOutSound;
    public AudioClip actionSound, dryFireSound; // Pump shotgun, bolt action etc.
    public float actionDelay = 0f; // Seconds to wait before playing action sound

    [Header("Other Things")]
    [Tooltip("What layers the gun can hit")] public LayerMask targetLayers;
    public GameObject gunTip, aimingSpot;
    public Transform casingTransform;
    public AnimationClip reloadAnimation;
    public string overrideReloadName;
    [HideInInspector] public Camera scopeCam = null;
    [HideInInspector] public GameObject ImpactEffect;
    [HideInInspector] public ParticleSystem PS;
    [HideInInspector] public Vector3 equipVector;
    [HideInInspector] public Camera mainCamera, weaponCam;
    [HideInInspector] public BulletHoles bulletHoleScript;
    [HideInInspector] public bool isFiring = false;
    [HideInInspector] public bool isReloading = false, isAiming = false;
    [HideInInspector] public bool canAim; // True in update unless mid air etc.
    [HideInInspector] public float maxZoom, minZoom;
    [HideInInspector] public int shotsLeft;
    [HideInInspector] public string magString, totalAmmoString;
    // public GameObject damagePopupText;

    [SerializeField] private Animator animator;
    [SerializeField] private bool hasShootAnimation;
    private GameObject reloadSymbol;
    private Recoil recoilScript;
    private CanvasManager canvasManagerScript;
    private TextMeshProUGUI magazineText;
    private float shotCounter, fireRate;
    private int CurrentMagazine;
    private bool hasFired = false;
    private GameObject CrosshairContents;
    private Crosshair crosshairScript;
    private PlayerMovement playerMovementScript;
    private PlayerInventory inventoryScript;
    private float sprintLerp, unsprintLerp; // Timers to handle lerping

    // Original variables
    [HideInInspector] public float RPMOG;
    [HideInInspector] public GameObject aimingSpotOG;

    private AudioClip shootSoundOG;
    private GameObject gunTipOG;
    private ParticleSystem muzzleFlashOG;
    private float aimSpeedOG;
    private float recoilXOG, recoilYOG, recoilZOG;
    private float defaultFov;
    private VisualRecoil vire;

    protected override void Awake()
    {
        base.Awake();
        vire = GameObject.Find("ViRe").GetComponent<VisualRecoil>();

        if (bulletHoleScript == null)
            bulletHoleScript = GetComponent<BulletHoles>();

        defaultFov = GameManager.GM.playerScript.normalFov;
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
        muzzleFlashOG = muzzleFlash;
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
        CrosshairContents = GameObject.Find("CrosshairPanel");
        playerMovementScript = GameObject.Find("Player").GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        shotsLeft = pelletCount;
        CurrentMagazine = magazineSize;
        magString = CurrentMagazine.ToString() + " / " + magazineSize.ToString();
        magazineText.text = magString;

        UpdateFirerate();
        HandleAnimationStrings();
        UpdateRecoil();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetFOV(defaultFov); // Avoid bugs

        // Handle ammo UI 
        magString = CurrentMagazine.ToString() + " / " + magazineSize.ToString();
        magazineText.text = magString;
        inventoryScript.UpdateTotalAmmoText(ammoType);

        // Update desired aiming fov to FovController
        FovController.Instance.fovAim = zoomAmount * FovController.Instance.fovDefault;

        UpdateRecoil(); // Recoil is a singleton, update when taking weapon out
        EquipWeapon(); // Animations etc. when equpping weapon
    }

    protected override void Update()
    {
        base.Update();
        HandleShooting();
        HandleAiming();
        HandleReloading();
        HandleCrosshair();
        HandleScopeZoom();
        HandleSwitchingLerps();
        HandleSprinting();
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
            // WeaponSwayAndBob.instance.disableSwayBob = true;
            CrosshairContents.SetActive(false);
            WeaponSwitcher.CanSwitch(false);

            transform.position = Vector3.Lerp(transform.position, transform.parent.transform.position + (transform.position - aimingSpot.transform.position), aimSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 180, 0), aimSpeed * Time.deltaTime);
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
            // WeaponSwayAndBob.instance.disableSwayBob = false;
            CrosshairContents.SetActive(true);

            if (equipped == true && unequipping == false)
                transform.position = Vector3.Lerp(transform.position, weaponSpot.transform.position, (aimSpeed * 2f) * Time.deltaTime);

            if (Time.timeScale > 0 && equipped && !isReloading)
                WeaponSwitcher.CanSwitch(true);
        }
    }

    private void HandleAnimationStrings()
    {
        shootAnimationName = hasShootAnimation ? "Shoot " + weaponName : "";
        reloadAnimationName = (overrideReloadName == "") ? "Reload " + weaponName : overrideReloadName;
    }

    public void HandleReloading()
    {
        // Reloading
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && CurrentMagazine != magazineSize && Time.timeScale > 0 && inventoryScript.GetAmmoCount(ammoType) > 0)
        {
            if (animator == null) animator = gameObject.GetComponentInChildren<Animator>();
            isReloading = true;

            // Reset rotation
            ResetRotation();

            // Adjust reload speed to animation and sound
            animator.SetFloat("ReloadSpeedMultiplier", reloadAnimation.length / reloadTime);
            audioMixer.SetFloat("WeaponsPitch", reloadAnimation.length / reloadTime);

            WeaponSwitcher.CanSwitch(false);
            reloadSymbol.SetActive(true);
            shotCounter = reloadTime;
            audioSource.PlayOneShot(reloadSound);

            if (animator != null && reloadAnimationName != "")
                animator.Play(reloadAnimationName);

            // Handle ammo correctly
            if (inventoryScript.GetAmmoCount(ammoType) >= magazineSize)
            {
                // Debug.Log("Full reload");
                StartCoroutine(WaitReloadTime(reloadTime, magazineSize));
                inventoryScript.HandleAmmo(ammoType, CurrentMagazine - magazineSize);
            }
            else if (inventoryScript.GetAmmoCount(ammoType) + CurrentMagazine >= magazineSize)
            {
                // Debug.Log("Stock + clip >= full mag");
                StartCoroutine(WaitReloadTime(reloadTime, magazineSize));
                inventoryScript.HandleAmmo(ammoType, CurrentMagazine - magazineSize);
            }
            else if (inventoryScript.GetAmmoCount(ammoType) < magazineSize)
            {
                // Debug.Log("Stock + clip < full mag");
                StartCoroutine(WaitReloadTime(reloadTime, inventoryScript.GetAmmoCount(ammoType) + CurrentMagazine));
                inventoryScript.HandleAmmo(ammoType, inventoryScript.GetAmmoCount(ammoType) * -1);
            }
        }
    }

    public void HandleShooting()
    {
        shotCounter -= Time.deltaTime;
        if (!equipped) return;
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

            if (shotCounter <= 0 && CurrentMagazine > 0) //Shooting
            {
                shotCounter = fireRate;
                Shoot(pelletCount);
                vire.Recoil();
                --CurrentMagazine;
                magString = CurrentMagazine.ToString() + " / " + magazineSize.ToString();
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
            hasFired = true;

            if (shotCounter <= 0 && CurrentMagazine > 0) //Shooting
            {
                shotCounter = fireRate;
                Shoot(pelletCount);
                vire.Recoil();
                --CurrentMagazine;
                magString = CurrentMagazine.ToString() + " / " + magazineSize.ToString();
                magazineText.text = magString;
            }
            else if (shotCounter <= 0 && CurrentMagazine <= 0)
            {
                audioSource.PlayOneShot(dryFireSound);
                isFiring = false;
            }
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
        }
    }

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
        muzzleFlash.Play();
        int penetrationLeft = penetration;
        recoilScript.RecoilFire();
        audioSource.PlayOneShot(shootSound);
        Invoke("PlayActionSound", actionDelay);
        if (animator != null && shootAnimationName != "") animator.Play(shootAnimationName);
        DropCasing();

        int pelletsLeft = pelletCount;
        for (int i = pelletsLeft; i > 0; i--)
        {
            float deviation;
            if (isAiming)
                deviation = UnityEngine.Random.Range(0f, spread);
            else
                deviation = UnityEngine.Random.Range(0f, hipSpread);

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
        if (bloodFX != null)
        {
            ParticleSystem bloodFXGO = Instantiate(bloodFX, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(bloodFXGO.gameObject, 2f);
        }

        if (hitFX != null)
        {
            ParticleSystem hitFXGO = Instantiate(hitFX, hit.point, Quaternion.identity);
            Destroy(hitFXGO, 2f);
        }
    }

    // Ground impacts
    public void GroundImpactFX(RaycastHit hit)
    {
        ParticleSystem groundFXGO = Instantiate(groundFX, hit.point, Quaternion.identity);
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

    // public override void EquipWeapon()
    // {
    //     base.EquipWeapon();
    //     shotCounter = equipTime;
    // }
    //
    // public override void UnequipWeapon()
    // {
    //     base.UnequipWeapon();
    //     shotCounter = unequipTime + 0.01f;
    // }

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
        magString = CurrentMagazine.ToString() + " / " + magazineSize.ToString();
        magazineText.text = magString;
        WeaponSwitcher.CanSwitch(true);
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

    private void DropCasing()
    {
        if (!dropCasings) return;

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

    // Adjust values from other scripts
    #region Adjust Values 

    public void AdjustDamage(int amount)
    {
        damage = damage + amount;
    }

    public void AdjustReloadtime(float amount)
    {
        reloadTime = reloadTime + amount;
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
        muzzleFlash = muzzleFlashOG;
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
