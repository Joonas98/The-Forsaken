using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleDevice : MonoBehaviour
{

    public Gun gunScript;
    public ParticleSystem muzzleEffect;
    public GameObject muzzleTip;
    public AudioClip muzzleSound;

    [Header("Stat changes")]
    public float reloadTimeChange;
    public float adsTimeChange, equipTimeChange;
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
        if (muzzleTip != null) gunScript.gunTip = muzzleTip;
        if (muzzleEffect != null) gunScript.muzzleFlash = muzzleEffect;
        if (muzzleSound != null) gunScript.shootSound = muzzleSound;

        if (xRecoilChange != 0 && yRecoilChange != 0 && zRecoilChange != 0)
            gunScript.AdjustRecoil(-xRecoilChange, -yRecoilChange, -zRecoilChange);
    }

    private void OnDisable()
    {
        gunScript.ResetGunTip();
        gunScript.ResetRecoils();
    }

}
