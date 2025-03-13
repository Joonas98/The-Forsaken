using UnityEngine;

public class Interactable : MonoBehaviour
{
	[Header("Interaction Settings")]
	[Tooltip("Maximum distance to interact with the object.")]
	public float interactionDistance = 3f;
	[Tooltip("Maximum angle (in degrees) from the player's view to consider the object aimed at.")]
	public float interactionAngle = 30f;
	[Tooltip("Key to trigger interaction.")]
	public KeyCode interactionKey = KeyCode.E;

	private Transform playerTransform;

	void Start()
	{
		// Find the player by tag. Make sure your player has the "Player" tag.
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if (player != null)
			playerTransform = player.transform;
		else
			Debug.LogWarning("Player not found. Please tag your player with 'Player'.");
	}

	void Update()
	{
		if (playerTransform == null)
			return;

		// Calculate the vector from the player to this object.
		Vector3 toObject = transform.position - playerTransform.position;
		float distance = toObject.magnitude;

		// Check if within interaction distance.
		if (distance > interactionDistance)
			return;

		// Normalize the vector for angle calculation.
		toObject.Normalize();

		// Use the player's forward direction.
		Vector3 playerForward = playerTransform.forward;
		float dot = Vector3.Dot(playerForward, toObject);

		// Calculate threshold using cosine of the allowed angle.
		float angleThreshold = Mathf.Cos(interactionAngle * Mathf.Deg2Rad);

		// If the dot product exceeds the threshold, the object is being aimed at.
		if (dot >= angleThreshold)
		{
			// Optionally, you can display a prompt or highlight the object here.
			// For testing, we'll log and check if the interaction key is pressed.
			if (Input.GetKeyDown(interactionKey))
			{
				Interact();
			}
		}
	}

	// This method is called when the player interacts with the object.
	public void Interact()
	{
		Debug.Log("Interacted with " + gameObject.name);
		// Add your interaction logic here (e.g., open a door, pick up an item, etc.)
	}
}
