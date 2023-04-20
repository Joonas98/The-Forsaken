using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{

    [Header("Player Settings")]
    [HideInInspector] public int currentHealth;
    public int maxHealth;
    public int regenationAmount;
    public float regenationDelay, regenationDelayAfterDamage;

    public float sensitivity;
    public float movementSpeed, sprintingMovementSpeed;
    public float jumpForce, sprintingJumpForce;

    public float maxBloom, maxVignette, maxChromaticAberration, maxGrain;
    public float flinchY, flinchX;

    [Header("Other Stuff")]

    public Gradient healthGradient;
    public GameObject runningSymbol, fallingSymbol, regenSymbol;
    public TextMeshProUGUI healthText, healthTextRaw;
    public Image healthSlider;
    private string healthString, healthStringRaw;

    private Vector3 spawnLocation;
    private bool regenerating = true;
    private float currentHPPercentage = 100f;

    public IdleSway idleSwayScript;

    public DamagePP damagePPScript;

    public Animator animator;

    [Header("Audio")]
    public AudioSource playerAS;
    public AudioClip[] damageGrunts, kickSounds;
    public AudioClip regenSound;

    [Header("Kick values")]
    public Transform kickTransform;
    public int kickDamage;
    public float kickRadius, kickForce, kickCooldown;
    public Vector3 upVector;

    private List<Enemy> damagedEnemies = new List<Enemy>();
    private float kickTimeStamp; // Handles cooldown for kicking

    private void Awake()
    {
        spawnLocation = transform.position;
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Start()
    {
        runningSymbol = GameObject.Find("RunningSymbol");

        if (runningSymbol != null)
            runningSymbol.SetActive(false);

        kickTimeStamp = Time.time + kickCooldown;
    }

    void Update()
    {
        calculateRegen();
        HandleInputs();
    }

    private void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.F) && kickTimeStamp <= Time.time)
        {
            kickTimeStamp = Time.time + kickCooldown;
            StartCoroutine(WaitKickAnimation(0.15f));
        }
    }

    public void Kick()
    {
        animator.Play("Kick");

        int kickSFXIndex = Random.Range(0, kickSounds.Length);
        playerAS.PlayOneShot(kickSounds[kickSFXIndex]);

        damagedEnemies.Clear();

        Collider[] kickedColliders = Physics.OverlapSphere(kickTransform.position, kickRadius);

        foreach (Collider target in kickedColliders)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(kickForce, transform.position - upVector, kickRadius);
            }

            Enemy enemy = target.GetComponentInParent<Enemy>();

            if (enemy != null && !enemy.isDead && !damagedEnemies.Contains(enemy))
            {
                enemy.TurnOnRagdoll();
                enemy.TakeDamage(kickDamage);
                damagedEnemies.Add(enemy);
            }
        }
    }

    public void TakeDamage(int amount, float flinchMultiplier = 1f)
    {
        regenerating = false;
        StopAllCoroutines();

        if (currentHealth - amount <= 0) currentHealth = 0;
        else currentHealth -= amount;

        StartCoroutine(regenerate());
        currentHPPercentage = currentHealth * 1.0f / maxHealth * 1.0f * 100f; // 80 / 100 = 0.8

        float currentHPPercentagePP = 1f - (currentHPPercentage / 100f); // 1 - 0.8 = 0.2
        damagePPScript.UpdateDamagePP(maxBloom * currentHPPercentagePP, maxVignette * currentHPPercentagePP, maxChromaticAberration * currentHPPercentagePP, maxGrain * currentHPPercentagePP);

        UpdateHealthUI();

        if (currentHealth > 0)
        {
            int rindex = Random.Range(0, damageGrunts.Length);
            playerAS.PlayOneShot(damageGrunts[rindex]);
        }
        regenSymbol.SetActive(false);
        Recoil.Instance.DamageFlinch(flinchY, flinchX, flinchMultiplier);
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

            if (currentHealth >= maxHealth)
                currentHealth = maxHealth;

            float currentHPPercentagePP = 1f - (currentHPPercentage / 100f); // 1 - 0.8 = 0.2
            damagePPScript.UpdateDamagePP(maxBloom * currentHPPercentagePP, maxVignette * currentHPPercentagePP, maxChromaticAberration * currentHPPercentagePP, maxGrain * currentHPPercentagePP);

            UpdateHealthUI();
        }
    }

    public void UpdateHealthUI()
    {
        healthString = Mathf.Round(currentHPPercentage).ToString() + "%";
        healthText.text = healthString;

        healthStringRaw = currentHealth.ToString() + " / " + maxHealth.ToString();
        healthTextRaw.text = healthStringRaw;

        healthSlider.fillAmount = currentHPPercentage / 100;
        healthSlider.color = healthGradient.Evaluate(1f - currentHPPercentage / 100);
    }

    public void calculateRegen()
    {
        if (regenerating)
        {
            regenerating = false;
            StartCoroutine(regenerationDelay());
        }

        if (currentHealth >= maxHealth)
        {
            regenSymbol.SetActive(false);
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
        regenSymbol.SetActive(true);
        regenerating = true;
    }

    IEnumerator WaitKickAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        Kick();
    }

}
