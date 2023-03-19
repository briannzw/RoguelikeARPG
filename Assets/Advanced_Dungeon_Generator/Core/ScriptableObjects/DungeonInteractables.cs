using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ADG.Utilities.DungeonDecorationClases;

[CreateAssetMenu(fileName = "New Dungeon_Interactables", menuName = "Dungeon Component/Dungeon_Interactables", order = 4)]
[System.Serializable]
public class DungeonInteractables : ScriptableObject
{
    public int maxInteractablesSize { get; private set; } = 8;


    public float globalProbability = 10;
    public List<Decoration> interactables = new List<Decoration>();
}
