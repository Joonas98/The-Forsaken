using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleSway : MonoBehaviour
{

    [SerializeField] private float idleSwayAmountA;
    [SerializeField] private float idleSwayAmountB;
    [SerializeField] private float idleSwayScale;
    [SerializeField] private float idleSwayLerpSpeed;
    [SerializeField] private float movementSwayMultiplier;
    [SerializeField] private float runningSwayMultiplier;

    private float idleSwayTime;

    private Vector3 swayPosition;

    public GameObject Player;
    public GameObject weaponHolster;
    // private Rigidbody RB;

    private float ogAmountA;
    private float ogAmountB;
    private float ogScale;

    private Vector3 ogPosition;
    private CharacterController cc;
    private PlayerMovement playerMovementScript;

    private void Awake()
    {
        ogAmountA = idleSwayAmountA;
        ogAmountB = idleSwayAmountB;
        ogScale = idleSwayScale;
        ogPosition = transform.localPosition;

        cc = GameObject.Find("Player").GetComponent<CharacterController>();
        playerMovementScript = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (playerMovementScript.isRunning) // Running
        {
            idleSwayAmountA = ogAmountA * runningSwayMultiplier;
            idleSwayAmountB = ogAmountB * runningSwayMultiplier;
            idleSwayScale = ogScale * 0.5f;
        }
        else if (!playerMovementScript.isRunning && cc.velocity.magnitude >= 1.5f) // Moving but not running
        {
            idleSwayAmountA = ogAmountA * movementSwayMultiplier;
            idleSwayAmountB = ogAmountB * movementSwayMultiplier;
            idleSwayScale = ogScale * 0.5f;
        }
        else // Not moving
        {
            idleSwayAmountA = ogAmountA;
            idleSwayAmountB = ogAmountB;
            idleSwayScale = ogScale;
        }
    }

    private void LateUpdate()
    {
        CalculateWeaponSway();
    }

    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(idleSwayTime, idleSwayAmountA, idleSwayAmountB) / idleSwayScale;

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * idleSwayLerpSpeed);
        idleSwayTime += Time.deltaTime;

        if (idleSwayTime > 60.0f)
            idleSwayTime = 0;

        transform.localPosition = swayPosition;
    }

    private Vector3 LissajousCurve(float Time, float A, float B) // Formula for the movement
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }

    public void ResetPosition()
    {
        transform.localPosition = ogPosition;
    }


}
