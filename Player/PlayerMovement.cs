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

	[Header("Sisu Costs")]
	// Sisu drained per second when running (e.g. 10 means 10 Sisu/second)
	public float sisuDrainPerSecond = 10f;
	// Sisu cost per jump (an integer value)
	public int sisuJumpCost = 15;

	[Header("Audio")]
	public AudioSource audioSource;
	public AudioClip[] jumpSounds;

	[Header("Headbob")]
	[SerializeField] private AnimationCurve bobAmountCurve = AnimationCurve.Linear(0, 0, 1, 1); // Default linear
	[SerializeField] private AnimationCurve bobSpeedCurve = AnimationCurve.Linear(0, 1, 1, 2);  // Default linear
	[SerializeField] private float bobReturnSpeed = 0.25f; // How fast to return when not bobbing
	[SerializeField] private float bobMaxSpeed = 15f; // What movement speed gives the max bob curve
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
	private float initialYOffset; // For removing leg HUD bouncing
	private Vector3 velocity;
	private Vector3 moveDirection;
	private Vector3 lastPosition = Vector3.zero;
	private float timeOnSlope;

	private struct MovementSpeedEffect
	{
		public float speedDelta;    // Percentage of speed change (e.g., 0.25 for 25%)
		public float duration;       // Remaining duration of the speed effect
	}
	private MovementSpeedEffect[] movementSpeedEffects;

	// For custom sliding system
	private Vector3 hitPointNormal;

	// Custom slide system
	public float slideDelay; // Time on slope required to start sliding

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
					if (isSlideDelayOver)
					{
						return true;
					}
					else
					{
						timeOnSlope += Time.deltaTime;
						return false;
					}
				}
			}
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

		isStationary = (lastPosition == transform.position);

		if (Time.timeScale == 0)
		{
			_currentSpeed = 0f;
			return;
		}

		float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
		_currentSpeed = (transform.position - lastPosition).magnitude / deltaTime;
		lastPosition = transform.position;

		if (Input.GetKeyDown(KeyCode.T))
		{
			ApplySpeedEffect(5, 5);
			Player.instance.UpdateSisu(Player.instance.maxSisu);
		}
	}

	private void OnGUI()
	{
		// GUI.Label(new Rect(300, 300, 80, 20), CurrentSpeed.ToString());
	}

	private void HandleMovement()
	{
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		if (!isSliding)
		{
			moveDirection = transform.right * x + transform.forward * z;
			if (moveDirection.magnitude > 1f)
				moveDirection = moveDirection.normalized;
		}
		else
		{
			moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
			controller.Move(slopeSpeed * Time.deltaTime * moveDirection);
			return;
		}

		if (!isRunning)
		{
			controller.Move(walkingSpeed * Time.deltaTime * moveDirection);
		}
		else
		{
			controller.Move(runningSpeed * Time.deltaTime * moveDirection);
		}
	}

	private void HandleJump()
	{
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundmask);

		if (isGrounded)
		{
			// Only jump if there’s enough Sisu available
			if (Input.GetButtonDown("Jump") && Player.instance.currentSisu >= sisuJumpCost)
			{
				// Drain Sisu for jumping
				Player.instance.UpdateSisu(-sisuJumpCost);
				velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
				int raIndex = Random.Range(0, jumpSounds.Length);
				audioSource.PlayOneShot(jumpSounds[raIndex]);
			}
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
		// Conditions common to both starting and continuing running.
		bool shiftHeld = Input.GetKey(KeyCode.LeftShift) && Time.timeScale > 0 && canRun;
		bool sufficientVelocity = controller.velocity.magnitude >= runThreshold;

		if (shiftHeld)
		{
			if (!isRunning)
			{
				// When not running, require at least one second's worth of Sisu to initiate running.
				if (sufficientVelocity && Player.instance.currentSisu >= sisuDrainPerSecond)
				{
					Run(true);
				}
			}
			else
			{
				// Already running: continue as long as there's some Sisu left and velocity remains sufficient.
				if (!sufficientVelocity || Player.instance.currentSisu <= 0)
				{
					Run(false);
				}
			}
		}
		else
		{
			Run(false);
		}

		// Drain Sisu continuously while running.
		if (isRunning)
		{
			float drainAmount = sisuDrainPerSecond * Time.deltaTime;
			Player.instance.currentSisu = Mathf.Max(Player.instance.currentSisu - drainAmount, 0f);
			Player.instance.UpdateSisuUI();

			// If Sisu is fully drained, stop running.
			if (Player.instance.currentSisu <= 0f)
			{
				Run(false);
			}
		}
	}

	public void Run(bool run)
	{
		isRunning = run;
		if (runningSymbol != null)
		{
			runningSymbol.SetActive(run);
		}
	}

	private void CalculateSpeedEffects()
	{
		float cumulativeSpeedPercentage = 1.0f;

		for (int i = 0; i < movementSpeedEffects.Length; i++)
		{
			if (movementSpeedEffects[i].duration > 0.0f)
			{
				movementSpeedEffects[i].duration -= Time.deltaTime;
				float speedModifier = 1.0f;
				if (movementSpeedEffects[i].speedDelta > 0.0f)
				{
					speedModifier = movementSpeedEffects[i].speedDelta;
				}
				else if (movementSpeedEffects[i].speedDelta < 0.0f)
				{
					speedModifier = 1.0f - Mathf.Abs(movementSpeedEffects[i].speedDelta);
				}
				cumulativeSpeedPercentage *= speedModifier;
			}
		}

		walkingSpeed = ogSpeed * cumulativeSpeedPercentage;
		runningSpeed = ogRunningspeed * cumulativeSpeedPercentage;
		if (walkingSpeed < 0f) walkingSpeed = 0f;
		if (runningSpeed < 0f) runningSpeed = 0f;
	}

	private void HandleHeadbob()
	{
		if (Time.timeScale == 0.0f) return;

		float deltaTime = Mathf.Max(Time.deltaTime, 0.001f);

		if (!isGrounded || CurrentSpeed < 0.1f)
		{
			mainCamera.transform.localPosition = Vector3.MoveTowards(
				mainCamera.transform.localPosition,
				new Vector3(0, defaultYPos, 0),
				deltaTime * bobReturnSpeed
			);
			bobTimer = 0;
		}
		else
		{
			float normalizedSpeed = Mathf.Clamp01(CurrentSpeed / bobMaxSpeed);
			float dynamicBobSpeed = bobSpeedCurve.Evaluate(normalizedSpeed);
			float dynamicBobAmount = bobAmountCurve.Evaluate(normalizedSpeed);
			bobTimer += deltaTime * dynamicBobSpeed;
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
		for (int i = 0; i < movementSpeedEffects.Length; i++)
		{
			if (movementSpeedEffects[i].duration <= 0.0f)
			{
				movementSpeedEffects[i].speedDelta = multiplier;
				movementSpeedEffects[i].duration = duration;
				break;
			}
		}
	}

	public void Dash(float dashSpeed, float dashDuration)
	{
		// Implementation for dash, if needed.
	}
}
