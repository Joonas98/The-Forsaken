using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretTargeting : MonoBehaviour
{
    public GameObject rotatingPart;
    public float targetingRange;
    public float rotationSpeed;
    public float scanInterval; // How often to scan for new targets

    private bool lockedAtTarget;
    private Transform lockedTargetTrans;
    private float nextScanTime;

    private void Update()
    {
        if (Time.time >= nextScanTime)
        {
            FindNewTarget();
            nextScanTime = Time.time + scanInterval;
        }

        if (lockedAtTarget && lockedTargetTrans != null)
        {
            Vector3 targetDirection = (lockedTargetTrans.position - rotatingPart.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

            // Calculate the additional rotation needed to align the forward axis with the desired direction
            Quaternion additionalRotation = Quaternion.Euler(0, 90, 0); // Adjust as needed

            // Apply the target rotation with the additional rotation
            rotatingPart.transform.rotation = Quaternion.Slerp(rotatingPart.transform.rotation, targetRotation * additionalRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void FindNewTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, targetingRange);

        float closestDistance = targetingRange;
        Transform newTarget = null;

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Torso"))
            {
                Enemy enemyScript = collider.GetComponentInParent<Enemy>();

                if (enemyScript != null && enemyScript.GetHealth() > 0)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        newTarget = enemyScript.torsoTransform;
                    }
                }
            }
        }

        // Update the locked target with the new one
        lockedTargetTrans = newTarget;
        lockedAtTarget = newTarget != null;
    }
}
