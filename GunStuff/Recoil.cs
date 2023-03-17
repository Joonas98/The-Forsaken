using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Recoil : MonoBehaviour
{
    public bool useDebug;
    public static Recoil Instance;

    public Rigidbody RB;
    public Transform playerTrans;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Tooltip("Up and down")]
    [SerializeField] private float recoilX;
    [Tooltip("Left and right")]
    [SerializeField] private float recoilY;
    [Tooltip("Tilt")]
    [SerializeField] private float recoilZ;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    public float recoilMultiplier;
    public bool aiming;

    public PlayerMovement movementScript;
    public float rec1, rec2, rec3, rec4, rec5, rec6;

    [SerializeField] private GameObject debugTexts;
    [SerializeField] private TextMeshProUGUI recXText, recYText, recZText, snappinessText, returnSpeedText, recMultiplierText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        if (!useDebug) Destroy(debugTexts.gameObject);

    }

    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, new Vector3(targetRotation.x, 0, 0), returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);

        if (useDebug)
        {
            recXText.text = "X :" + recoilX.ToString();
            recYText.text = "Y :" + recoilY.ToString();
            recZText.text = "Z :" + recoilZ.ToString();
            snappinessText.text = "Snp :" + snappiness.ToString();
            returnSpeedText.text = "Rtn :" + returnSpeed.ToString();
            recMultiplierText.text = "RecMP :" + recoilMultiplier.ToString();
        }
    }

    private void LateUpdate()
    {
        playerTrans.rotation = new Quaternion(playerTrans.rotation.x, transform.localRotation.y + playerTrans.rotation.y, playerTrans.rotation.z, playerTrans.rotation.w);
    }

    public void RecoilFire()
    {
        if (!movementScript.isGrounded) // Ollaan ilmassa
        {
            recoilMultiplier = rec1; //1.5f;
                                     // print("Ilmassa");
        }
        else if (movementScript.isGrounded && !movementScript.isStationary && !aiming) // Maassa, liikutaan, ei t‰hd‰t‰
        {
            recoilMultiplier = rec2; //1f;
                                     // print("Maassa, liikutaan, ei t‰hd‰t‰");
        }
        else if (movementScript.isGrounded && !movementScript.isStationary && aiming) // Maassa, liikutaan, t‰hd‰t‰‰n
        {
            recoilMultiplier = rec3; //0.5f;
                                     // print("Maassa, liikutaan, t‰hd‰t‰‰n");
        }
        else if (movementScript.isGrounded && movementScript.isStationary && !aiming)  // Maassa, liikkumatta, ei t‰hd‰t‰
        {
            recoilMultiplier = rec4; //0.5f;
                                     // print("Maassa, liikkumatta, ei t‰hd‰t‰");
        }
        else if (movementScript.isGrounded && movementScript.isStationary && aiming)  // Maassa, liikkumatta, t‰hd‰t‰‰n
        {
            recoilMultiplier = rec5; //0.1f;
                                     // print("Maassa, liikkumatta, t‰hd‰t‰‰n");
        }
        else
        {
            recoilMultiplier = rec6; //0.5f;
                                     // print("Ei mik‰‰n edell‰");
        }

        targetRotation += new Vector3(recoilX, Random.Range(-recoilY * 0.1f, recoilY * 0.1f), Random.Range(-recoilZ, recoilZ)) * recoilMultiplier;
    }

    // Recoil from taking damage
    // Some variables like return speed is still determined by the held weapon
    public void DamageFlinch(float flinchY, float flinchX, float flinchMultiplier)
    {
        targetRotation += new Vector3(recoilX, Random.Range(-flinchY, flinchY), Random.Range(-flinchX, flinchX)) * flinchMultiplier;
    }

    #region Setters

    // Set functions
    public void SetRecoilX(float value)
    {
        recoilX = value;
    }

    public void SetRecoilY(float value)
    {
        recoilY = value;
    }

    public void SetRecoilZ(float value)
    {
        recoilZ = value;
    }

    public void SetSnappiness(float value)
    {
        snappiness = value;
    }

    public void SetReturnSpeed(float value)
    {
        returnSpeed = value;
    }

    #endregion

}
