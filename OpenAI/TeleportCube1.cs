using UnityEngine;

[RequireComponent(typeof(Transform))]
public class TeleportCube1 : MonoBehaviour
{
    [Tooltip("The distance in front of the player to teleport the cube.")]
    public float distance = 2f;

    [Tooltip("The player object to teleport the cube in front of.")]
    public GameObject player;

    private void Update()
    {
        // Get the player's position and forward direction
        Vector3 playerPos = player.transform.position;
        Vector3 playerForward = player.transform.forward;

        // Calculate the position to teleport the cube to
        Vector3 teleportPos = playerPos + playerForward * distance;

        // Teleport the cube to the calculated position
        transform.position = teleportPos;
    }
}