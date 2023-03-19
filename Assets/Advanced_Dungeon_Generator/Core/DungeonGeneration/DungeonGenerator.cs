using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;

using ADG.Utilities.DungeonGeneratorClases;
using ADG.Utilities.DungeonShapeClases;
using ADG.Tools.MapDesigner;
using ADG.Tools.MeshDesigner;
//using System.Drawing;


[RequireComponent(typeof(MeshBuilder))]
public class DungeonGenerator : MonoBehaviour
{
    // Dungeon Options
    // Generation Options  
    public bool useRandomSeed = true;
    public bool generateDungeonAtStart = true;

    // Loading Options
    public int roomsToLoadAtStart = 2;
    public int roomsLoadOffset = 1;
    public int roomDestructionOffset = 2;
    public bool destroyFarBackRooms = true;

    // Debug Options
    public int siteToCharge;
    public bool showDebugOptions = false;
    public bool canGenerateSites = false;



    // Key Variables
    public int seed;
    public Dungeon dungeon;
    
    public int loaderLayer;
    public bool usePrefabLoader = false;
    public GameObject loader;

    public List<int[,]> dungeonMaps;
    public List<DungeonFragment> dungeonFragments;
    public List<SegmentInformation> dungeonSegmentsInfo;

    public int dungeonFragmentsAmount;
    private int dungeonSegmentsAmount;

    private Vector2 offset;




    // State Variables
    private bool isTheDungeonInitialized = false;
    private bool isTheDungeonLoaded = false;
    private bool areGameObjectsCreated = false;     //Debug Purposes Only

    

    // System Dependencies
    public System.Random rng;
    private MeshBuilder meshBuilder;




    // System Events

    // This event will be called once the Dungeon has been loaded.
    // Use it as deem necessary
    public event Action OnDungeonComplete;

    private ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();
    private int currentLoadedDungeonFragments = 0;





    #region Singleton

    public static DungeonGenerator _instance;
    public static DungeonGenerator Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    private void Start()
    {
        // DungeonStart
        if (generateDungeonAtStart)
            DungeonStartTrigger();
    }

    private void Update()
    {
        if(mainThreadQueue.Count > 0)
        {
            Action callback;
            if(mainThreadQueue.TryDequeue(out callback))
                callback();
        }
    }



    #region ThreadStates_&_OnDungeonCompleteEvent

    public void OnEmptyMeshBuilderQueue()
    {
        if (isTheDungeonLoaded)
        {
            //Debug.Log("The Dungeon has already been loaded");
            return;
        }

        if (currentLoadedDungeonFragments < roomsToLoadAtStart)
        {
            //Debug.Log("There is not enough information to load the Dungeon");
            return;
        }


        OnDungeonComplete?.Invoke();
        isTheDungeonLoaded = true;
    }

    public void OnDungeonFragmentBuildRequestSet()
    {
        currentLoadedDungeonFragments++;
    }

    public void AddToMainQueue(Action action)
    {
        mainThreadQueue.Enqueue(action);
    }

    #endregion

    public void DungeonStartTrigger()
    {
        if (isTheDungeonInitialized)
        {
            Debug.LogWarning("The Dungeon is Already Initialized");
            return;
        }


        // DungeonLoader Inizialization & Check 
        if (!usePrefabLoader)
        {
            loader = new GameObject("Loader", typeof(BoxCollider), typeof(DungeonLoader));
            loader.GetComponent<BoxCollider>().center = new Vector3(0, 0.5f, 0);
            loader.GetComponent<BoxCollider>().size = new Vector3(1, 1, 0);
            loader.GetComponent<BoxCollider>().isTrigger = true;
            loader.layer = loaderLayer;
            loader.SetActive(false);
        }
        else
        {
            if (!loader)
            {
                Debug.LogError("The Loader to be used has not been defined in the Dungeon Generator");
                return;
            }
        }

        // Dungeon Check
        if (!dungeon)
        {
            Debug.LogError("The Dungeon to be loaded has not been defined in the Dungeon Generator");
            return;
        }


        // MeshBuilder Init
        meshBuilder = gameObject.GetComponent<MeshBuilder>();

        // DungeonGenerator Inizialization
        if (useRandomSeed)
            seed = UnityEngine.Random.Range(-1000000, 1000000);
        rng = new System.Random(seed);
        dungeonMaps = new List<int[,]>();
        dungeonFragments = new List<DungeonFragment>();
        dungeonSegmentsInfo = new List<SegmentInformation>();

        // DungeonFragments Count
        dungeonFragmentsAmount = 0;
        dungeonSegmentsAmount = dungeon.segments.Count;
        for (int i = 0; i < dungeonSegmentsAmount; i++)
        {
            dungeonFragmentsAmount += dungeon.segments[i].rooms;
        }

        // DungeonSegments Information Inizialization
        for (int i = 0; i < dungeonSegmentsAmount; i++)
        {
            dungeonSegmentsInfo.Add( new SegmentInformation( dungeon.segments[i].shape , dungeon.segments[i].brushes , dungeon.segments[i].decorations , dungeon.segments[i].useDecorations));
        }
        isTheDungeonInitialized = true;


        // DungeonFragments Information Creation
        Thread thread = new Thread( GenerateAllDungeonFragmentsInformation );
        thread.Start();
    }



