using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxItems;
    public List<Item> items = new List<Item>();

    public bool AddItem(Item item)
    {
        if (items.Count >= maxItems)
        {
            Debug.Log("Inventory full!");
            return false;
        }

        items.Add(item);
        Debug.Log("Added item: " + item.name);
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
        return true;
    }

    public void UseItem(Item item)
    {
        if (!items.Contains(item))
        {
            Debug.Log("Item not found in inventory!");
            return;
        }

        item.Use();
        RemoveItem(item);
    }
}
