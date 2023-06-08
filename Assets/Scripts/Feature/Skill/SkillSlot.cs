using System;

[System.Serializable]
public class SkillSlot
{
    public Skill Skill;
    public float CD;


    public Action<float> OnCooldownValueChanged;
}
