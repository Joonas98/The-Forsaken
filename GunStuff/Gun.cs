using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public Sprite gunSprite;
    public string gunName;
    public bool semiAutomatic;
    // public bool shotgun;
    public int pelletCount;
    // public float shotgunDeviation;
    public float hipSpread;
    public float spread;
    public int penetration;
    public int damage;
    public float headshotMultiplier;
    public float RPM;
    public int MagazineSize;
    public float ReloadTime;
    public float knockbackPower;
    public float range;
    [Tooltip("Should be more than 1. High = faster")] [SerializeField] public float aimSpeed;
    [Tooltip("Should be 0-1. Low = more zoom")] [SerializeField] public float zoomAmount;
    public float equipTime;
    public float unequipTime;
    public int ammoType; //0 = .22 LR, 1 = HK 4.6x30mm, 2 = .357 Magnum, 3 = .45 ACP, 4 = 12 Gauge, 5 = 5.45x39, 6 = 5.56 NATO, 7 = 7.62 NATO, 8 = .50 BMG


    [Header("Recoil Settings")]
    [Tooltip("Up and down")] [SerializeField] public float recoilX;
    [Tooltip("Left and right")] [SerializeField] public float recoilY;
    [Tooltip("Tilt")] [SerializeField] public float recoilZ;
    // public float aimRecoilMultiplier;
    [SerializeField] public float snappiness;
    [SerializeField] public float returnSpeed;
    // [Tooltip("Should be 0-1")] [SerializeField] public float stationaryAccuracy;
    [Tooltip("Rekyylin kerroin ilmassa")] public float rec1;
    [Tooltip("Rekyylin kerroin maassa liikutaan ei t‰hd‰t‰")] public float rec2;
    [Tooltip("Rekyylin kerroin maassa liikutaan t‰hd‰t‰‰n")] public float rec3;
    [Tooltip("Rekyylin kerroin maassa ei liikuta ei t‰hd‰t‰")] public float rec4;
    [Tooltip("Rekyylin kerroin maassa ei liikuta t‰hd‰t‰‰n")] public float rec5;
    [Tooltip("Rekyylin kerroin ei mik‰‰n aiempi")] public float rec6;

    [Header("Visual Recoil")]
    [Tooltip("Vire ylˆs")] public float vireX = -2;
    [Tooltip("Vire vasen ja oikea")] public float vireY = 2;
    [Tooltip("Vire k‰‰ntely")] public float vireZ = 7;
    [Tooltip("Ase potkaisee taaksep‰in")] public float vireKick = 0.2f;
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

    private string reloadAnimationName, shootAnimationName, equipAnimationName, unequipAnimationName;
    private bool playedAimSound = false;
    private bool playedUnaimSound = true;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip dryFireSound;
    public AudioClip zoomScopeInSound;
    public AudioClip zoomScopeOutSound;

    public AudioClip equipSound;
    public AudioClip unequipSound;
    public AudioClip aimSound;
    public AudioClip unaimSound;

    public AudioSource audioSource;
    public AudioMixer audioMixer;

    [Header("Other Things")]
    public GameObject gunTip;
    public Transform casingTransform;
    public AnimationClip reloadAnimation;
    public String overrideReloadName;

    [SerializeField] private Animator animator;
    public GameObject aimingSpot;
    [SerializeField] private bool hasShootAnimation;

    private GameObject reloadSymbol;
    private Recoil recoilScript;
    private WeaponSway swayScript;
    private CanvasManager canvasManagerScript;
    private TextMeshProUGUI magazineText, totalAmmoText;
    private float shotCounter;
    private float FireRate;
    private int CurrentMagazine;
    [HideInInspector] public bool isFiring = false;
    private bool hasFired = false;
    private float defaultFov = 60f;

    [HideInInspector] public GameObject ImpactEffect;
    [HideInInspector] public ParticleSystem PS;
    [HideInInspector] public ParticleSystemRenderer rend;
    [HideInInspector] public Vector3 equipVector;
    [HideInInspector] public Transform equipTrans;
    [HideInInspector] public Camera mainCamera;
    [HideInInspector] public Camera weaponCam;
    [HideInInspector] public string magString, totalAmmoString;
    [HideInInspector] public bool isReloading = false;
    [HideInInspector] public bool isAiming = false;
    [HideInInspector] public bool canAim; // P‰ivitet‰‰n updatessa trueksi ellei olla ilmassa tms.
    [HideInInspector] public bool canAim2; // Aseen vaihtamisessa
    [HideInInspector] public float maxZoom, minZoom;
    [HideInInspector] public bool unequipping = false;
    [HideInInspector] public int shotsLeft;

    [HideInInspector] public BulletHoles bulletHoleScript;
    // public GameObject damagePopupText;

    private IdleSway idleSwayScript;
    private GameObject CrosshairContents;
    private Crosshair crosshairScript;
    private PlayerMovement playerMovementScript;
    private GameObject weaponSpot;
    private PlayerInventory inventoryScript;

    [HideInInspector] public Camera scopeCam = null;
    private float equipLerp;
    private float unequipLerp;
    private float equipRotX, equipRotY, equipRotZ;

    //OG THINGS
    private AudioClip shootSoundOG;
    private GameObject gunTipOG;
    [HideInInspector] public GameObject aimingSpotOG;
    private ParticleSystem muzzleFlashOG;
    private float originalSpread;
    private float aimSpeedOG;
    [HideInInspector] public float RPMOG;

    private float recoilXOG;
    private float recoilYOG;
    private float recoilZOG;

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
        totalAmmoText = GameObject.Find("TotalAmmo").GetComponent<TextMeshProUGUI>();
        GameObject CrosshairCanvas = GameObject.Find("CrossHairCanvas");
        crosshairScript = CrosshairCanvas.GetComponent<Crosshair>();
        reloadSymbol = CrosshairCanvas.transform.GetChild(0).gameObject;
        swayScript = GetComponent<WeaponSway>();
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
        originalSpread = spread;
        RPMOG = RPM;

        equipTrans = GameObject.Find("EquipTrans").transform;
    }

    private void Start()
    {

        if (weaponSpot == null)
            weaponSpot = GameObject.Find("WeaponSpot");
        idleSwayScript = GameObject.Find("WeaponHolster").GetComponent<IdleSway>();
        CrosshairContents = GameObject.Find("CrosshairPanel");
        playerMovementScript = GameObject.Find("Player").GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        shotsLeft = pelletCount;

        UpdateFirerate();

        CurrentMagazine = MagazineSize;

        magString = CurrentMagazine.ToString() + " / " + MagazineSize.ToString();
        magazineText.text = magString;

        HandleAnimationStrings();
        UpdateRecoil();
    }

    private void OnEnable()
    {
        SetFOV(defaultFov);

        magString = CurrentMagazine.ToString() + " / " + MagazineSize.ToString();
        magazineText.text = magString;

        // totalAmmoString = inventoryScript.GetAmmoCount(ammoType).ToString() + " / " + inventoryScript.GetMaxAmmoCount(ammoType).ToString() + " - " + inventoryScript.GetAmmoString(ammoType);
        // totalAmmoText.text = totalAmmoString;
        inventoryScript.UpdateTotalAmmoText(ammoType);

        UpdateRecoil(); // P‰ivitet‰‰n recoil scriptiin aseen recoil arvot
        EquipWeapon(); // Animaatiot yms. aseen esille ottamiseen

        // Kun otetaan ase esille, ase k‰‰nnet‰‰n satunnaisesta kulmasta
        equipRotX = UnityEngine.Random.Range(0, 360);
        equipRotY = UnityEngine.Random.Range(0, 360);
        equipRotZ = UnityEngine.Random.Range(0, 360);
    }

    private void Update()
    {
        HandleShooting();
        HandleAiming();
        HandleReloading();
        HandleCrosshair();
        HandleScopeZoom();
        HandleSwitchingLerps();
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

        if (Input.GetButton("Fire2") && canAim && canAim2 && !isReloading && Time.timeScale > 0)
        {
            isAiming = true;
            playedUnaimSound = false;
            idleSwayScript.ResetPosition();
            // swayScript.enabled = false;
            idleSwayScript.enabled = false;
            CrosshairContents.SetActive(false);
            WeaponSwitcher.canSwitch(false);

            transform.position = Vector3.Lerp(transform.position, transform.parent.transform.position + (transform.position - aimingSpot.transform.position), (aimSpeed * 2f) * Time.deltaTime);

            SetFOV(Mathf.Lerp(Camera.main.fieldOfView, zoomAmount * defaultFov, aimSpeed * Time.deltaTime));
            SetFOV(Mathf.Lerp(weaponCam.fieldOfView, zoomAmount * defaultFov, aimSpeed * Time.deltaTime));
        }
        else
        {
            if (playedUnaimSound == false)
            {
                playedUnaimSound = true;
                audioSource.PlayOneShot(unaimSound);
            }

            isAiming = false;
            playedAimSound = false;

            if (canAim2 == true && unequipping == false)
                transform.position = Vector3.Lerp(transform.position, weaponSpot.transform.position, (aimSpeed * 2f) * Time.deltaTime);

            SetFOV(Mathf.Lerp(Camera.main.fieldOfView, defaultFov, aimSpeed * Time.deltaTime));
            SetFOV(Mathf.Lerp(weaponCam.fieldOfView, defaultFov, aimSpeed * Time.deltaTime));
            // swayScript.enabled = true;
            idleSwayScript.enabled = true;
            CrosshairContents.SetActive(true);

            if (Time.timeScale > 0 && canAim2 && !isReloading)
                WeaponSwitcher.canSwitch(true);
        }

    }

    private void HandleAnimationStrings()
    {
        if (hasShootAnimation)
        {
            shootAnimationName = "Shoot " + gunName;
        }
        else
        {
            shootAnimationName = "";
        }

        if (overrideReloadName == "")
        {
            reloadAnimationName = "Reload " + gunName;
        }
        else
        {
            reloadAnimationName = overrideReloadName;
        }

        equipAnimationName = "Equip " + gunName;
        unequipAnimationName = "Unequip " + gunName;
    }

    public void HandleReloading()
    {

        if (isReloading)
        {
            swayScript.enabled = false;
        }

        // Reloading
        if (Input.GetKeyDown(KeyCode.R) && shotCounter <= 0 && CurrentMagazine != MagazineSize && Time.timeScale > 0 && inventoryScript.GetAmmoCount(ammoType) > 0)
        {
            swayScript.ResetSway();

            if (animator == null) animator = gameObject.GetComponentInChildren<Animator>();

            animator.SetFloat("ReloadSpeedMultiplier", reloadAnimation.length / ReloadTime);
            audioMixer.SetFloat("WeaponsPitch", reloadAnimation.length / ReloadTime);

            WeaponSwitcher.canSwitch(false);
            reloadSymbol.SetActive(true);
            shotCounter = ReloadTime;
            isReloading = true;
            audioSource.PlayOneShot(reloadSound);

            if (animator != null && reloadAnimationName != "")
                animator.Play(reloadAnimationName);

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
                shotCounter = FireRate;
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
                shotCounter = FireRate;
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

    public void HandleCrosshair()
    {
        crosshairScript.AdjustCrosshair(spread);
    }

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
    } // T‰ht‰imi‰ voi scrollata rullalla

    public void HandleSwitchingLerps()
    {
        // Ase esille
        if (canAim2 == false && !unequipping && equipLerp <= equipTime)
        {
            equipLerp += Time.deltaTime;
            transform.position = Vector3.Lerp(equipTrans.position, weaponSpot.transform.position, equipLerp / equipTime);
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(equipRotX, equipRotY, equipRotZ), weaponSpot.transform.rotation, equipLerp / equipTime);
        }

        // Ase poies
        if (unequipping && unequipLerp <= unequipTime)
        {
            unequipLerp += Time.deltaTime;
            transform.position = Vector3.Lerp(weaponSpot.transform.position, equipTrans.position, unequipLerp / unequipTime);
            transform.rotation = Quaternion.Lerp(weaponSpot.transform.rotation, Quaternion.Euler(equipRotX, equipRotY, equipRotZ), unequipLerp / unequipTime);
        }
    } // Aseen esille ottamiseen liittyv‰‰ lerppailua

    public void HandleImpact(RaycastHit hit)
    {
        if (hit.collider.gameObject.layer == 6) return; // Osuessa casings eli hylsyihin, ei tehd‰ mit‰‰n

        // Push rigidbodies
        if (hit.collider.GetComponent<Rigidbody>() != null)
        {
            Rigidbody targetRB = hit.collider.gameObject.GetComponent<Rigidbody>();
            targetRB.AddForce(((mainCamera.transform.position - hit.transform.position) * -1) * knockbackPower, ForceMode.Impulse);
        }

        // Tuhottavat jutut kuten ikkunat
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

            if (enemy.GetHealth() > 0)
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
                    if (enemy.isCrawling == false) enemy.StartCrawling();
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
                    if (enemy.isCrawling == false) enemy.StartCrawling();
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
                    if (enemy.isCrawling == false) enemy.StartCrawling();
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
                    if (enemy.isCrawling == false) enemy.StartCrawling();
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
            }

        }
        else
        {
            GroundImpactFX(hit);
            bulletHoleScript.AddBulletHole(hit);
        }
    } // K‰sitell‰‰n osuma, esim vihollisiin

    public void Shoot(int pelletCount)
    {
        MuzzleFlash.Play();
        int penetrationLeft = penetration;
        recoilScript.RecoilFire();
        // audioSource.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Length)]);
        audioSource.PlayOneShot(shootSound);

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

            if (!isAiming) hitPointsList = Physics.RaycastAll(mainCamera.transform.position, forwardVector, Mathf.Infinity);
            else hitPointsList = Physics.RaycastAll(aimingSpot.transform.position, forwardVector, Mathf.Infinity);

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
    } // Itse ampumisen funktio

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
    } // Veri efekti osumasta

    public void GroundImpactFX(RaycastHit hit)
    {
        ParticleSystem groundFXGO = Instantiate(GroundFX, hit.point, Quaternion.identity);
        Destroy(groundFXGO, 2f);
    } // Osutaan maahan

    public void DrawLaser(Vector3 v0, Vector3 v1)
    {
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
    } // Linerenderer piipun ja osutun pisteen v‰lille

    private void SetFOV(float fov)
    {
        Camera.main.fieldOfView = fov;
        weaponCam.fieldOfView = fov;
    }

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
    } // P‰ivitt‰‰ recoil skriptiin rekyyli- ja vire arvot

    private void Penetrate(RaycastHit hit, int penetrationLeft)
    {
        // OBSOLETE, NOT IN USE
        // NEW PENETRATION SYSTEM IS INSIDE SHOOT()
        Enemy hitEnemy = hit.collider.gameObject.GetComponentInParent<Enemy>();

        RaycastHit newHit;
        if (Physics.Raycast(hit.point, mainCamera.transform.TransformDirection(Vector3.forward), out newHit, range))
        {
            Enemy newEnemy = newHit.collider.gameObject.GetComponentInParent<Enemy>();
            if (penetrationLeft > 0)
            {
                penetrationLeft--;
                if (newEnemy != null)
                {
                    //  Debug.Log("New enemy penetrated");
                    HandleImpact(newHit);
                    Penetrate(newHit, penetrationLeft);

                }
                else
                {
                    //  Debug.Log("Non-enemy penetrated");
                    Penetrate(newHit, penetrationLeft);
                }
            }
        }
    }  // Ei ole en‰‰ k‰ytˆss‰

    public void EquipWeapon()
    {
        equipLerp = 0f;
        unequipLerp = 0f;
        WeaponSwitcher.canSwitch(false);
        canAim2 = false;
        unequipping = false;
        swayScript.enabled = false;
        shotCounter = equipTime;

        if (weaponSpot == null)
            weaponSpot = GameObject.Find("WeaponSpot");

        StartCoroutine(WaitEquipTime());
    } // Ase esiin

    public void UnequipWeapon()
    {
        equipLerp = 0f;
        unequipLerp = 0f;
        unequipping = true;
        canAim2 = false;
        swayScript.enabled = false;
        shotCounter = unequipTime + 0.01f;
        WeaponSwitcher.canSwitch(false);
        audioSource.PlayOneShot(unequipSound);
    } // Ase pois

    public void UpdateFirerate()
    {
        FireRate = (RPM / 60);
        FireRate = 1 / FireRate;
    }

    IEnumerator WaitReloadTime(float r, int ammoAmount)
    {
        swayScript.enabled = false;
        yield return new WaitForSeconds(r + 0.05f);
        CurrentMagazine = ammoAmount;
        magString = CurrentMagazine.ToString() + " / " + MagazineSize.ToString();
        magazineText.text = magString;
        WeaponSwitcher.canSwitch(true);
        audioMixer.SetFloat("WeaponsPitch", 1f);
        isReloading = false;
        reloadSymbol.SetActive(false);
        swayScript.enabled = true;
    } // Ienumi lataamiselle

    IEnumerator WaitEquipTime()
    {
        audioSource.PlayOneShot(equipSound);
        yield return new WaitForSeconds(equipTime);
        swayScript.enabled = true;
        canAim2 = true;
        WeaponSwitcher.canSwitch(true);
    } // Delay aseen esiin ottamiselle

    // Vaihdetaan arvoja muista skripteist‰
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

    // Palautetaan og arvot
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

    #endregion

}
