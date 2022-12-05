using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacing : MonoBehaviour
{

    public GameObject barricadeAimerPrefab;
    public GameObject barricadePrefab;

    public GameObject aimerObjectMinePrefab;
    public GameObject minePrefab;

    private RaycastHit hit;

    private GameObject barricadeAimer;
    private GameObject mineAimer;

    private void Awake()
    {
        barricadeAimer = Instantiate(barricadeAimerPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0));
        barricadeAimer.SetActive(false);

        mineAimer = Instantiate(aimerObjectMinePrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0));
        mineAimer.SetActive(false);
    }

    void Update()
    {
        BarricadePlacement();
        MinePlacement();
    }

    private void MinePlacement()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            mineAimer.SetActive(true);
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100f) && Input.GetKey(KeyCode.K))
        {
            mineAimer.transform.position = hit.point;
            mineAimer.transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            GameObject newObject = Instantiate(minePrefab, mineAimer.transform.position, mineAimer.transform.rotation);
            mineAimer.SetActive(false);
        }
    }

    private void BarricadePlacement()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            barricadeAimer.SetActive(true);
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100f) && Input.GetKey(KeyCode.L))
        {
            barricadeAimer.transform.position = hit.point;
            barricadeAimer.transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        }

        if (Input.GetKeyUp(KeyCode.L))
        {
            GameObject newObject = Instantiate(barricadePrefab, barricadeAimer.transform.position, barricadeAimer.transform.rotation);
            barricadeAimer.SetActive(false);
        }
    }

}
