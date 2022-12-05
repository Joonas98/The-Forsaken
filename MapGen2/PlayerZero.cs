using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerZero : MonoBehaviour
{


    [SerializeField] private GameObject MapMagic;
    [SerializeField] private NavigationBaker NavBaker;
    [SerializeField] private GameObject enemyContainer;

    [SerializeField]
    private float distance;

    private void LateUpdate()
    {
        if (Vector3.Distance(Vector3.zero, transform.position) > distance)
        {
            Debug.Log("Origon reset");
            GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");
            MapMagic.transform.position -= transform.position;
            transform.position = Vector3.zero;
        }
    }

}