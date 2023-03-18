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
    public string Description = "To be Implemented";
}
