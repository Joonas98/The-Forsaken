using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // 18.6.23 Class created to be inherited by Gun.cs and MeleeWeapon.cs
    [Header("Weapon class")]
    [HideInInspector] public bool equipped; // Set true when equip lerp is done
    public float equipTime, unequipTime;

    [Header("Weapon class audio")]
    public AudioSource audioSource;
    public AudioClip equipSound, unequipSound;

    // Private and protected
    private float equipLerp, unequipLerp;
    private float equipRotX, equipRotY, equipRotZ;

    protected Transform equipTrans;
    protected Transform weaponSpot;

    protected virtual void Awake()
    {
        // equipTrans = GameObject.Find("EquipTrans").transform;
        // weaponSpot = GameObject.Find("WeaponSpot");
        equipTrans = GameManager.GM.equipTrans;
        weaponSpot = GameManager.GM.weaponSpot;
    }

    protected virtual void Update()
    {
        HandleSwitchingLerps();
    }

    protected virtual void OnEnable()
    {
        // Random rotation when pulling out weapon
        equipRotX = Random.Range(0, 360);
        equipRotY = Random.Range(0, 360);
        equipRotZ = Random.Range(0, 360);
        Debug.Log("Randomed equip lerp");
    }

    public virtual void EquipWeapon()
    {
        Debug.Log("Equip called from: " + name);
        equipLerp = 0f;
        unequipLerp = 0f;
        WeaponSwitcher.CanSwitch(false);
        equipped = false;

        StartCoroutine(WaitEquipTime());
    }

    public virtual void UnequipWeapon()
    {
        Debug.Log("Unequip called from: " + name);
        equipLerp = 0f;
        unequipLerp = 0f;
        equipped = false;
        WeaponSwitcher.CanSwitch(false);
        audioSource.PlayOneShot(unequipSound);
    }

    // Handle lerps for switching weapons
    public void HandleSwitchingLerps()
    {
        // Take gun out
        if (equipped == false && equipLerp <= equipTime)
        {
            equipLerp += Time.deltaTime;
            transform.position = Vector3.Lerp(equipTrans.position, weaponSpot.transform.position, equipLerp / equipTime);
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(equipRotX, equipRotY, equipRotZ), weaponSpot.transform.rotation, equipLerp / equipTime);
        }

        // Put gun away
        if (equipped == false && unequipLerp <= unequipTime)
        {
            unequipLerp += Time.deltaTime;
            transform.position = Vector3.Lerp(weaponSpot.transform.position, equipTrans.position, unequipLerp / unequipTime);
            transform.rotation = Quaternion.Lerp(weaponSpot.transform.rotation, Quaternion.Euler(equipRotX, equipRotY, equipRotZ), unequipLerp / unequipTime);
        }
    }

    IEnumerator WaitEquipTime()
    {
        audioSource.PlayOneShot(equipSound);
        yield return new WaitForSeconds(equipTime);
        equipped = true;
        WeaponSwitcher.CanSwitch(true);
    }
}
