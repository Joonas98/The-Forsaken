using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
	[Header("Melee Weapon Settings")]
	public AnimationClip[] attackAnimations;

	[SerializeField] private int damage, damageSecondary;
	[SerializeField] private float headshotMultiplier;
	[SerializeField] private ParticleSystem bloodFX;
	private Animator animator;

	[Header("Audio")]
	public AudioClip[] stabSounds;
	public AudioClip[] swingSounds;
	public AudioClip hitFloorSound;

	[Header("Newer system for melee")]
	public Transform cameraTransform;  // The player's camera (or the player model)
	public int penetrationAmount = 3; // How many enemies each attack can damage
	public float attackRange = 1.0f;    // The forward distance of the attack
	public float attackRadius = 0.5f;   // The width of the capsule
	public float attackRate = 1.0f;     // Time between attacks in seconds (e.g., 1.0s for 1 attack per second)
	public float windupPercentage = 0.3f;   // Windup portion of the animation (e.g., 30% of the animation)
	public LayerMask enemyLayers;

	private float attackCooldown = 0f;  // Cooldown timer to handle attack intervals
	private bool isAttacking = false;   // Tracks if we're currently in the middle of an attack
	public bool readyToAttack; // Is weapon ready for attacking
	private float currentWindupTime = 0f;   // Time until the CapsuleCast should be triggered

	protected override void Awake()
	{
		base.Awake();
		//magazineText = GameObject.Find("MagazineNumbers").GetComponent<TextMeshProUGUI>();
		//totalAmmoText = GameObject.Find("TotalAmmo").GetComponent<TextMeshProUGUI>();
		animator = GetComponent<Animator>();

		cameraTransform = Camera.main.transform;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		AmmoHUD.Instance.DisableHUD();
		EquipWeapon();
	}

	protected override void Update()
	{
		base.Update();
		HandleAttacking();

		// When weapon is equipped and not attacking, make sure to return to correct position
		//if (equipped && !unequipping && readyToAttack)
		//{
		//	Debug.Log("Melee lerping");
		//	transform.SetPositionAndRotation(Vector3.Lerp(transform.position, weaponSpot.transform.position, 5f * Time.deltaTime), Quaternion.Lerp(transform.rotation, weaponSpot.transform.rotation, 5f * Time.deltaTime));
		//}
	}

	public override void EquipWeapon()
	{
		base.EquipWeapon();
		// TODO: Update animations to scale with attackrate changes
		//animator.SetFloat("StabSpeedMultiplier", attackAnimations[0].length / attackRate);
	}

	public override void UnequipWeapon()
	{
		base.UnequipWeapon();
	}

	private void OnTriggerEnter(Collider other)
	{
		/*
		// We can hit enemies only when in animation
		if (!IsAnimationPlaying(attackAnimations[0].name) && !IsAnimationPlaying(attackAnimations[1].name) && !IsAnimationPlaying(attackAnimations[2].name)) return;

		// Check if hit enemy and get it's script
		enemyScript = other.GetComponentInParent<Enemy>();
		if (enemyScript == null)
		{
			// audioSource.PlayOneShot(hitFloorSound);
			return;
		}

		// Make sure we don't call multiple hits on same enemy
		if (!attackedEnemies.Contains(enemyScript))
		{
			// Primary attack
			if (IsAnimationPlaying(attackAnimations[0].name))
			{
				enemyScript.GetHit(other, damage, headshotMultiplier);
				audioSource.PlayOneShot(stabSounds[0]);
			}
			else // Slash attack
			{
				enemyScript.GetHit(other, damageSecondary, headshotMultiplier);
				audioSource.PlayOneShot(stabSounds[1]);
			}

			// Play VFX
			Vector3 hitPosition = other.transform.position;
			Vector3 hitNormal = (hitPosition - other.transform.position).normalized;
			enemyScript.EnemyImpactFX(hitPosition, hitNormal);

			// Add enemy to list that keeps track on damaged enemies to avoid multiple hit calls
			if (!attackedEnemies.Contains(enemyScript))
				attackedEnemies.Add(enemyScript);
		}
		*/
	}

	private bool IsAnimationPlaying(string animationName)
	{
		// Get the hash of the animation state using its name
		int animationHash = Animator.StringToHash(animationName);

		// Check if the Animator is currently playing the animation state
		return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == animationHash;
	}

	private void HandleAttacking()
	{
		if (Time.time >= attackCooldown)
		{
			readyToAttack = true;
			//WeaponSwayAndBob.instance.disableSwayBob = false;
		}
		else
		{
			readyToAttack = false;
			//WeaponSwayAndBob.instance.disableSwayBob = true;
		}

		// Check if we can start a new attack
		if (Input.GetButton("Fire1") && readyToAttack)
		{
			// Start the attack
			StartAttack();
		}

		// Handle ongoing attack timing
		if (isAttacking)
		{
			// Check if it's time to perform the CapsuleCast (windup is complete)
			if (Time.time >= currentWindupTime)
			{
				Attack();
				isAttacking = false;  // Attack is done
			}
		}
	}

	private void StartAttack()
	{
		// Reset the cooldown timer to delay the next attack
		attackCooldown = Time.time + attackRate;

		// Calculate the windup timing based on the attack rate and windup percentage
		float windupDuration = attackRate * windupPercentage;
		currentWindupTime = Time.time + windupDuration;

		// Trigger the attack animation (this assumes the animation duration matches the attack rate)
		animator.Play(attackAnimations[0].name);

		// Mark that the player is currently in an attack state
		isAttacking = true;
	}

	private void Attack()
	{
		Vector3 point1 = cameraTransform.position;
		Vector3 point2 = cameraTransform.position + cameraTransform.forward * attackRange;

		RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, attackRadius, cameraTransform.forward, attackRange, enemyLayers);

		if (hits.Length > 0)
		{
			List<Enemy> hitEnemies = new List<Enemy>();
			int enemiesHit = 0;

			// Group hits by enemy
			Dictionary<Enemy, List<RaycastHit>> enemyHits = new Dictionary<Enemy, List<RaycastHit>>();

			foreach (RaycastHit hit in hits)
			{
				Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
				if (enemy == null) continue;

				if (!enemyHits.ContainsKey(enemy))
				{
					enemyHits[enemy] = new List<RaycastHit>();
				}
				enemyHits[enemy].Add(hit);
			}

			foreach (var kvp in enemyHits)
			{
				Enemy enemy = kvp.Key;
				List<RaycastHit> enemyCollisions = kvp.Value;

				// Find the hit closest to the center of the camera using dot product
				RaycastHit bestHit = enemyCollisions[0];
				float bestDot = -1f;

				foreach (RaycastHit h in enemyCollisions)
				{
					Vector3 toHit = (h.point - cameraTransform.position).normalized;
					float dot = Vector3.Dot(cameraTransform.forward, toHit);
					if (dot > bestDot)
					{
						bestDot = dot;
						bestHit = h;
					}
					else if (Mathf.Approximately(dot, bestDot))
					{
						// If dot is the same, prefer the closer hit
						if (h.distance < bestHit.distance)
						{
							bestHit = h;
						}
					}
				}

				// Now we have the best hit for this enemy
				if (!hitEnemies.Contains(enemy))
				{
					// Check if the chosen hit is a head hit
					float finalDamage = damage;
					if (bestHit.collider.CompareTag("Head"))
					{
						finalDamage = Mathf.RoundToInt(damage * headshotMultiplier);
					}

					enemy.GetShot(bestHit, (int)finalDamage, 0);
					hitEnemies.Add(enemy);
					enemiesHit++;

					if (enemiesHit >= penetrationAmount)
						break;
				}
			}

			// Play appropriate sound
			if (enemiesHit > 0)
			{
				audioSource.PlayOneShot(stabSounds[0]);
			}
			else
			{
				int randomSwingClip = Random.Range(0, swingSounds.Length);
				audioSource.PlayOneShot(swingSounds[randomSwingClip]);
			}
		}
		else
		{
			int randomSwingClip = Random.Range(0, swingSounds.Length);
			audioSource.PlayOneShot(swingSounds[randomSwingClip]);
		}
	}

}
