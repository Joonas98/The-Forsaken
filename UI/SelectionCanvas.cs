using UnityEngine;

public class SelectionCanvas : MonoBehaviour
{
	public static SelectionCanvas instance;
	public GameObject selectionMenu;

	[Header("Selection Settings")]
	public bool isChoosingObject = false;
	[Tooltip("Time scale when the object selection menu is active.")]
	[SerializeField] private float objectSelectionTimeSlow = 0.1f;

	private void Awake()
	{
		// Singleton pattern
		if (instance == null)
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		// Open the selection menu while the hotkey is held,
		// and close it when the hotkey is released.
		if (Input.GetKey(KeybindManager.Instance.selectionMenuKey))
		{
			OpenSelectionMenu();
		}
		else
		{
			CloseSelectionMenu();
		}
	}

	/// <summary>
	/// Opens the object selection menu.
	/// Disables camera rotation, unlocks the cursor, slows time,
	/// and stops placement if active.
	/// </summary>
	private void OpenSelectionMenu()
	{
		// If already open, do nothing.
		if (isChoosingObject)
			return;

		selectionMenu.SetActive(true);
		isChoosingObject = true;
		MouseLook.instance.canRotate = false;
		Cursor.lockState = CursorLockMode.None;
		Time.timeScale = objectSelectionTimeSlow;

		// If an object is being placed, stop that process.
		if (ObjectPlacing.instance.isPlacing)
		{
			ObjectPlacing.instance.StopPlacing();
		}
	}

	/// <summary>
	/// Closes the object selection menu.
	/// Re-enables camera rotation, locks the cursor, and resets time scale.
	/// </summary>
	private void CloseSelectionMenu()
	{
		// If already closed, do nothing.
		if (!isChoosingObject)
			return;

		selectionMenu.SetActive(false);
		isChoosingObject = false;
		MouseLook.instance.canRotate = true;
		Cursor.lockState = CursorLockMode.Locked;
		Time.timeScale = 1f;
	}
}
