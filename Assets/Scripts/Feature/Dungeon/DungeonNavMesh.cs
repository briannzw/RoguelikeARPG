using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class DungeonNavMesh : MonoBehaviour
{
    public NavMeshSurface[] surfaces;
    public Action OnDungeonNavMeshBuilt;

    private void Start()
    {
        //DungeonGenerator.Instance.OnDungeonComplete += Initialize; Handled by BossSpawner
    }

    public void Initialize()
    {
        foreach(var surface in surfaces)
        {
            surface.BuildNavMesh();
        }
        OnDungeonNavMeshBuilt?.Invoke();
        Debug.Log("Navmesh Built");
    }
}
