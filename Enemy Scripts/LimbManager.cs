using UnityEngine;

public class LimbManager : MonoBehaviour
{
	public enum Limb
	{
		Head = 0,
		LeftLowerLeg = 1,
		LeftUpperLeg = 2,
		RightLowerLeg = 3,
		RightUpperLeg = 4,
		RightArm = 5,
		RightShoulder = 6,
		LeftArm = 7,
		LeftShoulder = 8
	}

	[Header("Limb Objects")]
	[Tooltip("Assign limbs in order:\n" +
			 "0: Head\n" +
			 "1: LeftLowerLeg\n" +
			 "2: LeftUpperLeg\n" +
			 "3: RightLowerLeg\n" +
			 "4: RightUpperLeg\n" +
			 "5: RightArm\n" +
			 "6: RightShoulder\n" +
			 "7: LeftArm\n" +
			 "8: LeftShoulder")]
	public GameObject[] limbObjects;

	[Header("Head & Enemy")]
	public Transform head; // For headshot effect
	public Enemy enemyScript;

	[Header("Effects")]
	public ParticleSystem decapitationFX;
	public ParticleSystem dismembermentFX;

	[Header("Audio")]
	public AudioSource audioSource;
	public AudioClip decapitationSound;
	public AudioClip[] dismembermentSounds;

	public void RemoveLimb(Limb limb)
	{
		GameObject limbObject = limbObjects[(int)limb];
		bool applyCrawling = false;
		bool isHead = false;
		bool doDismemberment = true;

		switch (limb)
		{
			case Limb.Head:
				// Decapitation effect
				decapitationFX.Play();

				// Scale the neck object
				limbObject.transform.localScale = Vector3.zero;

				// The collider is on the 'head' object, not on the neck
				Collider headCollider = head.GetComponent<Collider>();
				if (headCollider != null)
				{
					headCollider.enabled = false;
				}

				audioSource.PlayOneShot(decapitationSound, 10f);
				isHead = true;
				break;

			case Limb.LeftShoulder:
				limbObject.transform.localScale = Vector3.zero;
				limbObject.GetComponent<Collider>().enabled = false;

				var leftArm = limbObjects[(int)Limb.LeftArm];
				leftArm.GetComponent<Collider>().enabled = false;
				break;

			case Limb.RightShoulder:
				limbObject.transform.localScale = Vector3.zero;
				limbObject.GetComponent<Collider>().enabled = false;

				var rightArm = limbObjects[(int)Limb.RightArm];
				rightArm.GetComponent<Collider>().enabled = false;
				break;

			case Limb.LeftUpperLeg:
				limbObject.transform.localScale = Vector3.zero;
				limbObject.GetComponent<Collider>().enabled = false;

				var leftLowerLeg = limbObjects[(int)Limb.LeftLowerLeg];
				leftLowerLeg.GetComponent<Collider>().enabled = false;

				applyCrawling = true;
				break;

			case Limb.RightUpperLeg:
				limbObject.transform.localScale = Vector3.zero;
				limbObject.GetComponent<Collider>().enabled = false;

				var rightLowerLeg = limbObjects[(int)Limb.RightLowerLeg];
				rightLowerLeg.GetComponent<Collider>().enabled = false;

				applyCrawling = true;
				break;

			case Limb.LeftLowerLeg:
				limbObject.transform.localScale = Vector3.zero;
				limbObject.GetComponent<Collider>().enabled = false;

				if (!enemyScript.isCrawling)
					applyCrawling = true;
				break;

			case Limb.RightLowerLeg:
				limbObject.transform.localScale = Vector3.zero;
				limbObject.GetComponent<Collider>().enabled = false;

				if (!enemyScript.isCrawling)
					applyCrawling = true;
				break;

			case Limb.LeftArm:
				limbObject.transform.localScale = Vector3.zero;
				limbObject.GetComponent<Collider>().enabled = false;
				break;

			case Limb.RightArm:
				limbObject.transform.localScale = Vector3.zero;
				limbObject.GetComponent<Collider>().enabled = false;
				break;

			default:
				doDismemberment = false;
				break;
		}

		// Apply crawling if needed
		if (applyCrawling && !enemyScript.isCrawling)
		{
			enemyScript.StartCrawling();
		}

		// Dismemberment FX & SFX
		if (doDismemberment && limbObject != null)
		{
			Vector3 fxPosition = isHead ? head.position : limbObject.transform.position;
			ParticleSystem limbFX = Instantiate(dismembermentFX, fxPosition, Quaternion.identity);
			Destroy(limbFX.gameObject, dismembermentFX.main.duration + 1f);

			if (!isHead)
			{
				audioSource.PlayOneShot(dismembermentSounds[Random.Range(0, dismembermentSounds.Length)], 10f);
			}
		}
	}
}
