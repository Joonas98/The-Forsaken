using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grip : AttachmentBase
{
	[Header("Grip Settings")]
	public float reloadTimeMultiplier, adsTimeMultiplier, equipTimeMultiplier;
	public float xRecoilMultiplier, yRecoilMultiplier, zRecoilMultiplier;

	private Gun gunScript;

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
		gunScript.AdjustReloadtime(reloadTimeMultiplier);
		gunScript.AdjustRecoil(xRecoilMultiplier, yRecoilMultiplier, zRecoilMultiplier);
		gunScript.AdjustAimspeed(adsTimeMultiplier);
	}

	private void OnDisable()
	{
		gunScript.ResetReloadtime();
		gunScript.ResetRecoils();
		gunScript.ResetAimSpeed();
	}

}
