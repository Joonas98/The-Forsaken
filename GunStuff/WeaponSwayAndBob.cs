using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// New universal sway and weapon bob system 8.5.2023
// Meant to replace WeaponSway and IdleSway scripts / systems.
// Basics are from video: https://www.youtube.com/watch?v=DR4fTllQnXg
public class WeaponSwayAndBob : MonoBehaviour
{
    [Header("Important")]
    public static WeaponSwayAndBob instance;
    public PlayerMovement mover;
    public Gun currentGun;

    public float returnSpeed;
    private bool aiming = false;

    [Header("Enable Components")]
    public bool disableSwayBob;
    public bool sway;
    public bool swayRotation;
    public bool bobOffset;
    public bool bobRotation;

    [Header("Sway")]
    public float step = 0.01f; // Multiplied by the value from the mouse for 1 frame
    public float maxStepDistance = 0.06f; // Max distance from the local origin
    Vector3 swayPos; // Store our value for later

    [Header("Sway Rotation")]
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    Vector3 swayEulerRot;

    public float smooth = 10f; // Used for BobOffset and Sway
    float smoothRot = 12f; // Used for BobSway and TiltSway

    [Header("Bobbing")]
    public float speedCurve; // Used by both bobbing types
    float curveSin { get => Mathf.Sin(speedCurve); }
    float curveCos { get => Mathf.Cos(speedCurve); }

    public Vector3 travelLimit = Vector3.one * 0.025f; // Max limits of travel from movement
    public Vector3 bobLimit = Vector3.one * 0.01f; // Limit travol from bobbing over time
    Vector3 bobPosition;

    public float bobExaggeration;

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    Vector3 bobEulerRotation;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (disableSwayBob)
        {
            ReturnToOriginal();
        }
        else
        {
            GetInput();
            if (sway && !disableSwayBob) Sway();
            if (swayRotation && !disableSwayBob) SwayRotation();
            if (bobOffset && !disableSwayBob) BobOffset();
            if (bobRotation && !disableSwayBob) BobRotation();
            CompositePositionRotation();
        }
    }

    Vector2 walkInput;
    Vector2 lookInput;

    void GetInput()
    {
        walkInput.x = Input.GetAxis("Horizontal");
        walkInput.y = Input.GetAxis("Vertical");
        walkInput = walkInput.normalized;

        lookInput.x = Input.GetAxis("Mouse X");
        lookInput.y = Input.GetAxis("Mouse Y");
    }

    public void ReturnToOriginal() // Return to original position and rotation
    {
        // swayPos = bobPosition = swayEulerRot = bobEulerRotation = new Vector3(0, 0, 0);

        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

        Debug.Log("Bob and Sway zeroed");
    }

    void Sway() // Mouse movement -> position change
    {
        Vector3 invertLook = lookInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

        swayPos = invertLook;
    }

    void SwayRotation() // Mouse movement -> rotation change (roll, pitch, yaw)
    {
        Vector2 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
        swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    void BobOffset() // Player movement -> position change
    {
        speedCurve += Time.deltaTime * (mover.isGrounded ? (Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"))) * bobExaggeration : 1f) + 0.01f;

        bobPosition.x = (curveCos * bobLimit.x * (mover.isGrounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
        bobPosition.y = (curveSin * bobLimit.y) - (Mathf.Abs(Input.GetAxis("Vertical")) * travelLimit.y);
        bobPosition.z = -(walkInput.y * travelLimit.z);
    }

    void BobRotation() // Player movement -> rotation change (roll, pitch, yaw)
    {
        bobEulerRotation.x = (walkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 2));
        bobEulerRotation.y = (walkInput != Vector2.zero ? multiplier.y * curveCos : 0);
        bobEulerRotation.z = (walkInput != Vector2.zero ? multiplier.z * curveCos * walkInput.x : 0);
    }

    void CompositePositionRotation()
    {
        if (!aiming)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, swayPos + bobPosition, Time.deltaTime * smooth);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation), Time.deltaTime * smoothRot);
        }
        else // Return to original spot
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, 0, 0), Time.deltaTime * returnSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(0, 0, 0)), Time.deltaTime * returnSpeed);
        }
    }
}
