using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbManager : MonoBehaviour
{

    [Header("Legs")]
    public GameObject LeftLowerLeg;   // 1
    public GameObject LeftUpperLeg;   // 2
    public GameObject RightLowerLeg;  // 3
    public GameObject RightUpperLeg;  // 4

    [Header("Arms")]
    public GameObject RightArm;       // 5
    public GameObject RightShoulder;  // 6
    public GameObject LeftArm;           // 7
    public GameObject LeftShoulder;    // 8

    [Header("Others")]
    public GameObject Neck;              // 0
    public Enemy enemyScript;

    public ParticleSystem headshotFX;
    public ParticleSystem normalHitFX;

    public AudioSource audioSource;
    public AudioClip decapitationSound;
    public AudioClip loseLegSound;

    public AudioClip[] loseLimbSounds;

    public void RemoveLimb(int limbNumber)
    {
        if (limbNumber == 0)
        {
            ParticleSystem headshotFXGO = Instantiate(headshotFX, Neck.transform.position, Quaternion.LookRotation(Vector3.up));
            headshotFXGO.transform.parent = Neck.transform;
            Destroy(headshotFXGO.gameObject, 2f);

            ParticleSystem basicFXGO = Instantiate(normalHitFX, Neck.transform.position, Quaternion.LookRotation(Vector3.up));
            basicFXGO.transform.parent = Neck.transform;
            Destroy(basicFXGO.gameObject, 2f);

            Neck.transform.localScale = new Vector3(0, 0, 0);
            audioSource.PlayOneShot(decapitationSound);
        }

        if (limbNumber == 1)
        {
            LeftLowerLeg.transform.localScale = new Vector3(0, 0, 0);
            audioSource.PlayOneShot(loseLimbSounds[Random.Range(0, loseLimbSounds.Length)]);

            if (!enemyScript.isCrawling)
                enemyScript.StartCrawling();
        }

        if (limbNumber == 2)
        {
            LeftUpperLeg.transform.localScale = new Vector3(0, 0, 0);
            audioSource.PlayOneShot(loseLimbSounds[Random.Range(0, loseLimbSounds.Length)]);

            if (!enemyScript.isCrawling)
                enemyScript.StartCrawling();
        }

        if (limbNumber == 3)
        {
            RightLowerLeg.transform.localScale = new Vector3(0, 0, 0);
            audioSource.PlayOneShot(loseLimbSounds[Random.Range(0, loseLimbSounds.Length)]);

            if (!enemyScript.isCrawling)
                enemyScript.StartCrawling();
        }

        if (limbNumber == 4)
        {
            RightUpperLeg.transform.localScale = new Vector3(0, 0, 0);
            audioSource.PlayOneShot(loseLimbSounds[Random.Range(0, loseLimbSounds.Length)]);

            if (!enemyScript.isCrawling)
                enemyScript.StartCrawling();
        }

        if (limbNumber == 5)
        {
            RightArm.transform.localScale = new Vector3(0, 0, 0);
            audioSource.PlayOneShot(loseLimbSounds[Random.Range(0, loseLimbSounds.Length)]);
        }

        if (limbNumber == 6)
        {
            RightShoulder.transform.localScale = new Vector3(0, 0, 0);
            audioSource.PlayOneShot(loseLimbSounds[Random.Range(0, loseLimbSounds.Length)]);
        }

        if (limbNumber == 7)
        {
            LeftArm.transform.localScale = new Vector3(0, 0, 0);
            audioSource.PlayOneShot(loseLimbSounds[Random.Range(0, loseLimbSounds.Length)]);
        }

        if (limbNumber == 8)
        {
            LeftShoulder.transform.localScale = new Vector3(0, 0, 0);
            audioSource.PlayOneShot(loseLimbSounds[Random.Range(0, loseLimbSounds.Length)]);
        }
    }

}
