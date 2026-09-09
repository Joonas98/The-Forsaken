using UnityEngine;

// Base class for abilities
// ScritableObject holds static data, runtime parts are found in AbilityHolder.cs
public class Ability : ScriptableObject
{
	public new string name;
	[TextArea] public string abilityDescription;
	public Sprite picture;

	// Active abilities activate with hotkey
	// Automatic are automatically activated, e.g. take damage to receive 10 second dmg buff
	// Passive abilities have constant buff, no cooldowns or active times
	public enum AbilityType { Active, Automatic, Passive }
	public AbilityType abilityType;

	public float cooldownTime, activeTime;
	public AudioClip activateSFX, endSFX;
	public AudioSource audioSource;

	protected virtual void OnEnable()
	{

	}

	public virtual void InitializeAbility()
	{

	}

	public virtual void Activate(GameObject parent) // Call base.Activate(parent) in all abilities
	{
		PlayPlayerSound(activateSFX);
	}

	public virtual void BeginCooldown(GameObject parent)
	{
		PlayPlayerSound(endSFX);
	}

	protected void PlayPlayerSound(AudioClip clip)
	{
		if (clip == null) return;

		if (PlayerAudioManager.Instance != null)
			PlayerAudioManager.Instance.PlayClip(clip);
		else if (audioSource != null)
			audioSource.PlayOneShot(clip);
	}
}
