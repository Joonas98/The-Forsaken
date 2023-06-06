using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    // Basic camera movement
    public float mouseSensitivity = 100f;
    public float aimSensMultiplier = 0.5f;
    public float minClamp, maxClamp;
    public Transform playerBody;
    public Transform recoilTrans;
    [HideInInspector] public bool canRotate = true;

    private float xRotation = 0f;
    private float mouseX, mouseY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        if (!canRotate) return;

        if (GameManager.GM.GetCurrentGun() != null && GameManager.GM.GetCurrentGun().isAiming)
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * aimSensMultiplier;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * aimSensMultiplier;
        }
        else
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minClamp, maxClamp);

        playerBody.Rotate(Vector3.up * mouseX);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

}
