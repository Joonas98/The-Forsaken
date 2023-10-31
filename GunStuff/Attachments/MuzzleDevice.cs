using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleDevice : AttachmentBase
{
	[Header("Muzzle Device Settings")]
	public ParticleSystem muzzleEffect;
	public GameObject muzzleTip;
	public bool isSilencer; // Useful for disabling muzzle light effect and other applications in future like possible abilities

	private Gun gunScript;

	[Header("Stat changes")]
	public float reloadTimeMultiplier;
	public float adsTimeMultiplier, equipTimeMultiplier;
	public float xRecoilMultiplier, yRecoilMultiplier, zRecoilMultiplier;

	private void OnValidate()
	{
		if (gunScript == null) gunScript = GetComponentInParent<Gun>();
	}

	private void Awake()
	{
		if (gunScript == null) gunScript = GetComponentInParent<Gun>();
	}

	private void OnEnable()
	{
		if (muzzleTip != null) gunScript.gunTip = muzzleTip.transform;
		if (muzzleEffect != null) gunScript.muzzleFlash = muzzleEffect;

		gunScript.AdjustRecoil(xRecoilMultiplier, yRecoilMultiplier, zRecoilMultiplier);
		gunScript.AdjustReloadtime(reloadTimeMultiplier);
		gunScript.AdjustAimspeed(adsTimeMultiplier);

		if (isSilencer) gunScript.isSilenced = true;
		else gunScript.isSilenced = false;
	}

	private void OnDisable()
	{
		gunScript.ResetGunTip();
		gunScript.ResetRecoils();
		gunScript.isSilenced = false; // Default is not silenced
	}

}
