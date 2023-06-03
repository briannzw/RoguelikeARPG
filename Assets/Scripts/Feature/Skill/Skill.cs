using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/New")]
public class Skill : ScriptableObject
{
    [Header("Animator")]
    public string IntName = "Skill";
    public int LoopCount = 1;
    public float Cooldown = 1f;

    [Header("Skill Info")]
    public string Name;
    public Sprite icon;
    public string Description = "To be Implemented";
    public float DamageScaling = 1f;

    public Action<float> OnCooldownValueChanged;
}
