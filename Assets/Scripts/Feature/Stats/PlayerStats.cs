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
    public Animator animator;
    public Slider healthBar;

    [SerializeField] private PlayerAttributes playerAttributes;
    private float currentHealth;

    private void Start()
    {
        currentHealth = playerAttributes.Health;
        healthBar.value = currentHealth / playerAttributes.Health;
        GameManager.Instance.GameTimerEnd += GameWon;
    }

    public override void TakeDamage(float damage)
    {
        animator.SetTrigger("Hurt");
        currentHealth -= damage;
        healthBar.value = currentHealth / playerAttributes.Health;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void GameWon()
    {
        animator.SetBool("Won", true);
    }

    private void Die()
    {
        animator.SetBool("Dead", true);
        animator.SetBool("canMove", false);
        GameManager.Instance.End();
    }
}
