using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    // Ammo types

    // 0 = .22 LR Vector
    // 1 = HK 4.6×30mm MP7
    // 2 = .357 Magnum Python
    // 3 = .45 ACP M1911, Deagle, USP

    // 4 = 12 Gauge Sawed Off, M1014

    // 5 = 5.45x39mm AK12
    // 6 = 5.56x45mm NATO Tar21, Aug A1, G36C, M249, M4
    // 7 = 7.62x51mm NATO Scar-H
    // 8 = .50 BMG M107

    public int ammo22LR, maxAmmo22LR;
    public int ammoHK46, maxAmmoHK46;
    public int ammo357Magnum, maxAmmo357Magnum;
    public int ammo45ACP, maxAmmo45ACP;

    public int ammo12Gauge, maxAmmo12Gauge;

    public int ammo545, maxAmmo545;
    public int ammo556, maxAmmo556;
    public int ammo762, maxAmmo762;
    public int ammo50BMG, maxAmmo50BMG;

    public int normalGrenadeCount, maxNormalGrenade;
    public int impactGrenadeCount, maxImpactGrenade;
    public int incendiaryGrenadeCount, maxIncendiaryGrenade;
    public int stunGrenadeCount, maxStunGrenade;

    [SerializeField] private TextMeshProUGUI totalAmmoText;
    private string totalAmmoString;

    // Add or reduce ammo
    public void HandleAmmo(int ammoTypeIndex, int ammoDelta)
    {
        if (ammoTypeIndex == 0) // .22 LR
        {
            ammo22LR += ammoDelta;
            UpdateTotalAmmoText(0);
        }
        else if (ammoTypeIndex == 1) // HK 4.6x30mm
        {
            ammoHK46 += ammoDelta;
            UpdateTotalAmmoText(1);
        }
        else if (ammoTypeIndex == 2) // .357 Magnum
        {
            ammo357Magnum += ammoDelta;
            UpdateTotalAmmoText(2);
        }
        else if (ammoTypeIndex == 3) // .45 ACP
        {
            ammo45ACP += ammoDelta;
            UpdateTotalAmmoText(3);
        }
        else if (ammoTypeIndex == 4) // 12 Gauge
        {
            ammo12Gauge += ammoDelta;
            UpdateTotalAmmoText(4);
        }
        else if (ammoTypeIndex == 5) // 5.45x39
        {
            ammo545 += ammoDelta;
            UpdateTotalAmmoText(5);
        }
        else if (ammoTypeIndex == 6) // 5.56 NATO
        {
            ammo556 += ammoDelta;
            UpdateTotalAmmoText(6);
        }
        else if (ammoTypeIndex == 7) // 7.62 NATO
        {
            ammo762 += ammoDelta;
            UpdateTotalAmmoText(7);
        }
        else if (ammoTypeIndex == 8) // .50 BMG
        {
            ammo50BMG += ammoDelta;
            UpdateTotalAmmoText(8);
        }
    }

    public int GetAmmoCount(int ammoTypeIndex)
    {
        if (ammoTypeIndex == 0) return ammo22LR;
        else if (ammoTypeIndex == 1) return ammoHK46;
        else if (ammoTypeIndex == 2) return ammo357Magnum;
        else if (ammoTypeIndex == 3) return ammo45ACP;
        else if (ammoTypeIndex == 4) return ammo12Gauge;
        else if (ammoTypeIndex == 5) return ammo545;
        else if (ammoTypeIndex == 6) return ammo556;
        else if (ammoTypeIndex == 7) return ammo762;
        else if (ammoTypeIndex == 8) return ammo50BMG;
        else return 0;
    }

    public int GetMaxAmmoCount(int ammoTypeIndex)
    {
        if (ammoTypeIndex == 0) return maxAmmo22LR;
        else if (ammoTypeIndex == 1) return maxAmmoHK46;
        else if (ammoTypeIndex == 2) return maxAmmo357Magnum;
        else if (ammoTypeIndex == 3) return maxAmmo45ACP;
        else if (ammoTypeIndex == 4) return maxAmmo12Gauge;
        else if (ammoTypeIndex == 5) return maxAmmo545;
        else if (ammoTypeIndex == 6) return maxAmmo556;
        else if (ammoTypeIndex == 7) return maxAmmo762;
        else if (ammoTypeIndex == 8) return maxAmmo50BMG;
        else return 0;
    }

    public string GetAmmoString(int ammoTypeIndex)
    {
        if (ammoTypeIndex == 0) return ".22LR";
        else if (ammoTypeIndex == 1) return "HK 4.6";
        else if (ammoTypeIndex == 2) return ".357 Magnum";
        else if (ammoTypeIndex == 3) return ".45 ACP";
        else if (ammoTypeIndex == 4) return "12 Gauge";
        else if (ammoTypeIndex == 5) return "5.45";
        else if (ammoTypeIndex == 6) return "5.56 NATO";
        else if (ammoTypeIndex == 7) return "7.62 NATO";
        else if (ammoTypeIndex == 8) return ".50 BMG";
        else return "";
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

    public void UpdateTotalAmmoText(int ammoType)
    {
        totalAmmoString = GetAmmoCount(ammoType).ToString() + " / " + GetMaxAmmoCount(ammoType).ToString() + " - " + GetAmmoString(ammoType);
        totalAmmoText.text = totalAmmoString;
    }

}
