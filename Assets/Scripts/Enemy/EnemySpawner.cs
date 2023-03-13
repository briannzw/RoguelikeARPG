using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    
    public EnemyScriptableObject enemyStats;
    public float spawnRate = 5f;
    public float spawnRadius = 10f;

    private float spawnTimer = 0f;

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnRate)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = 0f;

        GameObject enemy = Instantiate(enemyStats.model, spawnPosition, Quaternion.identity);
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        enemyScript.enemyStats = GetRandomEnemyStats();
    }

    private EnemyScriptableObject GetRandomEnemyStats()
    {
        EnemyScriptableObject[] allEnemyStats = Resources.LoadAll<EnemyScriptableObject>("Enemies");

        if (allEnemyStats.Length == 0)
        {
            Debug.LogWarning("No EnemyScriptableObject found in Resources/Enemies folder.");
            return null;
        }

        int randomIndex = Random.Range(0, allEnemyStats.Length);
        return allEnemyStats[randomIndex];
    }
}
