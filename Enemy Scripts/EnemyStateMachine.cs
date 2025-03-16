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
		currentState = newState;
		if (newState == EnemyState.Spawn)
		{
			spawnAnimationStarted = false;
		}
	}

	void HandleSpawn()
	{
		// Stop movement during spawn.
		navAgent.isStopped = true;
		navAgent.ResetPath();

		if (!spawnAnimationStarted)
		{
			animator.Play("Spawn");
			spawnAnimationStarted = true;
		}

		// Once the spawn animation finishes, re-enable movement and transition.
		if (AnimationFinished("Spawn"))
		{
			navAgent.isStopped = false;
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

	void HandleKnockback()
	{
		animator.Play(enemyBase.isCrawling ? "Knockback_Crawl" : "Knockback");
		if (KnockbackFinished())
		{
			ChangeState(EnemyState.Chase);
		}
	}

	void HandleElectrocuted()
	{
		animator.Play(enemyBase.isCrawling ? "Electrocuted_Crawl" : "Electrocuted");
		if (ElectrocutionFinished())
		{
			ChangeState(EnemyState.Chase);
		}
	}

	void HandleRagdoll()
	{
		// Disable the animator so physics control the ragdoll.
		if (animator.enabled)
			animator.enabled = false;
		// No further action here—your Enemy.cs ragdoll detection (via TurnOffRagdoll)
		// will call ChangeState(EnemyState.Standup) once the ragdoll is settled.
	}

	private bool standupInitiated = false;

	private bool standupNavRepositioned = false;

	void HandleStandup()
	{
		// If the enemy is crawling, skip standup.
		if (enemyBase.isCrawling)
		{
			standupInitiated = false;
			standupNavRepositioned = false;
			ChangeState(EnemyState.Chase);
			return;
		}

		// Only call MoveToNavMesh once.
		if (!standupNavRepositioned && !enemyBase.enemyNavScript.IsAgentOnNavMesh())
		{
			enemyBase.enemyNavScript.MoveToNavMesh();
			standupNavRepositioned = true;
		}

		// Ensure the animator is enabled.
		if (!animator.enabled)
		{
			animator.enabled = true;
		}

		// Only trigger the standup animation once.
		if (!standupInitiated)
		{
			animator.CrossFade("Stand up", 0.2f);
			standupInitiated = true;
			Debug.Log("Standup initiated.");
		}
		else
		{
			//Debug.Log("Standup in progress.");
		}

		// Check if the standup animation has finished.
		if (StandupFinished())
		{
			animator.Play("Idle");
			navAgent.isStopped = false;
			ChangeState(EnemyState.Chase);
			standupInitiated = false;
			standupNavRepositioned = false;
			enemyNavScript.MoveToNavMesh();
			Debug.Log("Standup finished; transitioning to Chase.");
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
		navAgent.isStopped = true;

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
		navAgent.isStopped = false;
		ChangeState(EnemyState.Chase);
	}

	// Placeholder methods—you need to implement these based on your game's logic.
	bool AttackFinished() { return true; }
	bool KnockbackFinished() { return true; }
	bool ElectrocutionFinished() { return true; }
	bool StandupFinished()
	{
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		// Ensure "Stand up" is non-looping; normalizedTime >= 1 means it finished one play-through.
		return stateInfo.IsName("Stand up") && stateInfo.normalizedTime >= 1f;
	}

}
