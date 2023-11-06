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

	protected override void Awake()
	{
		base.Awake();
		magazineText = GameObject.Find("MagazineNumbers").GetComponent<TextMeshProUGUI>();
		totalAmmoText = GameObject.Find("TotalAmmo").GetComponent<TextMeshProUGUI>();
		animator = GetComponent<Animator>();

		GameObject CrosshairCanvas = GameObject.Find("CrossHairCanvas");
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

		if (canAttack && attacking && equipped)
		{
			int randomSwingClip = Random.Range(0, swingSounds.Length);
			audioSource.PlayOneShot(swingSounds[randomSwingClip]);
			StartCoroutine(Attack(false));
		}

		if (canAttack && attackingSecondary && equipped)
		{
			int randomSwingClip = Random.Range(0, swingSounds.Length);
			audioSource.PlayOneShot(swingSounds[randomSwingClip]);
			StartCoroutine(Attack(true));
		}

		if (equipped && !unequipping && !attacking && !attackingSecondary && canAttack)
		{
			transform.SetPositionAndRotation(Vector3.Lerp(transform.position, weaponSpot.transform.position, 5f * Time.deltaTime), Quaternion.Lerp(transform.rotation, weaponSpot.transform.rotation, 5f * Time.deltaTime));
		}
	}

	public void HandleInputs()
	{
		if (GrenadeThrow.instance.selectingGrenade || ObjectPlacing.instance.isPlacing || ObjectPlacing.instance.isChoosingObject) return;

		if (Input.GetButtonDown("Fire1") && Time.timeScale > 0 && canAttack)
		{
			attacking = true;
		}
		else if (Input.GetButtonUp("Fire1"))
		{
			attacking = false;
		}

		if (Input.GetButtonDown("Fire2") && Time.timeScale > 0 && canAttack)
		{
			attackingSecondary = true;
		}
		else if (Input.GetButtonUp("Fire2"))
		{
			attackingSecondary = false;
		}
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

	IEnumerator Attack(bool secondaryAttack)
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
	}

	private void OnTriggerEnter(Collider other)
	{
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
	}

	private bool IsAnimationPlaying(string animationName)
	{
		// Get the hash of the animation state using its name
		int animationHash = Animator.StringToHash(animationName);

		// Check if the Animator is currently playing the animation state
		return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == animationHash;
	}
}
