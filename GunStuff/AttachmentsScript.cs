using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentsScript : MonoBehaviour
{

    [SerializeField] private Gun gunScript;

    public GameObject[] scopes;

    public GameObject[] muzzleDevices;

    public GameObject[] grips;
    public GameObject[] lasers;

    public GameObject ironSights;
    public GameObject ironSights2;
    public GameObject ironMask;
    public GameObject scopeMount;

    // Testing variables
    private int currentScope = -1;
    private int currentGrip = -1;
    private int currentSilencer = -1;

    private void Awake()
    {

        if (gunScript == null)
            gunScript = GetComponent<Gun>();

    }

    private void Update()
    {
        // Attachmenttien selaaminen numeronäppäimillä
        #region Scopes Cycle

        if (scopes.Length > 0)
        {

            // SCOPE CYCLING AND UNEQUIPPING
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                UnequipScope();
            }

            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                currentScope += 1;
                if (currentScope >= scopes.Length)
                {
                    UnequipScope();
                }
                else
                {
                    EquipIrons(false);
                    EquipScope(currentScope);
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                currentScope -= 1;
                if (currentScope == -1)
                {
                    UnequipScope();
                }
                else if (currentScope < -1)
                {
                    currentScope = scopes.Length - 1;
                    EquipScope(currentScope);
                }
                else
                {
                    EquipIrons(false);
                    EquipScope(currentScope);
                }
            }
        }

        #endregion 

        #region Grips Cycle

        if (grips.Length > 0)
        {

            // GRIPS CYCLING AND UNEQUIPPING
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                UnequipGrip();
            }

            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                currentGrip += 1;
                if (currentGrip >= grips.Length)
                {
                    UnequipGrip();
                }
                else
                {
                    EquipGrip(currentGrip);
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                currentGrip -= 1;
                if (currentGrip == -1)
                {
                    UnequipGrip();
                }
                else if (currentGrip < -1)
                {
                    currentGrip = grips.Length - 1;
                    EquipGrip(currentGrip);
                }
                else
                {
                    EquipGrip(currentGrip);
                }
            }
        }

        #endregion

        #region Silencers Cycle

        if (muzzleDevices.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                UnequipSilencer();
            }

            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                currentSilencer += 1;
                if (currentSilencer >= muzzleDevices.Length)
                {
                    UnequipSilencer();
                }
                else
                {
                    EquipSilencer(currentSilencer);
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                currentSilencer -= 1;
                if (currentSilencer == -1)
                {
                    UnequipSilencer();
                }
                else if (currentSilencer < -1)
                {
                    currentSilencer = muzzleDevices.Length - 1;
                    EquipSilencer(currentSilencer);
                }
                else
                {
                    EquipSilencer(currentSilencer);
                }
            }
        }

        #endregion

    }



    public void EquipScope(int scopeIndex)
    {

        foreach (GameObject scope in scopes)
        {
            scope.SetActive(false);
        }

        if (scopes.Length >= scopeIndex && scopeIndex != -1)
        {
            scopes[scopeIndex].gameObject.SetActive(true);
        }

        if (scopeIndex != -1)
        {
            EquipIrons(false);
        }
        else if (scopeIndex == -1)
        {
            EquipIrons(true);
        }

        // Scope mount pois tietyille tähtäimille
        if (scopeMount != null && scopes[scopeIndex].gameObject.name == "4 Kobra" || scopeMount != null && scopes[scopeIndex].gameObject.name == "14 PSO-1")
        {
            scopeMount.SetActive(false);
        }
    }

    public void UnequipScope()
    {
        currentScope = -1;
        foreach (GameObject scope in scopes)
        {
            scope.SetActive(false);
        }

        EquipIrons(true);
    }


    public void EquipGrip(int gripIndex)
    {
        foreach (GameObject grip in grips)
        {
            grip.SetActive(false);
        }

        if (grips.Length >= gripIndex)
        {
            grips[gripIndex].gameObject.SetActive(true);
        }

    }

    public void UnequipGrip()
    {
        currentGrip = -1;
        foreach (GameObject grip in grips)
        {
            grip.SetActive(false);
        }
    }


    public void EquipSilencer(int silencerIndex)
    {
        foreach (GameObject silencer in muzzleDevices)
        {
            silencer.SetActive(false);
        }

        if (muzzleDevices.Length >= silencerIndex)
        {
            muzzleDevices[silencerIndex].gameObject.SetActive(true);
        }
    }

    public void UnequipSilencer()
    {
        currentSilencer = -1;
        gunScript.ResetGunTip();

        foreach (GameObject silencer in muzzleDevices)
        {
            silencer.SetActive(false);
        }

    }

    public void EquipIrons(bool boolean)
    {
        if (boolean)
        {
            if (ironSights != null) ironSights.SetActive(true);
            if (ironSights2 != null) ironSights2.SetActive(true);
            if (ironMask != null) ironMask.SetActive(false);
            if (scopeMount != null) scopeMount.SetActive(false);
        }
        else
        {
            if (ironSights != null) ironSights.SetActive(false);
            if (ironSights2 != null) ironSights2.SetActive(false);
            if (ironMask != null) ironMask.SetActive(true);
            if (scopeMount != null) scopeMount.SetActive(true);
        }
    }

}
