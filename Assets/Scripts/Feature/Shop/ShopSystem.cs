using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSystem : MonoBehaviour
{
    // Reference to the player's inventory
    public Inventory playerInventory;

    // Reference to the shopkeeper's inventory
    public Inventory shopInventory;

    // Reference to the UI text that displays the player's gold amount
    public TextMeshPro goldText;

    // Reference to the UI panel that displays the shop interface
    public GameObject shopPanel;

    // Reference to the UI panel that displays the confirmation message when buying an item
    public GameObject confirmPanel;

    // Reference to the UI text that displays the name and price of the item being bought
    public Text confirmText;

    // The amount of gold the player has
    private int goldAmount;

    // Whether the shop interface is currently open
    private bool shopOpen = false;

    // The currently selected item in the shop interface
    private Item selectedItem;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the gold amount to 100
        goldAmount = 100;

        // Update the gold text
        UpdateGoldText();

        // Close the shop panel
        shopPanel.SetActive(false);

        // Close the confirm panel
        confirmPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player clicks on the shopkeeper
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "Shopkeeper")
            {
                // Open the shop interface
                shopOpen = true;
                shopPanel.SetActive(true);
            }
        }

        // Update the selected item if the player clicks on an item in the shop interface
        if (shopOpen && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "ShopItem")
            {
                selectedItem = hit.collider.gameObject.GetComponent<ItemButton>().item;
            }
        }
    }

    // Buy the selected item and update the inventories and gold amount
    public void BuySelectedItem()
    {
        // Calculate the cost of the item
        int cost = selectedItem.price;

        // Check if the player has enough gold to buy the item
        if (goldAmount >= cost)
        {
            // Subtract the cost from the player's gold amount
            goldAmount -= cost;

            // Update the gold text
            UpdateGoldText();

            // Add the item to the player's inventory
            playerInventory.AddItem(selectedItem);

            // Remove the item from the shopkeeper's inventory
            shopInventory.RemoveItem(selectedItem);

            // Close the shop interface
            shopOpen = false;
            shopPanel.SetActive(false);

            // Open the confirmation panel and display the item name and price
            confirmPanel.SetActive(true);
            confirmText.text = "Bought " + selectedItem.name + " for " + cost + " gold.";
        }
    }

    // Close the confirmation panel
    public void CloseConfirmPanel()
    {
        confirmPanel.SetActive(false);
    }

    // Update the text that displays the player's gold amount
    private void UpdateGoldText()
    {
        goldText.text = "Gold: " + goldAmount;
    }
}
