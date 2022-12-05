using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoles : MonoBehaviour
{

    public GameObject bulletHolePrefab;
    public float despawnTime;
    [Tooltip("Default 0.2, 0.2, 0.2")] public Vector3 decalScale;


    public void AddBulletHole(RaycastHit hit)
    {
        if (hit.collider.gameObject.layer == 6) return; // Osuessa casings eli hylsyihin, ei tehd‰ mit‰‰n
        GameObject newBulletHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity);
        if (!hit.transform.gameObject.isStatic) newBulletHole.transform.SetParent(hit.transform, true);
        newBulletHole.transform.localScale = decalScale;
        newBulletHole.transform.LookAt(hit.point + hit.normal);
        newBulletHole.transform.eulerAngles = new Vector3(newBulletHole.transform.eulerAngles.x, newBulletHole.transform.eulerAngles.y, Random.Range(0, 360));
        Destroy(newBulletHole, despawnTime);
    }

}
