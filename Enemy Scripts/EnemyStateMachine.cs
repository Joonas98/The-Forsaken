using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
	Spawn,
	Chase,
	Attack,
	Knockback,
	Electrocuted,
	Ragdoll,
	Standup,
	Dead
}

public class EnemyStateMachine : MonoBehaviour
{
	public EnemyState currentState;
	public Enemy enemyBase;
	public NavMeshAgent navAgent;
	public EnemyNav enemyNavScript;

	// Needed for electrocution handling
	[HideInInspector] public float electroStartTime;
	[HideInInspector] public float electroDuration;
	private bool electroInit = false;

	private Animator animator;
	private bool spawnAnimationStarted = false;

	void Awake()
	{
		animator = GetComponent<Animator>();
		enemyBase = GetComponent<Enemy>();
		if (navAgent == null)
			navAgent = GetComponent<NavMeshAgent>();
	}

	void Start()
	{
		ChangeState(EnemyState.Spawn);
	}

	void Update()
	{
		// Update the Velocity parameter if you use it in your Animator.
		if (animator != null && navAgent != null)
			animator.SetFloat("Velocity", navAgent.velocity.magnitude / (navAgent.speed > 0 ? navAgent.speed : 1));

		// Execute state-specific logic.
		switch (currentState)
		{
			case EnemyState.Spawn:
				HandleSpawn();
				break;
			case EnemyState.Chase:
				HandleChase();
				break;
			case EnemyState.Attack:
				HandleAttack();
				break;
			case EnemyState.Knockback:
				HandleKnockback();
				break;
			case EnemyState.Electrocuted:
				HandleElectrocuted();
				break;
			case EnemyState.Ragdoll:
				HandleRagdoll();
				break;
			case EnemyState.Standup:
				HandleStandup();
				break;
			case EnemyState.Dead:
				HandleDead();
				break;
		}
	}

	public void ChangeState(EnemyState newState)
	{
		//Debug.Log($"ChangeState: {currentState} → {newState}");

		// If we are leaving Knockback, restore the upper body layer weight.
		if (currentState == EnemyState.Knockback && newState != EnemyState.Knockback)
		{
			animator.SetLayerWeight(attackLayerIndex, 1f);
			knockbackInitiated = false;
		}

		// Entering Electrocuted → freeze everything
		if (newState == EnemyState.Electrocuted)
		{
			if (navAgent != null) enemyNavScript.StopNavigation();
			enemyBase.isAttacking = false;
			animator.speed = 0f;        // halt the Animator on the current frame
			electroInit = false;        // reset our one-time flag
		}

		// Exiting Electrocuted → unfreeze (but only restart nav if we’re not ragdolled)
		else if (currentState == EnemyState.Electrocuted && newState != EnemyState.Electrocuted)
		{
			animator.speed = 1f;
			if (!enemyBase.ragdolling && navAgent != null)
				enemyNavScript.ResumeNavigation();
		}

		currentState = newState;

		if (newState == EnemyState.Spawn)
			spawnAnimationStarted = false;
		if (newState == EnemyState.Chase && !navAgent.isActiveAndEnabled)
			navAgent.enabled = true;
	}

	void HandleSpawn()
	{
		// Stop movement during spawn.
		enemyNavScript.StopNavigation();
		navAgent.ResetPath();

		if (!spawnAnimationStarted)
		{
			animator.Play("Spawn");
			spawnAnimationStarted = true;
		}

		// Once the spawn animation finishes, re-enable movement and transition.
		if (AnimationFinished("Spawn"))
		{
			enemyNavScript.ResumeNavigation();
			ChangeState(EnemyState.Chase);
		}
	}

	void HandleChase()
	{
		// Play the appropriate chase animation.
		animator.Play(enemyBase.isCrawling ? "Base Blend Tree Crawl" : "Blend Tree Flailing Arms");

		// Transition to Attack if the base script sets isAttacking.
		if (enemyBase.isAttacking)
		{
			ChangeState(EnemyState.Attack);
		}
	}

	void HandleAttack()
	{
		animator.Play(enemyBase.isCrawling ? "Attack_Crawl" : "Attack");
		if (AttackFinished())
		{
			ChangeState(EnemyState.Chase);
		}
	}

	[SerializeField] private int attackLayerIndex = 1;
	private bool knockbackInitiated = false;

