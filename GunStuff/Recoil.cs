using TMPro;
using UnityEngine;

public class Recoil : MonoBehaviour
{
	[HideInInspector] public static Recoil Instance;
	public float flinchX, flinchY, flinchZ; // Flinch = take damage
	public float kickFlinch; // How much we recoil when kicking
	public float snappiness;
	public float returnSpeed;
	public Transform playerTrans;
	public float recoilMultiplier;
	public PlayerMovement movementScript;

	private Vector3 currentRotation;
	private Vector3 targetRotation;
	[SerializeField] private TextMeshProUGUI recXText, recYText, recZText, snappinessText, returnSpeedText, recMultiplierText;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	void Update()
	{
		// Always lerp recoil back to zero.
		// If the player is firing, you'd increment targetRotation somewhere else (e.g., in Fire()).
		targetRotation = Vector3.Lerp(
			targetRotation,
			Vector3.zero,
			returnSpeed * Time.deltaTime
		);

		// Smoothly apply recoil over time
		currentRotation = Vector3.Slerp(
			currentRotation,
			targetRotation,
			snappiness * Time.fixedDeltaTime
		);
		transform.localRotation = Quaternion.Euler(currentRotation);
	}


	private void LateUpdate()
	{
		// 29.6.2023 To be honest, not sure what this was here for?!?
		// playerTrans.rotation = new Quaternion(playerTrans.rotation.x, transform.localRotation.y + playerTrans.rotation.y, playerTrans.rotation.z, playerTrans.rotation.w);
	}

	public void RecoilFire()
	{
		if (!movementScript.isGrounded) // Mid air
		{
			recoilMultiplier = GameManager.GM.currentGun.rec1;
		}
		else if (movementScript.isGrounded && !movementScript.isStationary && !GameManager.GM.CurrentGunAiming()) // Grounded, moving, not aiming
		{
			recoilMultiplier = GameManager.GM.currentGun.rec2;
		}
		else if (movementScript.isGrounded && !movementScript.isStationary && GameManager.GM.CurrentGunAiming()) // Grounded, moving, aiming
		{
			recoilMultiplier = GameManager.GM.currentGun.rec3;
		}
		else if (movementScript.isGrounded && movementScript.isStationary && !GameManager.GM.CurrentGunAiming()) // Grounded, not moving, not aiming
		{
			recoilMultiplier = GameManager.GM.currentGun.rec4;
		}
		else if (movementScript.isGrounded && movementScript.isStationary && GameManager.GM.CurrentGunAiming())  // Grounded, not moving, aiming
		{
			recoilMultiplier = GameManager.GM.currentGun.rec5;
		}
		else // Nothing above, probably never used
		{
			recoilMultiplier = GameManager.GM.currentGun.rec6;
		}

		// This is the recoil itself
		targetRotation += new Vector3(-GameManager.GM.currentGun.recoil.x,
			Random.Range(-GameManager.GM.currentGun.recoil.y * 0.1f, GameManager.GM.currentGun.recoil.y * 0.1f),
			Random.Range(-GameManager.GM.currentGun.recoil.z, GameManager.GM.currentGun.recoil.z)) * recoilMultiplier;
	}

	// Flinch from taking damage
	// Some variables like return speed are still determined by the held weapon
	public void DamageFlinch(float flinchMultiplier)
	{
		targetRotation += new Vector3(Random.Range(-flinchX, flinchX), Random.Range(-flinchY, flinchY), Random.Range(-flinchZ, flinchZ)) * flinchMultiplier;
	}

	// Flinch camera when player kicks
	public void KickFlinch()
	{
		targetRotation += new Vector3(-kickFlinch, Random.Range(-kickFlinch * 0.5f, kickFlinch * 0.5f), Random.Range(-kickFlinch * 0.5f, kickFlinch * 0.5f));
	}
}
