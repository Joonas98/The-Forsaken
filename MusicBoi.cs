using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBoi : MonoBehaviour
{

    public AudioSource audioSource;

    public List<AudioClip> musicClips = new List<AudioClip>();

    // Start is called before the first frame update
    void Start()
    {
        audioSource.ignoreListenerPause = true;
        audioSource.clip = musicClips[Random.Range(0, musicClips.Count)];
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
