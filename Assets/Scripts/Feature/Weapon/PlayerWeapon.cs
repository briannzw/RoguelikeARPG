using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : Weapon
{
    private PlayerAttack playerAttack;

    private void Start()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    public override float GetDamage()
    {
        playerAttack.Combo++; // TODO: Handle Combo
        return base.GetDamage();
    }

    public override void DamagePopup(Vector3 position, float damage, bool isCrit)
    {
        playerAttack.CreateDamagePopup(position, damage, isCrit);
    }
}
