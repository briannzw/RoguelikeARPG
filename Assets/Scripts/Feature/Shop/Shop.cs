using Player.Interaction;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour, IInteractable
{
    public List<Item> AvailableItems;
    [Header("References")]
    public GameObject shopPanel;
    public Transform shopItemList;
    public GameObject shopItemPrefab;

    private PlayerInventory targetInventory;
    private PlayerAction playerControls;

    private void Start()
    {
        playerControls = InputManager.playerAction;
        RegisterInputCallback();
        foreach (var item in AvailableItems)
        {
            GameObject go = Instantiate(shopItemPrefab, shopItemList);
            var shopItem = go.GetComponent<ShopItemUI>();
            shopItem.Initialize(this, item);
        }
    }

    #region Callbacks
    private void RegisterInputCallback()
    {
        if (playerControls == null) return;

        playerControls.Panel.Cancel.performed += (ctx) => { shopPanel.SetActive(false); InputManager.ToggleActionMap(playerControls.Gameplay); };
    }
    #endregion

    public void Interact()
    {
        InputManager.ToggleActionMap(playerControls.Panel);
        shopPanel.SetActive(true);
    }

    public void Interact(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            targetInventory = other.GetComponent<PlayerInventory>();
            Interact();
        }
    }

    public void Buy(Item item)
    {
        if (item.isConsumable && targetInventory.IsFull) return;

        if (GameManager.Instance.DeductCoins(item.price))
        {
            targetInventory.AddItem(item);
        }
        else Debug.Log("Not enough money to buy " + item.name);
    }
}
