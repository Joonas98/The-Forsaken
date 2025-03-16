using UnityEngine;

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
	private Animator animator;

	// Reference to your base Enemy script which handles things like health, limb status, etc.
	public Enemy enemyBase;

	void Awake()
	{
		animator = GetComponent<Animator>();
		enemyBase = GetComponent<Enemy>();  // Assumes both scripts are on the same GameObject.
	}

	void Start()
	{
		ChangeState(EnemyState.Spawn);
	}

	void Update()
	{
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

	void ChangeState(EnemyState newState)
	{
		currentState = newState;
		// Optionally reset timers or trigger transition-specific logic here.
	}

	void HandleSpawn()
	{
		// Choose appropriate spawn animation based on crawling status
		animator.Play("Spawn");
		// Once spawn animation is finished, transition to Idle.
		if (AnimationFinished("Spawn"))
		{
			ChangeState(EnemyState.Chase);
		}
	}

	void HandleChase()
	{
		animator.Play(enemyBase.isCrawling ? "Chase_Crawl" : "Chase");
		if (enemyBase.isAttacking = true)
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
		// Ragdoll state might use physics-based simulation.
		if (RagdollFinished())
		{
			ChangeState(EnemyState.Standup);
		}
	}

	void HandleStandup()
	{
		// Crawling enemies can't stand up, so they skip this state.
		if (enemyBase.isCrawling)
		{
			ChangeState(EnemyState.Chase);
			return;
		}

		animator.Play("Stand up");
		if (StandupFinished())
		{
			ChangeState(EnemyState.Chase);
		}
	}

	void HandleDead()
	{
		// Actions done in the enemy base class
	}

	bool AnimationFinished(string animationName)
	{
		// Remember, the parameter specifies the animator layer here
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		// Check if the current state's name matches and its normalized time is 1 or greater.
		// Note: normalizedTime is a float where 1.0 means the animation has finished one full play-through.
		// For looping animations, this value can exceed 1, so ensure your animations are non-looping if using this method.
		return stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1f;
	}

	bool InAttackRange()
	{
		// Implement your logic for determining attack range.
		return false; // Placeholder
	}

	bool AttackFinished()
	{
		// Check if the attack animation or process is finished.
		return true; // Placeholder
	}

	bool KnockbackFinished()
	{
		return true; // Placeholder
	}

	bool ElectrocutionFinished()
	{
		return true; // Placeholder
	}

	bool RagdollFinished()
	{
		return true; // Placeholder
	}

	bool StandupFinished()
	{
		return true; // Placeholder
	}
}
