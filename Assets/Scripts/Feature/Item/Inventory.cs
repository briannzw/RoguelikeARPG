using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxItems;
    public List<Item> items = new List<Item>();
    public Action OnInventoryValueChanged;

    public bool IsFull => items.Count >= maxItems;

    public bool AddItem(Item item)
    {
        if (items.Count >= maxItems)
        {
            Debug.Log("Inventory full!");
            return false;
        }

        items.Add(item);
        Debug.Log("Added item: " + item.name);
        OnInventoryValueChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(Item item)
    {
        if (!items.Contains(item))
        {
            Debug.Log("Item not found in inventory!");
            return false;
        }

        items.Remove(item);
        Debug.Log("Removed item: " + item.name);
        OnInventoryValueChanged?.Invoke();
        return true;
    }

}
