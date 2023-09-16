using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectPlacing : MonoBehaviour
{
    [Header("Variables")]
    public float rotationSpeed;
    [System.Serializable]
    public struct PlacingInfo
    {
        public GameObject aimerPrefab;
        public GameObject prefab;

        public float radius;
        public float minAllowedAngle;
        public float maxAllowedAngle;
    }

    public List<PlacingInfo> placingObjects;
    public KeyCode placingModeHotkey, choosingMenuHotkey;
    public Color validPlacementColor, invalidPlacementColor;
    public Color defaultColor, highlightColor;
    [HideInInspector] public static ObjectPlacing instance;
    public bool isPlacing = false;
    public bool isChoosingObject = false;

    [Header("UI & UX")]
    public GameObject choosingMenu;
    public Image[] objectPanels;
    public Image selectedImageHUD;
    public Sprite[] objectSprites;
    public float objectSelectionTimeSlow;
    public TextMeshProUGUI placeHotkeyTMP, changeHotkeyTMP;

    private RaycastHit hit;
    private GameObject activeAimer;
    private GameObject placingObject;
    private Vector3 initialAimerPosition;
    private int chosenObjectIndex = 0;
    private bool isRotating = false; // Flag to check if the player is currently rotating the object.
    private bool canPlace = false;

    private void OnValidate()
    {
        placeHotkeyTMP.text = placingModeHotkey.ToString();
        changeHotkeyTMP.text = choosingMenuHotkey.ToString();
    }

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

        // Open object selection menu
        if (Input.GetKey(choosingMenuHotkey) && !GrenadeThrow.instance.selectingGrenade)
        {
            if (!isChoosingObject)
            {
                choosingMenu.SetActive(true);
                isChoosingObject = true;
                MouseLook.instance.canRotate = false;
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = objectSelectionTimeSlow;
                if (isPlacing) StopPlacing();
            }
        }
        else // Close object selection menu
        {
            if (isChoosingObject)
            {
                choosingMenu.SetActive(false);
                isChoosingObject = false;
                MouseLook.instance.canRotate = true;
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1f;
            }
        }

        // Handle object placement
        if (Input.GetKeyDown(placingModeHotkey))
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
        initialAimerPosition = activeAimer.transform.position;
    }

    private void StopPlacing()
    {
        if (activeAimer != null) Destroy(activeAimer);
        isPlacing = false;
        placingObject = null;
        if (!isChoosingObject) MouseLook.instance.canRotate = true; // Bug prevention
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
				float angle = Vector3.Angle(hit.normal, Vector3.up);
				aimerPosition = hit.point;

				// Flag to check if any renderer has valid placement
				bool validPlacement = false;

				foreach (Renderer renderer in renderers)
				{
					if (angle >= placingInfo.minAllowedAngle && angle <= placingInfo.maxAllowedAngle)
					{
						// Change the material color for each renderer
						foreach (Material material in renderer.materials)
						{
							material.color = validPlacementColor;
						}
						validPlacement = true;
					}
					else
					{
						// Change the material color for each renderer
						foreach (Material material in renderer.materials)
						{
							material.color = invalidPlacementColor;
						}
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

				// activeAimer.transform.SetPositionAndRotation(aimerPosition, Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0));
				activeAimer.transform.position = (aimerPosition);

				// Place the aimed object when left clicking
				if (Input.GetMouseButtonDown(0))
				{
					// Check if placement is valid based on the trigger collider and angle
					if (validPlacement && IsPlacementValid(activeAimer.transform))
					{
						_ = Instantiate(placingInfo.prefab, activeAimer.transform.position, activeAimer.transform.rotation);
					}
				}
			}
		}
	}

	private bool IsPlacementValid(Transform placementTransform)
	{
		SphereCollider sphereCollider = placementTransform.GetComponent<SphereCollider>();
		BoxCollider boxCollider = placementTransform.GetComponent<BoxCollider>();

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

		Debug.Log("Returning true");
		return true;
	}


}
