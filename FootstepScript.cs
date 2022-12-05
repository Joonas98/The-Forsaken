using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepScript : MonoBehaviour
{

    public float stepThreshold;

    public AudioSource audioSource;
    public AudioClip stepSound1, stepSound2;

    public Player playerScript;

    private float distanceTravelled = 0;
    private Vector3 lastPosition;
    private bool stepTurn = true;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (playerScript.isGrounded)
            distanceTravelled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        if (distanceTravelled > stepThreshold)
        {
            if (stepTurn)
            {
                stepTurn = false;
                audioSource.PlayOneShot(stepSound1);
                distanceTravelled = 0;
            }
            else
            {
                stepTurn = true;
                audioSource.PlayOneShot(stepSound2);
                distanceTravelled = 0;
            }
        }
    }
}
