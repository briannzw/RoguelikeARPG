using System.Collections.Generic;
using UnityEngine;

public class SkillSlotsUI : MonoBehaviour
{
    [SerializeField] private PlayerSkill playerSkill;
    [SerializeField] private GameObject skillSlotPrefab;

    private List<SkillSlotUI> skillSlots;

    private void Awake()
    {
        skillSlots = new List<SkillSlotUI>();
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
            slot.OnCooldownValueChanged += (val) => { skillSlot.SetValue(val / slot.Skill.Cooldown); };
            skillSlots.Add(skillSlot);
        }

        playerSkill.OnSkillChanged += (index) => { skillSlots[index].Initialize(playerSkill.Slots[index].Skill.icon); };
    }
}
