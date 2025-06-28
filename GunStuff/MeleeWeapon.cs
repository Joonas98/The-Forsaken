using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
	[Header("Melee Weapon Settings")]
	public AnimationClip[] attackAnimations;    // [0]=primary, [1]=secondary

	[SerializeField] private int damage, damageSecondary;
	[SerializeField] private float headshotMultiplier;
	[SerializeField] private ParticleSystem bloodFX;
	private Animator animator;

	[Header("Audio")]
	public AudioClip[] stabSounds;
	public AudioClip[] swingSounds;
	public AudioClip hitFloorSound;

	[Header("Primary Attack")]
	public float attackRange = 1.0f;    // forward distance
	public float attackRadius = 0.5f;   // capsule radius
	public float attackRate = 1.0f;     // seconds between swings
	[Range(0, 1)] public float windupPercentage = 0.3f;

	[Header("Secondary Attack")]
	public float secondaryAttackRange = 1.5f;
	public float secondaryAttackRadius = 0.5f;
	public float secondaryAttackRate = 1.5f;
	[Range(0, 1)] public float secondaryWindupPercentage = 0.2f;

	[Header("Shared")]
	public int penetrationAmount = 3;
	public LayerMask enemyLayers;

	private float attackCooldown;
	private bool isAttacking;
	public bool readyToAttack;
	private float currentWindupTime;
	private bool currentSecondary;

	private Transform cameraTransform;

	protected override void Awake()
	{
		base.Awake();
		animator = GetComponent<Animator>();
		cameraTransform = Camera.main.transform;
	}

	protected void OnEnable()
	{
		AmmoHUD.Instance.DisableHUD();
		EquipWeapon();
	}

	protected void Update()
	{
		HandleAttacking();
	}

	private void HandleAttacking()
	{
		readyToAttack = Time.time >= attackCooldown;

		// primary
		if (Input.GetButton("Fire1") && readyToAttack && !isAttacking)
			StartAttack(false);
		// secondary
		else if (Input.GetButton("Fire2") && readyToAttack && !isAttacking)
			StartAttack(true);

		if (isAttacking && Time.time >= currentWindupTime)
		{
			Attack();
			isAttacking = false;
		}
	}

	private void StartAttack(bool secondary)
	{
		currentSecondary = secondary;

		float rate = secondary ? secondaryAttackRate : attackRate;
		float windupPct = secondary ? secondaryWindupPercentage : windupPercentage;

		attackCooldown = Time.time + rate;
		currentWindupTime = Time.time + rate * windupPct;
		isAttacking = true;
		readyToAttack = false;

		var clip = attackAnimations[secondary ? 1 : 0];
		animator.Play(clip.name);
	}

	private void Attack()
	{
		float range = currentSecondary ? secondaryAttackRange : attackRange;
		float radius = currentSecondary ? secondaryAttackRadius : attackRadius;

		Vector3 p1 = cameraTransform.position;
		Vector3 p2 = p1 + cameraTransform.forward * range;

		var hits = Physics.CapsuleCastAll(p1, p2, radius, cameraTransform.forward, range, enemyLayers);
		if (hits.Length == 0)
		{
			// miss sound only
			audioSource.PlayOneShot(swingSounds[Random.Range(0, swingSounds.Length)]);
			return;
		}

		var enemyHits = new Dictionary<Enemy, List<RaycastHit>>();
		foreach (var h in hits)
		{
			if (h.collider.GetComponentInParent<Enemy>() is Enemy e)
			{
				if (!enemyHits.ContainsKey(e)) enemyHits[e] = new List<RaycastHit>();
				enemyHits[e].Add(h);
			}
		}

		int hitsDone = 0;
		foreach (var kvp in enemyHits)
		{
			// find best hit by dot
			RaycastHit best = kvp.Value[0];
			float bestDot = Vector3.Dot((best.point - p1).normalized, cameraTransform.forward);
			foreach (var h in kvp.Value)
			{
				float dot = Vector3.Dot((h.point - p1).normalized, cameraTransform.forward);
				if (dot > bestDot)
				{
					bestDot = dot;
					best = h;
				}
			}

			// apply damage
			int dmg = currentSecondary ? damageSecondary : damage;
			if (best.collider.CompareTag("Head"))
				dmg = Mathf.RoundToInt(dmg * headshotMultiplier);

			kvp.Key.GetShot(best, dmg, 0);
			hitsDone++;

			// spawn blood effect at impact
			Vector3 hitPoint = best.point;
			Quaternion rot = Quaternion.LookRotation(best.normal);
			ParticlePool.Instance.Spawn(bloodFX, hitPoint, rot);

			if (hitsDone >= penetrationAmount)
				break;
		}

		// play stab vs swing
		if (hitsDone > 0)
			audioSource.PlayOneShot(stabSounds[0]);
		else
			audioSource.PlayOneShot(swingSounds[Random.Range(0, swingSounds.Length)]);
	}
}
