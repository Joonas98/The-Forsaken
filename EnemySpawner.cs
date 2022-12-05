using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ! Vanha systeemi "floating spawnaukseen" - ei käytössä
public class EnemySpawner : MonoBehaviour
{

    [SerializeField] private int spawnAmount;
    [SerializeField] private float spawnInterval;
    [SerializeField] private float spawnRadius;

    [SerializeField] private float floatingHeight = 500;
    [SerializeField] private GameObject[] SpawnLocations;
    [SerializeField] private GameObject enemyContainer;

    public GameObject enemyPrefab;

    private GameObject playerGO;

    private void Start()
    {
        playerGO = GameObject.Find("Player");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(Spawn(spawnAmount));
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(playerGO.transform.position.x, floatingHeight, playerGO.transform.position.z);
    }

    IEnumerator Spawn(int x)
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

                GameObject newGO = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                GameManager.enemyCount++;
                GameManager.UpdateEnemyCount();
            }
            else
            {
                Debug.Log("Spawn ray missed");
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

}
