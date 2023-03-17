using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ADG.Utilities.DungeonShapeClases;

[CreateAssetMenu(fileName = "New Dungeon_Shape", menuName = "Dungeon Component/Dungeon_Shape", order = 1)]
public class DungeonShape : ScriptableObject
{
    public int minChunkSize       { get; private set; } = 4;
    public int maxChunkSize       { get; private set; } = 32;
    public int maxRoomChunkWidth  { get; private set; } = 8;
    public int maxRoomChunkHeight { get; private set; } = 8;

    public int maxConcentration { get; private set; } = 128;
    public int maxRemoteness    { get; private set; } = 128;

    public int maxNoiseScale { get; private set; } = 128;
    public int maxOctaves    { get; private set; } = 8;
    public int minLacunarity { get; private set; } = -4;
    public int maxLacunarity { get; private set; } = 4;

    public int maxPassageSize   { get; private set; } = 16;

    [Header("Map Proportions")]
    public SegmentGenerationType generation;

    //Default Values
    public int chunkSize   = 16;
    public int chunkWidth  = 4;
    public int chunkHeight = 4;

    public NoiseInfo noiseInfo;
    public NoiseProcessInfo noiseProcessInfo;
    
    [Header("Background Properties")]
    //Default Values
    public float concentration = 2;
    public float remoteness    = 15;

    [Header("Artificiality Properties")]
    public bool itsAPredefinedRoom;
    public Texture2D canvasTexture;

    public bool prioritizeImageFidelity;

    public Vector3 spawnPosition;
}

public enum SegmentGenerationType { Natural, Artificial };