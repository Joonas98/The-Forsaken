using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using DamageNumbersPro;

public class Enemy : MonoBehaviour
{
    public GameObject[] zombieSkins;
    public int maxHealth;
    public int damage;
    public float speedType1, speedType2, speedType3;
    [Tooltip("Attack CD when hit")] [SerializeField] private float attackCooldown;
    [Tooltip("Attack CD when missed")] [SerializeField] private float swingCooldown;
    [SerializeField] private float despawnTime, despawnDistance;
    [SerializeField] private float runDistance, attackDistance;
    [SerializeField] private int moneyReward;
    [SerializeField] private float crawlingSpeedMultiplier;

    public int currentHealth, healthPercentage;
    [HideInInspector] public bool isDead = false;
    private GameObject player;
    private float distanceToPlayer;
    private Animator animator;
    private float ogMovementSpeed;

    [SerializeField] private Pathfinding.AIDestinationSetter destSetter;
    [SerializeField] private Pathfinding.RichAI richAI;
    [SerializeField] private Pathfinding.RVO.RVOController rvoController;

    public List<Collider> RagdollParts = new List<Collider>();
    public Rigidbody[] RigidBodies;
    public Collider[] Damagers;
    public Collider enemyCollider;

    private bool canAttack = true, canSwing = true, ragdolling = false;
    // private bool stoppedRunning = false;
    public bool isCrawling = false;

    public GameObject eyeRight, eyeLeft;
    public GameObject modelRoot; // Hips
    public Rigidbody bodyRB; // Rigidbody in waist or something (used for ragdoll magnitude checks etc.)
    public float standUpMagnitude, standUpDelay;
    private float countdown = 0f;
    private bool standCountdowActive = false;

    public GameObject damagePopupText;
    public Transform popupTransform;

    public DamageNumber numberPrefab;

    [Header("Effects")]
    public Color newEyeColor;
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

    private void Awake()
    {
        GameManager.GM.enemiesAlive.Add(this);
        GameManager.GM.enemiesAliveGos.Add(gameObject);

        RandomizeSkins();

        player = GameObject.Find("Player");

        RigidBodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        SetRagdollParts();

        bodyRB = modelRoot.GetComponent<Rigidbody>();

        float randomScaling = Random.Range(0.95f, 1.25f); // Default scale is 1.1
        transform.localScale = new Vector3(randomScaling, randomScaling, randomScaling);

        float animationFloat = UnityEngine.Random.value;
        if (animationFloat < 0.33f)
        {
            animator.Play("Base Blend Tree");
            richAI.maxSpeed = speedType1;
        }
        else if (animationFloat < 0.66f && animationFloat > 0.33f)
        {
            animator.Play("Blend Tree v2");
            richAI.maxSpeed = speedType2;
        }
        else
        {
            animator.Play("Blend Tree v3");
            richAI.maxSpeed = speedType3;
        }

        ogMovementSpeed = richAI.maxSpeed;

        if (!GameManager.GM.useEnemyDebug)
        {
            Destroy(debugVelocityTextfield.GetComponentInParent<Canvas>().gameObject);
        }
    }

    private void Start()
    {
        SetRagdollParts();
        currentHealth = maxHealth;

        destSetter.target = player.transform;
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        animator.SetFloat("Velocity", richAI.velocity.magnitude / richAI.maxSpeed);
        HandleSwinging();
        CheckRagdollMagnitude();
        DebugUpdate();
    }

    private void FixedUpdate()
    {
        // Partial fix to make the enemy GO follow the ragdolling body
        // if (isDead) transform.position = transform.GetChild(0).position;
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
        enemyCollider.enabled = false;
        destSetter.target = null;
        rvoController.enabled = false;
        richAI.maxSpeed = 0;
        richAI.canMove = false;
        richAI.enabled = false;
        destSetter.enabled = false;

        TurnOnRagdoll();
        Destroy(gameObject, despawnTime);

        bodyRB.velocity = new Vector3(0, 0, 0);
        foreach (Rigidbody rb in RigidBodies) // Otherwise gameobjects keep moving forever
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthPercentage = (100 / maxHealth) * currentHealth;

        newMaterial.SetFloat("_BloodAmount", 1f);

        UpdateEyeColor(); // Enemy health can be seen from eyes

        if (!isDead) // Damage popups
        {
            if (damage == 0) return;
            DamageNumber damageNumber = numberPrefab.Spawn(popupTransform.position, damage);
            if (damage < 0) damageNumber.SetColor(Color.green); // Healing
        }

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
        ragdolling = true;

        foreach (Rigidbody rb in RigidBodies)
        {
            rb.isKinematic = false;
        }
        standCountdowActive = false;
        richAI.canMove = false;
        animator.enabled = false;
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

        foreach (Collider c in RagdollParts)
        {
            c.isTrigger = true;
        }
    }

    private void ContinueAfterRagdoll()
    {
        richAI.canMove = true;
    }

    public void Attack(Player playerScript)
    {
        if (isDead || ragdolling) return;
        if (canAttack)
        {
            playerScript.TakeDamage(damage);
            canAttack = false;
            PlayerMovement playerMovement = playerScript.GetComponent<PlayerMovement>();
            playerMovement.StartCoroutine(playerMovement.TemporarySpeedChange(0.25f, 0.5f));
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
        isCrawling = true;
        animator.Play("Start Crawling");
        SlowDown(crawlingSpeedMultiplier);
    }

    public void UpdateEyeColor() // Enemy HP% can be seen from the eye color
    {
        eyeMaterialR.SetColor("_EmissionColor", eyeGradient.Evaluate(healthPercentage / 100f) * Mathf.Pow(2, 2));
        eyeMaterialL.SetColor("_EmissionColor", eyeGradient.Evaluate(healthPercentage / 100f) * Mathf.Pow(2, 2));
    }

    public void SlowDown(float slowMultiplier)
    {
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
        debugVelocityTextfield.text = richAI.velocity.magnitude.ToString("F2");
        debugAnimatorVelocity.text = animator.GetFloat("Velocity").ToString();
    }

}
