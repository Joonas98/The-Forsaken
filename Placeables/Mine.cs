using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public Grenade grenadeScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11) // Explode when hitting enemy layer
        {
            grenadeScript.Explode();
        }
    }
}
