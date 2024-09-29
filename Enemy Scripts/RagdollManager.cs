using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 17.11.2023 New script to handle ragdolls for all enemies
public class RagdollManager : MonoBehaviour
{
	public float standUpMagnitude;
	public Rigidbody bodyRB; // Rigidbody in waist or something (used for ragdoll magnitude checks etc.)

	private Animator animator;
	private Enemy enemyScript;
	private NavMeshAgent navAgent;
	private EnemyNav navScript;
	private List<Collider> ragdollParts = new List<Collider>();
	private Rigidbody[] rigidbodies;

	private bool standCountdownActive = false;
	private float countdown = 0f;

	private void Awake()
	{
		// Get Script references
		animator = GetComponent<Animator>();
		enemyScript = GetComponent<Enemy>();
		navAgent = GetComponent<NavMeshAgent>();
		navScript = GetComponent<EnemyNav>();

		// Setup ragdoll functionality
		rigidbodies = GetComponentsInChildren<Rigidbody>();
		SetRagdollParts();
	}

	private void FixedUpdate()
	{
		CheckRagdollMagnitude();
	}

	private void CheckRagdollMagnitude()
	{
		// Don't calculate magnitude on non-ragdolling or dead enemies
		if (!enemyScript.ragdolling || enemyScript.isDead) return;

		// When magnitude has been low enough for certain time, stand up
		if (bodyRB.velocity.magnitude < standUpMagnitude && !standCountdownActive)
		{
			countdown = Time.time;
			standCountdownActive = true;
		}
		else if (bodyRB.velocity.magnitude > standUpMagnitude)
		{
			standCountdownActive = false;
		}

		if (Time.time > countdown && bodyRB.velocity.magnitude < standUpMagnitude)
		{
			TurnOffRagdoll();
		}
	}

	private void SetRagdollParts()
	{
		Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();

		foreach (Collider c in colliders)
		{
			if (c.gameObject != this.gameObject)
			{
				c.isTrigger = true;
				ragdollParts.Add(c);
			}

		}
	}

	public void TurnOnRagdoll()
	{
		if (enemyScript.ragdolling) return;
		Debug.Log("Turning on ragdoll");
		enemyScript.ragdolling = true;

		foreach (Rigidbody rb in rigidbodies)
		{
			rb.isKinematic = false;
		}

		foreach (Collider c in ragdollParts)
		{
			c.isTrigger = false;
		}

		// Stop and disable the NavMesh agent
		if (navAgent != null)
		{
			navAgent.isStopped = true;
			navAgent.enabled = false;
		}

		standCountdownActive = false;
		animator.enabled = false;
	}

	public void TurnOffRagdoll()
	{
		if (enemyScript.isDead || !enemyScript.ragdolling) return;
		Debug.Log("Turning off ragdoll");
		enemyScript.ragdolling = false;

		foreach (Rigidbody rb in rigidbodies)
		{
			rb.isKinematic = true;
		}

		foreach (Collider c in ragdollParts)
		{
			c.isTrigger = true;
		}

		transform.position = bodyRB.transform.position; //Enemy GO does not move with ragdoll, so do that when stop ragdoll
		animator.enabled = true;

		// Re-enable and start the NavMesh agent
		if (navAgent != null)
		{
			navAgent.enabled = true;
			navAgent.isStopped = true;

			animator.Play("Stand up", 0, 0f);

			ContinueAfterRagdoll();
		}
	}

	private void ContinueAfterRagdoll()
	{
		if (enemyScript.ragdolling) return;
		navScript.MoveToNavMesh();
		if (!navAgent.isActiveAndEnabled) navAgent.enabled = true;
		navAgent.isStopped = false;
	}

}
