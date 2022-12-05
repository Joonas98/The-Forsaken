using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryCanvas : MonoBehaviour
{

    public TextMeshProUGUI ammo22LRText, ammoHK46Text, ammo357MagnumText, ammo45ACPText, ammo12GaugeText, ammo545Text, ammo556Text, ammo762Text, ammo50BMGText;
    public TextMeshProUGUI grenade1Text, grenade2Text, grenade3Text;

    public PlayerInventory inventoryScript;


    private void OnEnable()
    {
        UpdateAmmoTexts();
        UpdateSuppliesTexts();
    }

    private void UpdateAmmoTexts()
    {
        ammo22LRText.text = inventoryScript.GetAmmoCount(0).ToString() + " / " + inventoryScript.GetMaxAmmoCount(0).ToString();
        ammoHK46Text.text = inventoryScript.GetAmmoCount(1).ToString() + " / " + inventoryScript.GetMaxAmmoCount(1).ToString();
        ammo357MagnumText.text = inventoryScript.GetAmmoCount(2).ToString() + " / " + inventoryScript.GetMaxAmmoCount(2).ToString();
        ammo45ACPText.text = inventoryScript.GetAmmoCount(3).ToString() + " / " + inventoryScript.GetMaxAmmoCount(3).ToString();
        ammo12GaugeText.text = inventoryScript.GetAmmoCount(4).ToString() + " / " + inventoryScript.GetMaxAmmoCount(4).ToString();
        ammo545Text.text = inventoryScript.GetAmmoCount(5).ToString() + " / " + inventoryScript.GetMaxAmmoCount(5).ToString();
        ammo556Text.text = inventoryScript.GetAmmoCount(6).ToString() + " / " + inventoryScript.GetMaxAmmoCount(6).ToString();
        ammo762Text.text = inventoryScript.GetAmmoCount(7).ToString() + " / " + inventoryScript.GetMaxAmmoCount(7).ToString();
        ammo50BMGText.text = inventoryScript.GetAmmoCount(8).ToString() + " / " + inventoryScript.GetMaxAmmoCount(8).ToString();
    }

    private void UpdateSuppliesTexts()
    {
        grenade1Text.text = inventoryScript.GetGrenadeCount(0).ToString() + " / " + inventoryScript.GetMaxGrenadeCount(0).ToString();
        grenade2Text.text = inventoryScript.GetGrenadeCount(1).ToString() + " / " + inventoryScript.GetMaxGrenadeCount(1).ToString();
        grenade3Text.text = inventoryScript.GetGrenadeCount(2).ToString() + " / " + inventoryScript.GetMaxGrenadeCount(2).ToString();
    }

    public void Add22LR(int amount)
    {
        inventoryScript.HandleAmmo(0, amount);
        UpdateAmmoTexts();
    }

    public void AddHK46(int amount)
    {
        inventoryScript.HandleAmmo(1, amount);
        UpdateAmmoTexts();
    }

    public void AddMagnum(int amount)
    {
        inventoryScript.HandleAmmo(2, amount);
        UpdateAmmoTexts();
    }

    public void Add45ACP(int amount)
    {
        inventoryScript.HandleAmmo(3, amount);
        UpdateAmmoTexts();
    }

    public void Add12Gauge(int amount)
    {
        inventoryScript.HandleAmmo(4, amount);
        UpdateAmmoTexts();
    }

    public void Add545(int amount)
    {
        inventoryScript.HandleAmmo(5, amount);
        UpdateAmmoTexts();
    }

    public void Add556(int amount)
    {
        inventoryScript.HandleAmmo(6, amount);
        UpdateAmmoTexts();
    }

    public void Add762(int amount)
    {
        inventoryScript.HandleAmmo(7, amount);
        UpdateAmmoTexts();
    }

    public void Add50BMG(int amount)
    {
        inventoryScript.HandleAmmo(8, amount);
        UpdateAmmoTexts();
    }


    public void AddM67(int amount)
    {
        inventoryScript.HandleGrenades(0, amount);
        UpdateSuppliesTexts();
    }

    public void AddRGD5(int amount)
    {
        inventoryScript.HandleGrenades(1, amount);
        UpdateSuppliesTexts();
    }

    public void AddIncGrenade(int amount)
    {
        inventoryScript.HandleGrenades(2, amount);
        UpdateSuppliesTexts();
    }

}
