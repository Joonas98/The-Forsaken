using UnityEngine;

public class NewCanvasManager : MonoBehaviour
{
	[Header("References")]
	// The main canvases
	public GameObject shopCanvas;
	public GameObject pauseCanvas;

	public AmmoPanel[] ammoPanels;

	// Close modal window when closing menus
	public Michsky.MUIP.ModalWindowManager confirmPurchaseModalWindow;

	// Privates
	private enum MenuType { None, Shop, Pause, Abilities }
	private MenuType currentMenu = MenuType.None;

	private void Start()
	{
		CloseAllMenus();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeybindManager.Instance.shopKey)) ToggleMenu(MenuType.Shop);
		if (Input.GetKeyDown(KeybindManager.Instance.pauseKey)) ToggleMenu(MenuType.Pause);
	}

	private void ToggleMenu(MenuType menuType)
	{
		// Prevent menu changing when purchase confirmation window is open
		if (confirmPurchaseModalWindow.GetComponent<CanvasGroup>().alpha > 0.05) return;

		if (currentMenu == menuType)
		{
			CloseMenu();
			PauseGame(false);
		}
		else
		{
			OpenMenu(menuType);
			PauseGame(true);
		}
	}

	private void OpenMenu(MenuType menuType)
	{
		CloseAllMenus();
		currentMenu = menuType;

		switch (menuType)
		{
			case MenuType.Shop:
				shopCanvas.SetActive(true);
				NewWeaponShop.instance.UpdateWeaponButtonColors();
				break;
			case MenuType.Pause:
				pauseCanvas.SetActive(true);
				break;
		}
	}

	private void CloseMenu()
	{
		switch (currentMenu)
		{
			case MenuType.Shop:
				shopCanvas.SetActive(false);
				break;
			case MenuType.Pause:
				pauseCanvas.SetActive(false);
				break;
		}

		currentMenu = MenuType.None;
	}

	public void CloseAllMenus()
	{
		shopCanvas.SetActive(false);
		pauseCanvas.SetActive(false);
		currentMenu = MenuType.None;
	}

	private void PauseGame(bool pause)
	{
		if (pause)
		{
			// Disable hud pieces
			AudioListener.pause = true;
			MouseLook.instance.canRotate = false;
			WeaponSwitcher.CanSwitch(false);
			Cursor.lockState = CursorLockMode.None;
			Time.timeScale = 0f;
		}
		else
		{
			// Activate hud pieces
			AudioListener.pause = false;
			MouseLook.instance.canRotate = true;
			WeaponSwitcher.CanSwitch(true);
			Cursor.lockState = CursorLockMode.Locked;
			Time.timeScale = 1f;
		}
	}

	public void ResumeButton()
	{
		// Prevent bugs
		if (confirmPurchaseModalWindow.GetComponent<CanvasGroup>().alpha > 0.05) return;

		CloseMenu();
		PauseGame(false);
	}

	public void UpdateAmmoMenu()
	{
		foreach (AmmoPanel ammopanel in ammoPanels)
		{
			ammopanel.UpdateAmmoText();
		}
	}

	public void QuitGame()
	{
		// In the Unity Editor, stop playing the game
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}

}
