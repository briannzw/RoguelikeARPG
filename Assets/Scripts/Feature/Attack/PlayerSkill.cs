using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill : MonoBehaviour
{
    [Header("References")]
    public Animator Animator;
    public PlayerWeapon playerWeapon;
    private PlayerAction playerControls;

    [Header("Parameters")]
    [SerializeField]
    private Skill[] Skills = new Skill[4];

    public SkillSlot[] Slots = new SkillSlot[4];

    public Action<int> OnSkillChanged;

    private void Start()
    {
        playerControls = InputManager.playerAction;
        RegisterInputCallback();

        for (int i = 0; i < 4; i++)
        {
            if (Skills[i] == null) continue;

            AddSkill(Skills[i]);
        }
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
            Slots[i].OnCooldownValueChanged?.Invoke(Slots[i].Skill.Cooldown - Slots[i].CD);
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

    public void AddSkill(Skill skill)
    {
        int index = 0;
        if (skill.isBaseSkill)
        {
            if (playerWeapon.SkillMap.Count >= 4) return;
            index = playerWeapon.SkillMap.Count;
            Slots[index].Skill = skill;
            Slots[index].CD = 0;
            Skills[index] = skill;
            playerWeapon.SkillMap.Add(skill, skill);
            return;
        }
        
        for(int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == null) continue;

            index = i;
            // Upgrade Base Skill
            if (Slots[i].Skill.isBaseSkill == skill.baseSkill)
            {
                Slots[i].Skill = skill;
                Skills[i] = skill;
                break;
            }
            // Upgrade previous upgraded skill
            else if (Slots[i].Skill.baseSkill == skill.baseSkill)
            {
                Slots[i].Skill = skill;
                Skills[i] = skill;
                break;
            }
        }

        playerWeapon.SkillMap[skill.baseSkill] = skill;
        OnSkillChanged?.Invoke(index);
    }
}
