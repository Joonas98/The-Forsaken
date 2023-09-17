using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Stuff")]
    public Color roundTextColorEnemies;
    public Color roundTextColorClear;
    public TextMeshProUGUI[] gunDebugTexts, recoilDebugTexts, vireDebugTexts; // Textfields for debug information
    public TextMeshProUGUI enemiesText, roundsText, moneyText; // Info texts

    [SerializeField] private TextMeshProUGUI timerTexts;

    [Header("References")]
    // GameObjects
    public GameObject playerGO;
    public GameObject weaponHolster, aimingSymbol;
    public GameObject gunDebugObjects, recoilDebugObjects, vireDebugObjects;
    // Scripts
    public Weapon currentWeapon;
    public Gun currentGun;
    public Recoil recoil;
    public VisualRecoil vire;
    public Player playerScript;
    public static GameManager GM;

    [Header("Various Lists / Arrays")]
    public List<Enemy> enemiesAlive = new List<Enemy>();
    public List<GameObject> enemiesAliveGos = new List<GameObject>();

    [Header("Settings")]
    public bool useGunDebug;
    public bool useRecoilDebug, useVireDebug, useSpawnDebug, useEnemyDebug;

    [Header("Audio")]
    public AudioSource playerAS;
    public AudioSource GMAS;
    public AudioClip[] confirmKillSFX;

    [Header("Other things")]
    public int money;
    public int enemyCount = 0;
    public int currentWave = 0;
    public int currentWeaponIndex = 0;
    public Transform equipTrans, weaponSpot; // Optimization: weapon.cs Awake() gets these variables from here
    public float gameTime; // Time elapsed since start of the game
    public bool currentGunAiming = false; // 1.7.2023 far better to get aiming info from here to other scripts

    private float startTime;

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

        weaponHolster = GameObject.Find("WeaponHolster"); // 6.5.23 WeaponSwitcher can be now referenced as WeaponSwitcher.instance
        if (!useVireDebug) Destroy(vireDebugObjects);
        if (!useGunDebug) Destroy(gunDebugObjects);
        if (!useRecoilDebug) Destroy(recoilDebugObjects);
    }

    private void Start()
    {
        startTime = Time.realtimeSinceStartup;
        if (enemiesText == null) enemiesText = GameObject.Find("EnemiesNumber").GetComponent<TextMeshProUGUI>();
        if (roundsText == null) roundsText = GameObject.Find("WaveNumber").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        HandleKeybinds();
        HandleDebugs();
        HandleAbilities();
        if (currentGun != null) currentGunAiming = currentGun.isAiming;
    }

    private void FixedUpdate()
    {
        gameTime = Time.realtimeSinceStartup - startTime;
        timerTexts.text = gameTime.ToString("F2");
    }

    public void HandleKeybinds()
    {
        if (Input.GetKey(KeyCode.N))
        {
            Time.timeScale = 0.1f;
        }

        if (Input.GetKey(KeyCode.M))
        {
            Time.timeScale = 1f;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Killing all enemies");
            GameObject[] enemiesToDestroy = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemiesToDestroy)
            {
                enemy.GetComponent<Enemy>().Die();
            }
        }
    }

    public void HandleDebugs()
    {
        // To update debugging text fields
        if (useGunDebug)
        {
            if (currentGun == null) return;
            gunDebugTexts[0].text = "Name: " + currentGun.weaponName;
            gunDebugTexts[1].text = "Dmg: " + currentGun.damage.ToString();
            gunDebugTexts[2].text = "Pellets: " + currentGun.pelletCount.ToString();
            gunDebugTexts[3].text = "Penetr: " + currentGun.penetration.ToString();
            gunDebugTexts[4].text = "RPM: " + currentGun.RPM.ToString();
        }

        if (useRecoilDebug)
        {
            recoilDebugTexts[0].text = "X: " + currentGun.recoil.x.ToString();
            recoilDebugTexts[1].text = "Y: " + currentGun.recoil.y.ToString();
            recoilDebugTexts[2].text = "Z: " + currentGun.recoil.z.ToString();
            recoilDebugTexts[3].text = "Snp: " + recoil.snappiness.ToString();
            recoilDebugTexts[4].text = "Rtn: " + recoil.returnSpeed.ToString();
            recoilDebugTexts[5].text = "RecMP: " + recoil.recoilMultiplier.ToString();
        }

        if (useVireDebug)
        {
            vireDebugTexts[0].text = "VRX: " + currentGun.vire.x.ToString();
            vireDebugTexts[1].text = "VRY: " + currentGun.vire.y.ToString();
            vireDebugTexts[2].text = "VRZ: " + currentGun.vire.z.ToString();
            vireDebugTexts[3].text = "VRKB: " + currentGun.vireKick.ToString();
            vireDebugTexts[4].text = "VRSnap: " + currentGun.vireSnap.ToString();
            vireDebugTexts[5].text = "VRRtrn: " + currentGun.vireReturn.ToString();
        }
    }

    public void HandleAbilities()
    {
        if (GetCurrentGun() != null && currentGunAiming)
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

    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public void ConfirmKillFX() // Extra effects for kills
    {
        GMAS.PlayOneShot(confirmKillSFX[Random.Range(0, confirmKillSFX.Length)]);
    }

    public void UpdateEnemyCount()
    {
        enemiesText.text = enemyCount.ToString();

        if (enemyCount > 0)
            roundsText.color = roundTextColorEnemies;
        else
            roundsText.color = roundTextColorClear;
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
