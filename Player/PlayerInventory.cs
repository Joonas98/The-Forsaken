using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
	public static PlayerInventory instance;

	// Define an enum for ammo types
	public enum AmmoType
	{
		// Pistol and SMG ammo
		LR22,
		MM9,
		ACP45,
		Magnum357,
		PDW46x30mm,

		// Shotguns
		Gauge12,

		// Rifles and snipers
		NATO556,
		NATO762,
		BMG50
	}

	// Define an enum for grenade types
	public enum GrenadeType
	{
		Normal = 0,
		Impact = 1,
		Incendiary = 2,
		Stun = 3
	}

	// These are given to guns to be ejected from the casing spot
	public GameObject[] casingPrefabs;

	// Ammo counts and capacities
	private Dictionary<AmmoType, int> ammoCounts = new()
	{
		{ AmmoType.LR22, 0 },
		{ AmmoType.MM9, 0 },
		{ AmmoType.ACP45, 0 },
		{ AmmoType.Magnum357, 0 },
		{ AmmoType.PDW46x30mm, 0 },
		{ AmmoType.Gauge12, 0 },
		{ AmmoType.NATO556, 0 },
		{ AmmoType.NATO762, 0 },
		{ AmmoType.BMG50, 0 }
	};

	private Dictionary<AmmoType, int> maxAmmoCounts = new()
	{
		{ AmmoType.LR22, 360 },
		{ AmmoType.MM9, 300 },
		{ AmmoType.ACP45, 240 },
		{ AmmoType.Magnum357, 150 },
		{ AmmoType.PDW46x30mm, 270 },
		{ AmmoType.Gauge12, 90 },
		{ AmmoType.NATO556, 210 },
		{ AmmoType.NATO762, 180 },
		{ AmmoType.BMG50, 60 }
	};

	// Grenade counts and capacities
	private Dictionary<GrenadeType, int> grenadeCounts = new()
	{
		{ GrenadeType.Normal, 0 },
		{ GrenadeType.Impact, 0 },
		{ GrenadeType.Incendiary, 0 },
		{ GrenadeType.Stun, 0 }
	};

	private Dictionary<GrenadeType, int> maxGrenadeCounts = new()
	{
		{ GrenadeType.Normal, 15 },
		{ GrenadeType.Impact, 15 },
		{ GrenadeType.Incendiary, 15 },
		{ GrenadeType.Stun, 15 }
	};

	[SerializeField] private TextMeshProUGUI totalAmmoText;
	private string totalAmmoString;

	// Grenade UI texts will be handled in this class
	// Example: throw incendiary grenade -> HUD and selection UI updates accordingly
	[SerializeField] private TextMeshProUGUI[] grenadeCountTexts;
	[SerializeField] private TextMeshProUGUI grenadeCountHUDText;

	private void Awake()
	{
		// Singleton setup
		if (instance == null)
			instance = this;
		else if (instance != this)
		{
			Destroy(gameObject);
			return;
		}

		// Initialize default ammo
		foreach (AmmoType ammo in System.Enum.GetValues(typeof(AmmoType)))
		{
			HandleAmmo(ammo, 5000);
		}


		foreach (GrenadeType nade in System.Enum.GetValues(typeof(GrenadeType)))
		{
			grenadeCounts[nade] = maxGrenadeCounts[nade];
		}
	}

	#region Ammo Methods

	// Add or reduce ammo
	public void HandleAmmo(AmmoType ammoType, int delta)
	{
		if (!ammoCounts.ContainsKey(ammoType)) return;
		ammoCounts[ammoType] = Mathf.Clamp(ammoCounts[ammoType] + delta, 0, maxAmmoCounts[ammoType]);
		UpdateTotalAmmoText(ammoType);
	}

	// Add or reduce max ammo capacity
	public void HandleMaxAmmo(AmmoType ammoType, int delta)
	{
		if (!maxAmmoCounts.ContainsKey(ammoType)) return;
		maxAmmoCounts[ammoType] = Mathf.Max(0, maxAmmoCounts[ammoType] + delta);
		UpdateTotalAmmoText(ammoType);
	}

	public int GetAmmoCount(AmmoType ammoType)
	{
		return ammoCounts.TryGetValue(ammoType, out int count) ? count : 0;
	}

	public int GetMaxAmmoCount(AmmoType ammoType)
	{
		return maxAmmoCounts.TryGetValue(ammoType, out int maxCount) ? maxCount : 0;
	}

	public string GetAmmoString(AmmoType ammoType)
	{
		return ammoType.ToString();
	}

	public GameObject GetCasingPrefab(AmmoType ammoType)
	{
		int index = (int)ammoType;
		if (index < 0 || index >= casingPrefabs.Length)
			return null;
		return casingPrefabs[index];
	}

	public void UpdateTotalAmmoText(AmmoType ammoType)
	{
		if (ammoCounts.ContainsKey(ammoType) && maxAmmoCounts.ContainsKey(ammoType))
		{
			totalAmmoString = $"{ammoCounts[ammoType]} / {maxAmmoCounts[ammoType]} - {ammoType}";
			totalAmmoText.text = totalAmmoString;
		}
	}

	#endregion

	#region Grenade Methods

	public void HandleGrenades(GrenadeType type, int delta)
	{
		if (!grenadeCounts.ContainsKey(type)) return;
		grenadeCounts[type] = Mathf.Clamp(grenadeCounts[type] + delta, 0, maxGrenadeCounts[type]);
		UpdateGrenadeUI(type);
	}

	public int GetGrenadeCount(GrenadeType type)
		=> grenadeCounts.TryGetValue(type, out int c) ? c : 0;

	public int GetMaxGrenadeCount(GrenadeType type)
		=> maxGrenadeCounts.TryGetValue(type, out int m) ? m : 0;

	public void UpdateGrenadeUI(GrenadeType type)
	{
		int idx = (int)type;
		int cur = GetGrenadeCount(type);
		int max = GetMaxGrenadeCount(type);
		if (idx >= 0 && idx < grenadeCountTexts.Length)
			grenadeCountTexts[idx].text = $"{cur} / {max}";

		grenadeCountHUDText.text = cur.ToString();
	}

	#endregion
}
