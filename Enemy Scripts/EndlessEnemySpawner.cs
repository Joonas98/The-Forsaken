using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EndlessEnemySpawner : MonoBehaviour
{
	[Header("Spawning")]
	[SerializeField] private bool spawnEnabled = true;
	[SerializeField] private GameObject[] enemyPrefabs;
	[SerializeField] private int targetEnemyCount = 25;
	[SerializeField] private int maxSpawnAttemptsPerTick = 12;
	[SerializeField] private int maxSpawnPerTick = 3;
	[SerializeField] private float spawnInterval = 2f;
	[SerializeField] private float minSpawnDistance = 70f;
	[SerializeField] private float maxSpawnDistance = 140f;
	[SerializeField] private LayerMask groundLayer = Physics.DefaultRaycastLayers;

	[Header("Spawn Direction Weights")]
	[SerializeField] private float front90Weight = 70f;
	[SerializeField] private float front180Weight = 25f;
	[SerializeField] private float front270Weight = 5f;

	[Header("Despawning")]
	[SerializeField] private bool despawnEnabled = true;
	[SerializeField] private float despawnDistance = 180f;
	[SerializeField] private float despawnInterval = 2f;

	[Header("Placement")]
	[SerializeField] private Transform player;
	[SerializeField] private float spawnRayHeight = 250f;
	[SerializeField] private float spawnRayDistance = 500f;
	[SerializeField] private float navMeshSampleDistance = 8f;
	[SerializeField] private float minDistanceFromOtherEnemies = 10f;
	[SerializeField] private bool debug;

	private WaitForSeconds spawnWait;
	private WaitForSeconds despawnWait;

	private void Awake()
	{
		if (player == null)
		{
			GameObject playerGO = GameObject.Find("Player");
			if (playerGO != null)
				player = playerGO.transform;
		}

		spawnWait = new WaitForSeconds(spawnInterval);
		despawnWait = new WaitForSeconds(despawnInterval);
	}

	private void OnValidate()
	{
		targetEnemyCount = Mathf.Max(0, targetEnemyCount);
		maxSpawnAttemptsPerTick = Mathf.Max(1, maxSpawnAttemptsPerTick);
		maxSpawnPerTick = Mathf.Max(1, maxSpawnPerTick);
		spawnInterval = Mathf.Max(0.1f, spawnInterval);
		despawnInterval = Mathf.Max(0.1f, despawnInterval);
		front90Weight = Mathf.Max(0f, front90Weight);
		front180Weight = Mathf.Max(0f, front180Weight);
		front270Weight = Mathf.Max(0f, front270Weight);
		maxSpawnDistance = Mathf.Max(minSpawnDistance, maxSpawnDistance);
		despawnDistance = Mathf.Max(maxSpawnDistance, despawnDistance);
	}

	private void OnEnable()
	{
		StartCoroutine(SpawnLoop());
		StartCoroutine(DespawnLoop());
	}

	private IEnumerator SpawnLoop()
	{
		yield return null;

		while (true)
		{
			if (spawnEnabled)
				TrySpawnMissingEnemies();

			yield return spawnWait;
		}
	}

	private IEnumerator DespawnLoop()
	{
		yield return null;

		while (true)
		{
			if (despawnEnabled)
				DespawnFarEnemies();

			yield return despawnWait;
		}
	}

	private void TrySpawnMissingEnemies()
	{
		if (player == null || enemyPrefabs == null || enemyPrefabs.Length == 0 || GameManager.GM == null)
			return;

		CleanupEnemyList();

		int missingEnemies = targetEnemyCount - GameManager.GM.enemyCount;
		if (missingEnemies <= 0)
			return;

		int enemiesToSpawn = Mathf.Min(missingEnemies, maxSpawnPerTick);
		int spawned = 0;
		int attempts = 0;

		while (spawned < enemiesToSpawn && attempts < maxSpawnAttemptsPerTick)
		{
			attempts++;

			if (!TryGetSpawnPosition(out Vector3 spawnPosition))
				continue;

			GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
			Instantiate(prefab, spawnPosition, GetSpawnRotation(spawnPosition));
			GameManager.GM.enemyCount++;
			GameManager.GM.UpdateEnemyCount();
			spawned++;

			if (debug)
				Debug.DrawRay(spawnPosition + Vector3.up, Vector3.up * 5f, Color.green, despawnInterval);
		}
	}

	private bool TryGetSpawnPosition(out Vector3 spawnPosition)
	{
		spawnPosition = Vector3.zero;

		Vector3 spawnDirection = GetWeightedSpawnDirection();
		Vector3 randomOffset = spawnDirection * Random.Range(minSpawnDistance, maxSpawnDistance);
		Vector3 rayOrigin = player.position + randomOffset + Vector3.up * spawnRayHeight;

		if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, spawnRayDistance, groundLayer))
		{
			if (debug)
				Debug.DrawRay(rayOrigin, Vector3.down * spawnRayDistance, Color.red, despawnInterval);
			return false;
		}

		if (!NavMesh.SamplePosition(groundHit.point, out NavMeshHit navHit, navMeshSampleDistance, NavMesh.AllAreas))
			return false;

		if (IsTooCloseToOtherEnemy(navHit.position))
			return false;

		spawnPosition = navHit.position;
		return true;
	}

	private Vector3 GetWeightedSpawnDirection()
	{
		Vector3 forward = player.forward;
		forward.y = 0f;

		if (forward.sqrMagnitude < 0.001f)
			forward = Vector3.forward;
		else
			forward.Normalize();

		float totalWeight = front90Weight + front180Weight + front270Weight;
		if (totalWeight <= 0f)
			return forward;

		float roll = Random.Range(0f, totalWeight);
		float angle;

		if (roll < front90Weight)
		{
			angle = Random.Range(-45f, 45f);
		}
		else if (roll < front90Weight + front180Weight)
		{
			angle = Random.Range(45f, 90f) * RandomSign();
		}
		else
		{
			angle = Random.Range(90f, 135f) * RandomSign();
		}

		return Quaternion.AngleAxis(angle, Vector3.up) * forward;
	}

	private Quaternion GetSpawnRotation(Vector3 spawnPosition)
	{
		Vector3 directionToPlayer = player.position - spawnPosition;
		directionToPlayer.y = 0f;

		if (directionToPlayer.sqrMagnitude < 0.001f)
			return Quaternion.identity;

		return Quaternion.LookRotation(directionToPlayer.normalized, Vector3.up);
	}

	private float RandomSign()
	{
		return Random.value < 0.5f ? -1f : 1f;
	}

	private bool IsTooCloseToOtherEnemy(Vector3 position)
	{
		if (GameManager.GM == null || minDistanceFromOtherEnemies <= 0f)
			return false;

		float minDistanceSqr = minDistanceFromOtherEnemies * minDistanceFromOtherEnemies;

		for (int i = 0; i < GameManager.GM.enemiesAliveGos.Count; i++)
		{
			GameObject enemyGO = GameManager.GM.enemiesAliveGos[i];
			if (enemyGO == null)
				continue;

			if ((enemyGO.transform.position - position).sqrMagnitude < minDistanceSqr)
				return true;
		}

		return false;
	}

	private void DespawnFarEnemies()
	{
		if (player == null || GameManager.GM == null)
			return;

		float despawnDistanceSqr = despawnDistance * despawnDistance;

		for (int i = GameManager.GM.enemiesAliveGos.Count - 1; i >= 0; i--)
		{
			GameObject enemyGO = GameManager.GM.enemiesAliveGos[i];
			if (enemyGO == null)
			{
				GameManager.GM.enemiesAliveGos.RemoveAt(i);
				continue;
			}

			if ((enemyGO.transform.position - player.position).sqrMagnitude <= despawnDistanceSqr)
				continue;

			Enemy enemy = enemyGO.GetComponent<Enemy>();
			if (enemy != null)
			{
				enemy.Despawn();
			}
			else
			{
				GameManager.GM.enemiesAliveGos.RemoveAt(i);
				GameManager.GM.enemyCount = Mathf.Max(0, GameManager.GM.enemyCount - 1);
				GameManager.GM.UpdateEnemyCount();
				Destroy(enemyGO);
			}
		}
	}

	private void CleanupEnemyList()
	{
		if (GameManager.GM == null)
			return;

		for (int i = GameManager.GM.enemiesAliveGos.Count - 1; i >= 0; i--)
		{
			if (GameManager.GM.enemiesAliveGos[i] == null)
				GameManager.GM.enemiesAliveGos.RemoveAt(i);
		}

		GameManager.GM.enemyCount = GameManager.GM.enemiesAliveGos.Count;
		GameManager.GM.UpdateEnemyCount();
	}
}
