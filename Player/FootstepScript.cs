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
			// Default
			_ => rockSteps[UnityEngine.Random.Range(0, rockSteps.Length)],
		};
	}

	// Return index of the terrain type player is on
	private int DetectTerrainType()
	{
		int terrainType = -1;  // Default to unknown terrain type

		// Cast a ray down
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 5f))
		{
			Terrain terrain = hit.collider.GetComponent<Terrain>();

			if (terrain != null)
			{
				// Convert hit point to local terrain coordinates
				Vector3 terrainLocalPos = terrain.transform.InverseTransformPoint(hit.point);
				TerrainData terrainData = terrain.terrainData;

				// Calculate the normalized position on the terrain
				float normX = terrainLocalPos.x / terrainData.size.x;
				float normY = terrainLocalPos.z / terrainData.size.z;

				// Calculate the position on the alpha map
				int mapX = (int)(normX * terrainData.alphamapWidth);
				int mapY = (int)(normY * terrainData.alphamapHeight);

				// Get the alpha map data at that point
				float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapY, 1, 1);

				// Determine the dominant texture index
				int dominantTextureIndex = 0;
				float maxAlpha = 0f;

				for (int i = 0; i < splatmapData.GetLength(2); i++)
				{
					if (splatmapData[0, 0, i] > maxAlpha)
					{
						maxAlpha = splatmapData[0, 0, i];
						dominantTextureIndex = i;
					}
				}

				// Determine the terrain type based on the dominant texture index
				if (dominantTextureIndex == 0 || dominantTextureIndex == 1)
				{
					// Grass terrain
					terrainType = 0;
				}
				else if (dominantTextureIndex == 2 || dominantTextureIndex == 3)
				{
					// Rock terrain
					terrainType = 1;
				}
			}
		}
		return terrainType;
	}

}
