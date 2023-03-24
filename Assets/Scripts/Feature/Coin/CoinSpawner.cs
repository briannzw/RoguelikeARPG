using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CoinSpawner : MonoBehaviour
{
    [Header("Enemy Spawn")]
    public EnemySpawner EnemySpawner;
    public float Radius = 5;
    public int Count = 5;

    [Header("References")]
    public Transform playerTransform;
    [Range(5, 80)] public float MinDistanceFromPlayer = 40f;
    [Range(3, 50)] public float MinDistanceBetweenCoins = 20f;

    public CoinNavigator Navigator;

    [Header("Parameters")]
    public GameObject CoinPrefab;
    public int StartCoin = 3;
    public int CoinValue = 1000;
    public List<Coin> CoinList;

    public Action<Coin> OnCoinCollected;

    public NavMeshSurface surface;
    private NavMeshTriangulation triangulation;

    public int MaximalRandomIteration = 100;
    private int currentIteration = 0;

    private void Start()
    {
        DungeonGenerator.Instance.OnDungeonComplete += Initialize;
    }


    private void Initialize()
    {
        EnemySpawner = FindObjectOfType<EnemySpawner>();
        triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0)
        {
            Debug.Log("No Navmesh Found");
            return;
        }
        //Debug.Log(triangulation.vertices.Length);
        CoinList = new List<Coin>();
        OnCoinCollected += OnCoinCollect;

        // Spawn Initial Coins
        for(int i = 0; i < StartCoin; i++)
        {
            CoinList.Add(SpawnCoin());
        }
    }

    // Only Created in Initialization
    private Coin SpawnCoin()
    {
        currentIteration = 0;
        GameObject coinGO = Instantiate(CoinPrefab, RandomCoinPos(), Quaternion.identity);
        // Trigger Object
        CoinField coinField = coinGO.GetComponent<CoinField>();
        Coin coin = coinGO.GetComponentInChildren<Coin>();
        coin.CoinSpawner = this;
        coin.Value = CoinValue;
        coinField.CoinSpawner = this;
        return coin;
    }

    // Random Position in NavMesh Surfaces
    // Condition : Far from player, Coin Distances
    public Vector3 RandomCoinPos()
    {
        int vertexIndex = Random.Range(0, triangulation.vertices.Length);

        NavMeshHit hit;
        // Walkable only
        if (NavMesh.SamplePosition(triangulation.vertices[vertexIndex], out hit, 2f, 1))
        {
            NavMeshPath path = new NavMeshPath();
            // Calculate if player can reach Coin or not
            if (NavMesh.CalculatePath(playerTransform.position, triangulation.vertices[vertexIndex], 1, path))
            {
                //Debug.Log(GetPathLength(path) + " " + path.status);

                // Apply Condition (Far From Player)
                if (path.status == NavMeshPathStatus.PathComplete && GetPathLength(path) >= MinDistanceFromPlayer)
                {
                    // Apply Condition (Coin Path Distance)
                    if(GetMinCoinPath(hit.position) > MinDistanceBetweenCoins)
                    {
                        // Spawn Enemy Guards After Position Fixed
                        EnemySpawner.SpawnEnemy(hit.position, Radius, Count);

                        return hit.position;
                    }
                    Debug.Log("Coins are too close on " + triangulation.vertices[vertexIndex]);
                }

                Debug.Log("Path invalid on " + triangulation.vertices[vertexIndex]);
            }

            Debug.Log("Path not found on " + triangulation.vertices[vertexIndex]);
        }

        Debug.Log("Cannot place coin on " + triangulation.vertices[vertexIndex]);
        currentIteration++;
        if (currentIteration > MaximalRandomIteration) return hit.position;
        return RandomCoinPos();
    }

    // Check Far Path Condition
    // Reference : https://forum.unity.com/threads/getting-the-distance-in-nav-mesh.315846/
    private float GetPathLength(NavMeshPath path)
    {
        float lng = 0.0f;

        if ((path.status != NavMeshPathStatus.PathInvalid) && (path.corners.Length > 1))
        {
            for (int i = 1; i < path.corners.Length; ++i)
            {
                lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
        }

        return lng;
    }

    // Check Coin Distance Condition
    private float GetMinCoinDistance(Vector3 position)
    {
        float minDistance = Mathf.Infinity;

        foreach (var coin in CoinList)
        {
            float distance = Vector3.Distance(position, coin.transform.parent.position);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    private float GetMinCoinPath(Vector3 pos)
    {
        float minDistance = Mathf.Infinity;

        foreach (var coin in CoinList)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(pos, coin.transform.parent.position, 1, path))
            {
                float distance = GetPathLength(path);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
        }

        return minDistance;
    }

    private void OnCoinCollect(Coin coin)
    {
        GameManager.Instance.CoinCollected();

        coin.gameObject.SetActive(false);
        // Respawn Coin
        currentIteration = 0;
        coin.transform.parent.position = RandomCoinPos();
        Navigator.Follow(null); 

        coin.gameObject.SetActive(true);
    }

    public void OnPlayerEnterTrigger(Transform _transform)
    {
        // Follow Closest Coin
        if (IsCloser(_transform, Navigator.CurrentFollow))
        {
            Navigator.Follow(_transform);
        }
    }

    public void OnPlayerExitTrigger(Transform _transform)
    {
        if(_transform == Navigator.CurrentFollow)
        {
            Navigator.Follow(null);
        }
    }

    private bool IsCloser(Transform a, Transform b)
    {
        if (b == null) return true;
        return (playerTransform.position - a.position).sqrMagnitude < (playerTransform.position - b.position).sqrMagnitude;
    }
}
