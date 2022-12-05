using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sykkii : MonoBehaviour
{

    public Image targetImage;
    public float fl1 = 0.15f, fl2 = 0.15f, fl3 = 0, time = 1f;

    private void Update()
    {
        Pulse();
    }

    public void Pulse()
    {
        System.Collections.Hashtable hash =
                   new System.Collections.Hashtable();
        hash.Add("amount", new Vector3(fl1, fl2, fl3));
        hash.Add("time", time);
        iTween.PunchScale(gameObject, hash);
    }


}
