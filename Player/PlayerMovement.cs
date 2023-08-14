using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Variables")]
    public float speed;
    public float runningSpeed, slopeSpeed, gravity, jumpHeight;
    private float runThreshold = 5f; // If controller.velocity.magnitude is less than this, can't be running

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] jumpSounds;

    [Header("Headbob")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float bobReturnSpeed = 0.25f; // How fast to return -> 0, 0, 0 when not bobbing
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

    private struct SlowEffect
    {
        public float slowPercentage;    // Percentage of slow (e.g., 0.25 for 25%)
        public float duration;              // Remaining duration of the slow effect
    }

    private SlowEffect[] slowEffects;

    // Important for custom sliding system
    private Vector3 hitPointNormal;

    // Custom slope sliding system
    private bool isSliding
    {
        get
        {
            if (isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f, groundmask))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > controller.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    private void Awake()
    {
        ogSpeed = speed;
        ogRunningspeed = runningSpeed;
        runningSymbol = GameObject.Find("RunningSymbol");
        if (runningSymbol != null) runningSymbol.SetActive(false);
        defaultYPos = mainCamera.transform.localPosition.y;
    }

    private void Start()
    {
        canRun = true;
        initialYOffset = legHud.position.y - mainCamera.transform.position.y;
        slowEffects = new SlowEffect[9]; // Maximum of 9 slow effects should be enough 
    }

    void Update()
    {
        CalculateSlows();
        HandleRunning();
        HandleJump();
        HandleMovement();
        HandleHeadbob();
        FallingSymbol();

        if (lastPosition != transform.position)
        {
            isStationary = false;
        }
        else
        {
            isStationary = true;
        }
        lastPosition = transform.position;
    }

    private void OnGUI()
    {
        // GUI.Label(new Rect(300, 300, 80, 20), speed.ToString());
    }

    private void HandleMovement()
    {
        float x, z;

        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        if (!isSliding)
        {
            moveDirection = transform.right * x + transform.forward * z;
        }
        else
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
            controller.Move(slopeSpeed * Time.deltaTime * moveDirection);
            goto skipMovement;
        }

        if (!isRunning)
        {
            controller.Move(speed * Time.deltaTime * moveDirection);
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

        if (Input.GetButtonDown("Jump") && isGrounded && !isSliding)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            int raIndex = Random.Range(0, jumpSounds.Length);
            audioSource.PlayOneShot(jumpSounds[raIndex]);
        }

        if (isGrounded && velocity.y < 0 && !isSliding)
        {
            velocity.y = -50f;
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

    private void CalculateSlows()
    {
        float cumulativeSlowPercentage = 1.0f;

        for (int i = 0; i < slowEffects.Length; i++)
        {
            if (slowEffects[i].duration > 0.0f)
            {
                // Reduce the duration of the slow effect
                slowEffects[i].duration -= Time.deltaTime;

                // Apply the slow percentage to the cumulativeSlowPercentage
                cumulativeSlowPercentage *= (1.0f - slowEffects[i].slowPercentage);
            }
        }

        // Calculate the effective movement speed
        speed = ogSpeed * cumulativeSlowPercentage;
        runningSpeed = ogRunningspeed * cumulativeSlowPercentage;

        if (speed < 0f) speed = 0f;
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

    private void HandleHeadbob() // Base from video: https://www.youtube.com/watch?v=_c5IoF1op4E
    {
        if (!isGrounded || isStationary) // Disable sway, return to original position if stationary or mid-air
        {
            mainCamera.transform.localPosition =
                 Vector3.MoveTowards(mainCamera.transform.localPosition, new Vector3(0, 0, 0), Time.deltaTime * bobReturnSpeed);
            bobTimer = 0;
        }
        else if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            bobTimer += Time.deltaTime * (isRunning ? sprintBobSpeed : walkBobSpeed);
            mainCamera.transform.localPosition = new Vector3(
                mainCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(bobTimer) * (isRunning ? sprintBobAmount : walkBobAmount),
                mainCamera.transform.localPosition.z);
        }

        // Leg HUD needs to be adjusted too, or it's bouncing annoyingly
        float newY = mainCamera.transform.position.y + initialYOffset;
        Vector3 newPosition = new Vector3(legHud.position.x, newY, legHud.position.z);
        legHud.position = newPosition;
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

    public void ApplySlowEffect(float slowPercentage, float duration)
    {
        // Find an available slot in the slow sources array
        for (int i = 0; i < slowEffects.Length; i++)
        {
            if (slowEffects[i].duration <= 0.0f)
            {
                // Set the slow source information
                slowEffects[i].slowPercentage = slowPercentage;
                slowEffects[i].duration = duration;

                // Exit the loop
                break;
            }
        }
    }

}
