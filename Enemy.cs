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

    [SerializeField] private TextMeshPro healthText;
    //  [SerializeField] private GameObject Torso;
    // [SerializeField] NavMeshAgent navMeshAgent;

    private int currentHealth, healthPercentage;
    private string healthString;
    public bool isDead = false;
    private GameObject[] BodyParts;
    private GameObject player;
    private float distanceToPlayer, velocity;

    private EnemyNav navScript;
    // private Rigidbody RB;
    private Animator animator;
    private NavMeshAgent navAgent;
    private float ogMovementSpeed;

    public List<Collider> RagdollParts = new List<Collider>();
    public Rigidbody[] RigidBodies;
    public Collider[] Damagers;
    public Collider enemyCollider;

    private bool canAttack = true, canSwing = true;
    // private bool stoppedRunning = false;
    public bool isCrawling = false;

    public GameObject eyeRight, eyeLeft;
    private Material eyeMaterialR, eyeMaterialL;

    public Color newEyeColor;
    public Gradient eyeGradient;

    public GameObject damagePopupText;
    public Transform popupTransform;
    private Vector3 newPosition; // Position where to warp navmeshagent when disabling ragdoll mode

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] takeDamageSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] randomSounds;

    private void Awake()
    {
        RigidBodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        SetRagdollParts();
        navScript = GetComponent<EnemyNav>();
        navAgent = GetComponent<NavMeshAgent>();
        ogMovementSpeed = navAgent.speed;

        eyeMaterialR = eyeRight.GetComponent<Renderer>().material;
        eyeMaterialL = eyeLeft.GetComponent<Renderer>().material;

        float randomScaling = Random.Range(1.0f, 1.2f); // Default scale is 1.1
        transform.localScale = new Vector3(randomScaling, randomScaling, randomScaling);
    }

    private void Start()
    {
        SetRagdollParts();
        player = GameObject.Find("Player");
        currentHealth = maxHealth;
        healthString = currentHealth + " / " + maxHealth;
        healthText.text = healthString;
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        velocity = navAgent.velocity.magnitude / navAgent.speed;
        animator.SetFloat("Velocity", velocity);

        HandleSwinging();
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

        navScript.enabled = false;
        navAgent.enabled = false;

        if (healthText != null)
            Destroy(healthText.gameObject);

        Destroy(gameObject, despawnTime);
        // StartCoroutine(DespawnSequence());
    }

    public void Despawn() // Not in use
    {
        GameManager.GM.enemyCount--;
        GameManager.GM.UpdateEnemyCount();

        if (healthText != null)
            Destroy(healthText.gameObject);

        Destroy(gameObject, 0f);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthString = currentHealth + " / " + maxHealth;
        healthText.text = healthString;

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
        foreach (Rigidbody rb in RigidBodies)
        {
            rb.isKinematic = false;
        }

        animator.enabled = false;
        navScript.enabled = false;

        navAgent.enabled = false;
        // navAgent.isStopped = true;
        // navAgent.updatePosition = false;

        foreach (Collider c in RagdollParts)
        {
            c.isTrigger = false;
        }
    }

    public void TurnOffRagdoll()
    {
        if (isDead) return;

        foreach (Rigidbody rb in RigidBodies)
        {
            rb.isKinematic = true;
        }

        navScript.enabled = true;
        navScript.MoveToNavMesh();

        navAgent.enabled = true;

        animator.enabled = true;
        foreach (Collider c in RagdollParts)
        {
            c.isTrigger = true;
        }
    }

    public void Attack(Player playerScript)
    {
        if (isDead) return;
        if (canAttack)
        {
            playerScript.TakeDamage(damage);
            canAttack = false;
            StartCoroutine(AttackCooldown());
        }
    }

    public void HandleSwinging()
    {
        if (isDead) return;
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

    IEnumerator WaitSeconds(float time)
    {
        yield return new WaitForSeconds(time);
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

    public void StartCrawling()
    {
        isCrawling = true;
        animator.Play("Start Crawling");
        SlowDown(crawlingSpeedMultiplier);
        navAgent.stoppingDistance = 1.75f;
        // attackDistance = attackDistance * 0.5f;
    }

    public void UpdateEyeColor() // Enemy HP% can be seen from the eye color
    {
        eyeMaterialR.SetColor("_EmissionColor", eyeGradient.Evaluate(healthPercentage / 100f) * Mathf.Pow(2, 2));
        eyeMaterialL.SetColor("_EmissionColor", eyeGradient.Evaluate(healthPercentage / 100f) * Mathf.Pow(2, 2));
    }

    public void SlowDown(float slowMultiplier)
    {
        navAgent.speed = navAgent.speed * slowMultiplier;
    }

    public void RestoreMovementSpeed()
    {
        if (!isCrawling)
        {
            navAgent.speed = ogMovementSpeed;
        }
        else
        {
            navAgent.speed = ogMovementSpeed * crawlingSpeedMultiplier;
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

}
