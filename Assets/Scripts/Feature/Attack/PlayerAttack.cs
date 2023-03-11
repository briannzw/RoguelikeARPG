using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public Animator playerAnimator;
    private PlayerAction playerControls;

    //[Header("Parameters")]
    
    //public int TotalCombo = 2;
    //private int currentCombo = 0;

    private void Start()
    {
        playerControls = InputManager.playerAction;
        RegisterInputCallback();
    }
    private void OnEnable()
    {
        RegisterInputCallback();
    }

    private void OnDisable()
    {
        UnregisterInputCallback();
    }

    #region Callbacks
    private void RegisterInputCallback()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.Attack.performed += OnAttack;
    }

    private void UnregisterInputCallback()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.Attack.performed -= OnAttack;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!playerControls.Gameplay.Attack.enabled) return;

        playerAnimator.SetTrigger("Attack");
    }
    #endregion
}
