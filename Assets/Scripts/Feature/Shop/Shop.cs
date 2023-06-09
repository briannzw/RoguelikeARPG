using Player.Interaction;
using System;
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

    public Action OnPanelOpen;

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

        playerControls.Panel.Cancel.performed += (ctx) => { if (!shopPanel.activeSelf) return; shopPanel.SetActive(false); InputManager.ToggleActionMap(playerControls.Gameplay); };
    }
    #endregion

    public void Interact()
    {
        InputManager.ToggleActionMap(playerControls.Panel);
        shopPanel.SetActive(true);
        OnPanelOpen?.Invoke();
    }

    public void Interact(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            targetInventory = other.GetComponent<PlayerInventory>();

            foreach (var item in AvailableItems) {

                if (item is SkillUpgrade)
                {
                    SkillUpgrade skillItem = item as SkillUpgrade;
                    int skillLevel = targetInventory.GetComponent<PlayerSkill>().playerWeapon.SkillMap[skillItem.baseSkill].skillLevel;
                    skillItem.currentUpgrade = skillLevel;
                    item.isDisabled = (skillLevel >= skillItem.totalSkillLevel);
                }
            }

            Interact();
        }
    }

    public void Buy(Item item)
    {
        if (item.isConsumable && targetInventory.IsFull) return;

        if (GameManager.Instance.DeductCoins(item.GetPrice()))
        {
            if(item.isConsumable) targetInventory.AddItem(item);
            if (item is SkillUpgrade)
            {
                // DO skill upgrade
                SkillUpgrade skillItem = item as SkillUpgrade;
                var playerSkill = targetInventory.GetComponent<PlayerSkill>();
                int skillLevel = playerSkill.playerWeapon.SkillMap[skillItem.baseSkill].skillLevel;
                if (skillItem.GetSkill(skillLevel).skillLevel >= skillItem.totalSkillLevel)
                {
                    item.isDisabled = true;
                    playerSkill.AddSkill(skillItem.GetSkill(skillLevel));
                    return;
                }
                skillItem.currentUpgrade = skillItem.GetSkill(skillLevel).skillLevel;
                playerSkill.AddSkill(skillItem.GetSkill(skillLevel));
            }
        }
        else Debug.Log("Not enough money to buy " + item.name);
    }
}
