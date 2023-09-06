using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffManager : MonoBehaviour
{
    // Create a dictionary to store active debuffs and their durations
    private Dictionary<Debuffs, float> activeDebuffs = new Dictionary<Debuffs, float>();

    // Reference to the effects game objects (FX)
    public GameObject[] effectList;
    public Color shockedEyeColor;

    public Enemy enemyScript;

    // Enum to represent debuff types
    public enum Debuffs
    {
        Arcane, Crimson, Dark, Fairy, Fire, Frost, Holy, Light, Mist, Nature, ShockBlue, ShockYellow,
        Universe, Void, Water, Wind
    }

    // Apply a debuff with a specified duration
    public void ApplyDebuff(Debuffs debuffenum, float duration)
    {
        // Activate the FX for the debuff
        effectList[(int)debuffenum].SetActive(true);

        // Check if the debuff already exists on the enemy
        if (activeDebuffs.ContainsKey(debuffenum))
        {
            // Update the existing debuff's duration
            activeDebuffs[debuffenum] += duration;
        }
        else
        {
            // Add the debuff to the dictionary with its initial duration
            activeDebuffs.Add(debuffenum, duration);
        }

        // Start a coroutine to automatically remove the debuff when its duration expires
        StartCoroutine(RemoveDebuffAfterDuration(debuffenum, duration));

        enemyScript.UpdateEyeColor();
        DebuffExtraFunctionalities(debuffenum, duration);
    }

    private void DebuffExtraFunctionalities(Debuffs debuffenum, float duration)
    {
        if (debuffenum == Debuffs.ShockBlue) enemyScript.Stun(duration);
    }

    // Coroutine to remove a debuff after a specified duration
    private IEnumerator RemoveDebuffAfterDuration(Debuffs debuffenum, float duration)
    {
        yield return new WaitForSeconds(duration);

        // Remove the debuff and deactivate its FX
        RemoveDebuff(debuffenum);
    }

    // Remove a debuff
    public void RemoveDebuff(Debuffs debuffenum)
    {
        // Deactivate the FX for the debuff
        effectList[(int)debuffenum].SetActive(false);

        // Remove the debuff from the active debuffs dictionary
        if (activeDebuffs.ContainsKey(debuffenum))
        {
            activeDebuffs.Remove(debuffenum);
        }
        enemyScript.UpdateEyeColor();
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
}
