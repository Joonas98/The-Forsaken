using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RagdollSoundManager : MonoBehaviour
{
	[Header("Audio")]
	public AudioClip[] impactClips;          // Assign several thud/bone‐crack clips
	public GameObject oneShotAudioPrefab;    // A prefab with just an AudioSource set to PlayOnAwake = false

	[Header("Settings")]
	public float minImpactVelocity = 2f;     // Only play sounds if relative speed > this
	public float volumeScale = 1f;           // Base volume multiplier
	public float pitchVariance = 0.1f;       // +- pitch randomization

	[Header("Cooldown")]
	public float minInterval = 0.1f;         // Don’t spam sounds if you have a flurry of small hits
	private float lastPlayTime;

	private void OnCollisionEnter(Collision collision)
	{
		// Calculate collision intensity by looking at relative velocity along contact normal
		float intensity = Vector3.Project(collision.relativeVelocity, collision.GetContact(0).normal).magnitude;

		if (intensity < minImpactVelocity) return;
		if (Time.time < lastPlayTime + minInterval) return;
		lastPlayTime = Time.time;

		// Spawn a one‐shot audio source at contact point
		if (impactClips.Length == 0 || oneShotAudioPrefab == null) return;

		var contactPoint = collision.GetContact(0).point;
		var go = Instantiate(oneShotAudioPrefab, contactPoint, Quaternion.identity);
		var src = go.GetComponent<AudioSource>();
		if (src == null)
		{
			Debug.LogWarning("Prefab needs an AudioSource component!");
			Destroy(go, 0.1f);
			return;
		}

		// Pick a random clip & apply some variation
		src.clip = impactClips[Random.Range(0, impactClips.Length)];
		src.volume = Mathf.Clamp01(intensity * 0.1f) * volumeScale;
		src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);

		src.Play();
		Destroy(go, src.clip.length + 0.1f);
	}
}
