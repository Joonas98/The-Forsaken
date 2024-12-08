using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public static PlayerMovement instance;

	[Header("Variables")]
	public float walkingSpeed;
	private float _currentSpeed; // Backing field
	public float CurrentSpeed => _currentSpeed; // Read-only property
	public float runningSpeed, slopeSpeed, gravity, jumpHeight;
	private float runThreshold = 5f; // If controller.velocity.magnitude is less than this, can't be running

	[Header("Audio")]
	public AudioSource audioSource;
	public AudioClip[] jumpSounds;

	[Header("Headbob")]
	[SerializeField] private AnimationCurve bobAmountCurve = AnimationCurve.Linear(0, 0, 1, 1); // Default linear
	[SerializeField] private AnimationCurve bobSpeedCurve = AnimationCurve.Linear(0, 1, 1, 2);  // Default linear

	[SerializeField] private float bobReturnSpeed = 0.25f; // How fast to return -> 0, 0, 0 when not bobbing
	[SerializeField] private float bobMaxSpeed = 15f; // What movement speed gives the max of bob curve
	private float defaultYPos = 0;
	private float bobTimer;

	[Header("Other stuff")]
	public CharacterController controller;
	public Transform legHud; // The HUD elements at the legs need to be moved with headbob
	public LayerMask groundmask;
	public Transform groundCheck;
	[Tooltip("Radius of ground check sphere")] public float groundDistance;
	public GameObject mainCamera;
	public GameObject fallingSymbol;

	[HideInInspector] public bool isStationary;
	[HideInInspector] public bool canRun = true;
	[HideInInspector] public bool isGrounded, isRunning;
	[HideInInspector] public bool isSlowed;
	[HideInInspector] public float ogSpeed, ogRunningspeed;

	// Private stuff
	private GameObject runningSymbol;
	private float initialYOffset; // Variable for removing leg HUD bouncing
	private Vector3 velocity;
	private Vector3 moveDirection;
	private Vector3 lastPosition = new Vector3(0, 0, 0);
	private float timeOnSlope;

	private struct MovementSpeedEffect
	{
		public float speedDelta;    // Percentage of speed change (e.g., 0.25 for 25%)
		public float duration;       // Remaining duration of the speed effect
	}

	private MovementSpeedEffect[] movementSpeedEffects;

	// Important for custom sliding system
	private Vector3 hitPointNormal;

	// Custom slide system
	public float slideDelay; // Be this time on slope to start sliding

	private bool isSlideDelayOver => timeOnSlope >= slideDelay;

	private bool isSliding
	{
		get
		{
			if (isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f, groundmask))
			{
				hitPointNormal = slopeHit.normal;

				if (Vector3.Angle(hitPointNormal, Vector3.up) > controller.slopeLimit)
				{
					// Check if the slide delay is over
					if (isSlideDelayOver)
					{
						return true;
					}
					else
					{
						// Increment the time on the slope
						timeOnSlope += Time.deltaTime;
						return false;
					}
				}
			}

			// Reset the time on the slope if not on a slope
			timeOnSlope = 0f;
			return false;
		}
	}

	private void Awake()
	{
		instance = this;

		ogSpeed = walkingSpeed;
		ogRunningspeed = runningSpeed;
		runningSymbol = GameObject.Find("RunningSymbol");
		if (runningSymbol != null) runningSymbol.SetActive(false);
		defaultYPos = mainCamera.transform.localPosition.y;
	}

	private void Start()
	{
		canRun = true;
		//initialYOffset = legHud.position.y - mainCamera.transform.position.y; // Needed if leg hud is used
		movementSpeedEffects = new MovementSpeedEffect[9]; // Maximum of 9 slow effects should be enough 
	}

	void Update()
	{
		CalculateSpeedEffects();
		HandleRunning();
		HandleJump();
		HandleMovement();
		HandleHeadbob();
		FallingSymbol();

		if (lastPosition != transform.position) isStationary = false;
		else isStationary = true;

		if (Time.timeScale == 0)
		{
			_currentSpeed = 0f; // Reset when paused
			return;
		}

		// Calculate movement speed
		float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
		_currentSpeed = (transform.position - lastPosition).magnitude / deltaTime;

		lastPosition = transform.position;
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(300, 300, 80, 20), CurrentSpeed.ToString());
	}

	private void HandleMovement()
	{
		float x, z;

		x = Input.GetAxis("Horizontal");
		z = Input.GetAxis("Vertical");

		if (!isSliding)
		{
			// Combine input directions
			moveDirection = transform.right * x + transform.forward * z;

			// Normalize the direction vector to prevent faster diagonal movement
			if (moveDirection.magnitude > 1f)
				moveDirection = moveDirection.normalized;
		}
		else
		{
			moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
			controller.Move(slopeSpeed * Time.deltaTime * moveDirection);
			goto skipMovement;
		}

		// Apply movement speed
		if (!isRunning)
		{
			controller.Move(walkingSpeed * Time.deltaTime * moveDirection);
		}
		else
		{
			controller.Move(runningSpeed * Time.deltaTime * moveDirection);
		}

	skipMovement:;
	}

	private void HandleJump()
	{
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundmask);

		if (isGrounded)
		{
			// Only allow jumping if not sliding on a steep slope
			if (!isSliding || Vector3.Angle(Vector3.up, hitPointNormal) <= controller.slopeLimit)
			{
				if (Input.GetButtonDown("Jump"))
				{
					velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
					int raIndex = Random.Range(0, jumpSounds.Length);
					audioSource.PlayOneShot(jumpSounds[raIndex]);
				}
			}

			// Apply gravity when grounded
			if (velocity.y < 0)
			{
				velocity.y = -50f;
			}
		}

		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}

	private void HandleRunning()
	{
		if (Input.GetKey(KeyCode.LeftShift) && Time.timeScale > 0 && canRun)
		{
			if (controller.velocity.magnitude >= runThreshold)
			{
				Run(true);
			}
			else
			{
				Run(false);
			}
		}
		else if (!Input.GetKey(KeyCode.LeftShift) && isRunning)
		{
			Run(false);
		}
	}

	private void CalculateSpeedEffects()
	{
		float cumulativeSpeedPercentage = 1.0f;

		for (int i = 0; i < movementSpeedEffects.Length; i++)
		{
			if (movementSpeedEffects[i].duration > 0.0f)
			{
				// Reduce the duration of the speed effect
				movementSpeedEffects[i].duration -= Time.deltaTime;

				// Determine whether it's a speed increase or decrease
				float speedModifier = 1.0f;
				if (movementSpeedEffects[i].speedDelta > 0.0f)
				{
					// It's a speed increase
					speedModifier = movementSpeedEffects[i].speedDelta;
				}
				else if (movementSpeedEffects[i].speedDelta < 0.0f)
				{
					// It's a speed decrease
					speedModifier = 1.0f - Mathf.Abs(movementSpeedEffects[i].speedDelta);
				}

				// Apply the speed modifier to the cumulativeSpeedPercentage
				cumulativeSpeedPercentage *= speedModifier;
			}
		}

		// Calculate the effective movement speed
		walkingSpeed = ogSpeed * cumulativeSpeedPercentage;
		runningSpeed = ogRunningspeed * cumulativeSpeedPercentage;

		if (walkingSpeed < 0f) walkingSpeed = 0f;
		if (runningSpeed < 0f) runningSpeed = 0f;
	}


	public void Run(bool run)
	{
		if (!run || !canRun)
		{
			isRunning = false;
			runningSymbol.SetActive(false);
		}
		else
		{
			isRunning = true;
			runningSymbol.SetActive(true);
		}
	}

	private void HandleHeadbob()
	{
		if (Time.timeScale == 0.0f) return;

		// Use a clamped deltaTime to avoid NaN errors
		float deltaTime = Mathf.Max(Time.deltaTime, 0.001f);

		// Disable bobbing and return to original position if stationary or mid-air
		if (!isGrounded || CurrentSpeed < 0.1f)
		{
			mainCamera.transform.localPosition = Vector3.MoveTowards(
				mainCamera.transform.localPosition,
				new Vector3(0, defaultYPos, 0),
				deltaTime * bobReturnSpeed
			);
			bobTimer = 0; // Reset bob timer when stationary
		}
		else
		{
			// Use movement speed directly for bob speed and amount
			float normalizedSpeed = Mathf.Clamp01(CurrentSpeed / bobMaxSpeed); // Normalize speed between 0 and 1
			float dynamicBobSpeed = bobSpeedCurve.Evaluate(normalizedSpeed); // Evaluate speed curve
			float dynamicBobAmount = bobAmountCurve.Evaluate(normalizedSpeed); // Evaluate amount curve

			// Update the bob timer based on dynamic speed
			bobTimer += deltaTime * dynamicBobSpeed;

			// Apply the bobbing effect
			mainCamera.transform.localPosition = new Vector3(
				mainCamera.transform.localPosition.x,
				defaultYPos + Mathf.Sin(bobTimer) * dynamicBobAmount,
				mainCamera.transform.localPosition.z
			);
		}
	}

	private void FallingSymbol()
	{
		if (!isGrounded)
		{
			fallingSymbol.SetActive(true);
		}
		else
		{
			fallingSymbol.SetActive(false);
		}
	}

	public void ApplySpeedEffect(float multiplier, float duration)
	{
		// Find an available slot in the effects sources array
		for (int i = 0; i < movementSpeedEffects.Length; i++)
		{
			if (movementSpeedEffects[i].duration <= 0.0f)
			{
				// Set the effect source information
				movementSpeedEffects[i].speedDelta = multiplier;
				movementSpeedEffects[i].duration = duration;

				// Exit the loop
				break;
			}
		}
	}

	public void Dash(float dashSpeed, float dashDuration)
	{

	}
}
