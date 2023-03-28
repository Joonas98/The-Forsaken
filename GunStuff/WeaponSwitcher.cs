using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwitcher : MonoBehaviour
{
    public int selectedWeapon = 0;
    public static bool canSwitchWeapon = true;

    public Gun currentGun;

    public GameObject weaponsPanel;
    private GameObject highlight;

    private float unequipTime;

    void Update()
    {
        int previousSelectedWeapon = selectedWeapon;

        #region Wheel Selection
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && canSwitchWeapon)
        {
            if (selectedWeapon >= transform.childCount - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon++;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f && canSwitchWeapon)
        {
            if (selectedWeapon <= 0)
            {
                selectedWeapon = transform.childCount - 1;
            }
            else
            {
                selectedWeapon--;
            }
        }
        #endregion

        #region Numbers Selection
        if (Input.GetKeyDown(KeyCode.Alpha1) && transform.childCount >= 1 && canSwitchWeapon)
        {
            selectedWeapon = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2 && canSwitchWeapon)
        {
            selectedWeapon = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3 && canSwitchWeapon)
        {
            selectedWeapon = 2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4 && canSwitchWeapon)
        {
            selectedWeapon = 3;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) && transform.childCount >= 5 && canSwitchWeapon)
        {
            selectedWeapon = 4;
        }

        if (Input.GetKeyDown(KeyCode.Alpha6) && transform.childCount >= 6 && canSwitchWeapon)
        {
            selectedWeapon = 5;
        }

        if (Input.GetKeyDown(KeyCode.Alpha7) && transform.childCount >= 7 && canSwitchWeapon)
        {
            selectedWeapon = 6;
        }

        if (Input.GetKeyDown(KeyCode.Alpha8) && transform.childCount >= 8 && canSwitchWeapon)
        {
            selectedWeapon = 7;
        }

        if (Input.GetKeyDown(KeyCode.Alpha9) && transform.childCount >= 9 && canSwitchWeapon)
        {
            selectedWeapon = 8;
        }
        #endregion

        if (previousSelectedWeapon != selectedWeapon && canSwitchWeapon)
        {
            if (currentGun != null)
            {
                StartCoroutine(UnequipTimer());
            }
            else
            {
                SelectWeapon();
            }
        }

    }

    public void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
            i++;
        }

        // currentGun = GameManager.GM.GetCurrentGun();
        currentGun = gameObject.GetComponentInChildren<Gun>();

        if (currentGun != null)
        {
            currentGun.ResetFOV();
            unequipTime = currentGun.unequipTime;
            GameManager.GM.currentGun = currentGun;
        }
        else
        {
            GameManager.GM.currentGun = null;
        }

        if (highlight != null) highlight.SetActive(false);

        // Highlight selected weapon
        highlight = weaponsPanel.transform.GetChild(selectedWeapon).GetChild(3).gameObject;
        highlight.SetActive(true);
    }

    public static void canSwitch(bool boolean)
    {
        canSwitchWeapon = boolean;
    }

    IEnumerator UnequipTimer()
    {
        currentGun.UnequipWeapon();
        yield return new WaitForSeconds(unequipTime - 0.01f);
        SelectWeapon();
        canSwitchWeapon = true;
    }

}
