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
    private List<Spawnable> SpawnList;

    public Action<Coin> OnCoinCollected;
    public Action<TreasureBox> OnTreasureCollected;

    public NavMeshSurface surface;
    private NavMeshTriangulation triangulation;

    public int MaximalRandomIteration = 100;
    private int currentIteration = 0;

    [Header("Item Spawn")]
    public List<Item> spawnItems;

    private void Start()
    {
        GameManager.Instance.DungeonNavMesh.OnDungeonNavMeshBuilt += Initialize;
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
        SpawnList = new();
        OnTreasureCollected += OnTreasureCollect;
        OnCoinCollected += OnCoinCollect;

        // Spawn Initial Coins
        for(int i = 0; i < StartCoin; i++)
        {
            SpawnList.Add(SpawnTreasure());
        }
    }

    // Only Created in Initialization
    private Coin SpawnCoin()
    {
        currentIteration = 0;
        Vector3 pos = RandomCoinPos();
        GameObject coinGO = Instantiate(CoinPrefab, pos, Quaternion.identity);
        // Trigger Object
        CoinField coinField = coinGO.GetComponent<CoinField>();
        Coin coin = coinGO.GetComponentInChildren<Coin>();
        coin.CoinSpawner = this;
        coin.Value = GameManager.Instance.coinsValue;
        coinField.CoinSpawner = this;
        return coin;
    }

    private TreasureBox SpawnTreasure()
    {
        currentIteration = 0;
        Vector3 pos = RandomCoinPos();
        GameObject treasureGO = Instantiate(CoinPrefab, pos, Quaternion.identity);
        TreasureBox treasure = treasureGO.GetComponentInChildren<TreasureBox>();
        // Initialize treasure
        treasure.CoinSpawner = this;
        treasure.coinValue = GameManager.Instance.coinsValue;
        treasure.Reset();
        treasure.dropItems.Clear();
        foreach(var item in spawnItems)
        {
            if(Random.Range(0, 100) < item.treasureSpawnChance)
            {
                treasure.dropItems.Add(item);
                //Debug.Log(item.name + " Added");
            }
        }
        return treasure;
    }

    // Random Position in NavMesh Surfaces
    // Condition : Far from player, Coin Distances
    public Vector3 RandomCoinPos()
    {
        int vertexIndex = Random.Range(0, triangulation.vertices.Length);

        NavMeshHit hit;
        // Walkable only
        if (NavMesh.SamplePosition(triangulation.vertices[vertexIndex], out hit, 2f, 1 << NavMesh.GetAreaFromName("Walkable")))
        {
            NavMeshPath path = new NavMeshPath();
            // Calculate if player can reach Coin or not
            if (NavMesh.CalculatePath(playerTransform.position, hit.position, 1 << NavMesh.GetAreaFromName("Walkable"), path))
            {
                //Debug.Log(GetPathLength(path) + " " + path.status);

                // Apply Condition (Far From Player)
                if (path.status == NavMeshPathStatus.PathComplete && GetPathLength(path) >= MinDistanceFromPlayer)
                {
                    // Apply Condition (Coin Path Distance)
                    if(GetMinCoinPath(hit.position) >= MinDistanceBetweenCoins)
                    {
                        // Spawn Enemy Guards After Position Fixed
                        if(EnemySpawner)
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

        foreach (var spawn in SpawnList)
        {
            float distance = Vector3.Distance(position, spawn.transform.parent.position);
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

        foreach (var spawn in SpawnList)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(pos, spawn.transform.parent.position, 1, path))
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

    private void OnTreasureCollect(TreasureBox treasure)
    {
        GameManager.Instance.CoinCollected();

        StartCoroutine(MoveAfter(3f, treasure.gameObject));
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

    IEnumerator MoveAfter(float time, GameObject go)
    {
        Navigator.Follow(null);
        yield return new WaitForSeconds(time);
        currentIteration = 0;
        go.transform.parent.position = RandomCoinPos();
        go.SetActive(true);
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
