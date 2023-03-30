using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.UI.MTP;

// ! Only script to spawn enemies with
public class WaveSpawner : MonoBehaviour
{
    public StyleManager styleManager;
    public TextMeshProUGUI roundPopup1, roundPopup2;

    public bool useFloatingSpawn;

    public int waveCount;
    public int baseEnemyCount;
    public int enemyCountIncrease;

    public float spawnRate;
    public float waveLenght;
    public float spawnRadius, spawnProtection;

    public Transform[] spawnPoints;
    public ParticleSystem[] spawnParticleSystems;
    public GameObject[] enemyPrefabs;

    public AudioSource audioSource;
    public AudioClip waveStartSound;

    public LayerMask groundLayer;

    public GameObject spawnDebugPrefab;
    // Privates
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
            GameManager.GM.UpdateWaveNumber(waveNumber + 1);
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

            roundPopup1.text = "ROUND " + waveNumber.ToString();
            roundPopup2.text = "ROUND " + waveNumber.ToString();
            styleManager.Play();

            yield return new WaitForSeconds(waveLenght);
        }
    }

    IEnumerator Spawn(int x) // Yksittäisen vihollisen spawnaaminen
    {
        for (int i = 0; i < x; i++)
        {
            int randomNumber = Random.Range(0, spawnPoints.Length);
            Vector3 positionToSpawn = spawnParticleSystems[randomNumber].transform.position;
            if (!spawnParticleSystems[randomNumber].isPlaying) spawnParticleSystems[randomNumber].Play();
            GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            GameObject newGO = Instantiate(enemyToSpawn, positionToSpawn, Quaternion.identity);
            GameManager.GM.enemyCount++;
            GameManager.GM.UpdateEnemyCount();

            yield return new WaitForSeconds(spawnRate);
        }
    }

    // Cast ray from sky to find spawn point
    IEnumerator SpawnFromAbove(int x)
    {
        for (int i = 0; i < x; i++)
        {
            // float spawnPointX = Random.Range(spawnRadius * -1, spawnRadius) + transform.position.x;
            // float spawnPointZ = Random.Range(spawnRadius * -1, spawnRadius) + transform.position.z;
            //  Vector3 spawnPosition = new Vector3(spawnPointX, transform.position.y, spawnPointZ);

            // Annulus shaped enemy spawning. Enemies don't spawn within spawnProtection and do spawn inside spawnRadius
            // Example: spawnProtection = 30, spawnRadius = 35 -> enemies spawn between 30 and 35 units from player
            Vector2 randomPoint = new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle.normalized * Random.Range(spawnProtection, spawnRadius);
            Vector3 spawnPosition = new Vector3(randomPoint.x, transform.position.y, randomPoint.y);

            Ray ray = new Ray(spawnPosition, -transform.up);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 10000, groundLayer))
            {
                spawnPosition = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);

                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                GameObject newGO = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                GameManager.GM.enemyCount++;
                GameManager.GM.UpdateEnemyCount();

                if (GameManager.GM.useSpawnDebug)
                {
                    Debug.Log("Spawned at: " + spawnPosition);
                    Debug.DrawRay(new Vector3(randomPoint.x, transform.position.y, randomPoint.y), -transform.up * 1000, Color.green, 15.0f);
                    GameObject newSpawnDebugPrefab = Instantiate(spawnDebugPrefab, spawnPosition, Quaternion.identity);
                }
            }
            else
            {
                if (GameManager.GM.useSpawnDebug)
                {
                    Debug.Log("Spawn ray missed");
                    Debug.DrawRay(new Vector3(randomPoint.x, transform.position.y, randomPoint.y), -transform.up * 1000, Color.red, 15.0f);
                }
            }
            yield return new WaitForSeconds(spawnRate);
        }
    }

}
