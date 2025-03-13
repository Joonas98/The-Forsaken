using UnityEngine;

// New universal sway and weapon bob system 8.5.2023
// Meant to replace WeaponSway and IdleSway scripts / systems.
// Original base for this system from this video: https://www.youtube.com/watch?v=DR4fTllQnXg
public class WeaponSwayAndBob : MonoBehaviour
{
	[Header("Important")]
	public static WeaponSwayAndBob instance;
	public PlayerMovement mover;
	public float returnSpeed;
	public Transform playerCameraTransform; // Assign this to the player's camera

	[Header("Enable Components")]
	public bool disableSwayBob;
	public bool swayOffset;
	public bool swayRotation;
	public bool bobOffset;
	public bool bobRotation;

	[Header("Sway")]
	public float step = 0.01f; // Multiplied by the value from the mouse for 1 frame
	public float maxStepDistance = 0.06f; // Max distance from the local origin
	Vector3 swayPos; // Store our value for later

	[Header("Sway Rotation")]
	public float rotationStep = 4f;
	public float maxRotationStep = 5f;
	Vector3 swayEulerRot;
	public float smooth = 5f; // Used for BobOffset and Sway
	public float smoothRot = 12f; // Used for BobSway and TiltSway

	[Header("Bobbing")]
	public float speedCurve; // Used by both bobbing types
	float curveSin { get => Mathf.Sin(speedCurve); }
	float curveCos { get => Mathf.Cos(speedCurve); }

	public Vector3 travelLimit = Vector3.one * 0.025f; // Max limits of travel from movement
	public Vector3 bobLimit = Vector3.one * 0.025f; // Limit travel from bobbing over time
	Vector3 bobPosition;

	public float bobExaggeration;

	[Header("Bob Rotation")]
	public Vector3 multiplier;
	public Vector3 runningMultiplier;

	private Vector3 defaultMultiplier;
	private Vector3 bobEulerRotation;

	[Header("Clipping prevention")]
	public float maxDistance;
	public float offsetDistance;

	// Important privates for multiple functions
	private Vector3 previousPosition;
	private Vector3 previousRotation;
	private Vector2 walkInput;
	private Vector2 lookInput;
	private float inputMagnitude;
	private float verticalMovement;


	private void Awake()
	{
		instance = this;
		defaultMultiplier = multiplier;
		previousPosition = mover.transform.position;
	}

	void Update()
	{
		GetInput();
		// When choosing grenades or objects, mouse is used for selection -> sway during selection is annoying bug
		if (disableSwayBob || ObjectPlacing.instance.isPlacing || SelectionCanvas.instance.isChoosingObject)
		{
			swayPos = Vector3.zero;
			bobPosition = Vector3.zero;
			swayEulerRot = Vector3.zero;
			bobEulerRotation = Vector3.zero;
		}
		else
		{
			if (swayOffset) SwayOffset();
			if (swayRotation) SwayRotation();
			if (bobOffset) BobOffset();
			if (bobRotation) BobRotation();
		}

		// Handle running
		if (mover.isRunning) multiplier = runningMultiplier;
		else multiplier = defaultMultiplier;

		// Do the lerps themselves at the end
		CompositePositionRotation();
	}

	void GetInput()
	{
		// Get movement input
		walkInput.x = Input.GetAxis("Horizontal");
		walkInput.y = Input.GetAxis("Vertical");
		walkInput = walkInput.normalized;
		inputMagnitude = walkInput.magnitude;

		// Y velocity from rigibody
		verticalMovement = (transform.position - previousPosition).y; // Calculate the vertical movement based on position changes
		previousPosition = transform.position;

		// This fixes diagonal movement BobOffset
		//if (inputMagnitude > 1f)
		//{
		//	walkInput /= inputMagnitude;
		//}

		// Get mouse movement input
		lookInput.x = Input.GetAxis("Mouse X");
		lookInput.y = Input.GetAxis("Mouse Y");
	}

	void SwayOffset() // Player rotation -> position change
	{
		// Calculate the difference in player rotation between frames
		Vector3 rotationDifference = playerCameraTransform.eulerAngles - previousRotation;

		// Normalize and apply the step and maxStepDistance as before
		Vector3 adjustedRotation = new Vector3(-rotationDifference.y, -rotationDifference.x, 0) * step;
		adjustedRotation.x = Mathf.Clamp(adjustedRotation.x, -maxStepDistance, maxStepDistance);
		adjustedRotation.y = Mathf.Clamp(adjustedRotation.y, -maxStepDistance, maxStepDistance);

		swayPos = adjustedRotation;

		// Store current rotation for the next frame comparison
		previousRotation = playerCameraTransform.eulerAngles;
	}

	void SwayRotation() // Mouse movement -> rotation change (roll, pitch, yaw)
	{
		Vector2 invertLook = lookInput * -rotationStep;
		invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
		invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
		swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
	}

	void BobOffset() // Player movemet -> position change
	{
		speedCurve += Time.deltaTime * (mover.isGrounded ? inputMagnitude * bobExaggeration : 1f) + 0.01f;

		bobPosition.x = (curveCos * bobLimit.x) - (walkInput.x * travelLimit.x);
		bobPosition.y = bobLimit.y * verticalMovement * 5f;
		bobPosition.z = -(walkInput.y * travelLimit.z);
	}

	void BobRotation() // Player movement -> rotation change (roll, pitch, yaw)
	{
		bobEulerRotation.x = (walkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 2));
		bobEulerRotation.y = (walkInput != Vector2.zero ? multiplier.y * curveCos : 0);
		bobEulerRotation.z = (walkInput != Vector2.zero ? multiplier.z * curveCos * walkInput.x : 0);
	}

	void CompositePositionRotation()
	{
		if (disableSwayBob || (GameManager.GM.currentGun != null && GameManager.GM.currentGunAiming))
		{
			// Lerp towards zero
			transform.SetLocalPositionAndRotation(Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * smooth),
			Quaternion.Lerp(transform.localRotation, Quaternion.Euler(Vector3.zero) * Quaternion.Euler(bobEulerRotation), Time.deltaTime * smoothRot));
		}
		else
		{
			// Apply sway
			transform.SetLocalPositionAndRotation(Vector3.Lerp(transform.localPosition, swayPos + bobPosition, Time.deltaTime * smooth),
			Quaternion.Lerp(transform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation), Time.deltaTime * smoothRot));
		}
	}
}
