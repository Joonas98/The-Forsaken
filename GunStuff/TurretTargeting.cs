using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretTargeting : MonoBehaviour
{

    public SphereCollider targetingCollider;
    public GameObject rotatingPart;
    public float targetingRange;

    private bool lockedAtTarget;
    private Transform lockedTargetTrans;

    private void Awake()
    {
        targetingCollider = transform.GetComponent<SphereCollider>();
        targetingCollider.radius = targetingRange;
    }

    private void Update()
    {
        if (lockedAtTarget)
        {
            Vector3 targetPosition = new Vector3(lockedTargetTrans.position.x, rotatingPart.transform.localPosition.y + 90f, lockedTargetTrans.position.z);
            rotatingPart.transform.LookAt(targetPosition, rotatingPart.transform.up);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Torso"))
        {
            Enemy enemyScript = other.GetComponentInParent<Enemy>();

            if (enemyScript != null && enemyScript.GetHealth() > 0)
            {
                lockedAtTarget = true;
                lockedTargetTrans = enemyScript.transform;
            }
            else
            {
                lockedAtTarget = false;
            }

        }
    }

}
