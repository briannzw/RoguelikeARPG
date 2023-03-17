using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill : MonoBehaviour
{
    [Header("References")]
    public Animator Animator;
    private PlayerAction playerControls;

    [Header("Parameters")]
    public Skill[] Skills = new Skill[4];

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
        playerControls.Gameplay.Skill1.performed += OnSkill1;
    }

    private void UnregisterInputCallback()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.Skill1.performed -= OnSkill1;
    }

    private void OnSkill1(InputAction.CallbackContext context)
    {
        if (!playerControls.Gameplay.Attack.enabled) return;

        Animator.SetInteger(Skills[0].IntName, Skills[0].LoopCount);
    }
    #endregion
}
