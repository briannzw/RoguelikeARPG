using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dungeon_Brush", menuName = "Dungeon Component/Dungeon_Brush", order = 2)]
public class DungeonBrush : ScriptableObject
{
    public int tileComposition = 0;
    public int transitionTileComposition = 1;

    [Header("Main Tile")]
    public GameObject mainTile;

    [Header("Transition Tiles (Fill, H, V, Corner, Round)")]
    public GameObject[] transitionTiles = new GameObject[5];

    [Header("Texture Atlas")]
    public bool useCombinedAtlas = true;
    public Material mainMaterial;
    public Material extraMaterial;

    public bool useDegenerateMainMeshes;
    public bool useDegenerateTransitionMeshes;

    public GameObject mainBlueprint;
    public GameObject transitionBlueprint;

    public float mainTileAddedHeight;
    public bool  applyMeshColliderToMainTile;
    public int   mainTileLayer;

    public float transitionTilesAddedHeight;
    public bool  applyMeshColliderToTransitionTiles;
    public int   transitionTilesLayer;

    public bool ignoreVoidTiles = true;
}
