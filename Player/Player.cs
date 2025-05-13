using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	public static Player instance;

	[Header("Player Settings")]
	[HideInInspector] public float currentHealth;
	public int maxHealth;
	public float healPerSecond;
	public float regenationDelayAfterDamage;
	public bool regenerating;

	// Time when we last took damage
	private float lastDamageTime;

	[Header("Sisu")]
	public int maxSisu;               // Maximum Sisu (used as an integer for costs/display)
	public float currentSisu;         // Now a float for smooth regeneration
	public float sisuRegenRate;       // Sisu regained per second (for continuous, smooth regeneration)
	public Image sisuSlider;

	// UI related stuff for Sisu text
	private float sisuUITextUpdateTimer = 0f;
	private float sisuUITextUpdateInterval = 0.1f; // Update text every 0.1 seconds

	public TextMeshProUGUI sisuText, sisuTextRaw;

	[Header("Visuals")]
	public float maxBloom;
	public float maxVignette, maxChromaticAberration, maxGrain;

	[Header("Other Stuff")]
	public Camera mainCamera;
	public Camera weaponCamera;
	public Gradient healthGradient;
	public GameObject runningSymbol, fallingSymbol, regenSymbol, kickSymbol;
	public TextMeshProUGUI healthText, healthTextRaw;
	public Image healthSlider;
	public DamagePP damagePPScript;
	public Animator animator;

	private string healthString, healthStringRaw;
	// This is used only for UI display purposes (as a percentage)
	private float currentHPPercentage = 100f;
	private PlayerMovement playerMovement;

	[Header("Audio")]
	public AudioSource playerAS;
	public AudioClip[] damageGrunts, kickSounds;
	public AudioClip regenSound;

	[Header("Kick values")]
	public Transform kickTransform;
	public int kickDamage, maxKickTargets;
	public float kickPlayerSlow, kickPlayerSlowDuration;
	public float kickRadius, kickForce, kickCooldown, kickHeight, kickDistance;
	public Vector3 upVector;

	private float kickTimeStamp; // Handles cooldown for kicking

	private void Awake()
	{
		instance = this; // Ensure singleton instance exists ASAP
	}

	private void Start()
	{
		currentHealth = maxHealth;
		currentSisu = maxSisu;  // Start full
		UpdateHealthUI();
		UpdateSisuUI(true);

		if (runningSymbol != null)
			runningSymbol.SetActive(false);

		kickTimeStamp = Time.time + kickCooldown;
		playerMovement = GetComponent<PlayerMovement>();

		// (No coroutine is used for Sisu anymore.)
	}

	void Update()
	{
		HandleInputs();
		HandleHealing();

		// --- Continuous Sisu regeneration (smooth update) ---
		if (currentSisu < maxSisu)
		{
			currentSisu = Mathf.Min(currentSisu + sisuRegenRate * Time.deltaTime, maxSisu);
			UpdateSisuUI();
		}

		// Update Sisu text at the desired interval
		sisuUITextUpdateTimer += Time.deltaTime;
		if (sisuUITextUpdateTimer >= sisuUITextUpdateInterval)
		{
			sisuUITextUpdateTimer = 0f; // Reset timer
			UpdateSisuUI(true);
		}
	}

	private void HandleInputs()
	{
		if (Input.GetKeyDown(KeybindManager.Instance.kickKey) && kickTimeStamp <= Time.time)
		{
			kickTimeStamp = Time.time + kickCooldown;
			playerMovement.ApplySpeedEffect(kickPlayerSlow, kickPlayerSlowDuration);
			Invoke("Kick", 0.15f);
		}
		if (kickTimeStamp <= Time.time)
			kickSymbol.SetActive(true);
	}

	private void OnGUI()
	{
		// Example: GUI.Label(new Rect(500, 500, 80, 20), targetFov.ToString());
	}

	// This visualizes the kick area in the editor
#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		if (kickTransform == null) return;

		// 1) Compute the two end-points along your forward axis
		Vector3 p1 = kickTransform.position;
		Vector3 p2 = kickTransform.position + transform.forward * kickDistance;
		float r = kickRadius;

		// 2) Draw the end-caps
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(p1, r);
		Gizmos.DrawWireSphere(p2, r);

		// 3) Draw the cylinder sides by connecting four lines around the capsule
		Vector3 axis = (p2 - p1).normalized;

		// pick any vector not parallel to axis
		Vector3 ortho = Vector3.up;
		if (Mathf.Abs(Vector3.Dot(axis, ortho)) > 0.99f)
			ortho = Vector3.right;

		Vector3 perp1 = Vector3.Cross(axis, ortho).normalized * r;
		Vector3 perp2 = Vector3.Cross(axis, perp1).normalized * r;

		Gizmos.DrawLine(p1 + perp1, p2 + perp1);
		Gizmos.DrawLine(p1 - perp1, p2 - perp1);
		Gizmos.DrawLine(p1 + perp2, p2 + perp2);
		Gizmos.DrawLine(p1 - perp2, p2 - perp2);
	}
