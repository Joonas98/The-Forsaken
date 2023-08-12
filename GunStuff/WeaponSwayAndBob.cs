using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// New universal sway and weapon bob system 8.5.2023
// Meant to replace WeaponSway and IdleSway scripts / systems.
// Original base for this system from this video: https://www.youtube.com/watch?v=DR4fTllQnXg
public class WeaponSwayAndBob : MonoBehaviour
{
    [Header("Important")]
    public static WeaponSwayAndBob instance;
    public PlayerMovement mover;
    public float returnSpeed;

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

    public float smooth = 500f; // Used for BobOffset and Sway
    float smoothRot = 12f; // Used for BobSway and TiltSway

    [Header("Bobbing")]
    public float speedCurve; // Used by both bobbing types
    float curveSin { get => Mathf.Sin(speedCurve); }
    float curveCos { get => Mathf.Cos(speedCurve); }

    public Vector3 travelLimit = Vector3.one * 0.025f; // Max limits of travel from movement
    public Vector3 bobLimit = Vector3.one * 0.01f; // Limit travel from bobbing over time
    Vector3 bobPosition;

    public float bobExaggeration;

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    public Vector3 runningMultiplier;

    private Vector3 defaultMultiplier;
    private Vector3 bobEulerRotation;

    // Important privates for multiple functions
    Vector2 walkInput;
    Vector2 lookInput;
    float inputMagnitude;
    float verticalMovement;
    private Vector3 previousPosition;

    private void Awake()
    {
        instance = this;
        defaultMultiplier = multiplier;
        previousPosition = transform.position;
    }

    void Update()
    {
        GetInput();
        // When choosing grenades or objects, mouse is used for selection -> sway during selection is annoying bug
        if (disableSwayBob || GrenadeThrow.instance.selectingGrenade || ObjectPlacing.instance.isPlacing || ObjectPlacing.instance.isChoosingObject) return;

        if (sway && !disableSwayBob) Sway();
        if (swayRotation && !disableSwayBob) SwayRotation();
        if (bobOffset && !disableSwayBob) BobOffset();
        if (bobRotation && !disableSwayBob) BobRotation();

        if (mover.isRunning)
            multiplier = runningMultiplier;
        else
            multiplier = defaultMultiplier;

        CompositePositionRotation();
    }

    void GetInput()
    {
        // Get movement input
        walkInput.x = Input.GetAxis("Horizontal");
        walkInput.y = Input.GetAxis("Vertical");
        walkInput = walkInput.normalized;
        inputMagnitude = walkInput.magnitude;

        // Y velocity from rigibody
        verticalMovement = Mathf.Abs((transform.position - previousPosition).y); // Calculate the vertical movement based on position changes
        previousPosition = transform.position;

        // This fixes diagonal movement BobOffset
        if (inputMagnitude > 1f)
        {
            walkInput /= inputMagnitude;
        }

        // Get mouse movement input
        lookInput.x = Input.GetAxis("Mouse X");
        lookInput.y = Input.GetAxis("Mouse Y");
    }

    public void ReturnToOriginal() // Return to original position and rotation
    {
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
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

    void BobOffset() // Player movemet -> position change
    {
        speedCurve += Time.deltaTime * (mover.isGrounded ? inputMagnitude * bobExaggeration : 1f) + 0.01f;

        bobPosition.x = (curveCos * bobLimit.x * (mover.isGrounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
        bobPosition.y = (curveSin * bobLimit.y) * verticalMovement * 5f;
        bobPosition.z = -(walkInput.y * travelLimit.z);
        // Debug.Log("Vertical movement: " + verticalMovement);
        // Debug.Log("Bob position Y: " + bobPosition.y);
    }

    void BobRotation() // Player movement -> rotation change (roll, pitch, yaw)
    {
        bobEulerRotation.x = (walkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 2));
        bobEulerRotation.y = (walkInput != Vector2.zero ? multiplier.y * curveCos : 0);
        bobEulerRotation.z = (walkInput != Vector2.zero ? multiplier.z * curveCos * walkInput.x : 0);
    }

    // TODO: add bob and sway to axis Y movement like jump and fall
    // void FallOffset()
    // {
    //     fallPosition.y = -(_currentVelocity.Value.y * _travelLimitFall.y);
    // }
    //
    // void FallRotatio()
    // {
    //     fallEulerRot.x = (_currentVelocity.Value.y * _fallMultiplier);
    // }

    void CompositePositionRotation()
    {
        if (!GameManager.GM.currentGunAiming)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, swayPos + bobPosition, Time.deltaTime * smooth);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation), Time.deltaTime * smoothRot);
            // Debug.Log("Not aiming");
        }
        else // Return to original spot
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, 0, 0), Time.deltaTime * GameManager.GM.currentGun.aimSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(0, 0, 0)), Time.deltaTime * GameManager.GM.currentGun.aimSpeed);
            //  Debug.Log("Aiming");
        }
    }
}
