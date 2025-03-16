using UnityEngine;
using UnityEngine.AI;

public class EnemyNav : MonoBehaviour
{
	[SerializeField] private float onMeshThreshold;
	[SerializeField] EnemyStateMachine stateMachine;
	private NavMeshAgent navAgent;
	// private EnemyBase enemyBase;
	private Enemy enemy;
	private Animator animator;
	private Transform targetLocation;
	private GameObject player;

	// Needed for rootmotion navmesh functionality
	private Vector2 velocity;
	private Vector2 smoothDeltaPosition;

	private void Awake()
	{
		// Set references
		animator = GetComponent<Animator>();
		navAgent = GetComponent<NavMeshAgent>();
		// enemyBase = GetComponent<EnemyBase>();
		enemy = GetComponent<Enemy>();

		//  Player reference
		player = GameObject.Find("Player");
		targetLocation = player.transform;

		// Root motion
		//animator.applyRootMotion = true;
		navAgent.updatePosition = false;
		navAgent.updateRotation = true;
	}

	private void Start()
	{
		if (!IsAgentOnNavMesh())
		{
			MoveToNavMesh();
			Debug.Log("Enemy moved to navmesh");
		}
	}

	private void FixedUpdate()
	{
		if (!navAgent.isActiveAndEnabled)
			return;

		// Only update destination if not ragdolling and not in Standup state.
		if (!enemy.ragdolling && stateMachine.currentState != EnemyState.Standup)
		{
			// Only set destination if the agent is on the NavMesh.
			if (IsAgentOnNavMesh())
			{
				navAgent.SetDestination(targetLocation.position);
			}
			else
			{
				// Log a warning but don't reposition here.
				MoveToNavMesh();
				Debug.LogWarning("Enemy is off NavMesh, but repositioning is handled only at spawn or after ragdoll.");
			}
		}
	}


	private void Update()
	{
		SynchronizeAnimatorAndAgent();
		// Debug.Log("Enemy speed: " + navAgent.speed.ToString());
	}

	public bool IsAgentOnNavMesh()
	{
		Vector3 agentPosition = transform.position;

		// Check for nearest point on navmesh to agent, within onMeshThreshold
		if (NavMesh.SamplePosition(agentPosition, out NavMeshHit hit, onMeshThreshold, NavMesh.AllAreas))
		{
			// Check if the positions are vertically aligned
			if (Mathf.Approximately(agentPosition.x, hit.position.x)
				&& Mathf.Approximately(agentPosition.z, hit.position.z))
			{
				// Lastly, check if object is below navmesh
				return agentPosition.y >= hit.position.y;
			}
		}
		return false;
	}

	public void MoveToNavMesh()
	{
		Debug.Log("Moved enemy to navmesh");
		NavMeshHit hit;
		if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
		{
			transform.position = hit.position;
		}
		else
		{
			// If a suitable position isn't found, handle it accordingly
			Debug.LogWarning("Could not find valid NavMesh position.");
		}
	}

	private void SynchronizeAnimatorAndAgent()
	{
		if (stateMachine.currentState == EnemyState.Ragdoll) return;

		Vector3 worldDeltaPosition = navAgent.nextPosition - transform.position;
		worldDeltaPosition.y = 0;

		float dx = Vector3.Dot(transform.right, worldDeltaPosition);
		float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
		Vector2 deltaPosition = new Vector2(dx, dy);

		float smooth = Mathf.Min(1, Time.deltaTime / 0.1f);
		smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

		velocity = smoothDeltaPosition / Time.deltaTime;
		if (navAgent.remainingDistance <= navAgent.stoppingDistance)
		{
			velocity = Vector2.Lerp(
				Vector2.zero,
				velocity,
				navAgent.remainingDistance / navAgent.stoppingDistance
			);
		}

		float deltaMagnitude = worldDeltaPosition.magnitude;
		if (deltaMagnitude > navAgent.radius / 2f)
		{
			transform.position = Vector3.Lerp(
				animator.rootPosition,
				navAgent.nextPosition,
				smooth
			);
		}
	}
}
