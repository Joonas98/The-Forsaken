using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
	public static PlayerInventory instance;
	// Define an enum for ammo types
	public enum AmmoType
	{
		// Note that these categories are not 100% accurate, there are expections
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

	// These are given to gun to be ejected from the casingspot
	public GameObject[] casingPrefabs;

	// Define dictionaries to store ammo counts and max ammo counts
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
		{ AmmoType.BMG50, 0 },
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
		{ AmmoType.BMG50, 60 },
	};

	// Grenade counts
	// TODO: use enums and dictionaries like with ammo types
	public int normalGrenadeCount, maxNormalGrenade;
	public int impactGrenadeCount, maxImpactGrenade;
	public int incendiaryGrenadeCount, maxIncendiaryGrenade;
	public int stunGrenadeCount, maxStunGrenade;

	[SerializeField] private TextMeshProUGUI totalAmmoText;
	private string totalAmmoString;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}

		// Add each ammo
		foreach (AmmoType ammoType in (AmmoType[])System.Enum.GetValues(typeof(AmmoType)))
		{
			HandleAmmo(ammoType, 5000);
		}
	}

	// Add or reduce ammo
	public void HandleAmmo(AmmoType ammoType, int ammoDelta)
	{
		if (ammoCounts.ContainsKey(ammoType))
		{
			ammoCounts[ammoType] += ammoDelta;
			UpdateTotalAmmoText(ammoType);
		}
	}

	// Add or reduce max ammo capacity
	public void HandleMaxAmmo(AmmoType ammoType, int ammoDelta)
	{
		if (maxAmmoCounts.ContainsKey(ammoType))
		{
			maxAmmoCounts[ammoType] += ammoDelta;
			UpdateTotalAmmoText(ammoType);
		}
	}

	public int GetAmmoCount(AmmoType ammoType)
	{
		if (ammoCounts.ContainsKey(ammoType))
		{
			return ammoCounts[ammoType];
		}
		return 0;
	}

	public int GetMaxAmmoCount(AmmoType ammoType)
	{
		if (maxAmmoCounts.ContainsKey(ammoType))
		{
			return maxAmmoCounts[ammoType];
		}
		return 0;
	}

	public string GetAmmoString(AmmoType ammoType)
	{
		return ammoType.ToString();
	}

	public void HandleGrenades(int grenadeIndex, int grenadeDelta)
	{
		switch (grenadeIndex)
		{
			case 0:
				normalGrenadeCount += grenadeDelta;
				break;

			case 1:
				impactGrenadeCount += grenadeDelta;
				break;

			case 2:
				incendiaryGrenadeCount += grenadeDelta;
				break;

			case 3:
				stunGrenadeCount += grenadeDelta;
				break;
		}

	}

	public int GetGrenadeCount(int grenadeTypeIndex)
	{
		switch (grenadeTypeIndex)
		{
			case 0: return normalGrenadeCount;
			case 1: return impactGrenadeCount;
			case 2: return incendiaryGrenadeCount;
			case 3: return stunGrenadeCount;
			default: return 0;
		}
	}

	public int GetMaxGrenadeCount(int grenadeTypeIndex)
	{
		switch (grenadeTypeIndex)
		{
			case 0: return maxNormalGrenade;
			case 1: return maxImpactGrenade;
			case 2: return maxIncendiaryGrenade;
			case 3: return maxStunGrenade;
			default: return 0;
		}
	}

	public void UpdateTotalAmmoText(AmmoType ammoType)
	{
		if (ammoCounts.ContainsKey(ammoType) && maxAmmoCounts.ContainsKey(ammoType))
		{
			totalAmmoString = $"{ammoCounts[ammoType]} / {maxAmmoCounts[ammoType]} - {ammoType}";
			totalAmmoText.text = totalAmmoString;
		}
	}

	// Guns get their casings automatically from here
	public GameObject GetCasingPrefab(AmmoType ammoType)
	{
		int index = (int)ammoType;
		if (index < 0 || index >= casingPrefabs.Length)
		{
			return null; // Invalid enum value or out of array bounds
		}

		return casingPrefabs[index];
	}
}
