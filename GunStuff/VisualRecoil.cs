using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualRecoil : MonoBehaviour
{
	private bool aiming;
	private Vector3 currentRotation, targetRotation, targetPosition, currentPosition, gunPositionOG;

	private void Start()
	{
		gunPositionOG = transform.localPosition;
	}

	private void Update()
	{
		if (GameManager.GM.currentGun == null) return;
		aiming = GameManager.GM.currentGunAiming;
		targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, Time.deltaTime * GameManager.GM.currentGun.vireReturn);
		currentRotation = Vector3.Slerp(currentRotation, targetRotation, Time.fixedDeltaTime * GameManager.GM.currentGun.vireSnap);
		transform.localRotation = Quaternion.Euler(currentRotation);
		Back();
	}

	public void Recoil()
	{
		if (aiming)
		{
			targetPosition -= new Vector3(0, 0, GameManager.GM.currentGun.vireKick * 0.2f);
			targetRotation += new Vector3(Random.Range(-GameManager.GM.currentGun.vire.x * 0.35f, GameManager.GM.currentGun.vire.x * 0.35f),
				Random.Range(-GameManager.GM.currentGun.vire.y * 0.35f, GameManager.GM.currentGun.vire.y * 0.35f),
				Random.Range(-GameManager.GM.currentGun.vire.z * 0.35f, GameManager.GM.currentGun.vire.z * 0.35f));
		}
		else
		{
			targetPosition -= new Vector3(0, 0, GameManager.GM.currentGun.vireKick);
			targetRotation += new Vector3(-GameManager.GM.currentGun.vire.x,
				Random.Range(-GameManager.GM.currentGun.vire.y, GameManager.GM.currentGun.vire.y),
				Random.Range(-GameManager.GM.currentGun.vire.z, GameManager.GM.currentGun.vire.z));
		}
	}

	void Back()
	{
		if (aiming)
		{
			targetPosition = Vector3.Lerp(targetPosition, gunPositionOG, Time.deltaTime * GameManager.GM.currentGun.vireReturn * 4);
			currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime * GameManager.GM.currentGun.vireSnap * 2);
			transform.localPosition = currentPosition;
		}
		else
		{
			targetPosition = Vector3.Lerp(targetPosition, gunPositionOG, Time.deltaTime * GameManager.GM.currentGun.vireReturn);
			currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime * GameManager.GM.currentGun.vireSnap);
			transform.localPosition = currentPosition;
		}
	}
}
