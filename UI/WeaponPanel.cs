using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
    public GameObject weaponHolster;
    public Button upButton;
    public Button downButton;
    public Button sellButton;
    public GameObject[] buttons;
    public GameObject highlightObject;

    private int currentIndex;
    private GameObject handledWeapon;

    private void Awake()
    {
        weaponHolster = GameObject.Find("WeaponHolster");
    }

    public void MoveWeaponUp()
    {
        currentIndex = transform.GetSiblingIndex();
        transform.SetSiblingIndex(currentIndex - 1);
        handledWeapon = weaponHolster.transform.GetChild(currentIndex).gameObject;
        handledWeapon.transform.SetSiblingIndex(currentIndex - 1);
    }

    public void MoveWeaponDown()
    {
        currentIndex = transform.GetSiblingIndex();
        transform.SetSiblingIndex(currentIndex + 1);
        handledWeapon = weaponHolster.transform.GetChild(currentIndex).gameObject;
        handledWeapon.transform.SetSiblingIndex(currentIndex + 1);
    }

    public void SellWeapon()
    {
        currentIndex = transform.GetSiblingIndex();
        handledWeapon = weaponHolster.transform.GetChild(currentIndex).gameObject;
        Destroy(handledWeapon);
        Destroy(gameObject);

        // If the weapon to be sold is currently equippped, equip knife
        if (GameManager.GM.currentWeaponIndex == currentIndex)
        {
            WeaponSwitcher.instance.selectedWeapon = 0;
            WeaponSwitcher.instance.SelectWeapon();
        }
    }

    public void EnableButtons()
    {
        foreach (GameObject go in buttons)
        {
            go.SetActive(true);
        }
    }

    public void DisableButtons()
    {
        foreach (GameObject go in buttons)
        {
            go.SetActive(false);
        }
    }

}
