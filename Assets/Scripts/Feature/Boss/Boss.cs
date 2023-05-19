using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss : Enemy
{
    private BossScriptableObject bossSO => enemyStats as BossScriptableObject;

    [SerializeField] private float skillFrequency = 5;
    [SerializeField, Range(0, 1)] private float skillUseChance = 0.2f;
    private float skillUseTimer = 0f;

    private float[] SkillCooldown;

    protected override void Start()
    {
        base.Start();
        SkillCooldown = new float[bossSO.skills.Length];
    }

    protected override void Update()
    {
        base.Update();
        if (!agent.enabled) return;

        for (int i = 0; i < SkillCooldown.Length; i++)
        {
            if (SkillCooldown[i] <= 0f) continue;

            SkillCooldown[i] -= Time.deltaTime;
        }

        skillUseTimer += Time.deltaTime;

        if (distanceToPlayer <= detectionRange && agent.remainingDistance < agent.stoppingDistance)
        {
            if (skillUseTimer > skillFrequency)
            {
                if (Math.Round(Random.Range(0f, 1f), 2) < skillUseChance)
                {
                    int randomIndex = Random.Range(0, bossSO.skills.Length);
                    if (SkillCooldown[randomIndex] > 0f) return;
                    //if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) return;

                    animator.SetTrigger(bossSO.skills[randomIndex].IntName);
                    SkillCooldown[randomIndex] = bossSO.skills[randomIndex].Cooldown;
                }
                skillUseTimer = 0f;
            }
        }
    }
}
