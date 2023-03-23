using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Animator Animator;
    public PlayerStats Stats;
    public DamagePopupGenerator DamagePopup;
    private PlayerAction playerControls;

    [Header("Parameters")]
    public bool ResetComboOnHit = true;
    public int Combo = 0;
    public float ComboDuration = 5f;
    public float AttackMultiplier = 1f;

    private float comboTimer = 0f;
    private Dictionary<CombatEnum, float> CombatValues = new Dictionary<CombatEnum, float>();

    private void Start()
    {
        playerControls = InputManager.playerAction;
        RegisterInputCallback();

        Stats.OnPlayerHurt += ResetCombo;
        CombatValues.Add(CombatEnum.HitCount, 0f);
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
        if (CombatValues[CombatEnum.HitCount] == 0) return;

        comboTimer += Time.deltaTime;
        if (comboTimer > ComboDuration)
        {
            CombatValues[CombatEnum.HitCount] = 0;
            comboTimer = 0f;
        }
    }

    private void ResetCombo()
    {
        if (!ResetComboOnHit) return;
        comboTimer = 0f;
        CombatValues[CombatEnum.HitCount]++;
    }

    public void CreateDamagePopup(Vector3 position, float damage, bool isCrit)
    {
        DamagePopup.CreatePopUp(position, damage, isCrit);
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
