using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using DamageNumbersPro;

public class Enemy : MonoBehaviour
{
    [Header("Basic variables")]
    public int moneyReward;

    [Header("Health variables")]
    public int maxHealth;
    [HideInInspector] public int currentHealth, healthPercentage;
    [HideInInspector] public bool isDead = false;

    private int[] limbHealths = new int[9];
    private float limbMinHealthPercentage = 0.2f;
    private float limbMaxHealthPercentage = 0.5f;

    [Header("Attack variables")]
    public int damage;
    [Tooltip("Attack CD when hit")] public float attackCooldown;
    [Tooltip("Attack CD when missed")] public float swingCooldown;
    [Tooltip("Delay after dying to removal")] public float despawnTime;
    [Tooltip("How close to player to start swinging")] public float attackDistance;

    [Header("References")]
    public List<Collider> ragdollParts = new List<Collider>();
    public Collider[] damagers;
    public Collider enemyCollider;
    public GameObject[] zombieSkins;
    public GameObject eyeRight, eyeLeft;
    public GameObject modelRoot; // Hips
    public Rigidbody[] rigidbodies;
    public Rigidbody bodyRB; // Rigidbody in waist or something (used for ragdoll magnitude checks etc.)
    public Animator animator;

    private GameObject player;

    [Header("Navigation and movement")]
    public EnemyNav enemyNavScript;
    public NavMeshAgent navAgent;
    public float standUpMagnitude, standUpDelay;
    public float movementSpeed;
    [HideInInspector] public bool isCrawling = false;

    [SerializeField] private float crawlingSpeedMultiplier;
    private float ogMovementSpeed;

    [Header("Damage Popup")]
    public Transform popupTransform;
    public DamageNumber dpopNormalPrefab;
    public Color normalColor, headshotColor, healingColor;

    [Header("Effects")]
    public Gradient eyeGradient;
    public GameObject[] effectList;
    public enum debuffs
    {
        Arcane, Crimson, Dark, Fairy, Fire, Frost, Holy, Light, Mist, Nature, ShockBlue, ShockYellow,
        Universe, Void, Water, Wind
    }
    public Material originalMaterial;

