using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ADG
{
    namespace Utilities
    {
        namespace DungeonGeneratorClases
        {
            [System.Serializable]
            public class DungeonSegment
            {
                [Header("Artificial Displacement")]
                public Vector3 artificialDisplacement;
                public bool useLoader = true;
                public float loaderAngle;
                public Vector3 deltaLoaderPos;

                [Header("Extra Objects")]
                public bool useExtra;
                public bool iterateExtra;
                public GameObject extra;
                public Vector3 extraPos;

                [Header("Dungeon Elements")]
                public int rooms;
                public DungeonShape shape;
                public bool useBorder;
                public int borderSize;

                [Header("Segment Brushes")]
                public DungeonBrush[] brushes = new DungeonBrush[3];

                [Header("Decorations")]
                public bool useDecorations;
                public DungeonDecoration[] decorations = new DungeonDecoration[3];

                [Header("Interactables")]
                public bool useInteractables;
                public DungeonInteractables[] interactables = new DungeonInteractables[3];
            }


            [System.Serializable]
            public class DungeonFragment
            {
                public GameObject mainObject;
                public bool isGenerated = false;

                public Vector3 position;
                public DungeonShapeClases.AccessInfo acInfo;

                public List<Coord> hotSpots;

                public int chunkSize;
                public int chunkWidth;
                public int chunkHeight;

                public bool useBorder;
                public int borderSize;

                public int parentSegment;
                public int dungeonOrder;

                public bool useDecorations;
                public bool useInteractables;

                public MeshConstructorClases.MapMatricesInfo mapMatricesInfo;

                public DungeonFragment(int parentSegment, int dungeonOrder)
                {
                    this.parentSegment = parentSegment;
                    this.dungeonOrder = dungeonOrder;
                }

                public DungeonFragment(int parentSegment, int dungeonOrder, bool useBorder, int borderSize, SegmentInformation seg, bool useDecorations, bool useInteractables)
                {
                    isGenerated = false;

                    chunkSize = seg.chunkSize;
                    chunkWidth = seg.chunkWidth;
                    chunkHeight = seg.chunkHeight;

                    this.useBorder = useBorder;
                    this.borderSize = borderSize;

                    this.parentSegment = parentSegment;
                    this.dungeonOrder = dungeonOrder;

                    this.useDecorations = useDecorations;
                    this.useInteractables = useInteractables;
                }
            }


            public class SegmentInformation
            {
                public int chunkSize;
                public int chunkWidth;
                public int chunkHeight;

                public int mapWidth;
                public int mapHeight;

                public int[,] canvasMask;
                public float[,] falloffMask;

                public int generationType;

                public PrefabInformation[] brushesMainTiles = new PrefabInformation[3];
                public PrefabInformation[,] brushesTransitionTiles = new PrefabInformation[3, 5];

                public List<PrefabInformation>[] decorationsMain = new List<PrefabInformation>[3];
                public List<PrefabInformation>[] decorationsTransition = new List<PrefabInformation>[3];


                public SegmentInformation(DungeonShape dSh, DungeonBrush[] dB, DungeonDecoration[] dD, bool useDecorations)
                {
                    chunkSize = dSh.chunkSize;
                    chunkWidth = dSh.chunkWidth;
                    chunkHeight = dSh.chunkHeight;

                    // The Width and Height of the Segments will Always be Two Blocks Larger
                    // This One Unit Border will carry essential information about the Segment surroundings 
                    if (dSh.generation == SegmentGenerationType.Natural)
                    {
                        mapWidth = (chunkSize * chunkWidth) + 2;
                        mapHeight = (chunkSize * chunkHeight) + 2;
                        falloffMask = Tools.MapDesigner.Noise.GenerateFalloffMap(mapWidth, mapHeight, dSh.concentration, dSh.remoteness);

                        generationType = 1;
                    }
                    else if (dSh.generation == SegmentGenerationType.Artificial)
                    {
                        chunkSize = 16;
                        chunkWidth = dSh.canvasTexture.width / 16;
                        chunkHeight = dSh.canvasTexture.height / 16;

                        mapWidth = (chunkSize * chunkWidth) + 2;
                        mapHeight = (chunkSize * chunkHeight) + 2;

                        canvasMask = Tools.MapDesigner.NoiseProcessor.GenerateMapFromTexture(dSh.canvasTexture, ref dSh.spawnPosition);

                        if (!dSh.itsAPredefinedRoom)
                            generationType = 2;
                        else
                            generationType = 3;
                    }


                    for (int i = 0; i < 3; i++)
                    {
                        if (dB[i].useDegenerateMainMeshes)
                        {
                            brushesMainTiles[i] = new PrefabInformation(dB[i].mainTile);
                        }

                        if (dB[i].useDegenerateTransitionMeshes)
                        {
                            for (int j = 0; j < 5; j++)
                            {
                                brushesTransitionTiles[i, j] = new PrefabInformation(dB[i].transitionTiles[j]);
                            }
                        }

                        if (useDecorations)
                        {
                            if (dD[i] != null)
                            {
                                decorationsMain[i] = new List<PrefabInformation>();
                                for (int j = 0; j < dD[i].mainDecorations.Count; j++)
                                {
                                    decorationsMain[i].Add(new PrefabInformation(dD[i].mainDecorations[j].decorationObject));
                                }

                                decorationsTransition[i] = new List<PrefabInformation>();
                                for (int j = 0; j < dD[i].transitionsDecorations.Count; j++)
                                {
                                    decorationsTransition[i].Add(new PrefabInformation(dD[i].transitionsDecorations[j].decorationObject));
                                }
                            }
                        }
                    }
                }

                public SegmentInformation(DungeonShape dSh)
                {
                    chunkSize = dSh.chunkSize;
                    chunkWidth = dSh.chunkWidth;
                    chunkHeight = dSh.chunkHeight;

                    // The Width and Height of the Segments will Always be Two Blocks Larger
                    // This One Unit Border will carry essential information about the Segment surroundings 
                    if (dSh.generation == SegmentGenerationType.Natural)
                    {
                        mapWidth = (chunkSize * chunkWidth) + 2;
                        mapHeight = (chunkSize * chunkHeight) + 2;
                        falloffMask = Tools.MapDesigner.Noise.GenerateFalloffMap(mapWidth, mapHeight, dSh.concentration, dSh.remoteness);

                        generationType = 1;
                    }
                    else if (dSh.generation == SegmentGenerationType.Artificial)
                    {
                        chunkSize = 16;
                        chunkWidth = dSh.canvasTexture.width / 16;
                        chunkHeight = dSh.canvasTexture.height / 16;

                        mapWidth = (chunkSize * chunkWidth) + 2;
                        mapHeight = (chunkSize * chunkHeight) + 2;

                        canvasMask = Tools.MapDesigner.NoiseProcessor.GenerateMapFromTexture(dSh.canvasTexture, ref dSh.spawnPosition);

                        if (!dSh.itsAPredefinedRoom)
                            generationType = 2;
                        else
                            generationType = 3;
                    }
                }
            }

            public class PrefabInformation
            {
                public Vector3 position;
                public Quaternion rotation;
                public Vector3 scale;

                public PrefabInformation(GameObject gameObject)
                {
                    if (gameObject)
                    {
                        position = gameObject.transform.position;
                        rotation = gameObject.transform.rotation;
                        scale = gameObject.transform.localScale;
                    }
                }
            }
        }

        namespace MeshConstructorClases
        {
            public class MapMatricesInfo
            {
                public Vector3 position;
                public Dictionary<Coord, ChunkMatricesContainer> chunks;

                public MapMatricesInfo(Vector3 position)
                {
                    this.position = position;
                    chunks = new Dictionary<Coord, ChunkMatricesContainer>();
                }
            }


            public class ChunkMatricesContainer
            {
                public bool isMarged;
                public Vector3 position;
                public GameObject chunkGameObject;

                public Dictionary<Coord, TileData> tiles;

                public ChunkMatricesContainer(Vector3 position)
                {
                    isMarged = false;
                    this.position = position;

                    tiles = new Dictionary<Coord, TileData>();
                }
            }


            public class TileData
            {
                public int tileType;

                public List<MeshData> mainMeshes;
                public List<MeshData> ornateMeshes;
                public InteracData interactable;

                public TileData(int tileType)
                {
                    this.tileType = tileType;

                    mainMeshes = new List<MeshData>();
                    ornateMeshes = new List<MeshData>();
                }
            }

            public class InteracData
            {
                public Vector3 position;
                //public Quaternion rotation;
                public int decoId;

                public InteracData(Vector3 position, int decoId)
                {
                    this.position = position;
                    //this.rotation = rotation;
                    this.decoId = decoId;
                }
            }

            public class MeshData
            {
                public Matrix4x4 matrix;
                public int tileCompositionId;
                public int decoId;

                public MeshData(Matrix4x4 matrix, int tileCompositionId)
                {
                    this.matrix = matrix;
                    this.tileCompositionId = tileCompositionId;
                }

                public MeshData(Matrix4x4 matrix, int tileCompositionId, int decoId)
                {
                    this.matrix = matrix;
                    this.tileCompositionId = tileCompositionId;
                    this.decoId = decoId;
                }
            }
        }

        namespace DungeonShapeClases
        {
            [System.Serializable]
            public class NoiseInfo
            {
                [Header("Noise Properties")]
                public float noiseScale;
                public int octaves;
                [Range(0, 1)]
                public float persistance;
                public float lacunarity;

                public NoiseInfo()      //Default values
                {
                    noiseScale = 9;
                    octaves = 4;
                    persistance = 0.5f;
                    lacunarity = 2;
                }

                public NoiseInfo(float noiseScale, int octaves, float persistance, float lacunarity)
                {
                    this.noiseScale = noiseScale;
                    this.octaves = octaves;
                    this.persistance = persistance;
                    this.lacunarity = lacunarity;
                }
            }


            [System.Serializable]
            public class NoiseProcessInfo
            {
                [Header("Noise Processing")]
                [Range(0, 1)]
                public float minFloorValue;
                [Range(0, 1)]
                public float minStructureValue;

                [Header("Floor Processing")]
                public int minTilesToGenWalls;
                public int minTilesToGenRooms;
                [Range(1, 2)]
                public int failedRoomValue;
                [Space]
                public int minTilesToGenStructures;
                public int maxHallwayRadius;

                public NoiseProcessInfo()   //Default values
                {
                    minFloorValue = 0.5f;
                    minStructureValue = 0.75f;
                    minTilesToGenWalls = 20;
                    minTilesToGenRooms = 50;
                    failedRoomValue = 1;
                    minTilesToGenStructures = 3;
                    maxHallwayRadius = 3;
                }

                public NoiseProcessInfo(float mfv, float msfv, int mcp, int mch, int fhb, int mcs, int mrp)
                {
                    minFloorValue = mfv;
                    minStructureValue = msfv;
                    minTilesToGenWalls = mcp;
                    minTilesToGenRooms = mch;
                    failedRoomValue = fhb;
                    minTilesToGenStructures = mcs;
                    maxHallwayRadius = mrp;
                }
            }


            [System.Serializable]
            public class AccessInfo
            {
                public bool normalAccessGen;
                [Space]

                [Header("Entrance")]
                public bool hasEntrance;
                [Range(1, 4)]
                public int entranceDirection;
                public Vector2 entranceEnd;

                [Header("Exit")]
                public bool hasExit;
                [Range(1, 4)]
                public int exitDirection;
                public Vector2 exitStart;

                public AccessInfo()         //Default values
                {
                    normalAccessGen = true;

                    hasEntrance = true;
                    entranceDirection = 1;
                    entranceEnd = Vector2.zero;

                    hasExit = true;
                    exitDirection = 3;
                    exitStart = Vector2.zero;
                }

                public AccessInfo(AccessInfo acInfo)
                {
                    normalAccessGen = acInfo.normalAccessGen;

                    hasEntrance = acInfo.hasEntrance;
                    entranceDirection = acInfo.entranceDirection;
                    entranceEnd = acInfo.entranceEnd;

                    hasExit = acInfo.hasExit;
                    exitDirection = acInfo.exitDirection;
                    exitStart = acInfo.exitStart;
                }
            }
        }

        namespace DungeonDecorationClases
        {
            [System.Serializable]
            public class Decoration
            {
                public GameObject decorationObject;
                public int parentCluster;

                public float spawnProbability;
                public float deltaProbability;
                public float deltaH;

                public bool useRandomRotation = true;
                public float rotation;

                public Decoration()         //Default values
                {
                    useRandomRotation = true;
                    rotation = 0.0f;
                }
            }


            [System.Serializable]
            public class DecorationsCluster
            {
                public Material material;
                public GameObject blueprint;
                public int clusterSize;

                public DecorationsCluster(int clusterSize)
                {
                    this.clusterSize = clusterSize;
                }
            }
        }
    }

    namespace Tools
    {
        public class SecurityCheck
        {
            public static bool ValidateDungeonComponent(Dungeon dungeon)
            {
                foreach (Utilities.DungeonGeneratorClases.DungeonSegment segment in dungeon.segments)
                {
                    if (!segment.shape)
                    {
                        Debug.LogError("The selected Dungeon cannot be accepted due to lack of essential information (DungeonShape)");
                        return false;
                    }

                    if (!segment.brushes[0])
                    {
                        Debug.LogError("The selected Dungeon cannot be accepted due to lack of essential information (DungeonBrush for Ground)");
                        return false;
                    }

                    if (!segment.brushes[1])
                    {
                        Debug.LogError("The selected Dungeon cannot be accepted due to lack of essential information (DungeonBrush for Walls)");
                        return false;
                    }
                }

                return true;
            }

            public static bool ValidateDungeonComponent(DungeonDecoration deco)
            {
                foreach (Utilities.DungeonDecorationClases.DecorationsCluster dc in deco.mainClusters)
                {
                    if (!dc.material)
                    {
                        Debug.LogError("The selected Dungeon_Decoration cannot be accepted due to lack of essential information (Main Decorations Material)");
                        return false;
                    }
                }

                foreach (Utilities.DungeonDecorationClases.Decoration dd in deco.mainDecorations)
                {
                    if (!dd.decorationObject)
                    {
                        Debug.LogError("The selected Dungeon_Decoration cannot be accepted due to lack of essential information (Main Decoration Mesh)");
                        return false;
                    }
                }

                if (deco.useDecorationsInTransitions)
                {
                    foreach (Utilities.DungeonDecorationClases.DecorationsCluster dc in deco.transitionsClusters)
                    {
                        if (!dc.material)
                        {
                            Debug.LogError("The selected Dungeon_Decoration cannot be accepted due to lack of essential information (Transition Decorations Material)");
                            return false;
                        }
                    }

                    foreach (Utilities.DungeonDecorationClases.Decoration dd in deco.transitionsDecorations)
                    {
                        if (!dd.decorationObject)
                        {
                            Debug.LogError("The selected Dungeon_Decoration cannot be accepted due to lack of essential information (Transition Decoration Mesh)");
                            return false;
                        }
                    }
                }

                return true;
            }

            public static bool ValidateDungeonComponent(DungeonInteractables interac)
            {
                foreach (Utilities.DungeonDecorationClases.Decoration dd in interac.interactables)
                {
                    if (!dd.decorationObject)
                    {
                        Debug.LogError("The selected Dungeon_Interactable cannot be accepted due to lack of essential information (Interactable Mesh)");
                        return false;
                    }
                }

                return true;
            }
        

            public static bool ValidateDungeonComponent(DungeonBrush brush)
            {
                if (!brush.mainTile && (brush.tileComposition == 0 || brush.tileComposition == 2))
                {
                    Debug.LogError("The selected Dungeon_Brush cannot be accepted due to lack of essential information (Main Mesh)");
                    return false;
                }

                if (brush.transitionTileComposition == 0)
                {
                    foreach (GameObject gO in brush.transitionTiles)
                    {
                        if (!gO)
                        {
                            Debug.LogError("The selected Dungeon_Brush cannot be accepted due to lack of essential information (Transition Meshes)");
                            return false;
                        }
                    }
                }

                if(brush.transitionTileComposition == 2)
                {
                    for(int i = 1; i < brush.transitionTiles.Length; i++)
                    {
                        if (!brush.transitionTiles[i])
                        {
                            Debug.LogError("The selected Dungeon_Brush cannot be accepted due to lack of essential information (Transition Meshes)");
                            return false;
                        }
                    }
                }

                if (!brush.mainMaterial)
                {
                    Debug.LogError("The selected Dungeon_Brush cannot be accepted due to lack of essential information (Material Texture Atlas)");
                    return false;
                }

                return true;
            }

            public static bool ValidateDungeonComponent(DungeonShape shape)
            {
                if (shape.generation == SegmentGenerationType.Artificial)
                {
                    if (!shape.canvasTexture)
                    {
                        Debug.LogError("The selected Dungeon_Shape cannot be accepted due to lack of essential information (Room Mask Texture)");
                        return false;
                    }
                }

                return true;
            }
        }
    }
}