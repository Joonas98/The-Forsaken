using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int damage;
    [Tooltip("Attack CD when hit")] [SerializeField] private float attackCooldown;
    [Tooltip("Attack CD when missed")] [SerializeField] private float swingCooldown;
    [SerializeField] private float despawnTime;
    [SerializeField] private float despawnDistance;
    [SerializeField] private float runDistance;
    [SerializeField] private float attackDistance;
    [SerializeField] private int moneyReward;
    [SerializeField] private float crawlingSpeedMultiplier;
    //  [SerializeField] private GameObject Torso;
    // [SerializeField] NavMeshAgent navMeshAgent;

    private int currentHealth, healthPercentage;
    private string healthString;
    public bool isDead = false;
    private GameObject[] BodyParts;
    private GameObject player;
    private float distanceToPlayer, velocity;
    private Animator animator;
    private float ogMovementSpeed;

    // private EnemyNav navScript;
    // private NavMeshAgent navAgent;
    [SerializeField] private Pathfinding.AIDestinationSetter destSetter;
    [SerializeField] private Pathfinding.RichAI richAI;

    public List<Collider> RagdollParts = new List<Collider>();
    public Rigidbody[] RigidBodies;
    public Collider[] Damagers;
    public Collider enemyCollider;

    private bool canAttack = true, canSwing = true, ragdolling = false;
    // private bool stoppedRunning = false;
    public bool isCrawling = false;

    public GameObject eyeRight, eyeLeft;
    public GameObject modelRoot;
    public Rigidbody bodyRB; // Rigidbody in waist or something (used for ragdoll magnitude checks etc.)
    public float standUpMagnitude, standUpDelay;
    private float countdown = 0f;
    private bool standCountdowActive = false;

    private Material eyeMaterialR, eyeMaterialL;
    public Color newEyeColor;
    public Gradient eyeGradient;

    public GameObject damagePopupText;
    public Transform popupTransform;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] takeDamageSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] randomSounds;

    [Header("Debug information")]
    public TextMeshProUGUI debugVelocityTextfield;

    private void Awake()
    {
        player = GameObject.Find("Player");

        RigidBodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        SetRagdollParts();

        bodyRB = modelRoot.GetComponent<Rigidbody>();

        eyeMaterialR = eyeRight.GetComponent<Renderer>().material;
        eyeMaterialL = eyeLeft.GetComponent<Renderer>().material;

        float randomScaling = Random.Range(1.0f, 1.2f); // Default scale is 1.1
        transform.localScale = new Vector3(randomScaling, randomScaling, randomScaling);

        // navScript = GetComponent<EnemyNav>();
        // navAgent = GetComponent<NavMeshAgent>();
        ogMovementSpeed = richAI.maxSpeed;
    }

    private void Start()
    {
        SetRagdollParts();
        currentHealth = maxHealth;
        healthString = currentHealth + " / " + maxHealth;

        destSetter.target = player.transform;
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        animator.SetFloat("Velocity", richAI.velocity.magnitude);
        HandleSwinging();
        CheckRagdollMagnitude();
        DebugUpdate();
    }

    private void CheckRagdollMagnitude()
    {
        // When magnitude has been low enough for certain time, stand up
        if (bodyRB.velocity.magnitude < standUpMagnitude && ragdolling && !isDead && !standCountdowActive)
        {
            countdown = Time.time;
            standCountdowActive = true;
        }
        else if (bodyRB.velocity.magnitude > standUpMagnitude && ragdolling && !isDead)
        {
            standCountdowActive = false;
        }

        if (Time.time > countdown + standUpDelay && ragdolling && !isDead && bodyRB.velocity.magnitude < standUpMagnitude)
        {
            TurnOffRagdoll();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        GameManager.GM.enemyCount--;
        GameManager.GM.UpdateEnemyCount();
        GameManager.GM.AdjustMoney(moneyReward);

        enemyCollider.enabled = false;

        TurnOnRagdoll();

        Destroy(gameObject, despawnTime);

        destSetter.target = null;
        richAI.maxSpeed = 0;

        richAI.enabled = false;
        destSetter.enabled = false;

        // navScript.enabled = false;
        // navAgent.enabled = false;
    }

    public void Despawn() // Not in use
    {
        GameManager.GM.enemyCount--;
        GameManager.GM.UpdateEnemyCount();

        Destroy(gameObject, 0f);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthString = currentHealth + " / " + maxHealth;

        healthPercentage = (100 / maxHealth) * currentHealth;

        UpdateEyeColor();

        if (!isDead) DamagePopup(damage);

        if (!isDead && Random.Range(0, 3) == 1)
        {
            int rindex = Random.Range(0, takeDamageSounds.Length);
            audioSource.PlayOneShot(takeDamageSounds[rindex]);
        }

        if (currentHealth <= 0)
        {
            if (isDead == false)
            {
                Die();
            }
        }
    }

    public void TakeDamagePercentage(int flatAmount, int percentageAmount)
    {
        var hpPercentage = (100 / maxHealth) * percentageAmount;
        TakeDamage(hpPercentage + flatAmount);
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    private void SetRagdollParts()
    {
        Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
        {
            if (c.gameObject != this.gameObject)
            {
                c.isTrigger = true;
                RagdollParts.Add(c);
            }

        }

        enemyCollider.isTrigger = false;

    }

    public void TurnOnRagdoll()
    {
        if (ragdolling) return;

        foreach (Rigidbody rb in RigidBodies)
        {
            rb.isKinematic = false;
        }
        ragdolling = true;
        standCountdowActive = false;

        animator.enabled = false;
        foreach (Collider c in RagdollParts)
        {
            c.isTrigger = false;
        }

        // navScript.enabled = false;
        // navAgent.isStopped = true;
        // navAgent.enabled = false;
    }

    public void TurnOffRagdoll()
    {
        if (isDead) return;

        foreach (Rigidbody rb in RigidBodies)
        {
            rb.isKinematic = true;
        }
        ragdolling = false;

        transform.position = modelRoot.transform.position; //Enemy GO does not move with ragdoll, so do that when stop ragdoll

        // Animator stuff
        animator.enabled = true;
        if (!isCrawling)
        {
            StartCoroutine(StandupDelay());
            animator.Play("Stand up");
        }
        else
        {
            // navAgent.isStopped = false;
            animator.Play("Base Blend Tree Crawl");
        }

        foreach (Collider c in RagdollParts)
        {
            c.isTrigger = true;
        }

        // navScript.enabled = true;
        // if (!navScript.IsAgentOnNavMesh(gameObject)) navScript.MoveToNavMesh();
        //
        // navAgent.enabled = true;
        // navAgent.isStopped = true; // Stop navAgent to wait standup animation to play
    }

    public void Attack(Player playerScript)
    {
        if (isDead || ragdolling) return;
        if (canAttack)
        {
            playerScript.TakeDamage(damage);
            canAttack = false;
            StartCoroutine(AttackCooldown());
        }
    }

    public void HandleSwinging()
    {
        if (isDead || ragdolling) return;
        if (distanceToPlayer < attackDistance && canSwing) // Try to attack
        {
            if (!isCrawling)
            {
                if (Random.value < 0.5f)
                {
                    if (Random.Range(0, 3) == 1)
                    {
                        int raIndex = Random.Range(0, attackSounds.Length);
                        audioSource.PlayOneShot(attackSounds[raIndex]);
                    }
                    animator.Play("Attack");
                    canSwing = false;
                    StartCoroutine(WaitSwing(swingCooldown));
                }
                else
                {
                    if (Random.Range(0, 3) == 1)
                    {
                        int raIndex = Random.Range(0, attackSounds.Length);
                        audioSource.PlayOneShot(attackSounds[raIndex]);
                    }
                    animator.Play("Attack Mirrored");
                    canSwing = false;
                    StartCoroutine(WaitSwing(swingCooldown));
                }
            }
            else
            {
                if (Random.value < 0.5f)
                {
                    if (Random.Range(0, 3) == 1)
                    {
                        int raIndex = Random.Range(0, attackSounds.Length);
                        audioSource.PlayOneShot(attackSounds[raIndex]);
                    }
                    animator.Play("Attack Crawl");
                    canSwing = false;
                    StartCoroutine(WaitSwing(swingCooldown));
                }
                else
                {
                    if (Random.Range(0, 3) == 1)
                    {
                        int raIndex = Random.Range(0, attackSounds.Length);
                        audioSource.PlayOneShot(attackSounds[raIndex]);
                    }
                    animator.Play("Attack Crawl Mirrored");
                    canSwing = false;
                    StartCoroutine(WaitSwing(swingCooldown));
                }
            }
        }
    }

    IEnumerator WaitSwing(float time)
    {
        yield return new WaitForSeconds(time);
        canSwing = true;
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator StandupDelay()
    {
        yield return new WaitForSeconds(2f);
        // if (!navScript.IsAgentOnNavMesh(gameObject)) navScript.MoveToNavMesh();
        // if (!isDead) navAgent.isStopped = false;
    }

    public void StartCrawling()
    {
        isCrawling = true;
        animator.Play("Start Crawling");
        SlowDown(crawlingSpeedMultiplier);
        // navAgent.stoppingDistance = 1.75f;
    }

    public void UpdateEyeColor() // Enemy HP% can be seen from the eye color
    {
        eyeMaterialR.SetColor("_EmissionColor", eyeGradient.Evaluate(healthPercentage / 100f) * Mathf.Pow(2, 2));
        eyeMaterialL.SetColor("_EmissionColor", eyeGradient.Evaluate(healthPercentage / 100f) * Mathf.Pow(2, 2));
    }

    public void SlowDown(float slowMultiplier)
    {
        // navAgent.speed = navAgent.speed * slowMultiplier;
        richAI.maxSpeed = richAI.maxSpeed * slowMultiplier;
    }

    public void RestoreMovementSpeed()
    {
        // if (!isCrawling)
        // {
        //     navAgent.speed = ogMovementSpeed;
        // }
        // else
        // {
        //     navAgent.speed = ogMovementSpeed * crawlingSpeedMultiplier;
        // }

        if (!isCrawling)
        {
            richAI.maxSpeed = ogMovementSpeed;
        }
        else
        {
            richAI.maxSpeed = ogMovementSpeed * crawlingSpeedMultiplier;
        }
    }

    public void DamagePopup(int number)
    {
        GameObject dmgPopupText = Instantiate(damagePopupText, popupTransform.position, Quaternion.identity);
        Rigidbody popRB = dmgPopupText.GetComponent<Rigidbody>();
        popRB.AddForce(new Vector3(UnityEngine.Random.Range(-1f, 1f), 1, UnityEngine.Random.Range(-1f, 1f)) * 150f);
        TextMeshPro popText = dmgPopupText.GetComponent<TextMeshPro>();
        popText.text = number.ToString();
        Destroy(dmgPopupText.gameObject, 2f);
    }

    public void DebugUpdate()
    {
        debugVelocityTextfield.text = richAI.velocity.magnitude.ToString("F2");
    }

}
