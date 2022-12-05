using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RigHandler : MonoBehaviour
{

    public TwoBoneIKConstraint RightHandIK;
    public TwoBoneIKConstraint LeftHandIK;

    public GameObject rightHandGrab;
    public GameObject leftHandGrab;

    public Animator animator;
    public RigBuilder rigBuilder;

    private void Awake()
    {
        RightHandIK = GameObject.Find("RightHandIK").GetComponent<TwoBoneIKConstraint>();
        LeftHandIK = GameObject.Find("LeftHandIK").GetComponent<TwoBoneIKConstraint>();
        rigBuilder = GameObject.Find("NewPlayer").GetComponent<RigBuilder>();
    }

    private void OnEnable()
    {
        RightHandIK.data.target = rightHandGrab.transform;
        LeftHandIK.data.target = leftHandGrab.transform;
        rigBuilder.Build();
    }
}
