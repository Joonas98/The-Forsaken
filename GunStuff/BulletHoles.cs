using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoles : MonoBehaviour
{
	// Script to create bullet holes when shooting walls for example
	// TODO: object pooling and make holes look nicer somehow
	public GameObject bulletHolePrefab;
	public float despawnTime = 15f;
	public Vector3 decalScale = new(0.2f, 0.2f, 0.2f);

	public void AddBulletHole(RaycastHit hit)
	{
		if (hit.collider.gameObject.layer == 6) return; // Ignore when hitting casings
		GameObject newBulletHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity);
		if (!hit.transform.gameObject.isStatic) newBulletHole.transform.SetParent(hit.transform, true);
		newBulletHole.transform.localScale = decalScale;
		newBulletHole.transform.LookAt(hit.point + hit.normal);
		newBulletHole.transform.eulerAngles = new Vector3(newBulletHole.transform.eulerAngles.x, newBulletHole.transform.eulerAngles.y, Random.Range(0, 360));
		Destroy(newBulletHole, despawnTime);
	}

}
