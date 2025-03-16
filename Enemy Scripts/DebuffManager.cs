using System.Collections.Generic;
using UnityEngine;

public class DebuffManager : MonoBehaviour
{
	// Create a dictionary to store active debuffs and their durations
	private Dictionary<Debuffs, float> activeDebuffs = new Dictionary<Debuffs, float>();

	// Reference to the effects game objects (FX)
	public GameObject[] effectList;
	public Color shockedEyeColor;

	// Reference to the enemy script
	public Enemy enemyScript;

	#region DoT System
	[System.Serializable]
	public struct DoTEffect // Damage over time effect, such as fire or poison
	{
		public string name;
		public int tickDamage;
		public float tickInterval;
	}

	public List<DoTEffect> dotEffects;

	private float fireLastTickTime = 0f;

	#endregion

	// Enum to represent debuff types
	public enum Debuffs
	{
		Arcane, Crimson, Dark, Fairy, Fire, Frost, Holy, Light, Mist, Nature, ShockBlue, ShockYellow,
		Universe, Void, Water, Wind
	}

	private void Update()
	{
		UpdateDebuffDurations();
		UpdateDots();
		// Iterate through the active debuffs and print them
		//foreach (var debuff in activeDebuffs)
		//{
		//	Debug.Log($"Active Debuff: {debuff.Key}, Duration: {debuff.Value}");
		//}
	}

	private void UpdateDebuffDurations()
	{
		// Create a list to store debuffs that have expired
		List<Debuffs> debuffsToRemove = new List<Debuffs>();

		// Iterate through active debuffs and update their durations
		foreach (var debuff in new List<Debuffs>(activeDebuffs.Keys)) // Create a copy of keys to avoid collection modified error
		{
			activeDebuffs[debuff] -= Time.deltaTime;

			if (activeDebuffs[debuff] <= 0)
			{
				// Duration has expired, mark for removal
				debuffsToRemove.Add(debuff);
			}
		}

		// Remove expired debuffs
		foreach (var debuff in debuffsToRemove)
		{
			RemoveDebuff(debuff);
		}
	}

	private void UpdateDots()
	{
		// Check if the "Fire" debuff is active
		if (activeDebuffs.ContainsKey(Debuffs.Fire))
		{
			// Get the DoTEffect for "Fire" from the list
			DoTEffect fireEffect = dotEffects[0];

			// Check if the "Fire" debuff is active
			if (activeDebuffs.ContainsKey(Debuffs.Fire))
			{
				// Apply damage each tick for the "Fire" effect
				if (Time.time - fireLastTickTime >= fireEffect.tickInterval)
				{
					// Apply damage
					enemyScript.TakeDamage(fireEffect.tickDamage, 0, Enemy.DamageType.Fire);

					// Update the last tick time for "Fire"
					fireLastTickTime = Time.time;
				}
			}
		}
	}


	public void ApplyDebuff(Debuffs debuffenum, float duration)
	{
		// Activate the FX for the debuff
		effectList[(int)debuffenum].SetActive(true);
		ParticleSystem debuffParticleSystem = effectList[(int)debuffenum].GetComponent<ParticleSystem>();
		var emission = debuffParticleSystem.emission;
		emission.enabled = true;

		if (activeDebuffs.ContainsKey(debuffenum))
		{
			// Increase the existing debuff's duration
			activeDebuffs[debuffenum] += duration;
		}
		else
		{
			// Add the debuff to the dictionary with its initial duration
			activeDebuffs.Add(debuffenum, duration);
		}

		enemyScript.UpdateEyeColor();
		DebuffExtraFunctionalities(debuffenum, duration);
	}

	// Remove a debuff
	public void RemoveDebuff(Debuffs debuffenum)
	{
		//Debug.Log("Disabling debuff: " + debuffenum);
		if (debuffenum == Debuffs.Crimson) enemyScript.ResetCrimsonDamage();

		// Deactivate the FX for the debuff
		if (debuffenum == Debuffs.Fire)
		{
			ParticleSystem debuffParticleSystem = effectList[(int)debuffenum].GetComponent<ParticleSystem>();
			var emission = debuffParticleSystem.emission;
			emission.enabled = false;
			Invoke(nameof(DisableFire), 3f); // Disable the fire particle system after all particles have faded away
		}
		else
		{
			effectList[(int)debuffenum].SetActive(false);
		}

		// Remove the debuff from the active debuffs dictionary
		activeDebuffs.Remove(debuffenum);

		// Update enemy eye color
		enemyScript.UpdateEyeColor();
	}

	private void DisableFire()
	{
		// If the effect to be disabled is not reactived in between disabling emission and gameobject, we disable the gameobject
		if (!IsDebuffActive(Debuffs.Fire)) effectList[(int)Debuffs.Fire].SetActive(false);
	}

	// Check if a specific debuff is active
	public bool IsDebuffActive(Debuffs debuffenum)
	{
		return activeDebuffs.ContainsKey(debuffenum);
	}

	// Get the remaining duration of a specific debuff
	public float GetDebuffDuration(Debuffs debuffenum)
	{
		if (activeDebuffs.TryGetValue(debuffenum, out float remainingDuration))
		{
			return remainingDuration;
		}
		return 0f; // Debuff not found or expired
	}

	private void DebuffExtraFunctionalities(Debuffs debuffenum, float duration)
	{
		// 16.3.2025 Adjust to use new state machine script
		//if (debuffenum == Debuffs.ShockBlue) enemyScript.Stun(duration);
	}

}
