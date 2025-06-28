using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	// 18.6.23 Class created, inherited by Gun.cs and MeleeWeapon.cs
	[Header("Weapon class")]
	public bool isMelee;
	public string weaponName;
	public int weaponPrice;
	public Sprite weaponSprite; // Image of the weapon
	public float equipTime;     // Time to take the weapon out
	[HideInInspector] public bool equipped;

	[Header("Weapon class audio")]
	[HideInInspector] public AudioSource audioSource;
	public AudioClip equipSound;

	protected Transform equipTrans;
	protected Transform weaponSpot;

	// track the active equip coroutine so we can cancel it
	private Coroutine _equipRoutine;

	protected virtual void OnValidate()
	{
		if (audioSource == null) audioSource = GetComponent<AudioSource>();
	}

	protected virtual void Awake()
	{
		equipTrans = GameManager.GM.equipTrans;
		weaponSpot = GameManager.GM.weaponSpot;
	}

	/// <summary>
	/// Starts (or restarts) the equip process, cancelling any in-flight equip first.
	/// </summary>
	public virtual void EquipWeapon()
	{
		// cancel any pending equip
		if (_equipRoutine != null)
		{
			StopCoroutine(_equipRoutine);
			_equipRoutine = null;
		}

		// kick off a new equip coroutine
		_equipRoutine = StartCoroutine(EquipRoutine());
	}

	/// <summary>
	/// Immediately aborts any equip in progress and marks as not equipped.
	/// </summary>
	public void CancelEquip()
	{
		if (_equipRoutine != null)
		{
			StopCoroutine(_equipRoutine);
			_equipRoutine = null;
		}
		equipped = false;
	}

	private IEnumerator EquipRoutine()
	{
		equipped = false;

		if (equipSound != null)
			audioSource.PlayOneShot(equipSound);

		yield return new WaitForSeconds(equipTime);

		equipped = true;
		WeaponSwitcher.canSwitchWeapon = true;

		// done
		_equipRoutine = null;
	}
}
