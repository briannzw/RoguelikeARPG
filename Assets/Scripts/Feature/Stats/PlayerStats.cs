using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerAttributes
{
    public float Health;
}

public class PlayerStats : Character
{
    [Header("References")]
    public Animator Animator;
    public CharaSound CharaSound;
    public Slider healthBar;

    [SerializeField] private PlayerAttributes playerAttributes;
    private float currentHealth;

    public Action OnPlayerHurt;

    private void Start()
    {
        currentHealth = playerAttributes.Health;
        healthBar.value = currentHealth / playerAttributes.Health;
        GameManager.Instance.GameTimerEnd += GameWon;
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        healthBar.value = currentHealth / playerAttributes.Health;
    }

    public override void TakeDamage(Damage damage)
    {
        OnPlayerHurt?.Invoke();
        currentHealth -= damage.value;
        healthBar.value = currentHealth / playerAttributes.Health;

        if (damage.critted)
        {
            CharaSound.OnPlayerHeavyDamaged();
            if(Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Blend Tree"))
            Animator.SetTrigger("Hurt");
        }
        else CharaSound.OnPlayerHurt();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void GameWon()
    {
        Animator.SetBool("Won", true);
    }

    private void Die()
    {
        CharaSound.OnPlayerDead();
        Animator.SetBool("Dead", true);
        Animator.SetBool("canMove", false);
        GameManager.Instance.PlayerLose?.Invoke();
        GameManager.Instance.End();
    }
}
