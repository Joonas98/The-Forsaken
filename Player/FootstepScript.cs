using UnityEngine;

public class FootstepScript : MonoBehaviour
{
	// Script to handle footstep sounds
	public float stepThreshold;
	public float walkVolume, runVolume;
	public PlayerMovement movementScript;
	public AudioSource audioSource;
	public AudioClip[] grassSteps;
	public AudioClip[] rockSteps;
	public AudioClip[] sandSteps;

	private float distanceTravelled = 0;
	private Vector3 lastPosition;

	private void Start()
	{
		lastPosition = transform.position;
	}

	// Use update to figure when player has moved enough to play 
	private void Update()
	{
		// No footsteps midair
		if (movementScript.isGrounded)
			distanceTravelled += Vector3.Distance(transform.position, lastPosition);
		lastPosition = transform.position;

		if (distanceTravelled > stepThreshold)
		{
			PlayFootstep();
			distanceTravelled = 0;
		}
	}

	// Play the SFX, louder when running
	private void PlayFootstep()
	{
		float volume;
		if (movementScript.isRunning) volume = runVolume;
		else volume = walkVolume;

		AudioClip clip = GetRandomSFX();
		audioSource.PlayOneShot(clip, volume);
	}

	// Get a random SFX from a list depending on the terrain player is moving on
	private AudioClip GetRandomSFX()
	{
		int terrainTextureIndex = DetectTerrainType();

		return terrainTextureIndex switch
		{
			// Grass
			0 => grassSteps[UnityEngine.Random.Range(0, grassSteps.Length)],
			// Rock
			1 => rockSteps[UnityEngine.Random.Range(0, rockSteps.Length)],
			// Rock2
			2 => rockSteps[UnityEngine.Random.Range(0, rockSteps.Length)],
			// Sand
			3 => sandSteps[UnityEngine.Random.Range(0, sandSteps.Length)],
			// Default
			_ => rockSteps[UnityEngine.Random.Range(0, rockSteps.Length)],
		};
	}

	// Return the dominant texture index at the player's current position on the terrain
	private int DetectTerrainType()
	{
		// Cast a ray down from the player's position
		if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5f))
		{
			Terrain terrain = hit.collider.GetComponent<Terrain>();
			if (terrain != null)
			{
				TerrainData terrainData = terrain.terrainData;

				// Convert the hit point to the terrain's local coordinates
				Vector3 terrainLocalPos = terrain.transform.InverseTransformPoint(hit.point);

				// Normalize the local coordinates relative to the terrain size
				float normX = terrainLocalPos.x / terrainData.size.x;
				float normZ = terrainLocalPos.z / terrainData.size.z;

				// Calculate the corresponding position on the alpha map (splat map)
				int mapX = (int)(normX * terrainData.alphamapWidth);
				int mapZ = (int)(normZ * terrainData.alphamapHeight);

				// Retrieve the alpha map data at that point
				float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

				// Determine the dominant texture index
				int dominantTextureIndex = 0;
				float maxAlpha = 0f;
				int numTextures = splatmapData.GetLength(2);
				for (int i = 0; i < numTextures; i++)
				{
					float alpha = splatmapData[0, 0, i];
					if (alpha > maxAlpha)
					{
						maxAlpha = alpha;
						dominantTextureIndex = i;
					}
				}

				return dominantTextureIndex;
			}
		}

		// Return -1 if no terrain was hit
		return -1;
	}
}
