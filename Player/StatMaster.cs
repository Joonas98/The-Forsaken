using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatMaster : MonoBehaviour
{
    public static StatMaster SM;

    public float damagePercentage;
    public int rangedDamage, meleeDamage, abilityDamage;

    private void Awake()
    {
        // Singleton
        if (SM == null)
        {
            DontDestroyOnLoad(gameObject);
            SM = this;
        }
        else if (SM != this)
        {
            Destroy(gameObject);
        }
    }

}