    private Material eyeMaterialR, eyeMaterialL;
    private SkinnedMeshRenderer smr;
    private Material newMaterial;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] takeDamageSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] randomSounds;

    [Header("Debug information")]
    public TextMeshProUGUI debugVelocityTextfield;
    public TextMeshProUGUI debugAnimatorVelocity;

    // Various privates
    private bool canAttack = true, canSwing = true, ragdolling = false;
    private bool standCountdownActive = false;
    private float countdown = 0f;
    private float distanceToPlayer;

    // System to handle slows
    private struct SlowEffect
    {
        public float slowPercentage;
        public float duration;
    }

    private SlowEffect[] slowEffects;
    private int barricadeCount = 0; // Count if we are inside one or more barricades (to apply slow while inside 1 or more)

    private void Awake()
    {
        GameManager.GM.enemiesAlive.Add(this);
        GameManager.GM.enemiesAliveGos.Add(gameObject);

        player = GameObject.Find("Player");
        rigidbodies = GetComponentsInChildren<Rigidbody>();

        SetRagdollParts();
        RandomizeSkins();
        SetLimbHealth();

        float randomScaling = Random.Range(1.0f, 1.2f); // Default scale is 1.1
        transform.localScale = new Vector3(randomScaling, randomScaling, randomScaling);
        currentHealth = maxHealth;
        navAgent.speed = movementSpeed;
        ogMovementSpeed = navAgent.speed;

        if (!GameManager.GM.useEnemyDebug)
        {
            Destroy(debugVelocityTextfield.GetComponentInParent<Canvas>().gameObject);
        }
    }

    private void Start()
    {
        slowEffects = new SlowEffect[5]; // Maximum of 5 slow effects should be enough 
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        animator.SetFloat("Velocity", navAgent.velocity.magnitude / navAgent.speed);
        CalculateSlows();
        HandleSwinging();
        CheckRagdollMagnitude();
        DebugUpdate();
    }

    private void SetLimbHealth()
    {
        for (int i = 0; i < limbHealths.Length; i++)
        {
            if (i == 0)
            {
                limbHealths[i] = maxHealth;
            }
            else
            {
                float healthPercentage = Random.Range(limbMinHealthPercentage, limbMaxHealthPercentage);
                int finalLimbHealth = Mathf.RoundToInt(maxHealth * healthPercentage);
                limbHealths[i] = finalLimbHealth;
            }
        }
    }

    private void CheckRagdollMagnitude()
    {
        // When magnitude has been low enough for certain time, stand up
        if (bodyRB.velocity.magnitude < standUpMagnitude && ragdolling && !isDead && !standCountdownActive)
        {
            countdown = Time.time;
            standCountdownActive = true;
        }
        else if (bodyRB.velocity.magnitude > standUpMagnitude && ragdolling && !isDead)
        {
            standCountdownActive = false;
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
        enemyCollider.enabled = false;

        TurnOnRagdoll();
        Destroy(gameObject, despawnTime);

        navAgent.speed = 0;
        navAgent.isStopped = true;
        enemyNavScript.enabled = false;
        navAgent.enabled = false;

        bodyRB.velocity = new Vector3(0, 0, 0);
        foreach (Rigidbody rb in rigidbodies) // Otherwise gameobjects keep moving forever
        {
            rb.velocity = new Vector3(0, 0, 0);
        }

        GameManager.GM.enemyCount--;
        GameManager.GM.UpdateEnemyCount();
        GameManager.GM.AdjustMoney(moneyReward);
        GameManager.GM.ConfirmKillFX();
        GameManager.GM.enemiesAlive.Remove(this);
        GameManager.GM.enemiesAliveGos.Remove(gameObject);
    }

    public void Despawn() // Not in use
    {
        GameManager.GM.enemyCount--;
        GameManager.GM.UpdateEnemyCount();

        Destroy(gameObject, 0f);
    }

    public void HandlePopup(int number, bool headshot)
    {
        if (isDead) return;
        if (number == 0) return;

        if (dpopNormalPrefab == null) return;
        DamageNumber damageNumber = dpopNormalPrefab.Spawn(popupTransform.position, number);
        // damageNumber.SetFollowedTarget(popupTransform);

        if (number < 0) damageNumber.SetColor(healingColor);
        else if (headshot) damageNumber.SetColor(headshotColor);
        else damageNumber.SetColor(normalColor);
    }

    // The actual damage processing, should be always called via TakeDamage() functions
    public void TakeDamage(int damage, int percentageAmount = 0, bool headshot = false)
    {
        // If optional percentageAmount was given, add % based damage to the actual damage
        if (percentageAmount > 0)
        {
            var hpPercentage = (100 / maxHealth) * percentageAmount;
            damage = damage + hpPercentage;
        }
        HandlePopup(damage, headshot);
        currentHealth -= damage;
        healthPercentage = (100 / maxHealth) * currentHealth;
        newMaterial.SetFloat("_BloodAmount", 1f);
        UpdateEyeColor(); // Enemy health can be seen from eyes

        if (!isDead && Random.Range(0, 3) == 1)
        {
            int rindex = Random.Range(0, takeDamageSounds.Length);
            audioSource.PlayOneShot(takeDamageSounds[rindex]);
        }

        if (currentHealth <= 0 && !isDead)
        {
            // Todo: add death sound?
            Die();
        }
    }

    public void DamageLimb(int limbIndex, int damage)
    {
        limbHealths[limbIndex] -= damage;
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    public int GetHealth(int limbIndex)
    {
        return limbHealths[limbIndex];
    }

    public IEnumerator ApplyDebuff(debuffs debuffenum, float duration)
    {
        effectList[(int)debuffenum].SetActive(true); // FX
        yield return new WaitForSeconds(duration);
        RemoveDebuff(debuffenum);
    }

    public void RemoveDebuff(debuffs debuffenum)
    {
        effectList[(int)debuffenum].SetActive(false); // FX
    }

    private void SetRagdollParts()
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
        {
            if (c.gameObject != this.gameObject)
            {
                c.isTrigger = true;
                ragdollParts.Add(c);
            }

        }
    }

    public void TurnOnRagdoll()
    {
        if (ragdolling) return;
        ragdolling = true;

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }

        navAgent.isStopped = true;
        standCountdownActive = false;
        animator.enabled = false;

        foreach (Collider c in ragdollParts)
        {
            c.isTrigger = false;
        }
    }

    public void TurnOffRagdoll()
    {
        if (isDead) return;

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }

        ragdolling = false;
        transform.position = modelRoot.transform.position; //Enemy GO does not move with ragdoll, so do that when stop ragdoll
        animator.enabled = true;
        if (!isCrawling)
        {
            animator.Play("Stand up");
            Invoke("ContinueAfterRagdoll", 2f);
        }
        else
        {
            animator.Play("Base Blend Tree Crawl");
            Invoke("ContinueAfterRagdoll", 1f);
        }

        foreach (Collider c in ragdollParts)
        {
            c.isTrigger = true;
        }
    }

    private void ContinueAfterRagdoll()
    {
        if (enemyNavScript.IsAgentOnNavMesh(gameObject) == false) enemyNavScript.MoveToNavMesh();
        navAgent.isStopped = false;
    }

    public void Attack(Player playerScript)
    {
        if (isDead || ragdolling) return;
        if (canAttack)
        {
            playerScript.TakeDamage(damage);
            canAttack = false;
            PlayerMovement playerMovement = playerScript.GetComponent<PlayerMovement>();
            // playerMovement.StartCoroutine(playerMovement.TemporarySpeedChange(0.25f, 0.5f));
            playerMovement.ApplySlowEffect(0.50f, 0.5f);
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

    public void StartCrawling()
    {
        // When any of part of the leg is destroyed, the zombie becomes a crawler
        isCrawling = true;
        animator.Play("Start Crawling");

        // Crawlers never can restore legs, so the ogMovementSpeed can be adjusted to avoid problems
        ogMovementSpeed *= crawlingSpeedMultiplier;
        navAgent.speed = ogMovementSpeed;

        // Prevent clipping through the ground
        navAgent.baseOffset = 0.05f;
    }

    private void CalculateSlows()
    {
        float cumulativeSlowPercentage = 1.0f;
        float barricadeSlow = barricadeCount >= 1 ? 0.8f : 0.0f; // Apply barricade slow if in at least 1 barricade

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

        // Apply barricade slow to the cumulativeSlowPercentage
        cumulativeSlowPercentage *= (1.0f - barricadeSlow);

        // Calculate the effective movement speed
        movementSpeed = ogMovementSpeed * cumulativeSlowPercentage;
        if (movementSpeed < 0f) movementSpeed = 0f;

        navAgent.speed = movementSpeed;
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

    public void EnterBarricade()
    {
        barricadeCount++;
    }

    public void ExitBarricade()
    {
        barricadeCount--;
        if (barricadeCount < 0)
        {
            barricadeCount = 0;
        }
    }

    public void UpdateEyeColor() // Enemy HP% can be seen from the eye color
    {
        eyeMaterialR.SetColor("_EmissionColor", eyeGradient.Evaluate(healthPercentage / 100f) * Mathf.Pow(2, 2));
        eyeMaterialL.SetColor("_EmissionColor", eyeGradient.Evaluate(healthPercentage / 100f) * Mathf.Pow(2, 2));
    }

    private void RandomizeSkins()
    {
        // 18.6.23 All zombies are 1 prefab and the skin is randomized on awake
        if (zombieSkins.Length > 0)
        {
            int randomIndex = Random.Range(0, zombieSkins.Length);
            for (int i = 0; i < zombieSkins.Length; i++)
            {
                // Set the selected object active and the rest inactive
                zombieSkins[i].SetActive(i == randomIndex);
            }
        }

        // Set materials
        eyeMaterialR = eyeRight.GetComponent<Renderer>().material;
        eyeMaterialL = eyeLeft.GetComponent<Renderer>().material;
        newMaterial = new Material(originalMaterial);
        smr = GetComponentInChildren<SkinnedMeshRenderer>();
        smr.material = newMaterial;
    }

    public void DebugUpdate()
    {
        debugVelocityTextfield.text = navAgent.velocity.magnitude.ToString("F2");
        debugAnimatorVelocity.text = animator.GetFloat("Velocity").ToString();
    }

}
