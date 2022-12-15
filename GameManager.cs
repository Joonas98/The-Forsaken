using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    public static GameObject WeaponHolster;
    public static int enemyCount = 0;

    public static TextMeshProUGUI enemiesText;
    public static TextMeshProUGUI roundsText;

    public static int currentWave = 0;

    public GameObject aimingSymbol;

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

    }

    public static Gun GetCurrentGun()
    {
        return WeaponHolster.GetComponentInChildren<Gun>();
    }

    public static void UpdateEnemyCount()
    {
        enemiesText.text = enemyCount.ToString();
    }

    public static void UpdateWaveNumber(int wave)
    {
        currentWave = wave;
        roundsText.text = wave.ToString();
    }

}
