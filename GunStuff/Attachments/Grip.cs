using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grip : MonoBehaviour
{
    public Gun gunScript;
    public float reloadTimeChange, adsTimeChange, equipTimeChange;
    public float xRecoilChange, yRecoilChange, zRecoilChange;

    private void Awake()
    {
        if (gunScript == null)
        {
            gunScript = GetComponentInParent<Gun>();
        }
    }

    private void OnEnable()
    {
        gunScript.AdjustReloadtime(-reloadTimeChange);
        gunScript.AdjustRecoil(-xRecoilChange, -yRecoilChange, -zRecoilChange);
        gunScript.AdjustAimspeed(adsTimeChange);
    }

    private void OnDisable()
    {
        gunScript.AdjustReloadtime(reloadTimeChange);
        gunScript.ResetRecoils();
        gunScript.ResetAimSpeed();
    }

}
