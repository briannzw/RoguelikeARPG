using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public CoinSpawner CoinSpawner;
    public int Value;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinSpawner.OnCoinCollected?.Invoke(this);
        }
    }
}
