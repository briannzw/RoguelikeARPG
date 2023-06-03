using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill : MonoBehaviour
{
    [Header("References")]
    public Animator Animator;
    private PlayerAction playerControls;

    [Header("Parameters")]
    [SerializeField]
    private Skill[] Skills = new Skill[4];

    public SkillSlot[] Slots = new SkillSlot[4];

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Skills[i] == null) continue;

            Slots[i].Skill = Skills[i];
            Slots[i].CD = 0f;
        }
    }

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

    private void Update()
    {
        for(int i = 0; i < 4; i++)
        {
            if (Slots[i].CD <= 0f) continue;

            Slots[i].CD -= Time.deltaTime;
            Slots[i].Skill.OnCooldownValueChanged?.Invoke(Slots[i].Skill.Cooldown - Slots[i].CD);
        }
    }

    #region Callbacks
    private void RegisterInputCallback()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.Skill1.performed += OnSkill1;
        playerControls.Gameplay.Skill2.performed += OnSkill2;
    }

    private void UnregisterInputCallback()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.Skill1.performed -= OnSkill1;
        playerControls.Gameplay.Skill2.performed -= OnSkill2;
    }

    private void OnSkill1(InputAction.CallbackContext context)
    {
        if (!playerControls.Gameplay.Attack.enabled) return;
        if (Slots[0].CD > 0f) return;
        if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Blend Tree")) return;

        Animator.SetInteger(Skills[0].IntName, Skills[0].LoopCount);
        Slots[0].CD = Skills[0].Cooldown;
    }

    private void OnSkill2(InputAction.CallbackContext context)
    {
        if (!playerControls.Gameplay.Attack.enabled) return;
        if (Slots[1].CD > 0f) return;
        if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Blend Tree")) return;

        Animator.SetTrigger(Skills[1].IntName);
        Slots[1].CD = Skills[1].Cooldown;
    }
    #endregion
}
