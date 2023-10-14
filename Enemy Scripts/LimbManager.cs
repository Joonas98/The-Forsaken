using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbManager : MonoBehaviour
{
	[Header("Legs")]
	public GameObject leftLowerLeg;   // 1
	public GameObject leftUpperLeg;   // 2
	public GameObject rightLowerLeg;  // 3
	public GameObject rightUpperLeg;  // 4

	[Header("Arms")]
	public GameObject rightArm;       // 5
	public GameObject rightShoulder;  // 6
	public GameObject leftArm;           // 7
	public GameObject leftShoulder;    // 8

	[Header("Others")]
	public GameObject neck;  // 0
	public Transform head; // Used only for headshot effect
	public Enemy enemyScript;

	public ParticleSystem decapitationFX;
	public ParticleSystem dismembermentFX;

	public AudioSource audioSource;
	public AudioClip decapitationSound;
	public AudioClip[] dismembermentSounds;

	// This is the dismemberment or decapitation system
	// When deleting a limb, it's scale is changed to 0, 0, 0 and SFX + VFX are played
	public void RemoveLimb(int limbNumber)
	{
		ParticleSystem limbFX;
		switch (limbNumber)
		{
			case 0:
				// Headshot FX
				ParticleSystem fxGO = Instantiate(decapitationFX, neck.transform.position, neck.transform.rotation);
				fxGO.transform.parent = neck.transform; // Make the particle system follow the neck
				Destroy(fxGO.gameObject, decapitationFX.main.duration + 1f); // Let the emission stop and particles fade before deletion

				// Destructed limb FX
				limbFX = Instantiate(dismembermentFX, head.position, Quaternion.identity);
				Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f); // Let the emission stop and particles fade before deletion

				neck.transform.localScale = new Vector3(0, 0, 0);
				audioSource.PlayOneShot(decapitationSound, 10f);
				break;

			case 1:
				leftLowerLeg.transform.localScale = new Vector3(0, 0, 0);
				audioSource.PlayOneShot(dismembermentSounds[Random.Range(0, dismembermentSounds.Length)], 10f);

				// Destructed limb FX
				limbFX = Instantiate(dismembermentFX, leftLowerLeg.transform.position, Quaternion.identity);
				Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f); // Let the emission stop and particles fade before deletion

				if (!enemyScript.isCrawling)
					enemyScript.StartCrawling();
				break;

			case 2:
				leftUpperLeg.transform.localScale = new Vector3(0, 0, 0);
				audioSource.PlayOneShot(dismembermentSounds[Random.Range(0, dismembermentSounds.Length)], 10f);

				// Destructed limb FX
				limbFX = Instantiate(dismembermentFX, leftUpperLeg.transform.position, Quaternion.identity);
				Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f); // Let the emission stop and particles fade before deletion

				if (!enemyScript.isCrawling)
					enemyScript.StartCrawling();
				break;

			case 3:
				rightLowerLeg.transform.localScale = new Vector3(0, 0, 0);
				audioSource.PlayOneShot(dismembermentSounds[Random.Range(0, dismembermentSounds.Length)], 10f);

				// Destructed limb FX
				limbFX = Instantiate(dismembermentFX, rightLowerLeg.transform.position, Quaternion.identity);
				Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f); // Let the emission stop and particles fade before deletion

				if (!enemyScript.isCrawling)
					enemyScript.StartCrawling();
				break;

			case 4:
				rightUpperLeg.transform.localScale = new Vector3(0, 0, 0);
				audioSource.PlayOneShot(dismembermentSounds[Random.Range(0, dismembermentSounds.Length)], 10f);

				// Destructed limb FX
				limbFX = Instantiate(dismembermentFX, rightUpperLeg.transform.position, Quaternion.identity);
				Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f); // Let the emission stop and particles fade before deletion

				if (!enemyScript.isCrawling)
					enemyScript.StartCrawling();
				break;

			case 5:
				rightArm.transform.localScale = new Vector3(0, 0, 0);
				audioSource.PlayOneShot(dismembermentSounds[Random.Range(0, dismembermentSounds.Length)], 10f);

				// Destructed limb FX
				limbFX = Instantiate(dismembermentFX, rightArm.transform.position, Quaternion.identity);
				Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f); // Let the emission stop and particles fade before deletion
				break;

			case 6:
				rightShoulder.transform.localScale = new Vector3(0, 0, 0);
				audioSource.PlayOneShot(dismembermentSounds[Random.Range(0, dismembermentSounds.Length)], 10f);

				// Destructed limb FX
				limbFX = Instantiate(dismembermentFX, rightShoulder.transform.position, Quaternion.identity);
				Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f); // Let the emission stop and particles fade before deletion
				break;

			case 7:
				leftArm.transform.localScale = new Vector3(0, 0, 0);
				audioSource.PlayOneShot(dismembermentSounds[Random.Range(0, dismembermentSounds.Length)], 10f);

				// Destructed limb FX
				limbFX = Instantiate(dismembermentFX, leftArm.transform.position, Quaternion.identity);
				Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f); // Let the emission stop and particles fade before deletion
				break;

			case 8:
				leftShoulder.transform.localScale = new Vector3(0, 0, 0);
				audioSource.PlayOneShot(dismembermentSounds[Random.Range(0, dismembermentSounds.Length)], 10f);

				// Destructed limb FX
				limbFX = Instantiate(dismembermentFX, leftShoulder.transform.position, Quaternion.identity);
				Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f); // Let the emission stop and particles fade before deletion
				break;

			default:

				break;
		}
	}
}
