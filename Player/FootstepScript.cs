using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepScript : MonoBehaviour
{
	// Script to handle footstep sounds
	public float stepThreshold, stepVolume;
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

	private void Update()
	{
		if (movementScript.isGrounded)
			distanceTravelled += Vector3.Distance(transform.position, lastPosition);
		lastPosition = transform.position;

		if (distanceTravelled > stepThreshold)
		{
			PlayFootstep();
			distanceTravelled = 0;
		}
	}

	private void PlayFootstep()
	{
		AudioClip clip = GetRandomSFX();
		audioSource.PlayOneShot(clip);
	}

	private AudioClip GetRandomSFX()
	{
		int terrainTextureIndex = DetectTerrainType();

		switch (terrainTextureIndex)
		{
			case 0: // Grass
				return grassSteps[UnityEngine.Random.Range(0, grassSteps.Length)];
			case 1:  // Rock
				return rockSteps[UnityEngine.Random.Range(0, rockSteps.Length)];
			default: // Default
				return rockSteps[UnityEngine.Random.Range(0, rockSteps.Length)];
		}
	}

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
