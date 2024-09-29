using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeleeWeapon : Weapon
{
	[Header("Melee Weapon Settings")]
	public AnimationClip[] attackAnimations;

	[SerializeField] private float attackDuration;
	[SerializeField] private float secondaryAttackDuration;
	[SerializeField] private int damage, damageSecondary;
	[SerializeField] private float headshotMultiplier;
	[SerializeField] private ParticleSystem bloodFX;
	private bool attacking = false;
	private bool attackingSecondary = false;
	private bool canAttack = true;
	private bool mirroredNext = false; // Alternate with normal and mirrored slash
	private string magString = "Melee";
	private string totalAmmoString = "Unlimited ammo";
	private Animator animator;
	private Enemy enemyScript;
	private TextMeshProUGUI magazineText, totalAmmoText;
	private List<Enemy> attackedEnemies = new List<Enemy>();

	[Header("Audio")]
	public AudioClip[] stabSounds;
	public AudioClip[] swingSounds;
	public AudioClip hitFloorSound;

	[Header("Newer system for melee")]
	public Transform cameraTransform;  // The player's camera (or the player model)
	public int penetrationAmount = 3; // How many enemies each attack can damage
	public float attackRange = 1.0f;    // The forward distance of the attack
	public float attackRadius = 0.5f;   // The width of the capsule
	public LayerMask enemyLayers;

	protected override void Awake()
	{
		base.Awake();
		magazineText = GameObject.Find("MagazineNumbers").GetComponent<TextMeshProUGUI>();
		totalAmmoText = GameObject.Find("TotalAmmo").GetComponent<TextMeshProUGUI>();
		animator = GetComponent<Animator>();

		cameraTransform = Camera.main.transform;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		magazineText.text = magString;
		totalAmmoText.text = totalAmmoString;
		EquipWeapon();
	}

	protected override void Update()
	{
		base.Update();
		HandleInputs();

		//if (canAttack && attacking && equipped)
		//{
		//	int randomSwingClip = Random.Range(0, swingSounds.Length);
		//	audioSource.PlayOneShot(swingSounds[randomSwingClip]);
		//	StartCoroutine(Attack(false));
		//}
		//
		//if (canAttack && attackingSecondary && equipped)
		//{
		//	int randomSwingClip = Random.Range(0, swingSounds.Length);
		//	audioSource.PlayOneShot(swingSounds[randomSwingClip]);
		//	StartCoroutine(Attack(true));
		//}

		if (equipped && !unequipping && !attacking && !attackingSecondary && canAttack)
		{
			transform.SetPositionAndRotation(Vector3.Lerp(transform.position, weaponSpot.transform.position, 5f * Time.deltaTime), Quaternion.Lerp(transform.rotation, weaponSpot.transform.rotation, 5f * Time.deltaTime));
		}
	}

	public void HandleInputs()
	{
		// Prevent attacking while doing other actions
		if (GrenadeThrow.instance.selectingGrenade || ObjectPlacing.instance.isPlacing || ObjectPlacing.instance.isChoosingObject) return;

		if (Input.GetButtonDown("Fire1") && Time.timeScale > 0 && canAttack)
		{
			Attack();
			canAttack = false;
		}

		if (Input.GetButtonUp("Fire1"))
		{
			canAttack = true;
		}

		//	if (Input.GetButtonDown("Fire1") && Time.timeScale > 0 && canAttack)
		//	{
		//		attacking = true;
		//	}
		//	else if (Input.GetButtonUp("Fire1"))
		//	{
		//		attacking = false;
		//	}
		//
		//	if (Input.GetButtonDown("Fire2") && Time.timeScale > 0 && canAttack)
		//	{
		//		attackingSecondary = true;
		//	}
		//	else if (Input.GetButtonUp("Fire2"))
		//	{
		//		attackingSecondary = false;
		//	}
	}

	public override void EquipWeapon()
	{
		base.EquipWeapon();
		animator.SetFloat("StabSpeedMultiplier", attackAnimations[0].length / attackDuration);
		animator.SetFloat("SlashSpeedMultiplier", attackAnimations[1].length / secondaryAttackDuration);
	}

	public override void UnequipWeapon()
	{
		base.UnequipWeapon();
	}

	/*IEnumerator Attack(bool secondaryAttack)
	{
		if (!secondaryAttack)
		{
			animator.Play(attackAnimations[0].name);
			canAttack = false;
			yield return new WaitForSeconds(attackDuration);
			attackedEnemies.Clear();
			canAttack = true;
		}
		else
		{
			if (!mirroredNext)
			{
				animator.Play(attackAnimations[1].name);
				mirroredNext = true;
			}
			else
			{
				animator.Play(attackAnimations[2].name);
				mirroredNext = false;
			}

			canAttack = false;
			yield return new WaitForSeconds(secondaryAttackDuration);
			attackedEnemies.Clear();
			canAttack = true;
		}
	} */

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

	private void Attack()
	{
		// 29.9.2024 New way of attacking 
		Vector3 point1 = cameraTransform.position;  // Start point of the capsule cast
		Vector3 point2 = cameraTransform.position + cameraTransform.forward * attackRange;  // End point of the attack range

		RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, attackRadius, cameraTransform.forward, attackRange, enemyLayers);
		animator.Play(attackAnimations[0].name);

		if (hits.Length > 0)
		{
			List<Enemy> hitEnemies = new List<Enemy>();  // To track which enemies have been hit
			int enemiesHit = 0;

			foreach (RaycastHit hit in hits)
			{
				Debug.Log("Foreach triggered");
				Collider hitCollider = hit.collider;
				Enemy enemy = hitCollider.GetComponentInParent<Enemy>();

				// Ensure the object hit is an enemy and we haven't hit them already
				if (enemy != null && !hitEnemies.Contains(enemy))
				{
					// Call GetHit only if we haven't hit this enemy yet
					Debug.Log("Applying melee hit");
					enemy.GetHit(hitCollider, damage, headshotMultiplier);
					audioSource.PlayOneShot(stabSounds[0]);

					hitEnemies.Add(enemy);  // Mark this enemy as hit
					enemiesHit++;  // Increment the counter for enemies hit

					// If we've hit the maximum number of enemies, stop
					if (enemiesHit >= penetrationAmount) break;
				}
			}
		}
	}

}
