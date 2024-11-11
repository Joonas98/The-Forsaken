using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoHUD : MonoBehaviour
{
	public TextMeshProUGUI currentAmmoText, maxAmmoText;
	public Image ammoBar;
	public static AmmoHUD Instance;

	private Vector3 originalScale = Vector3.zero; // Used for pulsing ammo count

	private int currentMaxMag; // Local variable

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

	public void UpdateAllAmmoUI(int currentAmmo, int maxAmmoCount)
	{
		UpdateAmmoUI(currentAmmo);
		UpdateMaxAmmoUi(maxAmmoCount);
	}

	// Update only current ammo, like shoot or reload
	public void UpdateAmmoUI(int currentAmmo)
	{
		currentAmmoText.text = currentAmmo.ToString();
		UpdateAmmoBar(currentAmmo);
		StartCoroutine(PulseText(currentAmmoText, 0.1f, 1.2f));

		if (currentAmmo == currentMaxMag) StartCoroutine(PulseText(maxAmmoText, 0.1f, 1.2f));
	}

	// Update the max ammo count, like change weapon or upgrade magazine size
	public void UpdateMaxAmmoUi(int maxAmmoCount)
	{
		maxAmmoText.text = maxAmmoCount.ToString();
		currentMaxMag = maxAmmoCount;
	}

	public void UpdateAmmoBar(int currentAmmo)
	{
		// Calculate the fill amount based on current ammo percentage
		float fillAmount = (float)currentAmmo / currentMaxMag;
		ammoBar.fillAmount = fillAmount;
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
}
