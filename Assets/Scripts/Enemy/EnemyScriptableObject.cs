using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class EnemyScriptableObject : ScriptableObject
{
    [Header("Attributes")]
    public string enemyName;
    public float maxHealth;
    public float attackDamage;
    public float critRate;
    public float moveSpeed;
    public float attackRange;
    public float attackCooldown;
}
