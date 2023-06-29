using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwitcher : MonoBehaviour
{
    public int selectedWeapon = 0;
    public static bool canSwitchWeapon = true;
    public Weapon currentWeapon;
    public GameObject weaponsPanel;

    public static WeaponSwitcher instance; // Singleton reference

    private WeaponPanel handledPanel;
    private float unequipTime;
    private Gun currentGun;

    private void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

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
            if (currentWeapon != null)
            {
                StartCoroutine(UnequipTimer());
            }
            else
            {
                SelectWeapon();
            }
        }

    }

    private void OnGUI()
    {
        // GUIStyle myStyle = new GUIStyle();
        // myStyle.fontSize = 12;
        // GUI.Label(new Rect(0, 0, 80, 20), currentWeapon.name, myStyle);
        //
        // if (currentGun != null)
        // {
        //     GUI.Label(new Rect(0, 20, 80, 20), currentGun.name, myStyle);
        // }
        // else
        // {
        //     GUI.Label(new Rect(0, 20, 80, 20), "No gun found", myStyle);
        // }
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

        currentWeapon = gameObject.GetComponentInChildren<Weapon>();
        currentGun = gameObject.GetComponentInChildren<Gun>();

        if (currentWeapon != null)
        {
            unequipTime = currentWeapon.unequipTime;
        }

        if (currentGun != null)
        {
            currentGun.ResetFOV();
            GameManager.GM.currentGun = currentGun;
            WeaponSwayAndBob.instance.currentGun = currentGun;
        }
        else
        {
            GameManager.GM.currentGun = null;
            WeaponSwayAndBob.instance.currentGun = null;
        }

        // Handle highlight for HUD
        if (handledPanel != null) handledPanel.highlightObject.SetActive(false);
        handledPanel = weaponsPanel.transform.GetChild(selectedWeapon).GetComponent<WeaponPanel>();
        handledPanel.highlightObject.SetActive(true);
    }

    public static void CanSwitch(bool boolean)
    {
        canSwitchWeapon = boolean;
    }

    IEnumerator UnequipTimer()
    {
        currentWeapon.UnequipWeapon();
        yield return new WaitForSeconds(unequipTime + 0.01f);
        SelectWeapon();
        canSwitchWeapon = true;
    }

}
