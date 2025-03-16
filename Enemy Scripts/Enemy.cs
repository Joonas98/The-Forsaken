using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
	[Header("Basic variables")]
	public int moneyReward;

	[Header("Health variables")]
	public int maxHealth;
	[HideInInspector] public int currentHealth, healthPercentage;
	[HideInInspector] public bool isDead = false;
	[Tooltip("Delay after dying to removal")] public float despawnTime;

	private int[] limbHealths = new int[9];
	private float limbMinHealthPercentage = 0.2f;
	private float limbMaxHealthPercentage = 0.5f;

	[Header("Attack Settings")]
	public float attackDistance = 2f;      // The distance within which the enemy can attack
	public float attackAnimationTime = 1.0f; // Total duration of the attack animation
	public float attackHitTime = 0.5f;     // When damage is applied during the animation
	public int damage = 20;
	public float slowAmount = 0.5f; // Hitting the player slows them down
	public float slowDuration = 0.5f;
	public bool isAttacking = false;

	private bool canAttack = true;

	[Header("References")]
	public LimbManager limbManager;
	public List<Collider> ragdollParts = new List<Collider>();
	public Collider[] damagers;
	public GameObject[] zombieSkins;
	public GameObject eyeRight, eyeLeft;
	public GameObject modelRoot; // Hips
	public Rigidbody[] rigidbodies;
	public Rigidbody bodyRB; // Rigidbody in waist or something (used for ragdoll magnitude checks etc.)
	public Transform torsoTransform; // Used e.g. turret targeting
	public DebuffManager debuffManager;

	[SerializeField] private Animator animator; // In this script to update the velocity parameter
	[SerializeField] private EnemyStateMachine stateMachine;
	private GameObject player;

	[Header("Debuff variables")]
	public int crimsonDamage; // How much crimson debuff damage ramps up each time damage is taken

	private int crimsonDamageCurrent; // The current amount of damage crimson debuff deals each time

	[Header("Navigation and movement")]
	public EnemyNav enemyNavScript;
	public NavMeshAgent navAgent;
	public float standUpMagnitude, standUpDelay;
	public float movementSpeed;
	public bool isCrawling = false;
	public bool ragdolling = false;

	[SerializeField] private float crawlingSpeedMultiplier;
	private float ogMovementSpeed;

	[Header("Damage Popup")]
	public Transform popupTransform;
	public DamageNumber[] dpopPrefabs;
	public enum DamageType // Dpops for different purposes
	{
		Normal, Headshot, Healing, Fire, Shock, Crimson
	}

	[Header("Effects")]
	public ParticleSystem bloodFX;
	public Gradient eyeGradient;
	public Material originalMaterial;

	[SerializeField] private float eyeEmissionIntensity;
	private Material eyeMaterialR, eyeMaterialL;
	private SkinnedMeshRenderer smr;
	private Material newMaterial;

	[Header("Audio")]
	public AudioSource audioSource;
	public AudioClip[] takeDamageSounds;
	public AudioClip[] attackSounds;
	public AudioClip[] randomSounds;

	// Various privates
	private bool standCountdownActive = false;
	private float countdown = 0f;
	private float distanceToPlayer;

	// System to handle slows
	private struct SlowEffect
	{
		public float slowPercentage;
		public float duration;
	}

	private SlowEffect[] slowEffects;
	private int barricadeCount = 0; // Count if we are inside one or more barricades (to apply slow while inside 1 or more)

	private void Awake()
	{
		//GameManager.GM.enemiesAlive.Add(this);
		GameManager.GM.enemiesAliveGos.Add(gameObject);

		player = GameObject.Find("Player");
		rigidbodies = GetComponentsInChildren<Rigidbody>();

		SetRagdollParts();
		RandomizeSkins();
		SetLimbHealth();

		float randomScaling = Random.Range(1.0f, 1.2f); // Default scale is 1.1
		transform.localScale = new Vector3(randomScaling, randomScaling, randomScaling);
		currentHealth = maxHealth;
		navAgent.speed = movementSpeed;
		ogMovementSpeed = navAgent.speed;

		healthPercentage = Mathf.Clamp((100 * currentHealth) / maxHealth, 0, 100); // Right HP% at the start
		UpdateEyeColor(); // Make sure that the eyes are correct at the start

		// Make sure that the enemy is spawned on navmesh
		if (enemyNavScript.IsAgentOnNavMesh(gameObject) == false) enemyNavScript.MoveToNavMesh();
	}

	private void Start()
	{
		slowEffects = new SlowEffect[5]; // Maximum of 5 slow effects should be enough 
	}

	private void Update()
	{
		distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
		CalculateSlows();
		HandleSwinging();

		//if (animator != null) animator.SetFloat("Locomotion", navAgent.velocity.magnitude / navAgent.speed);
		if (animator != null) animator.SetFloat("Velocity", navAgent.velocity.magnitude / navAgent.speed);
	}

	private void FixedUpdate()
	{
		CheckRagdollMagnitude();
	}

	private void SetLimbHealth()
	{
		for (int i = 0; i < limbHealths.Length; i++)
		{
			if (i == 0) // Set head to maxHealth
			{
				limbHealths[i] = maxHealth;
			}
			else
			{
				float healthPercentage = Random.Range(limbMinHealthPercentage, limbMaxHealthPercentage);
				int finalLimbHealth = Mathf.RoundToInt(maxHealth * healthPercentage);
				limbHealths[i] = finalLimbHealth;
			}
		}
	}

	private void CheckRagdollMagnitude()
	{
		if (!ragdolling || isDead) return;
		// When magnitude has been low enough for certain time, stand up
		if (bodyRB.linearVelocity.magnitude < standUpMagnitude && ragdolling && !standCountdownActive)
		{
			countdown = Time.time;
			standCountdownActive = true;
		}
		else if (bodyRB.linearVelocity.magnitude > standUpMagnitude && ragdolling)
		{
			standCountdownActive = false;
		}

		if (Time.time > countdown + standUpDelay && bodyRB.linearVelocity.magnitude < standUpMagnitude)
		{
			TurnOffRagdoll();
		}
	}

	public void Die()
	{
		if (isDead) return;
		isDead = true;

		TurnOnRagdoll();
		Destroy(gameObject, despawnTime);

		navAgent.speed = 0;
		enemyNavScript.enabled = false;

		bodyRB.linearVelocity = new Vector3(0, 0, 0);
		foreach (Rigidbody rb in rigidbodies) // Otherwise gameobjects keep moving forever
		{
			rb.linearVelocity = new Vector3(0, 0, 0);
		}

		GameManager.GM.enemyCount--;
		GameManager.GM.UpdateEnemyCount();
		GameManager.GM.AdjustMoney(moneyReward);
		GameManager.GM.ConfirmKillFX();
		//GameManager.GM.enemiesAlive.Remove(this);
		GameManager.GM.enemiesAliveGos.Remove(gameObject);
	}

	public void Despawn() // Not in use
	{
		GameManager.GM.enemyCount--;
		GameManager.GM.UpdateEnemyCount();

		Destroy(gameObject, 0f);
	}

	public void Stun(float duration)
	{
		if (ragdolling || isDead) return;
		navAgent.isStopped = true;
		animator.enabled = false;
		Invoke(nameof(Unstun), duration);
	}

	public void Unstun()
	{
		if (ragdolling || isDead) return;
		navAgent.isStopped = false;
		animator.enabled = true;
	}

	public void HandlePopup(int number, DamageType type = DamageType.Normal)
	{
		if (isDead) return;
		if (number == 0) return;

		DamageNumber damageNumber;

		// We choose the right damage popup with the type enum
		switch (type)
		{
			case DamageType.Normal:
				// Debug.Log("Normal popup");
				damageNumber = dpopPrefabs[0].Spawn(popupTransform.position, number);
				break;

			case DamageType.Headshot:
				// Debug.Log("Headshot popup");
				damageNumber = dpopPrefabs[1].Spawn(popupTransform.position, number);
				break;

			case DamageType.Healing:
				// Debug.Log("Healing popup");
				damageNumber = dpopPrefabs[2].Spawn(popupTransform.position, number);
				break;

			case DamageType.Fire:
				// Debug.Log("Fire popup");
				damageNumber = dpopPrefabs[3].Spawn(popupTransform.position, number);
				break;

			case DamageType.Shock:
				// Debug.Log("Shock popup");
				damageNumber = dpopPrefabs[4].Spawn(popupTransform.position, number);
				break;

			case DamageType.Crimson:
				// Debug.Log("Crimson popup");
				damageNumber = dpopPrefabs[5].Spawn(popupTransform.position, number);
				break;

			default:
				// Debug.Log("Default popup");
				damageNumber = dpopPrefabs[0].Spawn(popupTransform.position, number);
				break;
		}
		damageNumber.spamGroup = gameObject.GetInstanceID().ToString();
		damageNumber.followedTarget = torsoTransform;
	}

	// The actual damage processing, should be always called via TakeDamage() functions
	public void TakeDamage(int damage, int percentageAmount = 0, DamageType type = DamageType.Normal)
	{
		if (debuffManager.IsDebuffActive(DebuffManager.Debuffs.Crimson) && type != DamageType.Crimson)
		{
			// Debug.Log("Dealing crimson damage");
			crimsonDamageCurrent += crimsonDamage;
			TakeDamage(crimsonDamageCurrent, 0, DamageType.Crimson);
		}

		// If optional percentageAmount was given, add % based damage to the actual damage
		if (percentageAmount > 0)
		{
			// Use float arithmetic to avoid truncation
			float hpPercentageFloat = (maxHealth * percentageAmount) / 100f;
			int hpPercentage = Mathf.RoundToInt(hpPercentageFloat);
			damage += hpPercentage;
		}

		// Handle popup according to the dmg amount and type
		HandlePopup(damage, type);

		// Adjust health and health %
		currentHealth -= damage;
		healthPercentage = Mathf.Clamp((100 * currentHealth) / maxHealth, 0, 100);

		// Visual updates according to damage
		newMaterial.SetFloat("_BloodAmount", 1f); // Blood on the skinned mesh renderer
		UpdateEyeColor(); // Enemy health can be seen from eyes

		// Sometimes make noise when damaged
		if (!isDead && Random.Range(0, 3) == 1)
		{
			int rindex = Random.Range(0, takeDamageSounds.Length);
			audioSource.PlayOneShot(takeDamageSounds[rindex]);
		}

		if (currentHealth <= 0 && !isDead)
		{
			// Todo: add death sound?
			Die();
		}
	}

	public void Heal(int amount)
	{
		HandlePopup(amount, DamageType.Healing);
		currentHealth += amount;
		if (currentHealth > maxHealth) currentHealth = maxHealth;
		healthPercentage = Mathf.Clamp((100 * currentHealth) / maxHealth, 0, 100);
		UpdateEyeColor(); // Enemy health can be seen from eyes
	}

	public void GetShot(RaycastHit hit, int damageAmount, int percentageDamage = 0)
	{
		if (AbilityMaster.instance.HasAbility("Viper Venom"))
			percentageDamage += 5;

		EnemyImpactFX(hit.point, hit.normal);

		// Helper for limb hits
		void HandleLimbHit(LimbManager.Limb limb, int dmg, int pct)
		{
			TakeDamage(dmg, pct);
			DamageLimb(limb, dmg);
			if (limbManager != null && GetHealth(limb) <= 0)
			{
				limbManager.RemoveLimb(limb);
			}
		}

		switch (hit.collider.tag)
		{
			// HEAD
			case "Head":
				TakeDamage(damageAmount, percentageDamage, DamageType.Headshot);
				DamageLimb(LimbManager.Limb.Head, damageAmount);
				if (limbManager != null && GetHealth(LimbManager.Limb.Head) <= 0)
					limbManager.RemoveLimb(LimbManager.Limb.Head);
				break;

			// LEGS
			case "UpperLegL":
				HandleLimbHit(LimbManager.Limb.LeftUpperLeg, damageAmount, percentageDamage);
				break;
			case "UpperLegR":
				HandleLimbHit(LimbManager.Limb.RightUpperLeg, damageAmount, percentageDamage);
				break;
			case "LowerLegL":
				HandleLimbHit(LimbManager.Limb.LeftLowerLeg, damageAmount, percentageDamage);
				break;
			case "LowerLegR":
				HandleLimbHit(LimbManager.Limb.RightLowerLeg, damageAmount, percentageDamage);
				break;

			// ARMS
			case "ArmL":
				HandleLimbHit(LimbManager.Limb.LeftArm, damageAmount, percentageDamage);
				break;
			case "ArmR":
				HandleLimbHit(LimbManager.Limb.RightArm, damageAmount, percentageDamage);
				break;
			case "ShoulderL":
				HandleLimbHit(LimbManager.Limb.LeftShoulder, damageAmount, percentageDamage);
				break;
			case "ShoulderR":
				HandleLimbHit(LimbManager.Limb.RightShoulder, damageAmount, percentageDamage);
				break;

			// TORSO
			case "Torso":
				TakeDamage(damageAmount, percentageDamage);
				break;
		}
	}

	public void GetHit(Collider hitCollider, int damageAmount, float headshotMultiplier, int percentageDamage = 0)
	{
		// Helper function for limb hits to avoid repetition
		void HandleLimbHit(LimbManager.Limb limb, int dmg, int pct)
		{
			TakeDamage(dmg, pct);
			DamageLimb(limb, dmg);
			if (limbManager != null && GetHealth(limb) <= 0)
			{
				limbManager.RemoveLimb(limb);
			}
		}

		switch (hitCollider.tag)
		{
			// HEAD - Special case with headshot multiplier
			case "Head":
				{
					int finalDamage = Mathf.RoundToInt(damageAmount * headshotMultiplier);
					TakeDamage(finalDamage, percentageDamage, DamageType.Headshot);
					DamageLimb(LimbManager.Limb.Head, finalDamage);  // Neck enum represents head area
					if (limbManager != null && GetHealth(LimbManager.Limb.Head) <= 0)
						limbManager.RemoveLimb(LimbManager.Limb.Head);
				}
				break;

			// LEGS
			case "UpperLegL":
				HandleLimbHit(LimbManager.Limb.LeftUpperLeg, damageAmount, percentageDamage);
				break;
			case "UpperLegR":
				HandleLimbHit(LimbManager.Limb.RightUpperLeg, damageAmount, percentageDamage);
				break;
			case "LowerLegL":
				HandleLimbHit(LimbManager.Limb.LeftLowerLeg, damageAmount, percentageDamage);
				break;
			case "LowerLegR":
				HandleLimbHit(LimbManager.Limb.RightLowerLeg, damageAmount, percentageDamage);
				break;

			// ARMS
			case "ArmL":
				HandleLimbHit(LimbManager.Limb.LeftArm, damageAmount, percentageDamage);
				break;
			case "ArmR":
				HandleLimbHit(LimbManager.Limb.RightArm, damageAmount, percentageDamage);
				break;
			case "ShoulderL":
				HandleLimbHit(LimbManager.Limb.LeftShoulder, damageAmount, percentageDamage);
				break;
			case "ShoulderR":
				HandleLimbHit(LimbManager.Limb.RightShoulder, damageAmount, percentageDamage);
				break;

			// TORSO - No limb removal, just normal damage
			case "Torso":
				TakeDamage(damageAmount, percentageDamage);
				break;
		}
	}

	public void DamageLimb(LimbManager.Limb limb, int damage)
	{
		limbHealths[(int)limb] -= damage;
	}

	// Get enemy health
	public int GetHealth()
	{
		return currentHealth;
	}

	// Get limb health with index
	public int GetHealth(int limbIndex)
	{
		return limbHealths[limbIndex];
	}

	// Get limb health with Limb enum
	public int GetHealth(LimbManager.Limb limb)
	{
		return limbHealths[(int)limb];
	}

	private void SetRagdollParts()
	{
		Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();

		foreach (Collider c in colliders)
		{
			if (c.gameObject != this.gameObject)
			{
				c.isTrigger = true;
				ragdollParts.Add(c);
			}

		}
	}

	public void TurnOnRagdoll()
	{
		if (ragdolling) return;
		ragdolling = true;

		foreach (Rigidbody rb in rigidbodies)
		{
			rb.isKinematic = false;
		}

		foreach (Collider c in ragdollParts)
		{
			c.isTrigger = false;
		}

		// Stop and disable the NavMesh agent
		if (navAgent != null)
		{
			navAgent.isStopped = true;
			navAgent.enabled = false;
		}

		standCountdownActive = false;
		animator.enabled = false;
	}

	public void TurnOffRagdoll()
	{
		if (isDead || !ragdolling) return;
		ragdolling = false;

		foreach (Rigidbody rb in rigidbodies)
		{
			rb.isKinematic = true;
		}

		foreach (Collider c in ragdollParts)
		{
			c.isTrigger = true;
		}

		transform.position = modelRoot.transform.position; //Enemy GO does not move with ragdoll, so do that when stop ragdoll
		animator.enabled = true;

		// Re-enable and start the NavMesh agent
		if (navAgent != null)
		{
			navAgent.enabled = true;
			navAgent.isStopped = true;
			if (!isCrawling)
			{
				animator.Play("Stand up");
				Invoke(nameof(ContinueAfterRagdoll), 2f);
			}
			else
			{
				animator.Play("Base Blend Tree Crawl");
				Invoke(nameof(ContinueAfterRagdoll), 1f);
			}
		}
	}

	private void ContinueAfterRagdoll()
	{
		// 6.9.2023 Trying to always call MoveToNavMesh() because this is causing errors sometimes
		// if (enemyNavScript.IsAgentOnNavMesh(gameObject) == false) enemyNavScript.MoveToNavMesh();
		if (ragdolling) return;
		enemyNavScript.MoveToNavMesh();
		if (!navAgent.isActiveAndEnabled) navAgent.enabled = true;
		navAgent.isStopped = false;
	}

	private void HandleSwinging()
	{
		if (distanceToPlayer < attackDistance && canAttack && !isAttacking)
		{
			// Start the attack
			StartCoroutine(AttackRoutine());
		}
	}

	private IEnumerator AttackRoutine()
	{
		if (isDead || ragdolling)
			yield break;

		isAttacking = true;   // We are in the middle of an attack
		canAttack = false;    // Disallow immediate re-attack

		// Optionally play random attack sound (only sometimes)
		if (!isDead && !ragdolling && Random.Range(0, 3) == 1)
		{
			int raIndex = Random.Range(0, attackSounds.Length);
			audioSource.PlayOneShot(attackSounds[raIndex]);
		}

		// Choose the correct animation
		// E.g., different anim if crawling or mirrored
		if (!isCrawling)
		{
			if (Random.value < 0.5f)
			{
				animator.Play("Attack");
			}
			else
			{
				animator.Play("Attack Mirrored");
			}
		}
		else
		{
			if (Random.value < 0.5f)
			{
				animator.Play("Attack Crawl");
			}
			else
			{
				animator.Play("Attack Crawl Mirrored");
			}
		}

		// Wait until the "hit moment" in the animation
		yield return new WaitForSeconds(attackHitTime);

		// Check again if still alive and not ragdolling
		if (isDead || ragdolling)
		{
			isAttacking = false;
			yield break;
		}

		// Check distance again, in case the player moved
		distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
		if (distanceToPlayer < attackDistance && !isDead && !ragdolling)
		{
			// Apply damage
			GameManager.GM.playerScript.TakeDamage(damage);

			// Optionally slow player down or apply some effect
			PlayerMovement playerMovement = GameManager.GM.playerScript.GetComponent<PlayerMovement>();
			playerMovement.ApplySpeedEffect(0.50f, 0.5f);
		}

		// Now wait for the rest of the animation to finish
		yield return new WaitForSeconds(attackAnimationTime - attackHitTime);

		// Final check
		if (isDead || ragdolling)
		{
			yield break;
		}

		// Attack is done, allow next attack
		isAttacking = false;
		canAttack = true;
	}

	public void StartCrawling()
	{
		// When any of part of the leg is destroyed, the zombie becomes a crawler
		isCrawling = true;
		animator.Play("Start Crawling");

		// Crawlers never can restore legs, so the ogMovementSpeed can be adjusted to avoid problems
		ogMovementSpeed *= crawlingSpeedMultiplier;
		navAgent.speed = ogMovementSpeed;

		// Prevent clipping through the ground
		navAgent.baseOffset = 0.05f;
	}

	private void CalculateSlows()
	{
		float cumulativeSlowPercentage = 1.0f;
		float barricadeSlow = barricadeCount >= 1 ? 0.8f : 0.0f; // Apply barricade slow if in at least 1 barricade

		for (int i = 0; i < slowEffects.Length; i++)
		{
			if (slowEffects[i].duration > 0.0f)
			{
				// Reduce the duration of the slow effect
				slowEffects[i].duration -= Time.deltaTime;

				// Apply the slow percentage to the cumulativeSlowPercentage
				cumulativeSlowPercentage *= (1.0f - slowEffects[i].slowPercentage);
			}
		}

		// Apply barricade slow to the cumulativeSlowPercentage
		cumulativeSlowPercentage *= (1.0f - barricadeSlow);

		// Calculate the effective movement speed
		movementSpeed = ogMovementSpeed * cumulativeSlowPercentage;
		if (movementSpeed < 0f) movementSpeed = 0f;

		navAgent.speed = movementSpeed;
	}

	public void ApplySlowEffect(float slowPercentage, float duration)
	{
		// Find an available slot in the slow sources array
		for (int i = 0; i < slowEffects.Length; i++)
		{
			if (slowEffects[i].duration <= 0.0f)
			{
				// Set the slow source information
				slowEffects[i].slowPercentage = slowPercentage;
				slowEffects[i].duration = duration;

				// Exit the loop
				break;
			}
		}
	}

	public void EnterBarricade()
	{
		barricadeCount++;
	}

	public void ExitBarricade()
	{
		barricadeCount--;
		if (barricadeCount < 0)
		{
			barricadeCount = 0;
		}
	}

	public void UpdateEyeColor()
	{
		Color eyeColor;

		// Adjust eye color according to possible debuffs
		if (debuffManager.IsDebuffActive(DebuffManager.Debuffs.ShockBlue))
		{
			eyeColor = debuffManager.shockedEyeColor;
		}
		else
		{
			// On default, the eye color indicates HP%
			eyeColor = eyeGradient.Evaluate(healthPercentage / 100f);
		}

		// Adjusting the emission intensity
		eyeMaterialR.SetColor("_EmissionColor", eyeColor * eyeEmissionIntensity);
		eyeMaterialL.SetColor("_EmissionColor", eyeColor * eyeEmissionIntensity);
	}

	private void RandomizeSkins()
	{
		// 18.6.23 All zombies are 1 prefab and the skin is randomized on awake
		if (zombieSkins.Length > 0)
		{
			int randomIndex = Random.Range(0, zombieSkins.Length);
			for (int i = 0; i < zombieSkins.Length; i++)
			{
				// Set the selected object active and the rest inactive
				zombieSkins[i].SetActive(i == randomIndex);
			}
		}

		// Set materials
		eyeMaterialR = eyeRight.GetComponent<Renderer>().material;
		eyeMaterialL = eyeLeft.GetComponent<Renderer>().material;
		newMaterial = new Material(originalMaterial);
		smr = GetComponentInChildren<SkinnedMeshRenderer>();
		smr.material = newMaterial;
	}

	// Blood effect at enemies
	public void EnemyImpactFX(Vector3 position, Vector3 normal)
	{
		if (bloodFX != null)
		{
			ParticleSystem bloodFXGO = Instantiate(bloodFX, position, Quaternion.identity);
			Destroy(bloodFXGO.gameObject, 2f);
		}
	}

	public void ResetCrimsonDamage()
	{
		crimsonDamageCurrent = 0;
	}
}
