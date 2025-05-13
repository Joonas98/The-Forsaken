using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrenadeThrow : MonoBehaviour
{
	public static GrenadeThrow instance;

	[Header("Throw Settings")]
	public float throwForce;
	public Transform throwingPosition;
	public float grenadeSelectionTimeSlow; // Slow time down when selecting grenade

	[HideInInspector]
	public PlayerInventory.GrenadeType selectedGrenade = PlayerInventory.GrenadeType.Normal;

	[Header("Grenade Prefabs")]
	public GameObject normalGrenadePrefab;
	public GameObject impactGrenadePrefab;
	public GameObject incendiaryGrenadePrefab;

	[Header("UI")]
	public GameObject selectionMenu;
	public Image[] grenadePanels;
	public Color defaultColor;
	public Color highlightColor;

	public Image grenadeImageHUD;
	public Sprite[] grenadeSprites;
	public TextMeshProUGUI throwNadeTMP;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
			return;
		}

		// Initialize UI to current selection
		UpdateSelectionUI((int)selectedGrenade);
	}

	private void Start()
	{
		throwNadeTMP.text = KeybindManager.Instance.throwGrenadeKey.ToString();
	}

	private void Update()
	{
		if (Time.timeScale <= 0) return; // Game paused
		if (Input.GetKeyDown(KeybindManager.Instance.throwGrenadeKey))
		{
			ThrowGrenade();
		}
	}

	public void ThrowGrenade()
	{
		// Check inventory using enum from PlayerInventory
		if (PlayerInventory.instance.GetGrenadeCount(selectedGrenade) <= 0)
			return;

		// Instantiate selected prefab
		GameObject prefab = GetGrenadePrefab(selectedGrenade);
		GameObject newGrenade = Instantiate(prefab, throwingPosition.position, Camera.main.transform.rotation);

		// Apply force
		Rigidbody rb = newGrenade.GetComponent<Rigidbody>();
		rb.AddForce(newGrenade.transform.forward * throwForce);

		// Reduce grenade count via enum
		PlayerInventory.instance.HandleGrenades(selectedGrenade, -1);
	}

	private GameObject GetGrenadePrefab(PlayerInventory.GrenadeType type)
	{
		switch (type)
		{
			case PlayerInventory.GrenadeType.Impact:
				return impactGrenadePrefab;
			case PlayerInventory.GrenadeType.Incendiary:
				return incendiaryGrenadePrefab;
			case PlayerInventory.GrenadeType.Normal:
			default:
				return normalGrenadePrefab;
		}
	}

	/// <summary>
	/// Select grenade by enum index (0 = Normal, 1 = Impact, 2 = Incendiary).
	/// </summary>
	public void SelectGrenade(int index)
	{
		int maxTypes = System.Enum.GetValues(typeof(PlayerInventory.GrenadeType)).Length;
		if (index < 0 || index >= maxTypes)
			index = 0;

		selectedGrenade = (PlayerInventory.GrenadeType)index;
		UpdateSelectionUI(index);
	}

	private void UpdateSelectionUI(int index)
	{
		// Reset all panels
		for (int i = 0; i < grenadePanels.Length; i++)
		{
			grenadePanels[i].color = defaultColor;
		}

		// Highlight selected
		grenadePanels[index].color = highlightColor;
		grenadeImageHUD.sprite = grenadeSprites[index];

		// Update the grenade count UI for this type
		PlayerInventory.instance.UpdateGrenadeUI((PlayerInventory.GrenadeType)index);
	}
}
