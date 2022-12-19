using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;

    public GameObject WeaponHolster, aimingSymbol;
    public TextMeshProUGUI enemiesText, roundsText, moneyText;

    public int enemyCount = 0;
    public int currentWave = 0;
    public int money;

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
    }

    private void Start()
    {
        if (enemiesText == null) enemiesText = GameObject.Find("EnemiesNumber").GetComponent<TextMeshProUGUI>();
        if (roundsText == null) roundsText = GameObject.Find("WaveNumber").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
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
            Debug.Log("New firerate: " + GetCurrentGun().RPMOG * ((1f + enemyCount / 100f)));
        }

    }

    public Gun GetCurrentGun()
    {
        return WeaponHolster.GetComponentInChildren<Gun>();
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
