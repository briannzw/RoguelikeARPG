using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage
{
    public float value = 0;
    public bool critted = false;
}

public class Weapon : MonoBehaviour
{
    [Header("Parameters")]
    public float Damage = 1f;
    //public Effect[] Effects; --> Bloodsteal, Bleeding, Damage per Second
    public float CritRate = 5f;
    public float AttackMultiplier = 1f;
    public WeaponHit LocalHit;

    private AudioSource audioSource;
    public AudioClip attackSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        LocalHit = GetComponentInChildren<WeaponHit>(includeInactive: true);
        if(audioSource != null)
            audioSource.clip = attackSound;
    }

    public virtual Damage GetDamage()
    {
        Damage damage = new Damage();
        damage.critted = IsCrit();
        damage.value = Damage * AttackMultiplier * (damage.critted ? 2 : 1);
        return damage;
    }

    public virtual Damage GetDamage(Skill skill)
    {
        Damage damage = new Damage();

        damage.critted = IsCrit();
        damage.value = Damage * skill.DamageScaling * AttackMultiplier * (damage.critted ? 2 : 1);
        return damage;
    }

    private bool IsCrit()
    {
        return Random.Range(0, 100) < CritRate;
    }

    public virtual void DamagePopup(Vector3 position, Damage damage)
    {
    }

    public void PlaySound()
    {
        if (audioSource == null) return;

        audioSource.Play();
    }
}
