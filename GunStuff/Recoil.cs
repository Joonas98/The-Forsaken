using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Recoil : MonoBehaviour
{
    [HideInInspector] public static Recoil Instance;
    [Tooltip("Will the camera return where it was before recoiling. Needs to be balanced seperately")] public bool useReturningRecoil;
    [Tooltip("Up and down")] public float recoilX;
    [Tooltip("Left and right")] public float recoilY;
    [Tooltip("Tilt")] public float recoilZ;

    public float flinchX, flinchY, flinchZ; // Flinch = take damage

    public float rec1, rec2, rec3, rec4, rec5, rec6;
    public float snappiness;
    public float returnSpeed;
    public Transform playerTrans;
    public float recoilMultiplier;
    [HideInInspector] public bool aiming;
    public PlayerMovement movementScript;

    private Vector3 currentRotation;
    private Vector3 targetRotation;
    [SerializeField] private TextMeshProUGUI recXText, recYText, recZText, snappinessText, returnSpeedText, recMultiplierText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (useReturningRecoil)
        {
            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        }
        else
        {
            targetRotation = Vector3.Lerp(targetRotation, new Vector3(targetRotation.x, 0, 0), returnSpeed * Time.deltaTime);
        }

        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    private void LateUpdate()
    {
        // 29.6.2023 To be honest, not sure what this was here for?!?
        // playerTrans.rotation = new Quaternion(playerTrans.rotation.x, transform.localRotation.y + playerTrans.rotation.y, playerTrans.rotation.z, playerTrans.rotation.w);
    }

    public void RecoilFire()
    {
        if (!movementScript.isGrounded) // Mid air
        {
            recoilMultiplier = rec1;
        }
        else if (movementScript.isGrounded && !movementScript.isStationary && !aiming) // Grounded, moving, not aiming
        {
            recoilMultiplier = rec2;
        }
        else if (movementScript.isGrounded && !movementScript.isStationary && aiming) // Grounded, moving, aiming
        {
            recoilMultiplier = rec3;
        }
        else if (movementScript.isGrounded && movementScript.isStationary && !aiming) // Grounded, not moving, not aiming
        {
            recoilMultiplier = rec4;
        }
        else if (movementScript.isGrounded && movementScript.isStationary && aiming)  // Grounded, not moving, aiming
        {
            recoilMultiplier = rec5;
        }
        else // Nothing above, probably never used
        {
            recoilMultiplier = rec6;
        }

        targetRotation += new Vector3(recoilX, Random.Range(-recoilY * 0.1f, recoilY * 0.1f), Random.Range(-recoilZ, recoilZ)) * recoilMultiplier;
    }

    // Recoil from taking damage
    // Some variables like return speed is still determined by the held weapon
    public void DamageFlinch(float flinchMultiplier)
    {
        targetRotation += new Vector3(Random.Range(-flinchX, flinchX), Random.Range(-flinchY, flinchY), Random.Range(-flinchZ, flinchZ)) * flinchMultiplier;
    }

    #region Setters

    // Set functions
    public void SetRecoilX(float value)
    {
        recoilX = value;
    }

    public void SetRecoilY(float value)
    {
        recoilY = value;
    }

    public void SetRecoilZ(float value)
    {
        recoilZ = value;
    }

    public void SetSnappiness(float value)
    {
        snappiness = value;
    }

    public void SetReturnSpeed(float value)
    {
        returnSpeed = value;
    }

    #endregion

}
