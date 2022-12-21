using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNav : MonoBehaviour
{


    [SerializeField] private Transform targetLocation;
    [SerializeField] private float onMeshThreshold;
    private NavMeshAgent navMeshAgent;
    private GameObject Player;

    private Enemy enemyScript;

    private void Awake()
    {
        enemyScript = GetComponent<Enemy>();
        GameObject Player = GameObject.Find("Player");
        targetLocation = Player.transform;
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (!IsAgentOnNavMesh(gameObject))
        {
            MoveToNavMesh();
            // Debug.Log("Enemy moved to navmesh");
        }
    }

    private void Update()
    {
        if (!navMeshAgent.isActiveAndEnabled) return; // Avoid errors when agent is disabled

        navMeshAgent.destination = targetLocation.position;

        if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance) // Rotate towards player when stoppingDistance is reached
        {
            navMeshAgent.updateRotation = false;
            Vector3 lookPos = targetLocation.position - transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f);
        }
        else
        {
            if (navMeshAgent.updateRotation != true) navMeshAgent.updateRotation = true;
        }
    }

    public bool IsAgentOnNavMesh(GameObject agentObject)
    {
        Vector3 agentPosition = agentObject.transform.position;
        NavMeshHit hit;

        // Check for nearest point on navmesh to agent, within onMeshThreshold
        if (NavMesh.SamplePosition(agentPosition, out hit, onMeshThreshold, NavMesh.AllAreas))
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
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(transform.position, out myNavHit, 100, -1))
        {
            transform.position = myNavHit.position;
        }
    }

}
