using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FindClosestObject1 : MonoBehaviour
{
    [Tooltip("The tag of the object to find.")]
    public string objectTag = "Player";
    [Tooltip("The maximum distance to search for the object.")]
    public float maxDistance = 10f;

    private GameObject closestObject;

    private void Start()
    {
        FindClosestObjectWithTag();
    }

    private void FindClosestObjectWithTag()
    {
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(objectTag);
        float closestDistance = Mathf.Infinity;

        foreach (GameObject obj in objectsWithTag)
        {
            if (obj != gameObject)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);

                if (distance < closestDistance && distance <= maxDistance)
                {
                    closestDistance = distance;
                    closestObject = obj;
                }
            }
        }

        if (closestObject != null)
        {
            Debug.Log("Closest object with tag " + objectTag + " is " + closestObject.name);
        }
        else
        {
            Debug.Log("No object with tag " + objectTag + " found within " + maxDistance + " units.");
        }
    }
}