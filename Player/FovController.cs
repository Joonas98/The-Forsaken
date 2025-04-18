using System.Collections;
using UnityEngine;

public class FovController : MonoBehaviour
{
	[HideInInspector] public static FovController Instance { get; private set; }
	public PlayerMovement playerMovement;
	public Camera mainCamera;
	public Camera weaponCamera; // Camera that renders weapons
	public float fovDefault, fovSprint, fovLerpSpeed; // e.g., 60f, 90f, 5f 
	[HideInInspector] public float fovAim; // Varies by weapon

	private bool isRunning = false;
	private float fovCurrent, fovTarget;

	// Temporary offset added during a pulse
	private float pulseOffset = 0f;
	private Coroutine pulseCoroutine;

	private void Awake()
	{
		// Singleton
		if (Instance == null)
		{
			// Persistence not required for this script
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		fovCurrent = fovDefault;
		fovTarget = fovDefault;
	}

	private void Update()
	{
		// Update running and aiming states
		isRunning = playerMovement.isRunning;

		// Determine the base target FOV based on player state
		float baseFov;
		if (isRunning)
		{
			baseFov = fovSprint;
		}
		else if (GameManager.GM.CurrentGunAiming())
		{
			baseFov = fovAim;
		}
		else
		{
			baseFov = fovDefault;
		}

		// Add the pulse offset (if any) to the base FOV
		fovTarget = baseFov + pulseOffset;

		// Smoothly interpolate current FOV towards the target FOV
		fovCurrent = Mathf.Lerp(fovCurrent, fovTarget, fovLerpSpeed * Time.deltaTime);
		mainCamera.fieldOfView = fovCurrent;
		weaponCamera.fieldOfView = fovCurrent;
	}

	/// <summary>
	/// Triggers a pulse effect on the camera's FOV.
	/// The effect smoothly adds an offset (specified by amount) over rampUpDuration,
	/// then smoothly returns it back over rampDownDuration.
	/// </summary>
	/// <param name="amount">The FOV offset to be applied during the pulse.</param>
	/// <param name="rampUpDuration">Duration to reach the target offset.</param>
	/// <param name="rampDownDuration">Duration to return the offset to zero.</param>
	public void PulseFov(float amount, float rampUpDuration, float rampDownDuration)
	{
		// Reduced amount of pulse if the player is aiming
		if (GameManager.GM.CurrentGunAiming()) amount *= GameManager.GM.currentGun.zoomAmount;

		// If a pulse is already running, stop it to avoid overlaps.
		if (pulseCoroutine != null)
		{
			StopCoroutine(pulseCoroutine);
		}
		pulseCoroutine = StartCoroutine(PulseFovRoutine(amount, rampUpDuration, rampDownDuration));
	}

	private IEnumerator PulseFovRoutine(float amount, float rampUpDuration, float rampDownDuration)
	{
		float elapsed = 0f;

		// Ramp up the pulse offset to the specified amount
		while (elapsed < rampUpDuration)
		{
			elapsed += Time.deltaTime;
			pulseOffset = Mathf.Lerp(0f, amount, elapsed / rampUpDuration);
			yield return null;
		}
		pulseOffset = amount;

		// Ramp down the pulse offset back to 0
		elapsed = 0f;
		while (elapsed < rampDownDuration)
		{
			elapsed += Time.deltaTime;
			pulseOffset = Mathf.Lerp(amount, 0f, elapsed / rampDownDuration);
			yield return null;
		}
		pulseOffset = 0f;
		pulseCoroutine = null;
	}
}
