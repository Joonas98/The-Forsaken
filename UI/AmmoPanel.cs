using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoPanel : MonoBehaviour
{
	// This script is for handling ammo panels in the ammo shop
	// Each panel has their own instance

	[Header("Basics")]
	public PlayerInventory.AmmoType ammoType;
	public int buyBulletAmount, buyBulletPrice; // How many bullets and for what price you can buy
	public int ammoCapIncreaseAmount, ammoCapIncreasePrice; // How much ammo capacity is increased and for what price

	[Header("References")]
	public TextMeshProUGUI ammoAmountText; // E.G. "90 / 270"

	[Header("Interactable")]
	public Michsky.MUIP.ButtonManager buyAmmo;
	public Michsky.MUIP.ButtonManager buyMoreAmmo;
	public Michsky.MUIP.ButtonManager buyMaxAmmo;
	public Michsky.MUIP.ButtonManager increaseMax;

	// Update the ammo amount text (E.G. 180 / 360)
	public void UpdateAmmoText()
	{
		ammoAmountText.text = PlayerInventory.instance.GetAmmoCount(ammoType).ToString() + " / " + PlayerInventory.instance.GetMaxAmmoCount(ammoType).ToString();
	}

	public void BuyAmmo1()
	{
		if (GameManager.GM.money >= buyBulletPrice)
		{
			PlayerInventory.instance.HandleAmmo(ammoType, buyBulletAmount);
			GameManager.GM.AdjustMoney(-buyBulletPrice);
		}
		else
		{
			Debug.Log("Too poor lmao");
		}
	}

	public void BuyAmmo2()
	{
		if (GameManager.GM.money >= buyBulletPrice * 3)
		{
			PlayerInventory.instance.HandleAmmo(ammoType, buyBulletAmount * 3);
			GameManager.GM.AdjustMoney(-buyBulletPrice * 3);
		}
		else
		{
			Debug.Log("Too poor lmao");
		}
	}

	public void BuyAmmoMax()
	{
		int currentAmmoCount = PlayerInventory.instance.GetAmmoCount(ammoType);
		int maxAmmoCount = PlayerInventory.instance.GetMaxAmmoCount(ammoType);

		// Calculate the price per bullet
		int pricePerBullet = (int)Mathf.Ceil(buyBulletPrice / buyBulletAmount);

		// Calculate the number of bullets needed to reach max ammo
		int bulletsNeeded = maxAmmoCount - currentAmmoCount;

		// Calculate the cost
		int totalCost = bulletsNeeded * pricePerBullet;

		if (GameManager.GM.money >= totalCost)
		{
			// Deduct money and add bullets
			GameManager.GM.AdjustMoney(-totalCost);
			PlayerInventory.instance.HandleAmmo(ammoType, bulletsNeeded);
			Debug.Log("Bought " + bulletsNeeded + " " + ammoType + " bullets for " + totalCost + " money.");
		}
		else
		{
			Debug.Log("Too poor lmao");
		}
	}

	public void BuyAmmoCapIncrease()
	{
		if (GameManager.GM.money >= ammoCapIncreasePrice)
		{
			PlayerInventory.instance.HandleMaxAmmo(ammoType, ammoCapIncreaseAmount);
			GameManager.GM.AdjustMoney(-ammoCapIncreasePrice);
		}
		else
		{
			Debug.Log("Too poor lmao");
		}
	}
}
