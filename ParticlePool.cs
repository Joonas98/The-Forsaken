using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
	public static ParticlePool Instance { get; private set; }

	[Tooltip("Assign your blood prefab here (from Project, not Hierarchy)")]
	public ParticleSystem bloodFX;

	[Tooltip("Assign your hit prefab here (from Project, not Hierarchy)")]
	public ParticleSystem hitFX;

	// Maps prefab → queue of *instances*
	private Dictionary<ParticleSystem, Queue<ParticleSystem>> _pools
		= new Dictionary<ParticleSystem, Queue<ParticleSystem>>();

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else { Destroy(gameObject); return; }

		DontDestroyOnLoad(gameObject);

		// Preload some instances into each pool
		Preload(bloodFX, 50);
		Preload(hitFX, 10);
	}

	public void Preload(ParticleSystem prefab, int count)
	{
		if (!_pools.ContainsKey(prefab))
			_pools[prefab] = new Queue<ParticleSystem>();

		var queue = _pools[prefab];
		for (int i = 0; i < count; i++)
		{
			var inst = Instantiate(prefab, transform);
			inst.gameObject.SetActive(false);
			queue.Enqueue(inst);
		}
	}

	public ParticleSystem Spawn(ParticleSystem prefab, Vector3 worldPos, Quaternion worldRot)
	{
		// Get or create the queue
		if (!_pools.TryGetValue(prefab, out var queue))
		{
			queue = new Queue<ParticleSystem>();
			_pools[prefab] = queue;
		}

		ParticleSystem ps = null;

		// Dequeue until we find a *valid* instance or run out
		while (queue.Count > 0)
		{
			var candidate = queue.Dequeue();
			if (candidate != null)  // Unity‐overloaded null check filters out destroyed
			{
				ps = candidate;
				break;
			}
		}

		// If none found, instantiate a fresh one (at the right spot)
		if (ps == null)
		{
			ps = Instantiate(prefab, worldPos, worldRot, transform);
		}
		else
		{
			// Re-parent (world pos stays) and snap to exact hit
			ps.transform.SetParent(transform, true);
			ps.transform.SetPositionAndRotation(worldPos, worldRot);
		}

		// Activate & play
		ps.gameObject.SetActive(true);
		ps.Play();

		// Schedule its return
		StartCoroutine(ReturnWhenFinished(prefab, ps));
		return ps;
	}

	private IEnumerator ReturnWhenFinished(ParticleSystem prefab, ParticleSystem ps)
	{
		// Wait exactly long enough for emission + lifetime
		var main = ps.main;
		float total = main.duration + main.startLifetime.constantMax;
		yield return new WaitForSeconds(total);

		// If still around, deactivate & enqueue
		if (ps != null)
		{
			ps.gameObject.SetActive(false);
			_pools[prefab].Enqueue(ps);
		}
	}
}
