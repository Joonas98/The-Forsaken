using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float distance = 7.0f;
    Vector3 startScale;

    private void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        ScaleTextSize();
        transform.LookAt(2 * transform.position - Camera.main.transform.position);
    }

    void ScaleTextSize()
    {
        float dist = Vector3.Distance(Camera.main.transform.position, transform.position);
        Vector3 newScale = startScale * (dist / distance);
        transform.localScale = newScale;
    }

}
