using Player.Interaction;
using System.Collections.Generic;
using UnityEngine;

public class TreasureBox : Spawnable, IInteractable
{
    public CoinSpawner CoinSpawner;
    public int coinValue;

    public List<Item> dropItems;

    private Collider collider;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider>();
    }

    public void Reset()
    {
        collider.enabled = true;
        animator.SetTrigger("Close");
        animator.Play("Closed");
    }

    public void Interact()
    {
        animator.SetTrigger("Interact");
        CoinSpawner.OnTreasureCollected?.Invoke(this);
    }

    public void Interact(GameObject player)
    {
        collider.enabled = false;
        var inventory = player.GetComponent<Inventory>();
        foreach (var item in dropItems)
        {
            if (inventory.IsFull)
            {
                Debug.Log("Inventory is full!");
                return;
            }
            inventory.AddItem(item);
        }
        
        Interact();
    }
}
