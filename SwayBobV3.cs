using UnityEngine;

public class SwayBobV3 : MonoBehaviour
{
	[Header("References")]
	public Transform playerTransform; // Player's root transform
	public Transform cameraTransform; // Player's camera transform

	[Header("Sway Settings")]
	public float swayStrength = 0.02f;
	public float swaySmoothness = 10f;

	[Header("Bob Settings")]
	public float bobExaggeration = 1.5f; // Overall bob strength
	public Vector3 bobLimits = new Vector3(0.025f, 0.01f, 0.025f); // Movement bob limits
	public Vector3 travelLimits = new Vector3(0.025f, 0, 0.025f); // Player movement travel limits
	public float bobFrequency = 1.5f; // Oscillation speed
	public float bobSmoothing = 8f;

	[Header("Tilt Settings")]
	public float tiltStrength = 2f; // Controls how much the weapon tilts
	public float tiltSmoothing = 8f; // Controls how smoothly the tilt is applied
	private Vector3 tiltOffset;

	private Vector3 swayOffset;
	private Vector3 bobOffset;
	private Vector3 finalOffset;
	private Vector3 previousPosition;
	private Quaternion previousRotation;
	private Vector3 smoothedVelocity;
	private Vector3 smoothedRotationDelta;



	private float speedCurve; // Used for sinusoidal bob calculations

	private void Start()
	{
		if (playerTransform != null)
			previousPosition = playerTransform.position;

		if (cameraTransform != null)
			previousRotation = cameraTransform.rotation;
	}

	private void Update()
	{
		if (Time.timeScale == 0) return; // Avoid updates when paused

		UpdateMovementVariables();
		UpdateRotationVariables();
		ApplySway();
		ApplyBob();
		ApplyTilt();
		CombineEffects();
	}

	private void UpdateMovementVariables()
	{
		if (playerTransform == null) return;

		// Calculate velocity
		float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f); // Avoiding errors when deltatime is too small after pausing
		Vector3 velocity = (playerTransform.position - previousPosition) / deltaTime;

		smoothedVelocity = Vector3.Lerp(smoothedVelocity, velocity, Time.deltaTime * swaySmoothness);
		previousPosition = playerTransform.position;
	}

	private void UpdateRotationVariables()
	{
		if (cameraTransform == null) return;

		// Calculate rotation deltas
		Quaternion currentRotation = cameraTransform.rotation;
		Vector3 rotationDelta = new Vector3(
			Mathf.DeltaAngle(previousRotation.eulerAngles.x, currentRotation.eulerAngles.x),
			Mathf.DeltaAngle(previousRotation.eulerAngles.y, currentRotation.eulerAngles.y),
			Mathf.DeltaAngle(previousRotation.eulerAngles.z, currentRotation.eulerAngles.z)
		);

		// Smooth rotation deltas
		smoothedRotationDelta = Vector3.Lerp(smoothedRotationDelta, rotationDelta, Time.deltaTime * swaySmoothness);
		previousRotation = currentRotation;
	}

	private void ApplySway()
	{
		// Sway from camera rotation and player velocity
		swayOffset = new Vector3(
			-smoothedRotationDelta.y * swayStrength, // Horizontal rotation sway
			-smoothedVelocity.y * swayStrength,     // Vertical movement sway
			0
		);
	}

	private void ApplyBob()
	{
		// Update speed curve for sinusoidal calculations
		speedCurve += Time.deltaTime * bobFrequency * smoothedVelocity.magnitude;

		// Calculate movement bob offsets
		bobOffset.x = Mathf.Cos(speedCurve) * bobLimits.x - smoothedVelocity.x * travelLimits.x;
		bobOffset.y = Mathf.Abs(Mathf.Sin(speedCurve)) * bobLimits.y; // Breathing-like motion
		bobOffset.z = -smoothedVelocity.z * travelLimits.z;
	}

	private void ApplyTilt()
	{
		if (playerTransform == null) return;

		// Convert smoothed velocity to local space relative to the player
		Vector3 localVelocity = playerTransform.InverseTransformDirection(smoothedVelocity);

		// Use the local X-axis velocity for lateral movement
		float lateralVelocity = localVelocity.x;

		// Calculate tilt amount based on lateral velocity
		float tiltAmount = lateralVelocity * tiltStrength;

		// Apply tilt around the Z-axis (roll)
		tiltOffset = new Vector3(0, 0, -tiltAmount);
	}

	private void CombineEffects()
	{
		// When aiming, there should be no sway and bob 
		if (GameManager.GM.currentGunAiming)
		{
			finalOffset = Vector3.zero;
			tiltOffset = Vector3.zero;
		}
		else
		{
			// Combine sway and bob offsets into a final offset
			finalOffset = swayOffset + bobOffset;
		}

		// Smoothly apply the final offset to the weapon's local position
		transform.localPosition = Vector3.Lerp(transform.localPosition, finalOffset, Time.deltaTime * bobSmoothing);

		// Smoothly apply the tilt to the weapon's local rotation
		transform.localRotation = Quaternion.Lerp(
			transform.localRotation,
			Quaternion.Euler(tiltOffset),
			Time.deltaTime * tiltSmoothing
		);
	}
}
