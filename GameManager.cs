using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool useVireDebug, useGunDebug;
    public GameObject vireDebugObjects, gunDebugObjects;
    public TextMeshProUGUI[] vireDebugTexts, gunDebugTexts; // Textfields for debug information

    public static GameManager GM;

    public GameObject WeaponHolster, aimingSymbol;
    public TextMeshProUGUI enemiesText, roundsText, moneyText;

    public int enemyCount = 0;
    public int currentWave = 0;
    public int money;

    public Gun currentGun;

    private void Awake()
    {
        // Singleton logiikka
        if (GM == null)
        {
            DontDestroyOnLoad(gameObject);
            GM = this;
        }
        else if (GM != this)
        {
            Destroy(gameObject);
        }
        WeaponHolster = GameObject.Find("WeaponHolster");

        if (!useVireDebug) Destroy(vireDebugObjects.gameObject);
        if (!useGunDebug) Destroy(gunDebugObjects.gameObject);
    }

    private void Start()
    {
        if (enemiesText == null) enemiesText = GameObject.Find("EnemiesNumber").GetComponent<TextMeshProUGUI>();
        if (roundsText == null) roundsText = GameObject.Find("WaveNumber").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        HandleKeybinds();
        HandleDebugs();
    }

    public void HandleKeybinds()
    {
        // if (Input.GetKey(KeyCode.N))
        // {
        //     Time.timeScale = 0.25f;
        // }
        //
        // if (Input.GetKey(KeyCode.M))
        // {
        //     Time.timeScale = 1f;
        // }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GameObject[] enemiesToDestroy = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemiesToDestroy)
            {
                enemy.GetComponent<Enemy>().Die();
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            GameObject[] enemiesToDestroy = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemiesToDestroy)
            {
                // Debug.Log("Now do the harlem shake");
                enemy.GetComponent<Enemy>().TurnOnRagdoll();
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            GameObject[] enemiesToDestroy = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemiesToDestroy)
            {
                // Debug.Log("Harlem shake cancelled");
                enemy.GetComponent<Enemy>().TurnOffRagdoll();
            }
        }

        if (GetCurrentGun() != null && GetCurrentGun().isAiming)
        {
            aimingSymbol.SetActive(true);
        }
        else
        {
            aimingSymbol.SetActive(false);
        }

        // Functionality for underdog ability
        if (AbilityMaster.abilities.Contains(3))
        {
            if (GetCurrentGun() == null) return;
            GetCurrentGun().RPM = GetCurrentGun().RPMOG * (1f + (enemyCount / 100f));
            GetCurrentGun().UpdateFirerate();
            // Debug.Log("New firerate: " + GetCurrentGun().RPMOG * ((1f + enemyCount / 100f)));
        }
    }

    public void HandleDebugs()
    {
        if (useVireDebug)
        {

        }

        if (useGunDebug)
        {
            if (currentGun == null) return;
            gunDebugTexts[0].text = "Name: " + currentGun.gunName;
            gunDebugTexts[1].text = "Dmg: " + currentGun.damage.ToString();
            gunDebugTexts[2].text = "Pellets: " + currentGun.pelletCount.ToString();
            gunDebugTexts[3].text = "Penetr: " + currentGun.penetration.ToString();
            gunDebugTexts[4].text = "RPM: " + currentGun.RPM.ToString();
        }
    }

    public Gun GetCurrentGun() // Easy way to get reference to current gun script from anywhere
    {
        return currentGun;
    }

    public void UpdateEnemyCount()
    {
        enemiesText.text = enemyCount.ToString();
    }

    public void UpdateWaveNumber(int wave)
    {
        currentWave = wave;
        roundsText.text = wave.ToString();
    }

    public void AdjustMoney(int amount)
    {
        money += amount;
        moneyText.text = money.ToString() + " €";
    }

}
