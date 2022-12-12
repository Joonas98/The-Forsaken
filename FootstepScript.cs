using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepScript : MonoBehaviour
{

    public float stepThreshold, stepVolume;

    public AudioSource audioSource;
    public AudioClip stepSound1, stepSound2;

    public PlayerMovement movementScript;

    private float distanceTravelled = 0;
    private Vector3 lastPosition;
    private bool stepTurn = true;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (movementScript.isGrounded)
            distanceTravelled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        if (distanceTravelled > stepThreshold)
        {
            if (stepTurn)
            {
                stepTurn = false;
                audioSource.PlayOneShot(stepSound1, stepVolume);
                distanceTravelled = 0;
            }
            else
            {
                stepTurn = true;
                audioSource.PlayOneShot(stepSound2, stepVolume);
                distanceTravelled = 0;
            }
        }
    }
}
