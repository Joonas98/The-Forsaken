using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoHUD : MonoBehaviour
{
	public TextMeshProUGUI currentAmmoText, maxAmmoText;
	public Image ammoBar;
	public static AmmoHUD Instance;
	public Gradient ammoCountGradient;

	private Vector3 originalScale = Vector3.zero; // Used for pulsing ammo count
	private Vector3 originalScaleBar = Vector3.zero; // Used for pulsing ammo bar
	private float currentAmmoPercentage = 0;

	private void Awake()
	{
		// Singleton
		if (Instance == null)
		{
			//DontDestroyOnLoad(gameObject);
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		// No ammohud at start
		AmmoHUD.Instance.DisableHUD();
	}

	public void UpdateAmmoHUD(int currentAmmo, int maxAmmoCount)
	{
		// E.g. switching from melee weapon to gun
		if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

		currentAmmoPercentage = (float)currentAmmo / (float)maxAmmoCount;
		currentAmmoText.text = currentAmmo.ToString();
		maxAmmoText.text = maxAmmoCount.ToString();

		// Ammo bar
		ammoBar.fillAmount = currentAmmoPercentage;
		currentAmmoText.color = ammoCountGradient.Evaluate(currentAmmoPercentage);
		ammoBar.color = ammoCountGradient.Evaluate(currentAmmoPercentage);

		// Pulse current ammo text
		StartCoroutine(PulseText(currentAmmoText, 0.1f, 1.2f));

		// Pulse ammo bar
		StartCoroutine(PulseSprite(ammoBar, 0.1f, 1.3f));

		// Magazine filled, reload etc.
		if (currentAmmo == maxAmmoCount) StartCoroutine(PulseText(maxAmmoText, 0.2f, 1.4f));
	}

	public void DisableHUD()
	{
		gameObject.SetActive(false);
	}

	public IEnumerator PulseText(TextMeshProUGUI text, float duration = 0.1f, float maxScale = 1.2f)
	{
		// First time, save the original scale
		if (originalScale == Vector3.zero)
			originalScale = text.rectTransform.localScale;

		float time = 0f;

		while (time < duration)
		{
			// Calculate the scale factor based on time
			float scale = Mathf.Lerp(1f, maxScale, Mathf.PingPong(time * 2 / duration, 1));
			text.rectTransform.localScale = originalScale * scale;

			time += Time.deltaTime;
			yield return null;
		}

		// Reset the scale to the original at the end
		text.rectTransform.localScale = originalScale;
	}

	public IEnumerator PulseSprite(Image sprite, float duration = 0.1f, float maxScale = 1.2f)
	{
		// First time, save the original scale
		if (originalScaleBar == Vector3.zero)
			originalScaleBar = sprite.rectTransform.localScale;

		float time = 0f;

		while (time < duration)
		{
			// Calculate the scale factor based on time
			float scale = Mathf.Lerp(1f, maxScale, Mathf.PingPong(time * 2 / duration, 1));
			sprite.rectTransform.localScale = originalScaleBar * scale;

			time += Time.deltaTime;
			yield return null;
		}

		// Reset the scale to the original at the end
		sprite.rectTransform.localScale = originalScaleBar;
	}
}
