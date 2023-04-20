using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool useGunDebug, useRecoilDebug, useVireDebug, useSpawnDebug, useEnemyDebug;
    public GameObject gunDebugObjects, recoilDebugObjects, vireDebugObjects;
    public TextMeshProUGUI[] gunDebugTexts, recoilDebugTexts, vireDebugTexts; // Textfields for debug information

    public static GameManager GM;

    public GameObject WeaponHolster, aimingSymbol;
    public TextMeshProUGUI enemiesText, roundsText, moneyText;

    public int enemyCount = 0;
    public int currentWave = 0;
    public int money;

    public Gun currentGun;
    public Recoil recoil;
    public VisualRecoil vire;

    public List<Enemy> enemiesAlive = new List<Enemy>();
    public List<GameObject> enemiesAliveGos = new List<GameObject>();

    public GameObject playerGO;
    public AudioSource playerAS, GMAS;
    public AudioClip[] confirmKillSFX;

    private void Awake()
    {
        // Singleton
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
        HandleAbilities();
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
            Debug.Log("Killing all enemies");
            GameObject[] enemiesToDestroy = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemiesToDestroy)
            {
                enemy.GetComponent<Enemy>().Die();
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            Time.timeScale = 0f;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Time.timeScale = 1f;
        }

        // if (Input.GetKeyDown(KeyCode.N))
        // {
        //     GameObject[] enemiesToDestroy = GameObject.FindGameObjectsWithTag("Enemy");
        //     foreach (GameObject enemy in enemiesToDestroy)
        //     {
        //         enemy.GetComponent<Enemy>().TurnOnRagdoll();
        //     }
        // }
        //
        // if (Input.GetKeyDown(KeyCode.M))
        // {
        //     GameObject[] enemiesToDestroy = GameObject.FindGameObjectsWithTag("Enemy");
        //     foreach (GameObject enemy in enemiesToDestroy)
        //     {
        //         enemy.GetComponent<Enemy>().TurnOffRagdoll();
        //     }
        // }

    }

    public void HandleDebugs()
    {
        // To update debugging text fields
        if (useGunDebug)
        {
            if (currentGun == null) return;
            gunDebugTexts[0].text = "Name: " + currentGun.gunName;
            gunDebugTexts[1].text = "Dmg: " + currentGun.damage.ToString();
            gunDebugTexts[2].text = "Pellets: " + currentGun.pelletCount.ToString();
            gunDebugTexts[3].text = "Penetr: " + currentGun.penetration.ToString();
            gunDebugTexts[4].text = "RPM: " + currentGun.RPM.ToString();
        }

        if (useRecoilDebug)
        {
            recoilDebugTexts[0].text = "X: " + recoil.recoilX.ToString();
            recoilDebugTexts[1].text = "Y: " + recoil.recoilY.ToString();
            recoilDebugTexts[2].text = "Z: " + recoil.recoilZ.ToString();
            recoilDebugTexts[3].text = "Snp: " + recoil.snappiness.ToString();
            recoilDebugTexts[4].text = "Rtn: " + recoil.returnSpeed.ToString();
            recoilDebugTexts[5].text = "RecMP: " + recoil.recoilMultiplier.ToString();
        }

        if (useVireDebug)
        {
            vireDebugTexts[0].text = "VRX: " + vire.vrecoilX.ToString();
            vireDebugTexts[1].text = "VRY: " + vire.vrecoilY.ToString();
            vireDebugTexts[2].text = "VRZ: " + vire.vrecoilZ.ToString();
            vireDebugTexts[3].text = "VRKB: " + vire.kickbackZ.ToString();
            vireDebugTexts[4].text = "VRSnap: " + vire.snappiness.ToString();
            vireDebugTexts[5].text = "VRRtrn: " + vire.returnAmount.ToString();
        }
    }

    public void HandleAbilities()
    {
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

    public Gun GetCurrentGun() // Easy way to get reference to current gun script from anywhere
    {
        return currentGun;
    }

    public void ConfirmKillFX() // Extra effects for kills
    {
        GMAS.PlayOneShot(confirmKillSFX[Random.Range(0, confirmKillSFX.Length)]);
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
