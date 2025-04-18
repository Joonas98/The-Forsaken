using SCPE;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingController : MonoBehaviour
{
	[HideInInspector] public static PostProcessingController Instance { get; private set; }

	[Header("Post Process Volume")]
	public PostProcessVolume volume;

	private ColorSplit colorSplit;
	private Sharpen sharpen;
	private Danger danger;

	[Header("Kill Effect Settings")]
	public float maxSplitAmount = 1f;
	public float maxSharpenAmount = 1f;
	public float rampUpDuration = 0.05f;
	public float rampDownDuration = 0.2f;
	private Coroutine killEffectCoroutine;

	[Header("Damage Flash Settings")]
	public float damageVignetteAmount = 0.5f;
	public float damageRampUpDuration = 0.1f;
	public float damageRampDownDuration = 0.5f;
	private Coroutine damageEffectCoroutine;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else if (Instance != this)
			Destroy(gameObject);
	}

	private void Start()
	{
		if (volume.profile == null)
		{
			Debug.LogError("PostProcessingController: No profile set on PostProcessVolume.");
			return;
		}
		volume.profile.TryGetSettings(out colorSplit);
		volume.profile.TryGetSettings(out sharpen);
		volume.profile.TryGetSettings(out danger);
	}

	public void OnPlayerKill()
	{
		if (killEffectCoroutine != null)
			StopCoroutine(killEffectCoroutine);

		killEffectCoroutine = StartCoroutine(KillEffectRoutine());
	}

	public static void TriggerKillEffect() => Instance?.OnPlayerKill();

	private IEnumerator KillEffectRoutine()
	{
		if (colorSplit == null && sharpen == null)
			yield break;

		float initialSplit = colorSplit != null ? colorSplit.offset.value : 0f;
		float initialSharpen = sharpen != null ? sharpen.amount.value : 0f;
		float targetSplit = maxSplitAmount;
		float targetSharpen = maxSharpenAmount;

		if (colorSplit != null)
			targetSplit = Mathf.Clamp(targetSplit, 0f, 1f);
		if (sharpen != null)
			targetSharpen = Mathf.Clamp(targetSharpen, 0f, 1f);

		bool needRampUp = initialSplit < targetSplit || initialSharpen < targetSharpen;

		if (needRampUp)
		{
			float elapsed = 0f;
			while (elapsed < rampUpDuration)
			{
				elapsed += Time.deltaTime;
				float t = Mathf.Clamp01(elapsed / rampUpDuration);
				if (colorSplit != null)
					colorSplit.offset.value = Mathf.Lerp(initialSplit, targetSplit, t);
				if (sharpen != null)
					sharpen.amount.value = Mathf.Lerp(initialSharpen, targetSharpen, t);
				yield return null;
			}
			if (colorSplit != null) colorSplit.offset.value = targetSplit;
			if (sharpen != null) sharpen.amount.value = targetSharpen;
		}
		else
		{
			if (colorSplit != null) colorSplit.offset.value = targetSplit;
			if (sharpen != null) sharpen.amount.value = targetSharpen;
		}

		float elapsedDown = 0f;
		float startSplit = colorSplit != null ? colorSplit.offset.value : 0f;
		float startSharpen = sharpen != null ? sharpen.amount.value : 0f;

		while (elapsedDown < rampDownDuration)
		{
			elapsedDown += Time.deltaTime;
			float t = Mathf.Clamp01(elapsedDown / rampDownDuration);
			if (colorSplit != null)
				colorSplit.offset.value = Mathf.Lerp(startSplit, 0f, t);
			if (sharpen != null)
				sharpen.amount.value = Mathf.Lerp(startSharpen, 0f, t);
			yield return null;
		}
		if (colorSplit != null) colorSplit.offset.value = 0f;
		if (sharpen != null) sharpen.amount.value = 0f;

		killEffectCoroutine = null;
	}

	public void OnPlayerDamageFlash()
	{
		if (damageEffectCoroutine != null)
			StopCoroutine(damageEffectCoroutine);

		damageEffectCoroutine = StartCoroutine(DamageFlashRoutine());
	}

	public static void TriggerDamageFlash() => Instance?.OnPlayerDamageFlash();

	private IEnumerator DamageFlashRoutine()
	{
		if (danger == null)
			yield break;

		float initialVignette = danger.size.value;
		float targetVignette = Mathf.Min(damageVignetteAmount, 1f);

		bool needRampUp = initialVignette < targetVignette;

		if (needRampUp)
		{
			float elapsed = 0f;
			while (elapsed < damageRampUpDuration)
			{
				elapsed += Time.deltaTime;
				float t = Mathf.Clamp01(elapsed / damageRampUpDuration);
				danger.size.value = Mathf.Lerp(initialVignette, targetVignette, t);
				yield return null;
			}
			danger.size.value = targetVignette;
		}
		else
		{
			danger.size.value = targetVignette;
		}

		float elapsedDown = 0f;
		float startVignette = danger.size.value;

		while (elapsedDown < damageRampDownDuration)
		{
			elapsedDown += Time.deltaTime;
			float t = Mathf.Clamp01(elapsedDown / damageRampDownDuration);
			danger.size.value = Mathf.Lerp(startVignette, 0f, t);
			yield return null;
		}
		danger.size.value = 0f;

		damageEffectCoroutine = null;
	}
}
