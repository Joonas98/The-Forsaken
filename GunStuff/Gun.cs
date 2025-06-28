using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class Gun : Weapon
{
	[Header("Gun Settings")]
	public bool semiAutomatic;
	public int pelletCount, penetration, damage, magazineSize;
	public float hipSpread, aimSpread, headshotMultiplier, RPM, reloadTime, knockbackPower, range;
	[Tooltip("Should be more than 1. High = faster")][SerializeField] public float aimSpeed;
	[Tooltip("Should be 0-1. Low = more zoom")][SerializeField] public float zoomAmount;

	public PlayerInventory.AmmoType gunAmmoType;

	[HideInInspector] public int percentageDamage;

	[Header("Recoil Settings")]
	[Tooltip("X = Up and down, Y = left and right, Z = tilt")] public Vector3 recoil;
	[SerializeField] public float snappiness, returnSpeed;
	[Tooltip("Recoil multiplier midair")] public float rec1;
	[Tooltip("Grounded, moving, not aiming")] public float rec2;
	[Tooltip("Grounded, moving, aiming")] public float rec3;
	[Tooltip("Grounded, not moving, not aiming")] public float rec4;
	[Tooltip("Grounded, not moving, aiming")] public float rec5;
	[Tooltip("Recoil multiplier if nothing previous matches")] public float rec6;

	[Header("Visual Recoil")]
	[Tooltip("Vire X = up, Y = left and right, Z = rotation")] public Vector3 vire;
	[Tooltip("Recoil kicking towards player")] public float vireKick = 0.2f;
	public float vireSnap = 5;
	public float vireReturn = 8;

	[Header("Effects")]
	public bool isSilenced = false; // Adjust sound and muzzle flash light
	[HideInInspector] public Light muzzleFlashLight;
	[HideInInspector] public ParticleSystem muzzleFlash;
	public ParticleSystem hitFX, groundFX;
	public LineRenderer LR;
	public bool dropCasings;
	public float casingDespawnTime = 1f;

	private GameObject casingGO; // Casing gameobject, get it from PlayerInventory
	private float laserTime = 0.05f;
	private string reloadAnimationName, shootAnimationName;
	private bool playedAimSound = false;
	private bool playedUnaimSound = true;

	[Header("Audio")]
	public AudioMixer audioMixer;
	public AudioClip shootSound, silencedShootSound;
	public AudioClip reloadSound, aimSound, unaimSound;
	public AudioClip zoomScopeInSound, zoomScopeOutSound;
	public AudioClip actionSound, dryFireSound; // Action sound is pump shotgun, bolt action etc.
	public float actionDelay = 0f; // Seconds to wait before playing action sound

	[Header("Other Things")]
	[Tooltip("What layers the gun can hit")] public LayerMask targetLayers;
	public Transform gunTip;
	[HideInInspector] public GameObject aimingSpot;
	[HideInInspector] public Transform casingTransform;
	public AnimationClip reloadAnimation;
	public TextMeshProUGUI spreadText;
	[HideInInspector] public Camera scopeCam = null;
	[HideInInspector] public GameObject ImpactEffect;
	[HideInInspector] public ParticleSystem PS;
	[HideInInspector] public Vector3 equipVector;
	[HideInInspector] public Camera mainCamera, weaponCam;
	[HideInInspector] public BulletHoles bulletHoleScript;
	[HideInInspector] public bool isFiring = false;
	[HideInInspector] public bool isReloading = false, isAiming = false, playingAction = false;
	[HideInInspector] public bool canAim; // True in update unless mid air etc.
	[HideInInspector] public float maxZoom = 0.25f; // Scope zoom default value for guns such as AUG that has integrated scope
	[HideInInspector] public float minZoom = 45f;
	[HideInInspector] public int currentMagazine;

	[SerializeField] private bool hasShootAnimation; // Is there animation of moving slides, pumping shotgun, bolt action rifles etc.
	[SerializeField] private bool shootAnimationMovement; // Does the shoot animation include movement or rotation of the weapon (if yes, don't apply rotation during it)
	[SerializeField] private AnimationClip shootAnimation; // Used purely to get the length of this animation

	private Animator animator;
	private Recoil recoilScript;
	private PlayerMovement playerMovementScript;
	private PlayerInventory inventoryScript;
	private GameObject crosshairContents;

	private float shotCounter, fireRate;
	private bool hasFired = false;
	private float sprintLerp, unsprintLerp; // Timers to handle lerping
	private float currentSpread;

	// Original variables
	[HideInInspector] public float RPMOG;
	[HideInInspector] public GameObject aimingSpotOG;

	private AudioClip shootSoundOG;
	private Transform gunTipOG;
	private ParticleSystem muzzleFlashOG;
	private float aimSpeedOG;
	private Vector3 recoilOG;
	protected float ogReloadTime;
	private VisualRecoil vireScript;
	private const float unsprintLerpThreshold = 30f; // We don't want to be able to shoot right away after sprinting
	private const float sprintLerpMultiplier = 15f; // Weapon sprint lerp and readiness: aimSpeed * this

	// OnValidate function is called on editor instead of runtime
	// So this is to automatize tasks like set references whilst keeping runtime activity minimal
	protected override void OnValidate()
	{
		base.OnValidate();
		HandlePrefabReferences();
	}

	protected override void Awake()
	{
		base.Awake();
		HandlePrefabReferences();
		HandleSceneReferences();
		currentMagazine = magazineSize;
	}

	private void Start()
	{
		RefreshGun();

		// Set shooting animation name if there is one
		shootAnimationName = hasShootAnimation ? "Shoot " + weaponName : "";
	}

	// Find some references for the script within the prefab
	private void HandlePrefabReferences()
	{
		if (animator == null) animator = GetComponent<Animator>();
		if (gunTip == null) gunTip = transform.Find("GunTip");
		if (bulletHoleScript == null) bulletHoleScript = GetComponent<BulletHoles>();
		if (muzzleFlash == null) muzzleFlash = gunTip.GetComponentInChildren<ParticleSystem>();
		if (muzzleFlashLight == null) muzzleFlashLight = gunTip.GetComponent<Light>();
		if (aimingSpot == null) aimingSpot = transform.Find("AimSpot").gameObject;
		if (casingTransform == null) casingTransform = transform.Find("CasingSpot");
		if (muzzleFlashLight != null) isSilenced = false;
	}

	// Find some references for the script within the scene
	private void HandleSceneReferences()
	{
		mainCamera = Camera.main;
		vireScript = GetComponentInParent<VisualRecoil>();
		recoilScript = GetComponentInParent<Recoil>();
		inventoryScript = GetComponentInParent<PlayerInventory>();
		playerMovementScript = GetComponentInParent<PlayerMovement>();

		equipTrans = GameObject.Find("EquipTrans").transform;
		crosshairContents = GameObject.Find("CrosshairPanel");

		// Original values
		ogReloadTime = reloadTime;
		aimSpeedOG = aimSpeed;
		shootSoundOG = shootSound;
		muzzleFlashOG = muzzleFlash;
		gunTipOG = gunTip;
		aimingSpotOG = aimingSpot;
		recoilOG = recoil;
		RPMOG = RPM;

		// Set casing gameobject
		if (gunAmmoType != PlayerInventory.AmmoType.Magnum357)
		{
			casingGO = PlayerInventory.instance.GetCasingPrefab(gunAmmoType);
		}
		else
		{
			casingGO = null;
		}
	}

	protected void OnEnable()
	{
		// Handle ammo UI 
		AmmoHUD.Instance.UpdateAmmoHUD(currentMagazine, magazineSize);
		inventoryScript.UpdateTotalAmmoText(gunAmmoType);

		// Update desired aiming fov to FovController
		FovController.Instance.fovAim = zoomAmount * FovController.Instance.fovDefault;


		RefreshGun();
		EquipWeapon(); // Animations etc. when equipping weapon
	}

	protected void Update()
	{
		if (Time.timeScale <= 0) return; // Game paused

		shotCounter -= Time.deltaTime;

		// Player can't shoot when selecting grenades or objects
		if (!equipped || ObjectPlacing.instance.isPlacing || SelectionCanvas.instance.isChoosingObject) goto selectionsSkip;
		HandleShooting();
		HandleAiming();
	selectionsSkip:
		HandleReloading();
		HandleScopeZoom();
		HandleSprinting();
		HandleSpread();
		// Debug.Log(unsprintLerp * sprintLerpMultiplier * aimSpeed);

		// Light from muzzle flash that is not too expensive and looks nice enough
		if (muzzleFlashLight == null || isSilenced) return;
		bool isEmitting = false;
		if (muzzleFlash != null) isEmitting = muzzleFlash.isEmitting;
		muzzleFlashLight.enabled = isEmitting;
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
		if (Input.GetButton("Fire2") && canAim && equipped && !isReloading)
		{
			isAiming = true;
			playedUnaimSound = false;

			transform.position = Vector3.Lerp(transform.position, transform.parent.transform.position + (transform.position - aimingSpot.transform.position), aimSpeed * Time.deltaTime);
			transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, 180, 0), aimSpeed * Time.deltaTime);
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

			if (equipped && !playingAction)
				transform.position = Vector3.Slerp(transform.position, weaponSpot.transform.position, aimSpeed * Time.deltaTime);
		}
	}

	public void HandleReloading()
	{
		// Reloading
		if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentMagazine != magazineSize && inventoryScript.GetAmmoCount(gunAmmoType) > 0)
		{
			isReloading = true;

			// Reset rotation
			ResetRotation();

			// Adjust reload speed to animation and sound
			if (reloadAnimation != null)
			{
				animator.SetFloat("ReloadSpeedMultiplier", reloadAnimation.length / reloadTime);
				audioMixer.SetFloat("WeaponsPitch", reloadAnimation.length / reloadTime);
			}

			shotCounter = reloadTime;
			audioSource.PlayOneShot(reloadSound);

			if (animator != null && reloadAnimation != null)
				animator.Play(reloadAnimation.name);

			// Handle ammo correctly
			if (inventoryScript.GetAmmoCount(gunAmmoType) >= magazineSize)
			{
				// Debug.Log("Full reload");
				StartCoroutine(WaitReloadTime(reloadTime, magazineSize));
				inventoryScript.HandleAmmo(gunAmmoType, currentMagazine - magazineSize);
			}
			else if (inventoryScript.GetAmmoCount(gunAmmoType) + currentMagazine >= magazineSize)
			{
				// Debug.Log("Stock + clip >= full mag");
				StartCoroutine(WaitReloadTime(reloadTime, magazineSize));
				inventoryScript.HandleAmmo(gunAmmoType, currentMagazine - magazineSize);
			}
			else if (inventoryScript.GetAmmoCount(gunAmmoType) < magazineSize)
			{
				// Debug.Log("Stock + clip < full mag");
				StartCoroutine(WaitReloadTime(reloadTime, inventoryScript.GetAmmoCount(gunAmmoType) + currentMagazine));
				inventoryScript.HandleAmmo(gunAmmoType, inventoryScript.GetAmmoCount(gunAmmoType) * -1);
			}
		}
	}

	public void HandleShooting()
	{
		// Shooting
		if (Input.GetButton("Fire1") && ((unsprintLerp * sprintLerpMultiplier * aimSpeed) > unsprintLerpThreshold || AbilityMaster.instance.HasAbility("Centaur")))
		{
			isFiring = true;
		}
		else if (Input.GetButtonUp("Fire1") || !Input.GetButton("Fire1"))
		{
			isFiring = false;
			hasFired = false;
		}

		// Check if the player is running without the "Centaur" ability
		if (playerMovementScript.isRunning && !AbilityMaster.instance.HasAbility("Centaur"))
		{
			// Prevent firing while running
			isFiring = false;
			hasFired = false; // Reset the hasFired state
			return;
		}

		// Fully automatic weapons
		if (isFiring && !semiAutomatic)
		{
			if (shotCounter <= 0 && currentMagazine > 0) // Shooting
			{
				shotCounter = fireRate;
				Shoot(pelletCount);
				vireScript.Recoil();
				--currentMagazine;
				AmmoHUD.Instance.UpdateAmmoHUD(currentMagazine, magazineSize);
			}
			else if (shotCounter <= 0 && currentMagazine <= 0) // Dry fire automatic
			{
				shotCounter = fireRate * 3; // Longer shotCounter so dryfire does not spam fast
				if (animator != null && shootAnimationName != "") animator.Play(shootAnimationName);
				audioSource.PlayOneShot(dryFireSound);
				isFiring = false;
			}

			// playingAction has to be on true whilst playing animation such as bolt action or pumping shotgun
			if (shootAnimationMovement && !playingAction && !isReloading)
			{
				// Action sound like pump shotgun or bolt action rifles
				if (actionSound != null) Invoke(nameof(PlayActionSound), actionDelay);

				playingAction = true;
				StartCoroutine(WaitActionAnimation(shootAnimation.length));
			}
		}

		// Semi automatic weapons
		else if (isFiring && semiAutomatic && !hasFired)
		{
			if (shotCounter <= 0 && currentMagazine > 0) //Shooting
			{
				shotCounter = fireRate;
				Shoot(pelletCount);
				vireScript.Recoil();
				--currentMagazine;
				AmmoHUD.Instance.UpdateAmmoHUD(currentMagazine, magazineSize);
			}
			else if (shotCounter <= 0 && currentMagazine <= 0) // Dry fire semi auto
			{
				shotCounter = fireRate;
				if (animator != null && shootAnimationName != "") animator.Play(shootAnimationName);
				audioSource.PlayOneShot(dryFireSound);
				isFiring = false;
			}
			hasFired = true;

			// playingAction has to be on true whilst playing animation such as bolt action or pumping shotgun
			if (shootAnimationMovement && !playingAction && !isReloading)
			{
				// Action sound like pump shotgun or bolt action rifles
				if (actionSound != null) Invoke(nameof(PlayActionSound), actionDelay);

				playingAction = true;
				StartCoroutine(WaitActionAnimation(shootAnimation.length));
			}
		}
	}

	// Lerp weapon rotation to sprinting angle when conditions are right
	public void HandleSprinting()
	{
		// No rotating if reloading or we have Bullet Ballet ability
		if (isReloading || AbilityMaster.instance.HasAbility("Centaur")) return;

		if (playingAction || !equipped) return;

		if (!playerMovementScript.isRunning)
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

	// Mouse wheel changes scope zoom
	private void HandleScopeZoom()
	{
		if (scopeCam == null) return;

		// Zoom out
		if (isAiming && Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			scopeCam.fieldOfView += 1;
			scopeCam.fieldOfView *= 1.1f;

			// Clamp zoom and play audio if not at limit
			if (scopeCam.fieldOfView > minZoom) scopeCam.fieldOfView = minZoom;
			else audioSource.PlayOneShot(zoomScopeInSound);
		}

		// Zoom in
		if (isAiming && Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			scopeCam.fieldOfView -= 1;
			scopeCam.fieldOfView *= 0.9f;

			// Clamp zoom and play audio if not at limit
			if (scopeCam.fieldOfView < maxZoom) scopeCam.fieldOfView = maxZoom;
			else audioSource.PlayOneShot(zoomScopeOutSound);
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

		// Handle enemies
		Enemy enemy = hit.collider.gameObject.GetComponentInParent<Enemy>();
		if (enemy != null)
		{

			if (enemy.GetHealth() > 0) // Hitmarker
			{
				if (hit.collider.CompareTag("Head"))
					HitmarkAndCrosshair.instance.Hitmarker(hit.point, true);
				else
					HitmarkAndCrosshair.instance.Hitmarker(hit.point, false);
			}

			if (hit.collider.CompareTag("Head"))
			{
				enemy.GetShot(hit, Mathf.RoundToInt(damage * headshotMultiplier), percentageDamage);
			}
			else
			{
				enemy.GetShot(hit, damage, percentageDamage);
			}

			// Torso Punch ability
			if (AbilityMaster.instance.HasAbility("Torso Punch") && hit.collider.CompareTag("Torso"))
			{
				if (UnityEngine.Random.value < 0.25f) // 25% chance
				{
					if (enemy.isDead) return;
					//enemy.TurnOnRagdoll();
					enemy.ApplyStagger();
					// TODO FIX DUE TO ABILITY REWORK 7.12.2024
					//audioSource.PlayOneShot(AbilityMaster.instance.abilitiesList[6].activateSFX);
				}
			}

		}
		else // Hit something like ground
		{
			GroundImpactFX(hit);

			// Unused for now at least
			//bulletHoleScript.AddBulletHole(hit);
		}
	}

	// Main shooting function
	public void Shoot(int pelletCount)
	{
		// Effects
		// Play muzzle flash VFX
		if (muzzleFlash != null) muzzleFlash.Play();
		else Debug.Log("No muzzle flash reference!");

		// Play shooting sound
		if (!isSilenced) audioSource.PlayOneShot(shootSound);
		else audioSource.PlayOneShot(silencedShootSound);

		// Play shooting animation if we have one
		if (animator != null && shootAnimationName != "") animator.Play(shootAnimationName);

		// Drop casing and recoil
		DropCasing();
		recoilScript.RecoilFire();

		// Initiate local values
		int penetrationLeft = penetration;
		int pelletsLeft = pelletCount;

		// Loop each bullet
		for (int i = pelletsLeft; i > 0; i--)
		{
			float deviation;
			deviation = UnityEngine.Random.Range(0f, currentSpread);
			Vector3 forwardVector = Vector3.forward;
			float angle = UnityEngine.Random.Range(0f, 360f);
			forwardVector = Quaternion.AngleAxis(deviation, Vector3.up) * forwardVector;
			forwardVector = Quaternion.AngleAxis(angle, Vector3.forward) * forwardVector;
			forwardVector = mainCamera.transform.rotation * forwardVector;

			RaycastHit[] hitPointsList;
			if (!isAiming) hitPointsList = Physics.RaycastAll(mainCamera.transform.position, forwardVector, Mathf.Infinity, targetLayers);
			else hitPointsList = Physics.RaycastAll(aimingSpot.transform.position, forwardVector, Mathf.Infinity, targetLayers);
			Array.Sort(hitPointsList, (x, y) => x.distance.CompareTo(y.distance));

			if (hitPointsList.Length == 0)
			{
				DrawLaser(gunTip.position, forwardVector * 5000);
			}
			else
			{
				DrawLaser(gunTip.position, hitPointsList[0].point);
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

	private void HandleSpread()
	{
		float targetSpread;
		if (isAiming)
		{
			targetSpread = aimSpread;
		}
		else
		{
			targetSpread = hipSpread;
		}

		currentSpread = Mathf.Lerp(currentSpread, targetSpread, aimSpeed * Time.deltaTime);
		if (spreadText != null) spreadText.text = currentSpread.ToString("F3") + " -spread";
	}

	// Ground impacts
	public void GroundImpactFX(RaycastHit hit)
	{
		ParticleSystem groundFXGO = Instantiate(groundFX, hit.point, Quaternion.identity);
		Destroy(groundFXGO.gameObject, 2f);
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

	public void UpdateFirerate()
	{
		fireRate = (RPM / 60);
		fireRate = 1 / fireRate;
	}

	// Reloading delay etc.
	IEnumerator WaitReloadTime(float r, int ammoAmount)
	{
		yield return new WaitForSeconds(r + 0.05f); // Wait the reload time + small extra to avoid bugs
		currentMagazine = ammoAmount;
		AmmoHUD.Instance.UpdateAmmoHUD(currentMagazine, magazineSize);
		audioMixer.SetFloat("WeaponsPitch", 1f); // Reset weapon pitch (it might be changed to match reload speed)
		isReloading = false;
	}

	IEnumerator WaitActionAnimation(float r)
	{
		yield return new WaitForSeconds(r + 0.01f);
		playingAction = false;
	}

	// Invoked after action delay
	public void PlayActionSound()
	{
		if (actionSound == null) return;
		audioSource.PlayOneShot(actionSound);
	}

	private void DropCasing()
	{
		if (!dropCasings || casingGO == null) return;

		GameObject newCasing = Instantiate(casingGO, casingTransform.position, transform.rotation * Quaternion.Euler(-90f, 0f, 0f));
		Rigidbody newCasingRB;

		if (newCasing.GetComponent<Rigidbody>() != null)
		{
			newCasingRB = newCasing.GetComponent<Rigidbody>();
		}
		else
		{
			newCasingRB = newCasing.GetComponentInChildren<Rigidbody>();
		}
		newCasingRB.AddForce(transform.up * 1f + transform.right * -1f);
		Destroy(newCasing, casingDespawnTime);
	}

	// Update values and stuff
	public void RefreshGun()
	{
		UpdateFirerate();
	}

	// Adjust values from other scripts
	#region Adjust Values 

	public void AdjustDamage(float multiplier)
	{
		damage = Mathf.RoundToInt(damage * multiplier);
	}

	public void AdjustReloadtime(float multiplier)
	{
		reloadTime *= multiplier;
	}

	public void AdjustRecoil(float xmultiplier, float ymultiplier, float zmultiplier)
	{
		recoil.x *= xmultiplier;
		recoil.y *= ymultiplier;
		recoil.z *= zmultiplier;
	}

	public void AdjustAimspeed(float multiplier)
	{
		aimSpeed *= 1f / multiplier;
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

	public void ResetRecoils()
	{
		recoil.x = recoilOG.x;
		recoil.y = recoilOG.y;
		recoil.z = recoilOG.z;
	}

	public void ResetRotation()
	{
		transform.localRotation = Quaternion.Euler(0, 180, 0);
	}

	public void ResetReloadtime()
	{
		reloadTime = ogReloadTime;
	}

	#endregion

}
