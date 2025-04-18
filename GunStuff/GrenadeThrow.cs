using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrenadeThrow : MonoBehaviour
{
	// Variables
	public float throwForce;
	public float throwForceImpact;
	public Transform throwingPosition;
	public float grenadeSelectionTimeSlow; // Slow time down when selecting grenade
	[HideInInspector] public int selectedGrenade = 0; // 0 = normal, 1 = impact, 2 = incendiary

	public GameObject normalGrenadePrefab, impactGrenadePrefab, incendiaryGrenadePrefab;
	public GameObject selectionMenu;
	public static GrenadeThrow instance;

	[Header("UI")]
	public Image[] grenadePanels;
	public Color defaultColor, highlightColor;

	public Image grenadeImageHUD;
	public Sprite[] grenadeSprites;
	public TextMeshProUGUI changeNadeTMP, throwNadeTMP;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(instance);
		}
		SelectGrenade(0);
	}

	private void Start()
	{
		changeNadeTMP.text = KeybindManager.Instance.selectionMenuKey.ToString();
		throwNadeTMP.text = KeybindManager.Instance.throwGrenadeKey.ToString();
	}

	private void Update()
	{
		HandleInputs();
	}

	private void HandleInputs()
	{
		if (Time.timeScale <= 0) return; // Game paused

		if (Input.GetKeyDown(KeybindManager.Instance.throwGrenadeKey))
		{
			ThrowGrenade();
		}
	}

	public void ThrowGrenade()
	{
		if (PlayerInventory.instance.GetGrenadeCount(selectedGrenade) <= 0) return;
		GameObject newGrenade;
		switch (selectedGrenade)
		{
			case 0:
				newGrenade = Instantiate(normalGrenadePrefab, throwingPosition.position, Camera.main.transform.rotation);
				break;

			case 1:
				newGrenade = Instantiate(impactGrenadePrefab, throwingPosition.position, Camera.main.transform.rotation);
				break;

			case 2:
				newGrenade = Instantiate(incendiaryGrenadePrefab, throwingPosition.position, Camera.main.transform.rotation);
				break;

			case 3:
				newGrenade = Instantiate(normalGrenadePrefab, throwingPosition.position, Camera.main.transform.rotation);
				break;

			default:
				newGrenade = Instantiate(normalGrenadePrefab, throwingPosition.position, Camera.main.transform.rotation);
				break;
		}

		Rigidbody rb = newGrenade.GetComponent<Rigidbody>();
		rb.AddForce(newGrenade.transform.forward * throwForce);

		PlayerInventory.instance.HandleGrenades(selectedGrenade, -1);
	}

	public void SelectGrenade(int index)
	{
		grenadePanels[selectedGrenade].color = defaultColor; // Previous selection to default color
		grenadePanels[index].color = highlightColor; // Highlight new selection
		selectedGrenade = index; // Update selectedGrenade variable for other uses

		grenadeImageHUD.sprite = grenadeSprites[index];
	}
}
