using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sykkii : MonoBehaviour
{
    public Image targetImage;
    public float fl1, fl2, fl3, time;

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.R))
        {
            Pulse();
        }

    }

    public void Pulse()
    {
        Hashtable hash = new Hashtable();
        hash.Add("amount", new Vector3(fl1, fl2, fl3));
        hash.Add("time", time);
        iTween.PunchScale(gameObject, hash);
    }


}
