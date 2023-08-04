using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using DamageNumbersPro;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int damage;
    public int moneyReward;
    [Tooltip("Attack CD when hit")] public float attackCooldown;
    [Tooltip("Attack CD when missed")] public float swingCooldown;
    [Tooltip("Delay after dying to removal")] public float despawnTime;
    [Tooltip("How close to player to start swinging")] public float attackDistance;

    [HideInInspector] public int currentHealth, healthPercentage;
    [HideInInspector] public bool isDead = false;
    private GameObject player;
    private float distanceToPlayer;
    private Animator animator;

    public List<Collider> RagdollParts = new List<Collider>();
    public Collider[] Damagers;
    public Collider enemyCollider;
    public GameObject[] zombieSkins;
    public GameObject eyeRight, eyeLeft;
    public GameObject modelRoot; // Hips
    public Rigidbody[] RigidBodies;
    public Rigidbody bodyRB; // Rigidbody in waist or something (used for ragdoll magnitude checks etc.)

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

    // This struct will be implemented later to handle multiple movement speed changes easily
    private struct MovementSpeedEffect
    {
        public float movementSpeedMultiplier;
        public float duration;
    }

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
        currentHealth = maxHealth;

        float randomScaling = Random.Range(0.95f, 1.25f); // Default scale is 1.1
        transform.localScale = new Vector3(randomScaling, randomScaling, randomScaling);

        navAgent.speed = movementSpeed;
        ogMovementSpeed = navAgent.speed;

        if (!GameManager.GM.useEnemyDebug)
        {
            Destroy(debugVelocityTextfield.GetComponentInParent<Canvas>().gameObject);
        }
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        animator.SetFloat("Velocity", navAgent.velocity.magnitude / navAgent.speed);
        HandleSwinging();
        CheckRagdollMagnitude();
        DebugUpdate();
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
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
        {
            if (c.gameObject != this.gameObject)
            {
                c.isTrigger = true;
                RagdollParts.Add(c);
            }

        }
    }

    public void TurnOnRagdoll()
    {
        if (ragdolling) return;
        ragdolling = true;

        foreach (Rigidbody rb in RigidBodies)
        {
            rb.isKinematic = false;
        }

        navAgent.isStopped = true;
        standCountdownActive = false;
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
        // When any of part of the leg is destroyed, the zombie becomes a crawler
        isCrawling = true;
        animator.Play("Start Crawling");

        // Crawlers never can restore legs, so the ogMovementSpeed can be adjusted to avoid problems
        ogMovementSpeed *= crawlingSpeedMultiplier;
        navAgent.speed = ogMovementSpeed;

        // Prevent clipping through the ground
        navAgent.baseOffset = 0.05f;
    }

    public void SlowDown(float slowMultiplier)
    {
        Debug.Log("Slowing enemy from: " + navAgent.speed + " to: " + (navAgent.speed *= slowMultiplier).ToString());
        navAgent.speed = ogMovementSpeed * slowMultiplier;
    }

    public void RestoreMovementSpeed()
    {
        navAgent.speed = ogMovementSpeed;
    }


    // Obsolete
    // public void DamagePopup(int number)
    // {
    //     GameObject dmgPopupText = Instantiate(damagePopupText, popupTransform.position, Quaternion.identity);
    //     Rigidbody popRB = dmgPopupText.GetComponent<Rigidbody>();
    //     popRB.AddForce(new Vector3(UnityEngine.Random.Range(-1f, 1f), 1, UnityEngine.Random.Range(-1f, 1f)) * 150f);
    //     TextMeshPro popText = dmgPopupText.GetComponent<TextMeshPro>();
    //     popText.text = number.ToString();
    //     Destroy(dmgPopupText.gameObject, 2f);
    // }

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
