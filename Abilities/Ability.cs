using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Testi toimiiko git

public class Ability : ScriptableObject
{
    public new string name;
    public Sprite picture;
    public bool passive;

    public float cooldownTime, activeTime;
    public AudioClip activateSFX, endSFX;
    public AudioSource audioSource;

    protected virtual void OnEnable() // Inherited in all abilities to find the AudioSource
    {
        audioSource = GameObject.Find("Player").GetComponent<AudioSource>();
    }

    public virtual void Activate(GameObject parent)
    {

    }

    public virtual void BeginCooldown(GameObject parent)
    {

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
