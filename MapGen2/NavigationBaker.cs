using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class NavigationBaker : MonoBehaviour
{
    public NavigationBaker navigationBakerScript;

    public NavMeshSurface[] surfaces;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            BuildNavMesh();
        }
    }

    public void BuildNavMesh()
    {
        for (int i = 0; i < surfaces.Length; i++)
        {
            surfaces[i].BuildNavMesh();
        }
    }

}