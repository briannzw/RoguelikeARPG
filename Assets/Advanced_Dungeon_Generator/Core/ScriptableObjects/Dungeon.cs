using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ADG.Utilities.DungeonGeneratorClases;

[CreateAssetMenu(fileName = "New Dungeon", menuName = "Dungeon", order = -1)]
[System.Serializable]
public class Dungeon : ScriptableObject
{
    public int maxDungeonSegments   { get; private set; } = 32;
    public int maxSegmentRooms      { get; private set; } = 64;


    public bool hidePredecesors = true;
    public bool useProceduralAsigment; 
    public bool useArtificialAsigment;

    public bool showGlobalSettings = true;

    public bool hasDecorations = false;
    public bool useGlobalDecoration = true;

    public bool hasInteractables = false;
    public bool useGlobalInteractables = true;

    public bool useGlobalBrush = true;

    [Space]
    public List<DungeonSegment> segments = new List<DungeonSegment>();
}



