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

    public float flinchY, flinchX, flinchMultiplier;

    [Header("Other Stuff")]

    public GameObject runningSymbol, fallingSymbol, regenSymbol;
    // public GameObject feetCollider;
    public TextMeshProUGUI healthText, healthTextRaw;
    public Gradient healthGradient;
    public Image healthSlider;
    private string healthString, healthStringRaw;

    private Vector3 spawnLocation;
    private bool regenerating = true;
    private float currentHPPercentage = 100f;
    private float currentPPPercentage;

    public IdleSway idleSwayScript;

    public DamagePP damagePPScript;

    public Animator animator;

    [Header("Audio")] // Handled now in playermovement script
    public AudioSource playerAS;
    public AudioClip[] damageGrunts;
    public AudioClip regenSound;

    private void Awake()
    {
        spawnLocation = this.transform.position;
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Start()
    {
        runningSymbol = GameObject.Find("RunningSymbol");

        if (runningSymbol != null)
            runningSymbol.SetActive(false);

    }

    void Update()
    {
        calculateRegen();
    }

    public void TakeDamage(int amount)
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

            float currentHPPercentagePP = 1f - (currentHPPercentage / 100f); // 1 - 0.8 = 0.2
            damagePPScript.UpdateDamagePP(maxBloom * currentHPPercentagePP, maxVignette * currentHPPercentagePP, maxChromaticAberration * currentHPPercentagePP, maxGrain * currentHPPercentagePP);

            UpdateHealthUI();

            if (currentHealth >= maxHealth)
                currentHealth = maxHealth;
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

}
