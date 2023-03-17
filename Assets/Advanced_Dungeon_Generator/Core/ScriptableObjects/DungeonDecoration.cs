using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ADG.Utilities.DungeonDecorationClases;

[CreateAssetMenu(fileName = "New Dungeon_Decoration", menuName = "Dungeon Component/Dungeon_Decoration", order = 3)]
[System.Serializable]
public class DungeonDecoration : ScriptableObject
{
    public int maxDecoSize  { get; private set; } = 8;

    public bool useDecorationsInTransitions = false;
    public bool excludeOverlappingDecorations = true;

    public bool useDegenerateMainMeshes;
    public bool useDegenerateTransitionMeshes;

    [Space]
    public float mainProbability = 10;
    public List<Decoration> mainDecorations = new List<Decoration>();
    public List<DecorationsCluster> mainClusters = new List<DecorationsCluster>();

    [Space]
    public float transitionsProbability = 10;
    public List<Decoration> transitionsDecorations = new List<Decoration>();
    public List<DecorationsCluster> transitionsClusters = new List<DecorationsCluster>();
}
