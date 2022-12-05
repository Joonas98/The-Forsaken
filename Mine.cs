using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{

    public Grenade grenadeScript;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Torso") && other.gameObject.layer == 2)
        {
            grenadeScript.Explode();
        }

    }

}
