using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{

    [SerializeField] private float movementAmount = 1;
    [SerializeField] private float rotationAmount = 0.2f;
    [SerializeField] private float smoothAmount = 2;
    private float ogSmoothAmount;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Gun currentGun;

    void Awake()
    {
        ogSmoothAmount = smoothAmount;
        currentGun = GetComponent<Gun>();
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        float movementX = Input.GetAxis("Mouse X") * movementAmount;
        float movementY = Input.GetAxis("Mouse Y") * movementAmount;

        float rotationX = Input.GetAxis("Mouse X") * rotationAmount;
        float rotationY = Input.GetAxis("Mouse Y") * rotationAmount;

        if (!currentGun.isAiming && !currentGun.isFiring)
        {
            Quaternion finalRotation = new Quaternion(rotationY * -0.5f, rotationX * -0.5f, 0, 0);
            // transform.localRotation = Quaternion.Lerp(transform.localRotation.eulerAngles, finalRotation + initialRotation, Time.deltaTime * smoothAmount);

            // if (rotationX == 0 && rotationY == 0) // Palauttaa aseen nopeammin paikalleen jos ei liikuta hiirtä
            // {
            //     smoothAmount = ogSmoothAmount * 2;
            // }
            // else
            // {
            //     smoothAmount = ogSmoothAmount;
            // }

            // Weapon rotation when not aiming
            transform.localRotation = Quaternion.Slerp(transform.localRotation, new Quaternion(finalRotation.x + initialRotation.x, finalRotation.y + initialRotation.y, finalRotation.z + initialRotation.z, finalRotation.w + initialRotation.w), Time.deltaTime * smoothAmount * 3);

        }
        else if (currentGun.isAiming && !currentGun.isFiring)
        {
            Quaternion finalRotation = new Quaternion(rotationY * -0.25f, rotationX * -0.25f, 0, 0);

            // if (rotationX == 0 && rotationY == 0) // Palauttaa aseen nopeammin paikalleen jos ei liikuta hiirtä
            // {
            //     smoothAmount = ogSmoothAmount * 3;
            // }
            // else
            // {
            //     smoothAmount = ogSmoothAmount;
            // }

            // Weapon rotation when aiming
            transform.localRotation = Quaternion.Slerp(transform.localRotation, new Quaternion(finalRotation.x + initialRotation.x, finalRotation.y + initialRotation.y, finalRotation.z + initialRotation.z, finalRotation.w + initialRotation.w), Time.deltaTime * smoothAmount * 3);
        }
        else
        {
            ResetRotation();
        }
    }

    public void ResetSway()
    {
        transform.localRotation = initialRotation;
    }

    public void ResetRotation()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, initialRotation, Time.deltaTime * 5f);
    }

}
