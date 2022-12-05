using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{

    [Header("Player Settings")]
    public int currentHealth;
    public int maxHealth;
    public int regenationAmount;
    public float regenationDelay;
    public float regenationDelayAfterDamage;

    public float sensitivity;
    public float movementSpeed;
    public float sprintingMovementSpeed;
    public float jumpForce;
    public float sprintingJumpForce;

    public float maxBloom;
    public float maxVignette;
    public float maxChromaticAberration;
    public float maxGrain;

    [Header("Other Stuff")]
    private Vector3 PlayerMovementInput;
    private Vector2 PlayerMouseInput;
    private float xRot;

    public LayerMask FloorMask;
    public Rigidbody PlayerRigidBody;
    public Transform PlayerCamera;
    public Transform Feet;


    //  public float fallMultiplier;
    //  public float lowJumpMultiplier;
    [HideInInspector] public float originalJumpForce;
    [HideInInspector] public float originalMovementSpeed;

    public bool isRunning = false;
    public bool isGrounded;
    public bool canRotate = true;

    public GameObject runningSymbol;
    public GameObject feetCollider;
    public TextMeshProUGUI healthText;
    public Slider healthSlider;
    private string healthString;

    private Vector3 spawnLocation;
    private bool regenerating = true;
    private float currentHPPercentage = 100f;
    private float currentPPPercentage;

    public IdleSway idleSwayScript;

    public DamagePP damagePPScript;

    public Animator animator;

    [Header("Audio")] // Missähän vitussa noi hyppyäänet käsitellään?
    public AudioSource playerAS;
    public AudioClip[] damageGrunts;
    public AudioClip regenSound;

    private void Awake()
    {
        spawnLocation = this.transform.position;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        runningSymbol = GameObject.Find("RunningSymbol");

        if (runningSymbol != null)
            runningSymbol.SetActive(false);

        originalMovementSpeed = movementSpeed;
        originalJumpForce = jumpForce;
    }

    void Update()
    {
        if (animator != null) animator.SetFloat("Velocity", PlayerRigidBody.velocity.magnitude);

        // PlayerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        // PlayerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // MovePlayerCamera(); // Tehdään mouselook scriptissä nykyään
        // HandleRunAndJump();

        // if (Input.GetKeyDown(KeyCode.X) && Time.timeScale > 0) // Palataan aloitus sijaintiin
        // {
        //     // this.transform.position = new Vector3(0, 50, 0);
        //     transform.position = new Vector3(spawnLocation.x, spawnLocation.y, spawnLocation.z);
        // }

        calculateRegen();
    }


    private void HandleRunAndJump()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Time.timeScale > 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            movementSpeed = sprintingMovementSpeed;
            jumpForce = sprintingJumpForce;
            isRunning = true;
            idleSwayScript.running = true;
            runningSymbol.SetActive(true);
        }
        else
        {
            movementSpeed = originalMovementSpeed;
            jumpForce = originalJumpForce;
            isRunning = false;
            idleSwayScript.running = false;
            runningSymbol.SetActive(false);
        }

        if (Input.GetButton("Jump") && isGrounded)
        {
            PlayerRigidBody.velocity = Vector3.up * jumpForce;
        }

        if (Physics.CheckSphere(Feet.position, 0.1f, FloorMask))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }


    private void FixedUpdate()
    {
        //  MovePlayer();
    }


    private void MovePlayer()
    {
        Vector3 MoveVector = transform.TransformDirection(PlayerMovementInput) * movementSpeed;
        PlayerRigidBody.velocity = new Vector3(MoveVector.x, PlayerRigidBody.velocity.y, MoveVector.z);

        // Alla jaloissa oleva collider joka aktivoidaan paikalla ollessa liukumisen estämiseksi
        // Suhteellisen kehno ratkaisu
        //  if (PlayerMovementInput == new Vector3(0f, 0f, 0f))
        //  {
        //      feetCollider.SetActive(true);
        //  }
        //  else
        //  {
        //      feetCollider.SetActive(false);
        //  }

        // Better Jump
        // if (PlayerRigidBody.velocity.y < 0)
        // {
        //     PlayerRigidBody.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        // }
        // else if (PlayerRigidBody.velocity.y > 0 && !Input.GetButton("Jump") && Time.timeScale > 0)
        // {
        //     PlayerRigidBody.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        // }

    }

    private void MovePlayerCamera()
    {
        if (canRotate == true) // canRotate taitaa olla, ettei käänny kun menut on auki
        {
            xRot -= PlayerMouseInput.y * sensitivity;

            transform.Rotate(0f, PlayerMouseInput.x * sensitivity, 0f);
            PlayerCamera.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);


            // Estetään liiallinen kääntyminen ylös ja alas
            //  if (xRot > 90f)
            //  {
            //      xRot = 90f;
            //      PlayerCamera.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            //  }
            //
            //  if (xRot < -90f)
            //  {
            //      xRot = -90f;
            //      PlayerCamera.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            //  }

        }
    }

    public void ToggleCameraRotation(bool boolean)
    {
        canRotate = boolean;
    }

    public bool CheckGrounded()
    {
        return isGrounded;
    }

    public void TakeDamage(int amount)
    {
        regenerating = false;
        StopAllCoroutines();

        if (currentHealth - amount <= 0) currentHealth = 0;
        else currentHealth -= amount;

        StartCoroutine(regenerate());
        currentHPPercentage = currentHealth * 1.0f / maxHealth * 1.0f * 100f; // 80 / 100 = 0.8
        healthString = currentHPPercentage.ToString() + "%";
        healthText.text = healthString;
        healthSlider.value = currentHPPercentage / 100;

        currentHPPercentage = 1f - (currentHPPercentage / 100f); // 1 - 0.8 = 0.2
        damagePPScript.UpdateDamagePP(maxBloom * currentHPPercentage, maxVignette * currentHPPercentage, maxChromaticAberration * currentHPPercentage, maxGrain * currentHPPercentage);

        if (currentHealth > 0)
        {
            int rindex = Random.Range(0, damageGrunts.Length);
            playerAS.PlayOneShot(damageGrunts[rindex]);
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += amount;
            currentHPPercentage = currentHealth * 1.0f / maxHealth * 1.0f * 100f; // 80 / 100 = 0.8
            healthString = currentHPPercentage.ToString() + "%";
            healthText.text = healthString;
            healthSlider.value = currentHPPercentage / 100;

            currentHPPercentage = 1f - (currentHPPercentage / 100f); // 1 - 0.8 = 0.2
            damagePPScript.UpdateDamagePP(maxBloom * currentHPPercentage, maxVignette * currentHPPercentage, maxChromaticAberration * currentHPPercentage, maxGrain * currentHPPercentage);

            if (currentHealth >= maxHealth)
                currentHealth = maxHealth;
        }
    }

    public void calculateRegen()
    {
        if (regenerating)
        {
            regenerating = false;
            StartCoroutine(regenerationDelay());
        }
    }

    IEnumerator regenerationDelay()
    {
        yield return new WaitForSeconds(regenationDelay);
        Heal(regenationAmount);
        regenerating = true;
    }

    IEnumerator regenerate()
    {
        yield return new WaitForSeconds(regenationDelayAfterDamage);
        playerAS.PlayOneShot(regenSound, 2f);
        regenerating = true;
    }

}
