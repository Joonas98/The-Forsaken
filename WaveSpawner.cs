using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.UI.MTP;

// ! Vihollisten spawnaus systeemi. K‰ytet‰‰n ainoana skriptin‰ spawnaukseen
public class WaveSpawner : MonoBehaviour
{

    public StyleManager styleManager;
    public TextMeshProUGUI roundPopup;

    public bool useFloatingSpawn;

    public int waveCount;
    public int baseEnemyCount;
    public int enemyCountIncrease;

    public float spawnRate;
    public float waveLenght;
    public float spawnRadius;

    public Transform[] spawnPoints;
    public ParticleSystem[] spawnParticleSystems;
    public GameObject[] enemyPrefabs;

    public AudioSource audioSource;
    public AudioClip waveStartSound;

    private int waveNumber = 0;
    private GameObject playerGO;
    private float floatingHeight = 500f;

    private void Awake()
    {
        playerGO = GameObject.Find("Player");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(StartWaves());
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(playerGO.transform.position.x, floatingHeight, playerGO.transform.position.z);
    }

    IEnumerator StartWaves()
    {
        for (int i = 0; i < waveCount; i++)
        {
            GameManager.UpdateWaveNumber(waveNumber + 1);
            audioSource.PlayOneShot(waveStartSound);
            // Debug.Log("Starting wave: " + (waveNumber + 1));
            int _enemiesToSpawn = baseEnemyCount + (enemyCountIncrease * waveNumber);

            if (!useFloatingSpawn)
            {
                StartCoroutine(Spawn(_enemiesToSpawn));
            }
            else
            {
                StartCoroutine(SpawnFromAbove(_enemiesToSpawn));
            }
            waveNumber++;

            roundPopup.text = "ROUND " + waveNumber.ToString();
            styleManager.Play();

            yield return new WaitForSeconds(waveLenght);
        }
    }

    IEnumerator Spawn(int x) // Yksitt‰isen vihollisen spawnaaminen
    {
        for (int i = 0; i < x; i++)
        {
            int randomNumber = Random.Range(0, spawnPoints.Length);
            Vector3 positionToSpawn = spawnParticleSystems[randomNumber].transform.position;
            if (!spawnParticleSystems[randomNumber].isPlaying) spawnParticleSystems[randomNumber].Play();
            GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            GameObject newGO = Instantiate(enemyToSpawn, positionToSpawn, Quaternion.identity);
            GameManager.enemyCount++;
            GameManager.UpdateEnemyCount();

            yield return new WaitForSeconds(spawnRate);
        }
    }

    IEnumerator SpawnFromAbove(int x)
    {
        for (int i = 0; i < x; i++)
        {
            float spawnPointX = Random.Range(spawnRadius * -1, spawnRadius) + transform.position.x;
            float spawnPointZ = Random.Range(spawnRadius * -1, spawnRadius) + transform.position.z;

            Vector3 spawnPosition = new Vector3(spawnPointX, transform.position.y, spawnPointZ);
            Ray ray = new Ray(spawnPosition, -transform.up);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                Debug.DrawRay(spawnPosition, -transform.up, Color.red);
                spawnPosition = new Vector3(hitInfo.point.x, hitInfo.point.y + 1f, hitInfo.point.z);

                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                GameObject newGO = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                GameManager.enemyCount++;
                GameManager.UpdateEnemyCount();
            }
            else
            {
                Debug.Log("Spawn ray missed");
            }
            yield return new WaitForSeconds(spawnRate);
        }
    }

}
