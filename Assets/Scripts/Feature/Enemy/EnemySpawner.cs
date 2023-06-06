using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.Image;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public int initialSpawn;
    public float spawnRate = 5f;
    public float spawnRadius = 10f;
    public Transform targetTransform;
    public Vector3 spawnOffset;

    private void Start()
    {
        // Prevent NavMesh null
        GameManager.Instance.DungeonNavMesh.OnDungeonNavMeshBuilt += Initialize;
    }

    private void Initialize()
    {
        StartCoroutine(SpawnCoroutine());
        SpawnEnemy(transform.position, spawnRadius, initialSpawn);
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRate);
            SpawnEnemy(transform.position, spawnRadius);
        }
    }

    public void SpawnEnemy(Vector3 origin, float radius, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            Vector3 randomPos = origin + Random.insideUnitSphere * radius + spawnOffset;
            randomPos.y = 0f;
            GameObject enemy = Instantiate(enemyPrefabs[randomIndex], CheckEnemyPos(randomPos), Quaternion.identity);
            Enemy enemyComp = enemy.GetComponent<Enemy>();
            enemyComp.playerTransform = targetTransform;
            enemyComp.agent.enabled = false;
        }
    }

    private Vector3 CheckEnemyPos(Vector3 pos)
    {
        if (NavMesh.SamplePosition(pos, out var hit, Mathf.Infinity, 1 << NavMesh.GetAreaFromName("Walkable")))
        {
            return hit.position + spawnOffset;
        }

        //return pos;
        return pos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
