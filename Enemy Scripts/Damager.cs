using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{

    [SerializeField] private Enemy enemyScript;

    private void Start()
    {
        if (enemyScript == null) enemyScript = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == true)
        {
            enemyScript.Attack(other.GetComponent<Player>());
        }
    }

}
