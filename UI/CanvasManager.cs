using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public GameObject[] hudPieces; // Elemetit jotka laitetaan pois p‰‰lt‰ menut avatessa
    public GameObject crosshairCanvas, inventoryCanvas, shopCanvas, upgradesPanel, shopPanel, abilitiesCanvas, pauseCanvas, roundPopup;

    public GameObject weaponsPanel;
    public WeaponPanel[] weaponPanelScripts;

    private Player playerScript;
    private MouseLook lookScript;
    public WeaponList weaponListScript;
    public WeaponSwitcher weaponSwitcherScript;

    public string selectedGunName = "Knife";
    public Sprite defaultGunSprite;

    public GameObject abilitiesTooltip;

    private void Start()
    {
        weaponListScript.AddWeaponToPanel(selectedGunName, defaultGunSprite);

        if (playerScript == null) playerScript = GameObject.Find("Player").GetComponent<Player>();
        if (lookScript == null) lookScript = GameObject.Find("Player").GetComponentInChildren<MouseLook>();

        weaponSwitcherScript.SelectWeapon();
    }

    void Update()
    {
        HandleInputs();
    }

    public void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseCanvas.activeInHierarchy)
            {
                PauseMenu(true);
            }
            else
            {
                PauseMenu(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!shopPanel.activeInHierarchy)
            {
                OpenWeaponShop();
            }
            else
            {
                CloseWeaponShop();
            }
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            if (!inventoryCanvas.activeInHierarchy)
            {
                OpenInventory();
            }
            else
            {
                CloseInventory();
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (!abilitiesCanvas.activeInHierarchy)
            {
                OpenAbilities();
            }
            else
            {
                CloseAbilities();
            }
        }

    }

    public void PauseMenu(bool boolean)
    {
        CloseAll();
        if (boolean)
        {
            InventoryPause();
        }
        else
        {
            InventoryUnpause();
        }
        pauseCanvas.SetActive(boolean);
    }

    public void OpenWeaponShop()
    {
        CloseAll();
        shopCanvas.SetActive(true);
        shopPanel.SetActive(true);
        WeaponPanelButtons(true);
        InventoryPause();
    }

    public void CloseWeaponShop()
    {
        CloseAll();
        InventoryUnpause();
    }

    public void OpenInventory()
    {
        CloseAll();
        inventoryCanvas.SetActive(true);
        InventoryPause();
    }

    public void CloseInventory()
    {
        CloseAll();
        InventoryUnpause();
    }

    public void OpenUpgrades()
    {
        CloseAll();
        shopCanvas.SetActive(true);
        upgradesPanel.SetActive(true);
    }

    public void OpenAbilities()
    {
        CloseAll();
        abilitiesCanvas.SetActive(true);
        InventoryPause();
    }

    public void CloseAbilities()
    {
        CloseAll();
        InventoryUnpause();
    }

    public void CloseAll()
    {
        WeaponPanelButtons(false);
        crosshairCanvas.SetActive(false);
        inventoryCanvas.SetActive(false);
        shopCanvas.SetActive(false);
        upgradesPanel.SetActive(false);
        shopPanel.SetActive(false);
        abilitiesCanvas.SetActive(false);
    }

    public void InventoryPause()
    {
        foreach (GameObject go in hudPieces)
        {
            go.SetActive(false);
        }
        roundPopup.SetActive(false);
        AudioListener.pause = true;
        if (lookScript != null) lookScript.canRotate = false;
        WeaponSwitcher.canSwitch(false);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }

    public void InventoryUnpause()
    {
        foreach (GameObject go in hudPieces)
        {
            go.SetActive(true);
        }
        AudioListener.pause = false;
        // if (playerScript != null) playerScript.canRotate = true;
        if (lookScript != null) lookScript.canRotate = true;
        WeaponSwitcher.canSwitch(true);
        Cursor.lockState = CursorLockMode.Locked;
        ShowTooltip(false);
        crosshairCanvas.SetActive(true);
        Time.timeScale = 1f;
    }

    public void WeaponPanelButtons(bool activate)
    {
        if (activate)
        {
            weaponPanelScripts = weaponsPanel.GetComponentsInChildren<WeaponPanel>();
            foreach (WeaponPanel x in weaponPanelScripts)
            {
                x.EnableButtons();
            }
        }
        else
        {
            weaponPanelScripts = weaponsPanel.GetComponentsInChildren<WeaponPanel>();
            foreach (WeaponPanel x in weaponPanelScripts)
            {
                x.DisableButtons();
            }
        }
    }

    public void Hitmarker(Vector3 hitPosition, bool isHeadshot)
    {
        #region old system
        // if (isHeadshot)
        // {
        //     StopAllCoroutines();
        //     foreach (Image img in hitmarkImages)
        //     {
        //         img.color = new Color(1, 1, 1, 1);
        //         StartCoroutine(FadeImage(img, true));
        //     }
        // }
        // else
        // {
        //     StopAllCoroutines();
        //     foreach (Image img in hitmarkImages)
        //     {
        //         img.color = new Color(1, 1, 1, 1);
        //         StartCoroutine(FadeImage(img, false));
        //     }
        // }

        // Yksi kuva systeemi
        //  if (isHeadshot)
        //  {
        //      StopAllCoroutines();
        //      Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPosition);
        //      oneHitmarkImage.rectTransform.position = screenPos;
        //      oneHitmarkImage.color = new Color(1, 1, 1, 1);
        //      StartCoroutine(FadeImage(oneHitmarkImage, true));
        //  }
        //  else
        //  {
        //      StopAllCoroutines();
        //      Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPosition);
        //      oneHitmarkImage.rectTransform.position = screenPos;
        //      oneHitmarkImage.color = new Color(1, 1, 1, 1);
        //      StartCoroutine(FadeImage(oneHitmarkImage, false));
        //  }
        #endregion

        // Object poolatut hitmarkerit
        GameObject hitmark = ObjectPool.SharedInstance.GetPooledObject();
        if (hitmark != null)
        {
            Image hitImage = hitmark.GetComponent<Image>();
            Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPosition);
            hitImage.rectTransform.position = screenPos;
            hitImage.color = new Color(1, 1, 1, 1);
            hitmark.SetActive(true);
            StartCoroutine(FadeImage(hitImage, isHeadshot));
            StartCoroutine(DisableDelay(hitmark));
        }

    }

    public void ShowTooltip(bool show)
    {
        if (show)
        {
            abilitiesTooltip.SetActive(true);
        }
        else
        {
            abilitiesTooltip.SetActive(false);
        }
    }

    IEnumerator FadeImage(Image image, bool headshot)
    {
        if (!headshot)
        {
            for (float i = 1; i >= -1; i -= Time.deltaTime * 5f)
            {
                image.color = new Color(1, 1, 1, i);
                yield return null;
            }
        }
        else
        {
            for (float i = 1; i >= -1; i -= Time.deltaTime * 5f)
            {
                image.color = new Color(2, 0.1f, 0.1f, i);
                yield return null;
            }
        }
    }

    IEnumerator DisableDelay(GameObject go)
    {
        yield return new WaitForSeconds(1f);
        go.SetActive(false);
    }

}
