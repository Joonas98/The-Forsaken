using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPlacing : MonoBehaviour
{
	[Header("Variables")]
	public float rotationSpeed;
	[System.Serializable]
	public struct PlacingInfo
	{
		public GameObject aimerPrefab;
		public GameObject prefab;

		public float maxAllowedAngle;
		public float minAllowedAngle;
	}

	public List<PlacingInfo> placingObjects;
	public Color validPlacementColor, invalidPlacementColor;
	public Color defaultColor, highlightColor;
	[HideInInspector] public static ObjectPlacing instance;
	public bool isPlacing = false;

	[Header("UI")]
	public Image[] objectPanels;
	public Image selectedImageHUD;
	public Sprite[] objectSprites;
	public TextMeshProUGUI placeHotkeyTMP, changeHotkeyTMP;

	private RaycastHit hit;
	private GameObject activeAimer;
	private GameObject placingObject;
	private int chosenObjectIndex = 0;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(instance);
		}
		ChooseObject(0);

		//placeHotkeyTMP.text = KeybindManager.Instance.placeObjectKey.ToString();
		//changeHotkeyTMP.text = KeybindManager.Instance.selectionMenuKey.ToString();
	}

	void Update()
	{
		if (Time.timeScale == 0.0f) return;
		HandleInputs();
		HandlePlacement(placingObjects[chosenObjectIndex]);
	}

	public void ChooseObject(int index)
	{
		objectPanels[chosenObjectIndex].color = defaultColor;
		objectPanels[index].color = highlightColor;
		chosenObjectIndex = index;
		selectedImageHUD.sprite = objectSprites[index];
	}

	private void HandleInputs()
	{
		if (Time.timeScale <= 0) return; // Game paused

		// Handle object placement
		if (Input.GetKeyDown(KeybindManager.Instance.placeObjectKey))
		{
			if (!isPlacing)
			{
				StartPlacing(placingObjects[chosenObjectIndex]);  // Use the chosen object index
			}
			else
			{
				StopPlacing();
			}
		}
	}

	private void StartPlacing(PlacingInfo placingInfo)
	{
		if (placingInfo.aimerPrefab == null)
		{
			Debug.LogError("Aimer Prefab is null for the chosen object!");
			return;
		}

		activeAimer = Instantiate(placingInfo.aimerPrefab, Vector3.zero, Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0));
		isPlacing = true;
		placingObject = placingInfo.prefab;
	}

	public void StopPlacing()
	{
		if (activeAimer != null) Destroy(activeAimer);
		isPlacing = false;
		placingObject = null;
		//if (!isChoosingObject) MouseLook.instance.canRotate = true; // Bug prevention
	}

	private void HandlePlacement(PlacingInfo placingInfo)
	{
		if (activeAimer == null) return;

		Renderer[] renderers = activeAimer.GetComponentsInChildren<Renderer>();

		if (renderers != null && renderers.Length > 0)
		{
			if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100f))
			{
				Vector3 aimerPosition = hit.point;
				aimerPosition = hit.point;

				bool canPlace = IsPlacementValid(activeAimer.transform);

				foreach (Renderer renderer in renderers)
				{
					foreach (Material material in renderer.materials)
					{
						material.color = canPlace ? validPlacementColor : invalidPlacementColor;
					}
				}

				// Rotate the aimed object with right mouse button
				if (Input.GetMouseButton(1))
				{
					float mouseX = Input.GetAxis("Mouse X");
					activeAimer.transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
					MouseLook.instance.canRotate = false;
				}

				if (Input.GetMouseButtonUp(1))
				{
					MouseLook.instance.canRotate = true;
				}

				activeAimer.transform.position = aimerPosition;

				// Place the aimed object when left clicking
				if (Input.GetMouseButtonDown(0) && canPlace)
				{
					_ = Instantiate(placingInfo.prefab, activeAimer.transform.position, activeAimer.transform.rotation);
				}
			}
		}
	}

	private bool IsPlacementValid(Transform placementTransform)
	{
		SphereCollider sphereCollider = placementTransform.GetComponent<SphereCollider>();
		BoxCollider boxCollider = placementTransform.GetComponent<BoxCollider>();

		// Check if the placement angle is allowed
		Vector3 placementPosition = placementTransform.position;
		Ray ray = new Ray(placementPosition, Vector3.down);

		// !!! Known issue, for some objects, the ray hits aimer object rather than aimed surface
		// Stills works fine right now for currently implemented objects 16.9.2023
		if (Physics.Raycast(ray, out RaycastHit hit))
		{
			float angle = Vector3.Angle(ray.direction, hit.normal);
			if (angle > placingObjects[chosenObjectIndex].maxAllowedAngle)
			{
				// Debug.Log("Placement not allowed, aimed angle is: " + angle);
				return false;
			}
		}

		if (sphereCollider != null)
		{
			Vector3 position = sphereCollider.transform.position + Vector3.up * sphereCollider.center.y;
			Collider[] colliders = Physics.OverlapSphere(position, sphereCollider.radius);

			// Check if any of the found colliders belong to the same object
			foreach (var collider in colliders)
			{
				// Check if the collider belongs to the same GameObject
				if (collider.gameObject == placementTransform.gameObject)
				{
					continue; // Skip this collider because it's attached to the same object.
				}

				return false;
			}
		}

		if (boxCollider != null)
		{
			Vector3 position = boxCollider.bounds.center;
			Vector3 size = boxCollider.bounds.size;
			Collider[] colliders = Physics.OverlapBox(position, size * 0.5f, boxCollider.transform.rotation);
			foreach (var collider in colliders)
			{
				// Check if the collider belongs to the same GameObject
				if (collider.gameObject == placementTransform.gameObject)
				{
					continue; // Skip this collider because it's attached to the same object.
				}

				return false;
			}
		}

		// New object is on allowed angle and not too close to anything
		return true;
	}
}
