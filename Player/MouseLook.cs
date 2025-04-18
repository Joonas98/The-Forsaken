using UnityEngine;

public class MouseLook : MonoBehaviour
{
	// Basic camera movement
	public float mouseSensitivity;
	public float aimSensMultiplier = 0.5f;
	public float minClamp, maxClamp;
	public Transform playerBody;
	public Transform recoilTrans;
	[HideInInspector] public bool canRotate = true;

	public static MouseLook instance;

	private float xRotation;
	private float mouseX, mouseY;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
		HandleInputs();
	}

	private void LateUpdate()
	{
		RotateCamera();
	}

	private void HandleInputs()
	{
		if (!canRotate) return;

		if (GameManager.GM.currentGun != null && GameManager.GM.CurrentGunAiming())
		{
			mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * aimSensMultiplier;
			mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * aimSensMultiplier;
		}
		else
		{
			mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
			mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
		}
	}

	private void RotateCamera()
	{
		if (!canRotate) return;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, minClamp, maxClamp);

		transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		playerBody.Rotate(Vector3.up * mouseX);
	}
}
