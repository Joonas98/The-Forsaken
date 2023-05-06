using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Tooltip("Variables")]
    public float speed, runningSpeed, slopeSpeed, gravity, jumpHeight;
    public AudioClip[] jumpSounds;

    [Tooltip("Other stuff")]
    public AudioSource audioSource;
    public CharacterController controller;
    public LayerMask groundmask;

    public Transform groundCheck;
    public float groundDistance; // Radius of sphere that checks if player is grounded
    public GameObject mainCamera;
    public GameObject fallingSymbol;
    public Animator camAnimator, weaponHolsterAnimator;

    [HideInInspector] public bool isStationary;
    [HideInInspector] public float ogSpeed;

    // Private stuff
    private GameObject runningSymbol;
    private Vector3 velocity;
    private Vector3 moveDirection;
    [HideInInspector] public bool isGrounded, isRunning;
    private Vector3 lastPosition = new Vector3(0, 0, 0);

    [HideInInspector] public bool canRun = true;

    // Sliding thingy
    private Vector3 hitPointNormal;

    // Headbob
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    private float defaultYPos = 0;
    private float bobTimer;

    private bool isSliding
    {
        get
        {
            if (/*controller.isGrounded && */ Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
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
    }

    void Update()
    {
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

        weaponHolsterAnimator.SetBool("Stationary", isStationary);
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
        if (Input.GetKey(KeyCode.LeftShift) && Time.timeScale > 0 && controller.velocity.magnitude >= 5f && canRun) //(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            Run(true);
        }
        else if (!Input.GetKey(KeyCode.LeftShift) && isRunning)
        {
            Run(false);
        }
    }

    public void Run(bool run)
    {
        if (run)
        {
            isRunning = true;
            speed = runningSpeed;
            runningSymbol.SetActive(true);
        }
        else
        {
            isRunning = false;
            speed = ogSpeed;
            runningSymbol.SetActive(false);
        }
    }

    private void HandleHeadbob() // From video: https://www.youtube.com/watch?v=_c5IoF1op4E
    {
        if (!isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            bobTimer += Time.deltaTime * (isRunning ? sprintBobSpeed : walkBobSpeed);
            mainCamera.transform.localPosition = new Vector3(
                mainCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(bobTimer) * (isRunning ? sprintBobAmount : walkBobAmount),
                mainCamera.transform.localPosition.z);
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

}
