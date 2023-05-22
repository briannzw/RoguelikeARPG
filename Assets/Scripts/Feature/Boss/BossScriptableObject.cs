using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Boss", menuName = "Enemy/Boss")]
public class BossScriptableObject : EnemyScriptableObject
{

    [Header("Boss Attributes")]
    public Skill[] skills;
}
