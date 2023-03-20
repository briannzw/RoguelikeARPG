using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinField : MonoBehaviour
{
    public CoinSpawner CoinSpawner;
    public float FieldCheckInterval = 1f;
    public float FieldRadius = 30f;
    private float timer = 0f;

    private void Start()
    {
        GetComponent<SphereCollider>().radius = FieldRadius;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            timer += Time.fixedDeltaTime;
            if (timer > FieldCheckInterval)
            {
                timer = 0f;
                CoinSpawner.OnPlayerEnterTrigger(transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinSpawner.OnPlayerExitTrigger(transform);
        }
    }
}
