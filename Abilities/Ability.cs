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

    public float cooldownTime;
    public float activeTime;

    public virtual void Activate(GameObject parent)
    {

    }

    public virtual void BeginCooldown(GameObject parent)
    {

    }


}
