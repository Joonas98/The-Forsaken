using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	// 18.6.23 Class created, inherited by Gun.cs and MeleeWeapon.cs
	[Header("Weapon class")]
	public string weaponName;
	public int weaponPrice;
	public Sprite weaponSprite; // Image of the weapon
	public float equipTime, unequipTime; // Time to take the weapon out and put it away
	[HideInInspector] public bool equipped, unequipping;

	[Header("Weapon class audio")]
	[HideInInspector] public AudioSource audioSource;
	public AudioClip equipSound, unequipSound;

	// Private and protected
	private float equipLerp, unequipLerp;
	private float equipRotX, equipRotY, equipRotZ;

	protected Transform equipTrans;
	protected Transform weaponSpot;

	protected virtual void OnValidate()
	{
		if (audioSource == null) audioSource = GetComponent<AudioSource>();
	}

	protected virtual void Awake()
	{
		equipTrans = GameManager.GM.equipTrans;
		weaponSpot = GameManager.GM.weaponSpot;
	}

	protected virtual void Update()
	{
		HandleSwitchingLerps();
	}

	// private void OnGUI()
	// {
	//     GUIStyle myStyle = new GUIStyle();
	//     myStyle.fontSize = 12;
	//     GUI.Label(new Rect(500, 0, 80, 20), equipLerp.ToString() + " = equip", myStyle);
	//     GUI.Label(new Rect(500, 20, 80, 20), unequipLerp.ToString() + " = unequip", myStyle);
	// }

	protected virtual void OnEnable()
	{
		// Random rotation when pulling out weapon
		equipRotX = Random.Range(0, 360);
		equipRotY = Random.Range(0, 360);
		equipRotZ = Random.Range(0, 360);
	}

	public virtual void EquipWeapon()
	{
		StartCoroutine(WaitEquipTime(true));
	}

	public virtual void UnequipWeapon()
	{
		StartCoroutine(WaitEquipTime(false));
	}

	// Handle lerps for switching weapons
	public void HandleSwitchingLerps()
	{
		// Take gun out
		if (!unequipping && equipLerp <= equipTime)
		{
			equipLerp += Time.deltaTime;
			transform.SetPositionAndRotation(Vector3.Lerp(equipTrans.position, weaponSpot.transform.position, equipLerp / equipTime), Quaternion.Lerp(Quaternion.Euler(equipRotX, equipRotY, equipRotZ), weaponSpot.transform.rotation, equipLerp / equipTime));
		}

		// Put gun away
		if (unequipping && unequipLerp <= unequipTime)
		{
			unequipLerp += Time.deltaTime;
			transform.SetPositionAndRotation(Vector3.Lerp(weaponSpot.transform.position, equipTrans.position, unequipLerp / unequipTime), Quaternion.Lerp(weaponSpot.transform.rotation, Quaternion.Euler(equipRotX, equipRotY, equipRotZ), unequipLerp / unequipTime));
		}
	}

	// Delay when switching weapons, true = equip, false = unequip
	IEnumerator WaitEquipTime(bool equip)
	{
		if (equip) // Equip weapon
		{
			equipLerp = 0f;
			// unequipLerp = 0f;
			WeaponSwitcher.CanSwitch(false);
			equipped = false;
			unequipping = false;
			audioSource.PlayOneShot(equipSound);

			yield return new WaitForSeconds(equipTime);

			equipped = true;
			WeaponSwitcher.CanSwitch(true);
		}
		else // Unequip weapon
		{
			// equipLerp = 0f;
			unequipLerp = 0f;
			WeaponSwitcher.CanSwitch(false);
			equipped = false;
			unequipping = true;
			audioSource.PlayOneShot(unequipSound);

			yield return new WaitForSeconds(unequipTime);

			equipped = false;
			unequipping = false;
			WeaponSwitcher.CanSwitch(true);
		}
	}
}
