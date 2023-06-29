using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Variables")]
    public float speed, runningSpeed, slopeSpeed, gravity, jumpHeight;
    public AudioClip[] jumpSounds;
    private float runThreshold = 5f; // If controller.velocity.magnitude is less than this, can't be running

    [Tooltip("Other stuff")]
    public AudioSource audioSource;
    public CharacterController controller;
    public LayerMask groundmask;

    public Transform legHud; // The HUD elements at the legs need to be moved with headbob
    public float initialYOffset; // Variable for removing leg HUD bouncing
    public Transform groundCheck;
    public float groundDistance; // Radius of sphere that checks if player is grounded
    public GameObject mainCamera;
    public GameObject fallingSymbol;

    [HideInInspector] public bool isStationary;
    [HideInInspector] public bool canRun = true;
    [HideInInspector] public bool isGrounded, isRunning;
    [HideInInspector] public bool isSlowed;
    [HideInInspector] public float ogSpeed;

    // Private stuff
    private GameObject runningSymbol;
    private Vector3 velocity;
    private Vector3 moveDirection;
    private Vector3 lastPosition = new Vector3(0, 0, 0);

    // Important for cystom sliding system
    private Vector3 hitPointNormal;

    // Headbob
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float bobReturnSpeed = 0.25f; // How fast to return -> 0, 0, 0 when not bobbing
    private float defaultYPos = 0;
    private float bobTimer;

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
        runningSymbol = GameObject.Find("RunningSymbol");
        if (runningSymbol != null) runningSymbol.SetActive(false);
        defaultYPos = mainCamera.transform.localPosition.y;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        canRun = true;
        initialYOffset = legHud.position.y - mainCamera.transform.position.y;
    }

    void Update()
    {
        if (speed >= ogSpeed) isSlowed = false;
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
        Debug.Log("Is running: " + isRunning);
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
        }

        controller.Move(moveDirection * speed * Time.deltaTime);
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

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
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

    public void Run(bool run)
    {
        if (!run || !canRun)
        {
            isRunning = false;
            if (!isSlowed) speed = ogSpeed;
            runningSymbol.SetActive(false);
        }
        else
        {
            isRunning = true;
            speed = runningSpeed;
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

    public IEnumerator TemporarySpeedChange(float multiplier, float duration)
    {
        if (multiplier < 1f) canRun = false;
        speed = speed * multiplier;
        yield return new WaitForSeconds(duration);
        if (!isSlowed) canRun = true;
        // Movement speed is returned to normal in Run()
    }

}
