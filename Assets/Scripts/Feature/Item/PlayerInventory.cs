using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : Inventory
{
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

        UseItem(items[0]);
        //Disini buat animasi make item kalo ada
        /*if(Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f || Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Blend Tree"))
        Animator.SetTrigger("Attack");*/
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
    #endregion
}