    // This method is called indirectly at the end of the multithreaded load of the GenerateAllDungeonFragmentsInformation method
    public void StartDungeonCreation()  
    {
        CreateAllGameObjects();
        areGameObjectsCreated = true;

        for (int i = 0; i < roomsToLoadAtStart; i++)
        {
            ChargeDungeonFragment(i, true, true);
        }
    }


    public void ChargeDungeonFragment(int n, bool visibility, bool isEntrance)
    {
        if(n >= 0 && n < dungeonFragmentsAmount)
        {
            if (!dungeonFragments[n].isGenerated)
            {
                dungeonFragments[n].mainObject.SetActive(true);
                GenerateFragmentMesh(n, isEntrance);
                dungeonFragments[n].isGenerated = true;
            }
            else
            {
                dungeonFragments[n].mainObject.SetActive(visibility);
            }
        }
    }

    void GenerateFragmentMesh(int n, bool isEntrance)
    {
        ThreadStart thS = delegate
        {
            int startX = 0;
            int startY = 0;

            // A new Fragment Mesh is always created from its Entrance
            if (isEntrance)     
            {
                startX = (int)dungeonFragments[n].acInfo.entranceEnd.x / dungeonFragments[n].chunkSize;
                startY = (int)dungeonFragments[n].acInfo.entranceEnd.y / dungeonFragments[n].chunkSize;
            }else{
                startX = (int)dungeonFragments[n].acInfo.exitStart.x / dungeonFragments[n].chunkSize;
                startY = (int)dungeonFragments[n].acInfo.exitStart.y / dungeonFragments[n].chunkSize;
            }

            if (dungeonFragments[n].useBorder)
            {
                startX++;
                startY++;
            }

            meshBuilder.GenerateChunk(dungeonFragments[n], new Coord(startX, startY));
        };
        new Thread(thS).Start();
    }

