using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
        DungeonGenerator.Instance.OnDungeonComplete += Initialize;
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
            Vector3 spawnPosition = origin + Random.insideUnitSphere * radius + spawnOffset;
            spawnPosition.y = 0f;

            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject enemy = Instantiate(enemyPrefabs[randomIndex], CheckEnemyPos(spawnPosition), Quaternion.identity);
            Enemy enemyComp = enemy.GetComponent<Enemy>();
            enemyComp.playerTransform = targetTransform;
            enemyComp.agent.enabled = false;
        }
    }

    private Vector3 CheckEnemyPos(Vector3 pos)
    {
        if(NavMesh.SamplePosition(pos, out var hit, Mathf.Infinity, 1))
        {
            return hit.position + spawnOffset;
        }

        return pos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
