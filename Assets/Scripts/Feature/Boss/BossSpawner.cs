using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [SerializeField] private float SpawnTime = 2f * 60;
    [SerializeField] private GameObject bossObject;

    private void Start()
    {
        bossObject.SetActive(false);
        DungeonGenerator.Instance.OnDungeonComplete += () => StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        yield return new WaitForSeconds(SpawnTime);
        bossObject.SetActive(true);
    }
}
