using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private float spinSpeed;
    [SerializeField] private float pausedSpinSpeed;
    [Tooltip("1 = X, 2 = Y, 3 = Z")] [SerializeField] private int spinAxis;

    void Update()
    {
        if (spinAxis == 1 && Time.timeScale > 0)
        {
            transform.Rotate(-50 * Time.deltaTime * spinSpeed, 0, 0, Space.World);
        }
        else if (spinAxis == 1 && Time.timeScale == 0)
        {
            transform.Rotate(-50 * pausedSpinSpeed * spinSpeed, 0, 0, Space.World);
        }

        if (spinAxis == 2 && Time.timeScale > 0)
        {
            transform.Rotate(0, -50 * Time.deltaTime * spinSpeed, 0, Space.World);
        }
        else if (spinAxis == 2 && Time.timeScale == 0)
        {
            transform.Rotate(0, -50 * pausedSpinSpeed * spinSpeed, 0, Space.World);
        }

        if (spinAxis == 3 && Time.timeScale > 0)
        {
            transform.Rotate(0, 0, -50 * Time.deltaTime * spinSpeed, Space.World);
        }
        else if (spinAxis == 3 && Time.timeScale == 0)
        {
            transform.Rotate(0, 0, -50 * pausedSpinSpeed * spinSpeed, Space.World);
        }
    }
}
