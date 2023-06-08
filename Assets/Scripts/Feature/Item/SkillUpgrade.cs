using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Upgrade", menuName = "Inventory/Skill Upgrade")]
public class SkillUpgrade : Item
{
    public Skill baseSkill;
    [SerializeField] private Skill[] SkillReferences;
    public float priceMultiplier;
    public int totalSkillLevel => SkillReferences[SkillReferences.Length - 1].skillLevel;
    public int currentUpgrade = 0;
    public Skill GetSkill(int level) => SkillReferences[level];
    public override int GetPrice() => Mathf.RoundToInt(price * Mathf.Pow(priceMultiplier, currentUpgrade));
}
