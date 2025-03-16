using UnityEngine;

public enum EnemyState
{
	Idle,
	Patrol,
	Chase,
	Attack,
	Dead
}

public class EnemyStateMachine : MonoBehaviour
{
	public EnemyState currentState;
	private Animator animator;

	void Start()
	{
		animator = GetComponent<Animator>();
		ChangeState(EnemyState.Idle);
	}

	void Update()
	{
		switch (currentState)
		{
			case EnemyState.Idle:
				HandleIdle();
				break;
			case EnemyState.Patrol:
				HandlePatrol();
				break;
			case EnemyState.Chase:
				HandleChase();
				break;
			case EnemyState.Attack:
				HandleAttack();
				break;
			case EnemyState.Dead:
				HandleDeath();
				break;
		}
	}

	void ChangeState(EnemyState newState)
	{
		currentState = newState;
		// Optionally, trigger transition animations or reset timers here.
	}

	void HandleIdle()
	{
		animator.Play("Idle");
		// Add idle behavior (e.g., waiting, looking around)
		// Example transition:
		// if (PlayerSpotted()) ChangeState(EnemyState.Chase);
	}

	void HandlePatrol()
	{
		animator.Play("Walk");
		// Add patrol logic
	}

	void HandleChase()
	{
		animator.Play("Run");
		// Add chase logic (e.g., moving toward the player)
	}

	void HandleAttack()
	{
		animator.Play("Attack");
		// Add attack logic
	}

	void HandleDeath()
	{
		animator.Play("Dead");
		// Add death logic
	}
}
