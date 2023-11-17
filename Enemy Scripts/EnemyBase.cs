using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DamageNumbersPro;

public class EnemyBase : MonoBehaviour
{
	[Header("Basics")]
	public int moneyReward;
	public enum EnemyType
	{
		Zombie, Minotaur, Hound
	}

	[Header("Health")]
	public int maxHealth;
	public int currentHealth;
	public int healthPercentage;
	public bool isDead = false;

	private int[] limbHealths = new int[9];
	private float limbMinHealthPercentage = 0.2f;
	private float limbMaxHealthPercentage = 0.5f;

	[Header("Movement")]
	public float movementSpeed;
	public bool ragdolling = false;

	private float ogMovementSpeed;
	private FIMSpace.FProceduralAnimation.RagdollAnimator activeRagdoll;

	[Header("References")]
	public LimbManager limbManager;
	public List<Collider> ragdollParts = new List<Collider>();
	public Rigidbody[] rigidbodies;

	[Header("Damage Popup")]
	public Transform popupTransform;
	public DamageNumber[] dpopPrefabs;
	public enum DamageType // Dpops for different purposes
	{
		Normal, Headshot, Healing, Fire, Shock, Crimson
	}

	[Header("Audio")]
	public AudioClip[] takeDamageSounds;
	public AudioClip[] attackSounds;

	protected AudioSource audioSource;

	// Other protected stuff
	protected Animator animator;
	protected GameObject player;
	protected float distanceToPlayer;
	protected NavMeshAgent navAgent;
	protected EnemyNav navScript;
	protected RagdollManager ragdollManager;

	// System to handle slows
	private struct SlowEffect
	{
		public float slowPercentage;
		public float duration;
	}
	private SlowEffect[] slowEffects;

	#region Update Functions
	virtual protected void OnValidate()
	{
		if (animator == null) animator = GetComponent<Animator>();
		if (audioSource == null) audioSource = GetComponent<AudioSource>();
		if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
		if (activeRagdoll == null) activeRagdoll = GetComponent<FIMSpace.FProceduralAnimation.RagdollAnimator>();
	}

	virtual protected void Awake()
	{
		// Get references
		ragdollManager = GetComponent<RagdollManager>();
		navScript = GetComponent<EnemyNav>();
		player = GameObject.Find("Player");

		// Set basic variables
		currentHealth = maxHealth;
		ogMovementSpeed = movementSpeed;
	}

	virtual protected void Start()
	{
		slowEffects = new SlowEffect[5]; // Maximum of 5 slow effects should be enough 
	}

	virtual protected void Update()
	{
		distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
	}
	#endregion

	// Health related functions
	public void Heal(int amount)
	{
		//HandlePopup(amount, DamageType.Healing);
		currentHealth += amount;
		if (currentHealth > maxHealth) currentHealth = maxHealth;
		healthPercentage = Mathf.Clamp((100 * currentHealth) / maxHealth, 0, 100);
		//UpdateEyeColor(); // Enemy health can be seen from eyes
	}

	// The actual damage processing, should be always called via TakeDamage() functions
	public void TakeDamage(int damage, int percentageAmount = 0, DamageType type = DamageType.Normal)
	{
		//	if (debuffManager.IsDebuffActive(DebuffManager.Debuffs.Crimson) && type != DamageType.Crimson)
		//	{
		//		// Debug.Log("Dealing crimson damage");
		//		crimsonDamageCurrent += crimsonDamage;
		//		TakeDamage(crimsonDamageCurrent, 0, DamageType.Crimson);
		//	}

		// If optional percentageAmount was given, add % based damage to the actual damage
		if (percentageAmount > 0)
		{
			var hpPercentage = (100 / maxHealth) * percentageAmount;
			damage += hpPercentage;
		}

		// Handle popup according to the dmg amount and type
		//	HandlePopup(damage, type);

		// Adjust health and health %
		currentHealth -= damage;
		healthPercentage = Mathf.Clamp((100 * currentHealth) / maxHealth, 0, 100);

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

	public void Die()
	{
		if (isDead) return;
		isDead = true;

		Debug.Log("Died lol");

		ragdollManager.TurnOnRagdoll();
		Destroy(gameObject, 60f);

		navAgent.speed = 0;
		navScript.enabled = false;

		//bodyRB.velocity = new Vector3(0, 0, 0);
		//foreach (Rigidbody rb in rigidbodies) // Otherwise gameobjects keep moving forever
		//{
		//	rb.velocity = new Vector3(0, 0, 0);
		//}

		GameManager.GM.enemyCount--;
		GameManager.GM.UpdateEnemyCount();
		GameManager.GM.AdjustMoney(moneyReward);
		GameManager.GM.ConfirmKillFX();
		GameManager.GM.enemiesAlive.Remove(this);
		GameManager.GM.enemiesAliveGos.Remove(gameObject);
	}

}
