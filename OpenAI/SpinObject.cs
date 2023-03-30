using UnityEngine;

[RequireComponent(typeof(Transform))]
public class SpinObject : MonoBehaviour
{
    [Tooltip("The speed at which the object spins.")]
    public float spinSpeed = 5f;

    [Tooltip("The axis around which the object spins.")]
    public Vector3 spinAxis = Vector3.up;

    private void Update()
    {
        // Rotate the object around the specified axis at the specified speed
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime);
    }
}