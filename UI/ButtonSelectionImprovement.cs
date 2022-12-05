using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSelectionImprovement : MonoBehaviour
{

    public EventSystem eventSystem;
    [HideInInspector] public GameObject lastSelected = null;

    private void Awake()
    {
        if (eventSystem == null) eventSystem = GetComponent<EventSystem>();
    }

    private void Update()
    {
        if (eventSystem != null)
        {
            if (eventSystem.currentSelectedGameObject != null)
            {
                lastSelected = eventSystem.currentSelectedGameObject;
            }
            else
            {
                eventSystem.SetSelectedGameObject(lastSelected);
            }
        }
    }

}
