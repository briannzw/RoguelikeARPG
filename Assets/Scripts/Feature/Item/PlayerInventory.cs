using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : Inventory
{
     public PlayerStats player;
     private PlayerAction playerControls;

      private void Start()
    {
        playerControls = InputManager.playerAction;
        RegisterInputCallback();
    }

    #region Callback
     private void OnEnable()
    {
        RegisterInputCallback();
    }

    private void OnDisable()
    {
        UnregisterInputCallback();
    }

      private void RegisterInputCallback()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.ItemUse.performed += UseItem;
    }

    private void UnregisterInputCallback()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.ItemUse.performed -= UseItem;
    }

     private void UseItem(InputAction.CallbackContext context)
    {
        if (!playerControls.Gameplay.ItemUse.enabled) return;

        if (items.Count == 0) return;
        
        if (items[0] is HealthPotion)
        {
            UseHealItem(items[0] as HealthPotion);
        }
        
        //Disini buat animasi make item kalo ada
        /*if(Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f || Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Blend Tree"))
        Animator.SetTrigger("Attack");*/
    }

    public void UseHealItem(HealthPotion item)
    {
        item.Use();
        float healAmount = item.getHealAmount();
        player.Heal(healAmount);
        RemoveItem(item);
    }
    #endregion
}
