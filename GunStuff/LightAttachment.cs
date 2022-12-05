using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAttachment : MonoBehaviour
{

    public Gun gunScript;
    public GameObject laser;

    private RaycastHit hit;


    private void Update()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100f))
        {
            laser.transform.position = hit.point;
            laser.transform.position = Vector3.MoveTowards(laser.transform.position, Camera.main.transform.position, 0.001f);
            //  laser.transform.rotation = Quaternion.Euler(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z);
        }
    }

}
