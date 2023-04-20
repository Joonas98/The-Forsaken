using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ability : ScriptableObject
{
    public new string name;
    public Sprite picture;
    public bool passive;

    public float cooldownTime, activeTime;
    public AudioClip activateSFX, endSFX;
    public AudioSource audioSource;

    protected virtual void OnEnable()
    {

    }

    public virtual void Activate(GameObject parent) // Call base.Activate(parent) in all abilities
    {
        if (audioSource == null && GameManager.GM.playerAS != null) // First activation sets audioSource
        {
            audioSource = GameManager.GM.playerAS;
        }

        if (audioSource != null && activateSFX != null) audioSource.PlayOneShot(activateSFX);

    }

    public virtual void BeginCooldown(GameObject parent)
    {
        if (audioSource != null && endSFX != null) audioSource.PlayOneShot(endSFX);
    }

    public bool GetPassiveType()
    {
        if (passive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
