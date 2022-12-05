using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualRecoil : MonoBehaviour
{

    private Vector3 currentRotation, targetRotation, targetPosition, currentPosition, gunPositionOG;

    public float vrecoilX;
    public float vrecoilY;
    public float vrecoilZ;
    public float kickbackZ;
    public float snappiness; // Nopeus, jolla aseen rekyyli aiheutuu
    public float returnAmount; // Nopeus, jolla ase palaa takaisin

    public bool aiming;

    private void Start()
    {
        gunPositionOG = transform.localPosition;
    }

    private void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, Time.deltaTime * returnAmount);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, Time.fixedDeltaTime * snappiness);
        transform.localRotation = Quaternion.Euler(currentRotation);
        Back();
    }

    public void Recoil()
    {
        if (aiming)
        {
            targetPosition -= new Vector3(0, 0, kickbackZ * 0.2f);
            targetRotation += new Vector3(vrecoilX * 0.35f, Random.Range(-vrecoilY * 0.35f, vrecoilY * 0.35f), Random.Range(-vrecoilZ * 0.35f, vrecoilZ * 0.35f));
        }
        else
        {
            targetPosition -= new Vector3(0, 0, kickbackZ);
            targetRotation += new Vector3(vrecoilX, Random.Range(-vrecoilY, vrecoilY), Random.Range(-vrecoilZ, vrecoilZ));
        }
    }

    void Back()
    {
        if (aiming)
        {
            targetPosition = Vector3.Lerp(targetPosition, gunPositionOG, Time.deltaTime * returnAmount * 4);
            currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime * snappiness * 2);
            transform.localPosition = currentPosition;
        }
        else
        {
            targetPosition = Vector3.Lerp(targetPosition, gunPositionOG, Time.deltaTime * returnAmount);
            currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime * snappiness);
            transform.localPosition = currentPosition;
        }
    }

    #region Setters

    public void SetVireX(float value)
    {
        vrecoilX = value;
    }
    public void SetVireY(float value)
    {
        vrecoilY = value;
    }
    public void SetVireZ(float value)
    {
        vrecoilZ = value;
    }

    public void SetVireKickback(float value)
    {
        kickbackZ = value;
    }

    public void SetVireSnappiness(float value)
    {
        snappiness = value;
    }

    public void SetVireReturn(float value)
    {
        returnAmount = value;
    }

    #endregion

}
