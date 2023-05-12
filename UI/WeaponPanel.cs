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
    private GameObject handledGun;

    private void Awake()
    {
        weaponHolster = GameObject.Find("WeaponHolster");
    }

    public void MoveWeaponUp()
    {
        currentIndex = transform.GetSiblingIndex();
        transform.SetSiblingIndex(currentIndex - 1);
        handledGun = weaponHolster.transform.GetChild(currentIndex).gameObject;
        handledGun.transform.SetSiblingIndex(currentIndex - 1);
    }

    public void MoveWeaponDown()
    {
        currentIndex = transform.GetSiblingIndex();
        transform.SetSiblingIndex(currentIndex + 1);
        handledGun = weaponHolster.transform.GetChild(currentIndex).gameObject;
        handledGun.transform.SetSiblingIndex(currentIndex + 1);
    }

    public void SellWeapon()
    {
        currentIndex = transform.GetSiblingIndex();
        handledGun = weaponHolster.transform.GetChild(currentIndex).gameObject;
        Destroy(handledGun);
        Destroy(gameObject);
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
