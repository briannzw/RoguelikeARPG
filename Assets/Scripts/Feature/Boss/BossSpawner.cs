using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [SerializeField] private float SpawnTime = 2f * 60;
    [SerializeField] private GameObject bossObject;

    DungeonNavMesh dungeonNavMesh;

    private void Start()
    {
        bossObject.SetActive(false);
        dungeonNavMesh = GameObject.FindGameObjectWithTag("NavmeshGenerator").GetComponent<DungeonNavMesh>();
        DungeonGenerator.Instance.OnDungeonComplete += () => StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        transform.parent.Find("Chunk(1, 1)/Ground").gameObject.layer = LayerMask.NameToLayer("BossArea");
        dungeonNavMesh.Initialize();
        yield return new WaitForSeconds(SpawnTime);
        bossObject.SetActive(true);
    }
}