	void HandleKnockback()
	{
		// Ensure the enemy does not move during knockback.

		if (navAgent != null)
			enemyNavScript.StopNavigation();

		// Cancel any ongoing attack.
		enemyBase.isAttacking = false;

		// Disable the attack layer by setting its weight to zero.
		animator.SetLayerWeight(attackLayerIndex, 0f);

		// Trigger the knockback animation only once.
		if (!knockbackInitiated)
		{
			string animName = enemyBase.isCrawling ? "Knockback_Crawl" : "Knockback";
			animator.Play(animName, 0, 0f);
			knockbackInitiated = true;
		}

		// Check if the knockback animation has finished.
		if (KnockbackFinished())
		{
			if (navAgent != null)
				enemyNavScript.ResumeNavigation();
			// Restore the upper body attack layer weight so that future attacks work.
			animator.SetLayerWeight(attackLayerIndex, 1f);
			ChangeState(EnemyState.Chase);
			knockbackInitiated = false;
		}
	}

	void HandleElectrocuted()
	{
		// (no dedicated clip → we just stay frozen)
		if (!electroInit)
		{
			// optionally play a zap SFX or VFX once here
			electroInit = true;
		}

		// once our timer runs out, pick the correct next state:
		if (Time.time - electroStartTime >= electroDuration)
		{
			if (enemyBase.isDead)
				ChangeState(EnemyState.Dead);
			else if (enemyBase.ragdolling)
				ChangeState(EnemyState.Ragdoll);
			else
				ChangeState(EnemyState.Chase);
		}
	}

	void HandleRagdoll()
	{
		// Set to standup animation to have the correct pose when re-enabling animator
		if (!animator.enabled) animator.Play("Stand up");

		if (animator.enabled)
			animator.enabled = false;

		//navAgent.isStopped = true;
		// No further action here—your Enemy.cs ragdoll detection (via TurnOffRagdoll)
		// will call ChangeState(EnemyState.Standup) once the ragdoll is settled.
	}

	private bool standupInitiated = false;

	void HandleStandup()
	{
		// If the enemy is crawling, skip standup.
		if (enemyBase.isCrawling)
		{
			standupInitiated = false;
			ChangeState(EnemyState.Chase);
			return;
		}

		// Ensure that the enemy is on the NavMesh.
		if (!enemyBase.enemyNavScript.IsAgentOnNavMesh())
		{
			enemyBase.enemyNavScript.MoveToNavMesh();
		}

		// Ensure the animator is enabled.
		if (!animator.enabled)
		{
			animator.enabled = true;
			animator.Play("Stand up", 0, 0f);
		}

		// Only trigger the standup animation once.
		if (!standupInitiated)
		{
			// Double calling stand up animation for avoiding a bug
			animator.Play("Stand up", 0, 0f);
			standupInitiated = true;
		}

		// Check if the standup animation has finished.
		if (StandupFinished())
		{
			animator.CrossFade("Blend Tree Flailing Arms", 0.1f);
			enemyNavScript.MoveToNavMesh();
			enemyNavScript.ResumeNavigation();
			ChangeState(EnemyState.Chase);
			standupInitiated = false;
		}
	}


	void HandleDead()
	{
		// Optionally play a death animation or freeze the state.
	}

	// Returns true if the specified non-looping animation has finished.
	bool AnimationFinished(string animationName)
	{
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		return stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1f;
	}

	public IEnumerator BlendToStandup()
	{
		// Disable NavMeshAgent movement while blending.
		enemyNavScript.StopNavigation();

		// Crossfade from current pose to "Stand up" animation over a short duration.
		animator.CrossFade("Stand up", 0.2f);

		// Wait for a bit longer than the blend duration.
		yield return new WaitForSeconds(0.3f);

		// Now wait until the "Stand up" animation has finished.
		while (!StandupFinished())
		{
			yield return null;
		}

		// Transition to Idle (or Chase) after standup.
		animator.Play("Idle");
		enemyNavScript.ResumeNavigation();
		ChangeState(EnemyState.Chase);
	}

	// Placeholder methods—you need to implement these based on your game's logic.
	bool AttackFinished() { return true; }
	bool KnockbackFinished()
	{
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		string animName = enemyBase.isCrawling ? "Knockback_Crawl" : "Knockback";
		return stateInfo.IsName(animName) && stateInfo.normalizedTime >= 1f;
	}
	bool ElectrocutionFinished() { return true; }
	bool StandupFinished()
	{
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		// Ensure "Stand up" is non-looping; normalizedTime >= 1 means it finished one play-through.
		return stateInfo.IsName("Stand up") && stateInfo.normalizedTime >= 1f;
	}

}
