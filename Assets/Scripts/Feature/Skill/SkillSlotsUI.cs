using UnityEngine;

public class SkillSlotsUI : MonoBehaviour
{
    [SerializeField] private PlayerSkill playerSkill;
    [SerializeField] private GameObject skillSlotPrefab;
    private void Awake()
    {
        if(playerSkill == null)
        {
            playerSkill = FindObjectOfType<PlayerSkill>().GetComponent<PlayerSkill>();
        }
    }

    private void Start()
    {
        foreach (var slot in playerSkill.Slots)
        {
            if (slot.Skill == null) continue;

            GameObject go = Instantiate(skillSlotPrefab, transform);
            SkillSlotUI skillSlot = go.GetComponent<SkillSlotUI>();
            skillSlot.Initialize(slot.Skill.icon);
            slot.Skill.OnCooldownValueChanged += (val) => { skillSlot.SetValue(val / slot.Skill.Cooldown); };
        }
    }
}