#endif

	public void Kick()
	{
		// FX and SFX
		animator.Play("Kick");
		int sfx = Random.Range(0, kickSounds.Length);
		playerAS.PlayOneShot(kickSounds[sfx]);
		kickSymbol.SetActive(false);
		Recoil.Instance.KickFlinch();

		// compute the two ends of our cylinder (as a capsule) in front of the player
		Vector3 origin = kickTransform.position;
		Vector3 forward = transform.forward;
		Vector3 p1 = origin;
		Vector3 p2 = origin + forward * kickDistance;
		float radius = kickRadius;
		Vector3 center = (p1 + p2) * 0.5f;

		// collect each unique, valid Enemy in that volume
		var enemies = new List<Enemy>();
		var cols = Physics.OverlapCapsule(p1, p2, radius);
		foreach (var col in cols)
		{
			var enemy = col.GetComponentInParent<Enemy>();
			if (enemy != null && !enemy.isDead && !enemies.Contains(enemy))
				enemies.Add(enemy);
		}

		// sort by distance from the player (p1) so we hit closest first
		enemies.Sort((a, b) =>
			Vector3.Distance(a.bodyRB.position, p1)
			  .CompareTo(Vector3.Distance(b.bodyRB.position, p1))
		);

		// apply to up to maxKickTargets enemies
		int hits = 0;
		foreach (var enemy in enemies)
		{
			if (hits >= maxKickTargets)
				break;

			// apply physics impulse only on the main body rigidbody
			if (enemy.bodyRB != null)
			{
				Vector3 explosionPos = center - upVector;
				enemy.bodyRB.AddExplosionForce(kickForce, explosionPos, radius);
			}

			enemy.ApplyStagger();
			enemy.TakeDamage(kickDamage);
			hits++;
		}
	}


	public void TakeDamage(int amount, float flinchMultiplier = 1f)
	{
		// Apply damage and ensure we don't drop below 0
		currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
		// Reset the healing delay timer
		lastDamageTime = Time.time;

		// Update UI and effects
		float currentHPPercentage = (currentHealth / maxHealth) * 100f;
		float currentHPPercentagePP = 1f - (currentHPPercentage / 100f);
		damagePPScript.UpdateDamagePP(maxBloom * currentHPPercentagePP,
									  maxVignette * currentHPPercentagePP,
									  maxChromaticAberration * currentHPPercentagePP,
									  maxGrain * currentHPPercentagePP);

		UpdateHealthUI();

		if (currentHealth > 0)
		{
			int rindex = Random.Range(0, damageGrunts.Length);
			playerAS.PlayOneShot(damageGrunts[rindex]);
		}

		regenSymbol.SetActive(false);
		Recoil.Instance.DamageFlinch(flinchMultiplier);
		PostProcessingController.TriggerDamageFlash();
	}

	// This method applies instant healing (e.g. from a health pickup)
	public void Heal(int amount)
	{
		currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
		UpdateHealthUI();
	}

	// Smooth healing over time (applied each frame) after a delay since the last damage
	private void HandleHealing()
	{
		bool canHeal = Time.time >= lastDamageTime + regenationDelayAfterDamage && currentHealth < maxHealth;

		if (canHeal)
		{
			if (!regenerating)
			{
				regenerating = true;
				regenSymbol.SetActive(true);
			}

			currentHealth += healPerSecond * Time.deltaTime;
			if (currentHealth >= maxHealth)
			{
				currentHealth = maxHealth;
				regenerating = false;
				regenSymbol.SetActive(false);
			}
			UpdateHealthUI();
		}
		else
		{
			if (regenerating)
			{
				regenerating = false;
				regenSymbol.SetActive(false);
			}
		}
	}

	public void UpdateHealthUI()
	{
		currentHPPercentage = (currentHealth / maxHealth) * 100f;
		//float clampedValue = Mathf.Clamp(Mathf.Round(currentHPPercentage), 0f, 100f);
		//healthString = clampedValue.ToString() + "%";
		//healthText.text = healthString;

		healthStringRaw = Mathf.FloorToInt(currentHealth).ToString() + " / " + maxHealth.ToString();
		healthTextRaw.text = healthStringRaw;

		healthSlider.fillAmount = currentHPPercentage / 100f;
		healthSlider.color = healthGradient.Evaluate(1f - currentHPPercentage / 100f);
	}

	#region Sisu
	/// <summary>
	/// Adjusts the current Sisu by a given integer amount.
	/// (When spending or adding Sisu, you work with int values.)
	/// </summary>
	public void UpdateSisu(int amount)
	{
		currentSisu = Mathf.Clamp(currentSisu + amount, 0, maxSisu);
		UpdateSisuUI();
	}

	/// <summary>
	/// Updates the Sisu slider (and optionally text). The slider is updated smoothly
	/// using the continuous float currentSisu value, but the displayed text shows an int.
	/// </summary>
	public void UpdateSisuUI(bool updateText = false)
	{
		if (sisuSlider)
		{
			sisuSlider.fillAmount = currentSisu / maxSisu;
		}

		if (sisuTextRaw && updateText)
		{
			sisuTextRaw.text = Mathf.FloorToInt(currentSisu) + " / " + maxSisu;
		}
	}
	#endregion
}
