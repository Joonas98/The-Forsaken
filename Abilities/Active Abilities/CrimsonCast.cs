using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

[CreateAssetMenu(menuName = "Abilities/CrimsonCast")]
public class CrimsonCast : Ability
{
	// Apply debuff to enemies, that cause them to take extra damage each time they are damaged
	// For example first hit deals 5 damage, next hit 10, 15 and so on
	public float duration;
	public float length, radius;
	public GameObject castPrefab;

	private List<DebuffManager> affectedEnemies = new List<DebuffManager>();

	public override void Activate(GameObject parent)
	{
		// Debug.Log("CrimsonCast activated");
		base.Activate(parent);
		GameObject crimsonCast = Instantiate(castPrefab, Camera.main.transform);
		Destroy(crimsonCast, 5f);
		//CrimsonCastParticle particle = crimsonCast.GetComponent<CrimsonCastParticle>();
		//particle.duration = duration;

		CalculateHitEnemies();
	}

	public override void BeginCooldown(GameObject parent)
	{
		// Debug.Log("CrimsonCast ended");
		base.BeginCooldown(parent);
		affectedEnemies.Clear();
	}

	private void CalculateHitEnemies()
	{
		Vector3 positionInFrontOfCamera = Camera.main.transform.position + Camera.main.transform.forward * length;
		LayerMask enemyLayerMask = 1 << 11;
		RaycastHit[] hits = Physics.CapsuleCastAll(Camera.main.transform.position, positionInFrontOfCamera, radius, Camera.main.transform.forward, length, enemyLayerMask);
		foreach (RaycastHit hit in hits)
		{
			//Debug.Log("Cast hit: " + hit.transform.name);
			DebuffManager debuffManager = hit.collider.GetComponentInParent<DebuffManager>();

			if (debuffManager != null && !affectedEnemies.Contains(debuffManager))
			{
				//Debug.Log("Cast hit an enemy");
				debuffManager.ApplyDebuff(DebuffManager.Debuffs.Crimson, duration);
				affectedEnemies.Add(debuffManager);
			}
		}
	}

}
