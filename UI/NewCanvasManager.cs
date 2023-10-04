using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCanvasManager : MonoBehaviour
{
	[Header("Keybinds")]
	public KeyCode shopKey;
	public KeyCode pauseKey;
	public KeyCode abilitiesKey;

	[Header("References")]
	public GameObject shopCanvas;
	public GameObject pauseCanvas;
	public GameObject abilityCanvas;

	// Privates
	private enum MenuType { None, Shop, Pause, Abilities }
	private MenuType currentMenu = MenuType.None;

	private void Start()
	{
		CloseAllMenus();
	}

	private void Update()
	{
		if (Input.GetKeyDown(shopKey)) ToggleMenu(MenuType.Shop);
		if (Input.GetKeyDown(pauseKey)) ToggleMenu(MenuType.Pause);
		if (Input.GetKeyDown(abilitiesKey)) ToggleMenu(MenuType.Abilities);
	}

	private void ToggleMenu(MenuType menuType)
	{
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
				break;
			case MenuType.Pause:
				pauseCanvas.SetActive(true);
				break;
			case MenuType.Abilities:
				abilityCanvas.SetActive(true);
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
			case MenuType.Abilities:
				abilityCanvas.SetActive(false);
				break;
		}

		currentMenu = MenuType.None;
	}

	private void CloseAllMenus()
	{
		shopCanvas.SetActive(false);
		pauseCanvas.SetActive(false);
		abilityCanvas.SetActive(false);
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
		CloseMenu();
		PauseGame(false);
	}

	public void QuitGame()
	{
		// In the Unity Editor, stop playing the game
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}

}
