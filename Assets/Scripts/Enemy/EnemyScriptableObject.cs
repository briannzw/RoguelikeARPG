using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class EnemyScriptableObject : ScriptableObject
{
    [Header("Attributes")]
    public string enemyName;
    public float maxHealth;
    public int attackDamage;
    public float moveSpeed;
    public GameObject model;
    public float attackRange;
}
