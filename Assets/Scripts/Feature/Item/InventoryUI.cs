using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private int totalSlot = 5;

    private List<Image> slots;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindObjectOfType<PlayerInventory>().GetComponent<PlayerInventory>();
        }

        slots = new List<Image>();
        for (int i = 0; i < totalSlot; i++)
        {
            GameObject go = Instantiate(itemSlotPrefab, transform);
            Image image = go.GetComponent<Image>();
            image.enabled = false;
            slots.Add(image);
        }
    }

    private void Start()
    {
        UpdateUI();

        inventory.OnInventoryValueChanged += UpdateUI;
    }

    private void UpdateUI()
    {
        for (int i = 0; i < inventory.items.Count; i++)
        {
            slots[i].enabled = true;
            slots[i].sprite = inventory.items[i].icon;
        }

        for(int i = inventory.items.Count; i < totalSlot; i++)
        {
            slots[i].enabled = false;
        }
    }
}
