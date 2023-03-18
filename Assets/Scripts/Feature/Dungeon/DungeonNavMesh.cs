using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class DungeonNavMesh : MonoBehaviour
{
    public NavMeshSurface surface;

    private void Start()
    {
        DungeonGenerator.Instance.OnDungeonComplete += Initialize;
        surface = GetComponent<NavMeshSurface>();
    }

    private void Initialize()
    {
        surface.BuildNavMesh();
    }
}
