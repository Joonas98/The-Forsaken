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
    [HideInInspector] public int percentageDamage;

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
    public AudioClip actionSound, dryFireSound; // Action sound is pump shotgun, bolt action etc.
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
    [HideInInspector] public int currentMagazine;
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
    private const float unsprintLerpThreshold = 30f; // We don't want to be able to shoot right away after sprinting
    private const float sprintLerpMultiplier = 15f; // Weapon sprint lerp and readiness: aimSpeed * this

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
        currentMagazine = magazineSize;
        magString = currentMagazine.ToString() + " / " + magazineSize.ToString();
        magazineText.text = magString;

        UpdateFirerate();
        HandleAnimationStrings();
        UpdateRecoil();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // SetFOV(defaultFov); // Avoid bugs

        // Handle ammo UI 
        magString = currentMagazine.ToString() + " / " + magazineSize.ToString();
        magazineText.text = magString;
        inventoryScript.UpdateTotalAmmoText(ammoType);

        // Update desired aiming fov to FovController
        FovController.Instance.fovAim = zoomAmount * FovController.Instance.fovDefault;

        RefreshGun();
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
        // Debug.Log(unsprintLerp * sprintLerpMultiplier * aimSpeed);
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
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentMagazine != magazineSize && Time.timeScale > 0 && inventoryScript.GetAmmoCount(ammoType) > 0)
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
                inventoryScript.HandleAmmo(ammoType, currentMagazine - magazineSize);
            }
            else if (inventoryScript.GetAmmoCount(ammoType) + currentMagazine >= magazineSize)
            {
                // Debug.Log("Stock + clip >= full mag");
                StartCoroutine(WaitReloadTime(reloadTime, magazineSize));
                inventoryScript.HandleAmmo(ammoType, currentMagazine - magazineSize);
            }
            else if (inventoryScript.GetAmmoCount(ammoType) < magazineSize)
            {
                // Debug.Log("Stock + clip < full mag");
                StartCoroutine(WaitReloadTime(reloadTime, inventoryScript.GetAmmoCount(ammoType) + currentMagazine));
                inventoryScript.HandleAmmo(ammoType, inventoryScript.GetAmmoCount(ammoType) * -1);
            }
        }
    }

    public void HandleShooting()
    {
        shotCounter -= Time.deltaTime;
        if (!equipped) return;
        // Can't shoot when running (unless got Bullet Ballet ability)
        if (playerMovementScript.isRunning && !AbilityMaster.abilities.Contains(7))
        {
            isFiring = false;
            return;
        }

        // Shooting
        if (Input.GetButton("Fire1") && Time.timeScale > 0 && (unsprintLerp * sprintLerpMultiplier * aimSpeed) > unsprintLerpThreshold)
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
            if (shotCounter <= 0 && currentMagazine > 0) //Shooting
            {
                shotCounter = fireRate;
                Shoot(pelletCount);
                vire.Recoil();
                --currentMagazine;
                magString = currentMagazine.ToString() + " / " + magazineSize.ToString();
                magazineText.text = magString;
            }
            else if (shotCounter <= 0 && currentMagazine <= 0) // Dry fire automatic
            {
                shotCounter = fireRate * 3; // Longer shotCounter so dryfire does not spam fast
                if (animator != null && shootAnimationName != "") animator.Play(shootAnimationName);
                audioSource.PlayOneShot(dryFireSound);
                isFiring = false;
            }
        }

        // Semi automatic weapons
        else if (isFiring == true && semiAutomatic == true && hasFired == false)
        {
            hasFired = true;

            if (shotCounter <= 0 && currentMagazine > 0) //Shooting
            {
                shotCounter = fireRate;
                Shoot(pelletCount);
                vire.Recoil();
                --currentMagazine;
                magString = currentMagazine.ToString() + " / " + magazineSize.ToString();
                magazineText.text = magString;
            }
            else if (shotCounter <= 0 && currentMagazine <= 0) // Dry fire semi auto
            {
                shotCounter = fireRate;
                if (animator != null && shootAnimationName != "") animator.Play(shootAnimationName);
                audioSource.PlayOneShot(dryFireSound);
                isFiring = false;
            }
        }
    }

    // Mostly to lerp weapons
    public void HandleSprinting()
    {
        // No rotating if reloading or we have Bullet Ballet ability
        if (isReloading || AbilityMaster.abilities.Contains(7)) return;

        if (!playerMovementScript.isRunning || !equipped)
        {
            // Return to default gun rotation
            unsprintLerp += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, 180, 0), unsprintLerp * sprintLerpMultiplier * aimSpeed * Time.deltaTime);
            sprintLerp = 0f;
        }
        else
        {
            // Move to running rotation
            sprintLerp += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(50, 180, 0), sprintLerp * sprintLerpMultiplier * aimSpeed * Time.deltaTime);
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
                    canvasManagerScript.Hitmarker(hit.point, true);
                else
                    canvasManagerScript.Hitmarker(hit.point, false);
            }

            switch (hit.collider.tag)
            {
                // HEAD
                case "Head":
                    enemy.TakeDamage(Mathf.RoundToInt(damage * headshotMultiplier), percentageDamage, true);
                    if (limbScript != null && enemy.GetHealth() <= 0) limbScript.RemoveLimb(0); // Beheading
                    break;

                // LEGS
                case "UpperLegL":
                    enemy.TakeDamage(damage, percentageDamage);
                    if (limbScript != null && enemy.GetHealth() <= 50) limbScript.RemoveLimb(2);
                    break;

                case "UpperLegR":
                    enemy.TakeDamage(damage, percentageDamage);
                    if (limbScript != null && enemy.GetHealth() <= 50) limbScript.RemoveLimb(4);
                    break;

                case "LowerLegL":
                    enemy.TakeDamage(damage, percentageDamage);
                    if (limbScript != null && enemy.GetHealth() <= 50) limbScript.RemoveLimb(1);
                    break;

                case "LowerLegR":
                    enemy.TakeDamage(damage, percentageDamage);
                    if (limbScript != null && enemy.GetHealth() <= 50) limbScript.RemoveLimb(3);
                    break;

                // ARMS
                case "ArmL":
                    enemy.TakeDamage(damage, percentageDamage);
                    if (limbScript != null && enemy.GetHealth() <= 50) limbScript.RemoveLimb(7);
                    break;

                case "ArmR":
                    enemy.TakeDamage(damage, percentageDamage);
                    if (limbScript != null && enemy.GetHealth() <= 50) limbScript.RemoveLimb(5);
                    break;

                case "ShoulderL":
                    enemy.TakeDamage(damage, percentageDamage);
                    if (limbScript != null && enemy.GetHealth() <= 50) limbScript.RemoveLimb(8);
                    break;

                case "ShoulderR":
                    enemy.TakeDamage(damage, percentageDamage);
                    if (limbScript != null && enemy.GetHealth() <= 50) limbScript.RemoveLimb(6);
                    break;

                // TORSO
                case "Torso":
                    enemy.TakeDamage(damage, percentageDamage);

                    // Torso Punch ability
                    if (AbilityMaster.abilities.Contains(6))
                    {
                        if (UnityEngine.Random.value < 0.25f) // 25% chance
                        {
                            enemy.TurnOnRagdoll();
                            audioSource.PlayOneShot(AbilityMaster.instance.abilitiesList[6].activateSFX);
                            enemy.Invoke("TurnOffRagdoll", 1f);
                        }
                    }
                    break;
            }
        }
        else // Hit something like ground
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
        currentMagazine = ammoAmount;
        magString = currentMagazine.ToString() + " / " + magazineSize.ToString();
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

    // Update values and stuff
    public void RefreshGun()
    {
        UpdateFirerate();
        UpdateRecoil();

        // If we own the viper venom ability, adjust percentage damage
        if (AbilityMaster.abilities.Contains(2)) percentageDamage = 5;
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
