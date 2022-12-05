using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope : MonoBehaviour
{

    public Gun gunScript;
    public Camera scopeCam;
    public GameObject aimPosition;
    private GameObject aimPositionOG;

    [SerializeField] private float maxZoom, minZoom;

    private void Awake()
    {
        if (scopeCam == null) scopeCam = GetComponentInChildren<Camera>();
        if (gunScript == null) gunScript = GetComponentInParent<Gun>();
    }

    private void OnEnable()
    {
        gunScript.aimingSpot = aimPosition;
        gunScript.maxZoom = maxZoom;
        gunScript.minZoom = minZoom;
        if (scopeCam != null) gunScript.scopeCam = scopeCam;
    }

    private void OnDisable()
    {
        if (scopeCam != null) gunScript.scopeCam = null;
        gunScript.ResetAimingSpot();
    }

}
