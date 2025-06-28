using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager GM;

	[Header("UI Stuff")]
	public Color roundTextColorEnemies;
	public Color roundTextColorClear;
	public TextMeshProUGUI enemiesText, roundsText, moneyText, moneyTextShop; // Info texts

	[SerializeField] private TextMeshProUGUI timerTexts;

	[Header("References")]
	// GameObjects
	public GameObject playerGO;
	public GameObject weaponHolster, aimingSymbol;

	// Scripts
	public Weapon currentWeapon;
	public Gun currentGun;
	public Recoil recoil;
	public VisualRecoil vire;
	public Player playerScript;

	[Header("Various Lists / Arrays")]
	public List<EnemyBase> enemiesAlive = new List<EnemyBase>();
	public List<GameObject> enemiesAliveGos = new List<GameObject>();

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
	public bool currentGunAiming = false; // 1.7.2023 far better to get aiming info from here to other scripts
	public bool meleeEquipped = false;

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
	}

	private void Start()
	{
		if (enemiesText == null) enemiesText = GameObject.Find("EnemiesNumber").GetComponent<TextMeshProUGUI>();
		if (roundsText == null) roundsText = GameObject.Find("WaveNumber").GetComponent<TextMeshProUGUI>();
		AdjustMoney(0);
	}

	private void Update()
	{
		HandleKeybinds();
		HandleAbilities();
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

		if (Input.GetKey(KeyCode.B))
		{
			AdjustMoney(1000);
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

	public bool CurrentGunAiming()
	{
		if (currentGun == null) return false;
		if (!currentGun.isAiming) return false;
		return true;
	}

	public void HandleAbilities()
	{
		if (GetCurrentGun() != null && CurrentGunAiming())
		{
			aimingSymbol.SetActive(true);
		}
		else
		{
			aimingSymbol.SetActive(false);
		}

		if (AbilityMaster.instance.HasAbility("Underdog"))
		{
			if (GetCurrentGun() == null) return;
			GetCurrentGun().RPM = GetCurrentGun().RPMOG * (1f + (enemyCount / 100f));
			GetCurrentGun().UpdateFirerate();
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
		moneyTextShop.text = money.ToString() + " €";
	}
}
