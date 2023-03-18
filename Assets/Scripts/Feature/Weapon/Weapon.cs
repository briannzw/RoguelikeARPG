using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Parameters")]
    public float Damage = 1f;
    //public Effect[] Effects; --> Bloodsteal, Bleeding, Damage per Second
    public float CritRate = 5f;

    private PlayerAttack playerAttack;
    public bool Critted = false;

    private void Start()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    public float GetDamage()
    {
        playerAttack.Combo++; // TODO: Handle Combo
        Critted = IsCrit();
        return Damage * (Critted ? 2 : 1);
    }

    private bool IsCrit()
    {
        return Random.Range(0, 100) < CritRate;
    }

    public void DamagePopup(Vector3 position, float damage, bool isCrit)
    {
        playerAttack.CreateDamagePopup(position, damage, isCrit);
    }
}