    public void DestroyDungeonFragment(int n)
    {
        if (n >= 0 && n < dungeonFragmentsAmount)
        {
            if (dungeonFragments[n].isGenerated)
            {
                foreach (Transform child in dungeonFragments[n].mainObject.transform)
                {
                    if (child.GetComponent<DungeonLoader>() == null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
            dungeonFragments[n].isGenerated = false;
        }
    }

    public void DebugGenerateDungeonFragment()
    {
        if (canGenerateSites && dungeon && loader)
        {
            if (!areGameObjectsCreated)
            {
                CreateAllGameObjects();
                areGameObjectsCreated = true;
            }
            ChargeDungeonFragment(siteToCharge, true, true);
        }
    }




    // The loading of all the crutial Dungeon Fragments Generation Information is done in a separate thread  
    void GenerateAllDungeonFragmentsInformation()
    {
        int fragmentsCount = 0;
        for (int i = 0; i < dungeonSegmentsAmount; i++)
        {
            bool useBorder = dungeon.segments[i].useBorder;
            int borderSize = dungeon.segments[i].borderSize;
            bool useDecorations = dungeon.segments[i].useDecorations;
            bool useInteractables = dungeon.segments[i].useInteractables;

            for (int j = 0; j < dungeon.segments[i].rooms; j++)
            {
                // A DungeonFragment is created for each Room
                // Each DungeonFragment has instructions on how it connects to the previous and next DungeonFragment
                dungeonFragments.Add( new DungeonFragment(i, fragmentsCount, useBorder, borderSize, dungeonSegmentsInfo[i], useDecorations, useInteractables ));
                dungeonFragments[fragmentsCount].acInfo = DefineMapAccess(fragmentsCount);

                // The DungeonSegment Generation Type will be use in the Map Creation of all its Fragments
                // The Maps of the DungeonFragments themselves are stored separately
                List<Coord> hotSpots;
                dungeonMaps.Add( GenerateMap(dungeonSegmentsInfo[i].generationType, i, fragmentsCount, out hotSpots) );
                dungeonFragments[fragmentsCount].hotSpots = hotSpots;

                fragmentsCount++;
            }
        }

        // Each DungeonFragment has a In-Game physical representation whose global position has to be defined
        // The center of the Room Map is considered to be the local (0, 0) of the DungeonFragment and the global position of the DungeonFragment itself 
        DefineSitesPositions();



        // Dungeon Border Beheaviour (MAY BE CHANGED)
        int count = 0;
        for (int i = 0; i < dungeonSegmentsAmount; i++)
        {
            if (dungeon.segments[i].useBorder)
            {
                for (int j = 0; j < dungeon.segments[i].rooms; j++)
                {
                    DungeonFragment next = (count < dungeonFragmentsAmount - 1) ? dungeonFragments[count + 1] : new DungeonFragment(0, 0);
                    DungeonFragment prev = (count > 0) ? dungeonFragments[count - 1] : new DungeonFragment(0, 0);
                    dungeonMaps[count] = MapMerger.ApplyMargenMap(dungeonMaps[count], dungeonFragments[count], next, prev);
                    dungeonFragments[count].chunkWidth += 2;
                    dungeonFragments[count].chunkHeight += 2;
                    dungeonFragments[count].position.x -= dungeonFragments[count].chunkSize;
                    dungeonFragments[count].position.z -= dungeonFragments[count].chunkSize;

                    count++;
                }
            }
            else
            {
                count += dungeon.segments[i].rooms;
            }
        }

        // Individual Tile Properties Information Generation of all the DungeonFragments based on its Dungeon Map 
        for (int i = 0; i < dungeonFragmentsAmount; i++)
        {
            dungeonFragments[i].mapMatricesInfo = MeshGenerator.GenerateSiteInformation(dungeonMaps[i], dungeonFragments[i], dungeon.segments[dungeonFragments[i].parentSegment], dungeonSegmentsInfo[dungeonFragments[i].parentSegment]);
        }


        //All Dungeon Fragments Information has been created and we will proceed to create all its GameObject containers on the main thread.
        AddToMainQueue(StartDungeonCreation);
    }

    private void CreateAllGameObjects()
    {
        // Each DungeonFragment has a In-Game physical representation encapsulated in a GameObject
        // This object is the recipient of all the Chunk Meshes of the DungeonFragment.
        int count = 0;
        for (int i = 0; i < dungeonSegmentsAmount; i++)
        {
            for(int j = 0; j < dungeon.segments[i].rooms; j++)
            {
                dungeonFragments[count].mainObject = new GameObject("Site " + count);
                dungeonFragments[count].mainObject.transform.position = dungeonFragments[count].position;
                dungeonFragments[count].mainObject.isStatic = true;
                dungeonFragments[count].mainObject.SetActive(false);

                // Each DungeonSegment can have an extra object associated with the creation of its fragments.
                // This feature is still primitive in nature
                if (dungeon.segments[i].useExtra)
                {
                    if (dungeon.segments[i].extra)
                    {
                        if (dungeon.segments[i].iterateExtra)
                        {
                            foreach (Coord coor in dungeonFragments[count].hotSpots)
                            {
                                GameObject extra = Instantiate(dungeon.segments[i].extra);

                                if (dungeon.segments[i].useBorder) {
                                    int size = dungeonFragments[count].chunkSize;
                                    extra.transform.position = new Vector3(coor.tileX + size, 0, coor.tileY + size);
                                } else {
                                    extra.transform.position = new Vector3(coor.tileX, 0, coor.tileY);
                                }
                                
                                extra.transform.SetParent(dungeonFragments[count].mainObject.transform, false);
                            }
                        }
                        else
                        {
                            GameObject extra = Instantiate(dungeon.segments[i].extra);
                            extra.transform.position = dungeon.segments[i].extraPos;
                            extra.transform.SetParent(dungeonFragments[count].mainObject.transform, false);
                        }
                    }
                }

                count++;
            }
        }

        // Each physical object has a room-sized Loader associated with it. 
        // This Loader is responsible for loading and unloading the adjacent DungeonFragments.
        if (dungeon.useProceduralAsigment)
        {
            for (int i = 1; i < dungeonFragmentsAmount - 1; i++)
            {
                int ratio;
                Vector3 size = Vector3.one;

                if (dungeonFragments[i].acInfo.exitDirection < dungeonFragments[i].acInfo.entranceDirection){
                    ratio = dungeonFragments[i].acInfo.entranceDirection + dungeonFragments[i].acInfo.exitDirection;
                }else{
                    ratio = (dungeonFragments[i].acInfo.entranceDirection + dungeonFragments[i].acInfo.exitDirection) - 4;
                }

                float angle = (ratio) * 45;
                
                if(dungeonFragments[i].chunkWidth == dungeonFragments[i].chunkHeight)   //It´s a square
                {
                    if (ratio % 2 == 0)  //It´s even
                    {
                        size.x = (dungeonFragments[i].useBorder) ? (dungeonFragments[i].chunkWidth - 2) * dungeonFragments[i].chunkSize : dungeonFragments[i].chunkWidth * dungeonFragments[i].chunkSize;
                    }else{
                        size.x = (dungeonFragments[i].useBorder) ? (dungeonFragments[i].chunkWidth - 2) * dungeonFragments[i].chunkSize * Mathf.Sqrt(2) : dungeonFragments[i].chunkWidth * dungeonFragments[i].chunkSize * Mathf.Sqrt(2);
                    }
                }
                else    //It´s a rectangle
                {
                    if (ratio % 2 == 0)
                    {
                        if ((ratio / 2) % 2 == 0)   //Horizontal
                        {
                            size.x = (dungeonFragments[i].useBorder) ? (dungeonFragments[i].chunkWidth - 2) * dungeonFragments[i].chunkSize : dungeonFragments[i].chunkWidth * dungeonFragments[i].chunkSize;
                        }else{                      //Vertical
                            size.x = (dungeonFragments[i].useBorder) ? (dungeonFragments[i].chunkHeight - 2) * dungeonFragments[i].chunkSize : dungeonFragments[i].chunkHeight * dungeonFragments[i].chunkSize;
                        }
                    }else{
                        float ang = (angle) * Mathf.Deg2Rad;

                        float x = (dungeonFragments[i].useBorder) ? Mathf.Round(Mathf.Cos(ang)) * (dungeonFragments[i].chunkWidth - 2) : Mathf.Round(Mathf.Cos(ang)) * dungeonFragments[i].chunkWidth;
                        float y = (dungeonFragments[i].useBorder) ? Mathf.Round(Mathf.Sin(ang)) * (dungeonFragments[i].chunkHeight - 2) : Mathf.Round(Mathf.Sin(ang)) * dungeonFragments[i].chunkHeight;

                        angle = Mathf.Atan(y / x) * Mathf.Rad2Deg;
                        if (x < 0)
                            angle += 180;

                        size.x = Mathf.Sqrt(Mathf.Pow(dungeonFragments[i].chunkWidth, 2) + Mathf.Pow(dungeonFragments[i].chunkHeight, 2)) * dungeonFragments[i].chunkSize;
                    }
                }

                Quaternion rotation = Quaternion.Euler(0, angle, 0);

                Vector3 position = new Vector3((dungeonFragments[i].chunkWidth * dungeonFragments[i].chunkSize) / 2 - 0.5f, 0, (dungeonFragments[i].chunkHeight * dungeonFragments[i].chunkSize) / 2 - 0.5f);
                GameObject load = Instantiate(loader, position, rotation);
                load.transform.localScale = size;
                load.transform.SetParent(dungeonFragments[i].mainObject.transform, false);
                load.SetActive(true);

                load.GetComponent<DungeonLoader>().site = i;
            }
        }
        else
        {
            for (int i = 1; i < dungeonFragmentsAmount - 1; i++)
            {
                if (dungeon.segments[dungeonFragments[i].parentSegment].useLoader)
                {
                    float height = (dungeonFragments[i].useBorder) ? (dungeonFragments[i].chunkHeight - 2) * dungeonFragments[i].chunkSize : dungeonFragments[i].chunkHeight * dungeonFragments[i].chunkSize;
                    float width = (dungeonFragments[i].useBorder) ? (dungeonFragments[i].chunkWidth - 2) * dungeonFragments[i].chunkSize : dungeonFragments[i].chunkWidth * dungeonFragments[i].chunkSize;

                    Vector3 size = Vector3.one;
                    float angle = dungeon.segments[dungeonFragments[i].parentSegment].loaderAngle;

                    float absCosAngulo = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));
                    float absSinAngulo = Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad));
                    if (width / 2 * absSinAngulo <= height / 2 * absCosAngulo)
                    {
                        size.x = width / absCosAngulo;
                    }
                    else
                    {
                        size.x = height / absSinAngulo;
                    }

                    Quaternion rotation = Quaternion.Euler(0, angle + 270, 0);

                    GameObject load = Instantiate(loader, new Vector3(-0.5f, 0, -0.5f) + dungeon.segments[dungeonFragments[i].parentSegment].deltaLoaderPos, rotation);
                    load.transform.localScale = size;
                    load.transform.SetParent(dungeonFragments[i].mainObject.transform, false);
                    load.SetActive(true);

                    load.GetComponent<DungeonLoader>().site = i;
                }
            }
        }

    }

    private void DefineSitesPositions()
    {
        // Each DungeonFragment has a In-Game physical representation whose global position has to be defined
        // The purpose is to have the entrances and exits of the fragments aligned.
        for (int i = 1; i < dungeonFragmentsAmount; i++)
        {
            if (dungeon.useProceduralAsigment)
            {
                int n = dungeonFragments[i - 1].acInfo.exitDirection;

                float dxPrevRoom = (dungeonFragments[i - 1].chunkWidth * dungeonFragments[i - 1].chunkSize);
                float dyPrevRoom = (dungeonFragments[i - 1].chunkHeight * dungeonFragments[i - 1].chunkSize);

                float dxCurrRoom = (dungeonFragments[i].chunkWidth * dungeonFragments[i].chunkSize);
                float dyCurrRoom = (dungeonFragments[i].chunkHeight * dungeonFragments[i].chunkSize);

                float dxEntries  = dungeonFragments[i - 1].acInfo.exitStart.x - dungeonFragments[i].acInfo.entranceEnd.x;
                float dyEntries  = dungeonFragments[i - 1].acInfo.exitStart.y - dungeonFragments[i].acInfo.entranceEnd.y;

                if (n == 1)         //Up
                {
                    dungeonFragments[i].position.x =  dungeonFragments[i - 1].position.x + dxEntries;
                    dungeonFragments[i].position.z =  (dyPrevRoom) + dungeonFragments[i - 1].position.z;
                }
                else if (n == 2)    //Right
                {
                    dungeonFragments[i].position.x =  (dxPrevRoom) + dungeonFragments[i - 1].position.x;
                    dungeonFragments[i].position.z =  dungeonFragments[i - 1].position.z + dyEntries;
                }
                else if (n == 3)    //Down
                {
                    dungeonFragments[i].position.x =  dungeonFragments[i - 1].position.x + dxEntries;
                    dungeonFragments[i].position.z = -(dyCurrRoom) + dungeonFragments[i - 1].position.z;
                }
                else                //Left
                {
                    dungeonFragments[i].position.x = -(dxCurrRoom) + dungeonFragments[i - 1].position.x;
                    dungeonFragments[i].position.z =  dungeonFragments[i - 1].position.z + dyEntries;
                }
            }
            else
            {
                dungeonFragments[i].position = dungeonFragments[i - 1].position + dungeon.segments[dungeonFragments[i].parentSegment].artificialDisplacement;
            }
        }
    }

    public AccessInfo DefineMapAccess(int fragmentOrder)
    {
        //The DefineMapAccess Method is only relevant for Procedurally Assigned Dungeons
        AccessInfo acInfo = new AccessInfo();
        acInfo.normalAccessGen = (dungeon.useProceduralAsigment) ? true : false;
        if (!acInfo.normalAccessGen)
            return acInfo;

        // The first DungeonFragment is Exit Only
        if(fragmentOrder == 0)
        {
            acInfo.hasEntrance = false;
            acInfo.hasExit = true;

            acInfo.entranceDirection = 0;
            acInfo.exitDirection = rng.Next(1, 5);

            return acInfo;
        }

        // The last DungeonFragment is Entance Only
        if(fragmentOrder == dungeonFragmentsAmount - 1)
        {
            acInfo.hasEntrance = true;
            acInfo.hasExit = false;

            acInfo.exitDirection = 0;
            acInfo.entranceDirection = dungeonFragments[fragmentOrder - 1].acInfo.exitDirection + 2;
            if (acInfo.entranceDirection > 4)
                acInfo.entranceDirection -= 4;

            return acInfo;
        }

        // The in between DungeonFragments are full trafficable
        // The DungeonFragments Entances are defined by the Exits of the previous ones
        // The DungeonFragmnets Exits are randomly set 
        acInfo.hasEntrance = true;
        acInfo.hasExit = true;

        acInfo.entranceDirection = dungeonFragments[fragmentOrder - 1].acInfo.exitDirection + 2;
        if (acInfo.entranceDirection > 4)
            acInfo.entranceDirection -= 4;

        int extra = 0;

        // /* Extra code to avoid DungeonFragments Overlapping
        float maxRoomRatio = 0.3f;
        AccessInfo prevAc = dungeonFragments[fragmentOrder - 1].acInfo;

        if (prevAc.exitDirection == 1 || prevAc.exitDirection == 3)
        {
            if (prevAc.exitStart.x > (dungeonFragments[fragmentOrder - 1].chunkWidth * dungeonFragments[fragmentOrder - 1].chunkSize) * maxRoomRatio)
            {
                extra = 2;
            } else {
                extra = 4;
            }
        } else {
            if (prevAc.exitStart.y > (dungeonFragments[fragmentOrder - 1].chunkHeight * dungeonFragments[fragmentOrder - 1].chunkSize) * maxRoomRatio)
            {
                extra = 1;
            } else {
                extra = 3;
            }
        }
        // */

        do
        {
            acInfo.exitDirection = rng.Next(1, 5);
        }
        while (acInfo.exitDirection == acInfo.entranceDirection || acInfo.exitDirection == extra);


        return acInfo;
    }

    public int[,] GenerateMap(int dungeonSegmentType, int segmentId, int fragmentOrder, out List<Coord> hotSpots)
    {
        int[,] processedNoise;

        SegmentInformation seg = dungeonSegmentsInfo[segmentId];
        DungeonShape dSh = dungeon.segments[segmentId].shape;
        AccessInfo acInfo = dungeonFragments[fragmentOrder].acInfo;

        if (dungeonSegmentType == 1)       // Natural Generation
        {
            float[,] noiseMap = Noise.GeneratePerlinNoiseMap(seed, offset, dSh.noiseInfo, seg.mapWidth, seg.mapHeight);
            noiseMap = MapMerger.ApplyFalloffMask(noiseMap, seg.falloffMask);
            processedNoise = NoiseProcessor.ProcessNoiseMap(noiseMap, dSh.noiseProcessInfo, ref acInfo, true, rng, out hotSpots);
        }
        else if (dungeonSegmentType == 2)  // Artificial Generation
        {
            float[,] noiseMap = Noise.GeneratePerlinNoiseMap(seed, offset, dSh.noiseInfo, seg.mapWidth, seg.mapHeight);
            processedNoise = NoiseProcessor.ProcessNoiseMap(noiseMap, dSh.noiseProcessInfo, ref acInfo, false, rng, out hotSpots);
            hotSpots.Clear();

            processedNoise = MapMerger.ApplyArtificialCanvas(seg.canvasMask, processedNoise, false);
            processedNoise = NoiseProcessor.ProcessIntMap(processedNoise, dSh.noiseProcessInfo, ref acInfo, false, rng, out hotSpots);

            if (dSh.prioritizeImageFidelity)
            {
                processedNoise = MapMerger.ApplyArtificialCanvas(seg.canvasMask, processedNoise, false);
            }
        }
        else    // Artificial Generation (Predefined Room Only)
        {
            processedNoise = NoiseProcessor.ProcessIntMap(seg.canvasMask, dSh.noiseProcessInfo, ref acInfo, true, rng, out hotSpots);
        }

        AddOffset(acInfo.exitDirection, dSh.chunkWidth, dSh.chunkHeight, dSh.chunkSize);

        return processedNoise;
    }

    public void AddOffset(int n, int chunksWidth, int chunksHeight, int chunkSize)
    {
        if (n == 1)
        {
            offset.x += 0;
            offset.y += chunksHeight * chunkSize;
        }
        else if (n == 2)
        {
            offset.x += chunksWidth * chunkSize;
            offset.y += 0;
        }
        else if (n == 3)
        {
            offset.x += 0;
            offset.y -= chunksHeight * chunkSize;
        }
        else
        {
            offset.x -= chunksWidth * chunkSize;
            offset.y += 0;

        }
    }
}


