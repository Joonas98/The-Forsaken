using UnityEngine;

/// <summary>
/// The single entry point for all sounds emitted by the player.
/// </summary>
public class PlayerAudioManager : MonoBehaviour
{
    public static PlayerAudioManager Instance { get; private set; }

    [Header("Audio source")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource breathingAudioSource;

    [Header("Player sounds")]
    [SerializeField] private AudioClip[] damageSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private AudioClip[] kickSounds;
    [SerializeField] private AudioClip[] idleSounds;
    [SerializeField] private AudioClip regenSound;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] grassSteps;
    [SerializeField] private AudioClip[] rockSteps;
    [SerializeField] private AudioClip[] sandSteps;

    [Header("Idle timing")]
    [SerializeField, Min(0f)] private float idleDelay = 8f;
    [SerializeField, Min(0f)] private float idleRandomDelay = 4f;

    [Header("Breathing")]
    [SerializeField] private AudioClip breathingClip;
    [SerializeField, Range(0f, 1f)] private float breathingStartStamina = 0.5f;
    [SerializeField, Range(0f, 1f)] private float breathingMinVolume = 0.05f;
    [SerializeField, Range(0f, 1f)] private float breathingMaxVolume = 1f;

    private float nextIdleTime;
    private float lastNonFootstepSoundTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (breathingAudioSource == null)
        {
            GameObject breathingObject = new GameObject("BreathingAudioSource");
            breathingObject.transform.SetParent(transform);
            breathingAudioSource = breathingObject.AddComponent<AudioSource>();
            breathingAudioSource.spatialBlend = 0f;
        }

        breathingAudioSource.loop = true;
        breathingAudioSource.playOnAwake = false;
        breathingAudioSource.clip = breathingClip;
        lastNonFootstepSoundTime = Time.time;
        ScheduleNextIdle();
    }

    private void Update()
    {
        UpdateBreathing();

        if (Time.timeScale <= 0f || idleSounds == null || idleSounds.Length == 0)
            return;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement == null || !movement.isGrounded || !movement.isStationary)
            return;

        if (Time.time >= nextIdleTime)
            PlayIdle();
    }

    public void PlayDamage()
    {
        PlayRandom(damageSounds);
    }

    public void PlayJump()
    {
        PlayRandom(jumpSounds);
    }

    public void PlayKick()
    {
        PlayRandom(kickSounds);
    }

    public void PlayIdle()
    {
        PlayRandom(idleSounds);
    }

    public void PlayRegen()
    {
		if (audioSource != null && regenSound != null)
		{
			audioSource.PlayOneShot(regenSound, 1f);
			RegisterNonFootstepSound();
		}
	}

    public void PlayFootstep(int terrainTextureIndex, float volume)
    {
        AudioClip[] clips = terrainTextureIndex switch
        {
            0 => grassSteps,
            1 => rockSteps,
            2 => rockSteps,
            3 => sandSteps,
            _ => rockSteps
        };

        PlayRandom(clips, volume);
    }

    public void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
            RegisterNonFootstepSound();
        }
    }

    private void PlayRandom(AudioClip[] clips, float volume = 1f)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
            RegisterNonFootstepSound();
        }
    }

    private void ScheduleNextIdle()
    {
        nextIdleTime = lastNonFootstepSoundTime + idleDelay + Random.Range(0f, idleRandomDelay);
    }

    private void RegisterNonFootstepSound()
    {
        lastNonFootstepSoundTime = Time.time;
        ScheduleNextIdle();
    }

    private void UpdateBreathing()
    {
        if (breathingAudioSource == null || breathingClip == null || Player.instance == null || Player.instance.maxSisu <= 0)
        {
            StopBreathing();
            return;
        }

        breathingAudioSource.clip = breathingClip;
        float staminaPercent = Mathf.Clamp01(Player.instance.currentSisu / Player.instance.maxSisu);
        if (staminaPercent >= breathingStartStamina || breathingStartStamina <= 0f)
        {
            StopBreathing();
            return;
        }

        float fatigue = 1f - (staminaPercent / breathingStartStamina);
        breathingAudioSource.volume = Mathf.Lerp(breathingMinVolume, breathingMaxVolume, fatigue);
        if (!breathingAudioSource.isPlaying)
            breathingAudioSource.Play();
    }

    private void StopBreathing()
    {
        if (breathingAudioSource != null && breathingAudioSource.isPlaying)
            breathingAudioSource.Stop();
    }
}
