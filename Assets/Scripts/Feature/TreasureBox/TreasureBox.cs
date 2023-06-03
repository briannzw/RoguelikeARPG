using Player.Interaction;
using UnityEngine;

public class TreasureBox : Spawnable, IInteractable
{
    public CoinSpawner CoinSpawner;
    public int coinValue;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        animator.SetTrigger("Interact");
        CoinSpawner.OnTreasureCollected?.Invoke(this);
    }
}
