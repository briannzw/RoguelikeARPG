using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Animator Animator;
    private PlayerAction playerControls;

    //[Header("Parameters")]
    
    public int Combo = 0;

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

        if(Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f || Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Blend Tree"))
        Animator.SetTrigger("Attack");
    }
    #endregion
}
