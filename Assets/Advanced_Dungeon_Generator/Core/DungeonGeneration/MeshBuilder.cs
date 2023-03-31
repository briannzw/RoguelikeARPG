using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

using ADG.Utilities.DungeonGeneratorClases;
using ADG.Utilities.MeshConstructorClases;
using ADG.Tools.MeshDesigner;

public class MeshBuilder : MonoBehaviour
{
    ConcurrentQueue<ThreadInfo<Info>> mainThreadQueue = new ConcurrentQueue<ThreadInfo<Info>>();
    bool hasTheQueueAlreadyBeenRequested;

    ConcurrentQueue<ThreadInfo<InteracInfo>> interactThreadQueue = new ConcurrentQueue<ThreadInfo<InteracInfo>>();
    
    void Update()
    {
        if (mainThreadQueue.Count > 0)
        {
            hasTheQueueAlreadyBeenRequested = false;

            ThreadInfo<Info> threadInfo;
            if (mainThreadQueue.TryDequeue(out threadInfo))
            {
                threadInfo.callback(threadInfo.parameter);
            }
        }
        else if (!hasTheQueueAlreadyBeenRequested) 
        {
            DungeonGenerator.Instance.OnEmptyMeshBuilderQueue();
            hasTheQueueAlreadyBeenRequested = true;
        }

        if (interactThreadQueue.Count > 0)
        {
            ThreadInfo<InteracInfo> threadInfo;
            if (interactThreadQueue.TryDequeue(out threadInfo))
            {
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }


    public void GenerateChunk(DungeonFragment fragment, Coord start)
    {
        Dictionary<Coord, ChunkMatricesContainer> information = fragment.mapMatricesInfo.chunks;
        int[,] mapFlags = new int[fragment.chunkWidth, fragment.chunkHeight];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(start);
        mapFlags[start.tileX, start.tileY] = 1;

        ChunkMatricesContainer kvp;
        if (information.TryGetValue(start, out kvp))
        {
            AddToQueue(kvp, fragment.dungeonOrder, start, fragment.useDecorations, fragment.useInteractables);
        }

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y, fragment.chunkWidth, fragment.chunkHeight) && (y == tile.tileY || x == tile.tileX))
                    {
                        if(mapFlags[x,y] == 0)
                        {
                            mapFlags[x, y] = 1;
                            Coord current = new Coord(x, y);
                            queue.Enqueue(current);

                            ChunkMatricesContainer cmc;
                            if (information.TryGetValue(current, out cmc))
                            {
                                AddToQueue(cmc, fragment.dungeonOrder, current, fragment.useDecorations, fragment.useInteractables);
                            }
                        }
                    }
                }
            }
        }

        DungeonGenerator.Instance.AddToMainQueue(DungeonGenerator.Instance.OnDungeonFragmentBuildRequestSet);
    }


    bool IsInMapRange(int x, int y, int mapWidth, int mapHeight)
    {
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }


    void AddToQueue(ChunkMatricesContainer cmc, int order, Coord coord, bool useDecorations, bool useInteractables)
    {
        List<MeshData>[] brushesMainTileData = new List<MeshData>[3];
        List<MeshData>[] brushesTransitionTilesData = new List<MeshData>[3];
        Dictionary<int, List<MeshData>>[] decorationsMainTileData = new Dictionary<int, List<MeshData>>[3];
        Dictionary<int, List<MeshData>>[] decorationsTransitionTileData = new Dictionary<int, List<MeshData>>[3];
        List<InteracData>[] interactables = new List<InteracData>[3];


        // The Type of the Tile in Question
        // 0 -> Ground
        // 1 -> Walls
        // 2 -> Structures
        for (int i = 0; i < 3; i++) {
            brushesMainTileData[i] = new List<MeshData>();
            brushesTransitionTilesData[i] = new List<MeshData>();
            decorationsMainTileData[i] = new Dictionary<int, List<MeshData>>();
            decorationsTransitionTileData[i] = new Dictionary<int, List<MeshData>>();
            interactables[i] = new List<InteracData>();
        }


        foreach (TileData tile in cmc.tiles.Values)
        {
            foreach (MeshData meshData in tile.mainMeshes)
            {
                if (meshData.tileCompositionId < 0)
                {
                    brushesMainTileData[tile.tileType].Add(meshData);
                } else {
                    brushesTransitionTilesData[tile.tileType].Add(meshData);
                }
            }

            
            if (useDecorations && tile.ornateMeshes.Count > 0)
            {
                foreach (MeshData meshData in tile.ornateMeshes)
                {
                    if (meshData.tileCompositionId < 0)
                    {
                        int element = (meshData.tileCompositionId * -1) - 1;
                        if (!decorationsMainTileData[tile.tileType].ContainsKey(element))
                        {
                            decorationsMainTileData[tile.tileType].Add(element, new List<MeshData>());
                        }
                        decorationsMainTileData[tile.tileType][element].Add(meshData);
                    }
                    else
                    {
                        int element = (meshData.tileCompositionId) - 1;
                        if (!decorationsTransitionTileData[tile.tileType].ContainsKey(element))
                        {
                            decorationsTransitionTileData[tile.tileType].Add(element, new List<MeshData>());
                        }
                        decorationsTransitionTileData[tile.tileType][element].Add(meshData);
                    }
                }
            }
            

            if(useInteractables && tile.interactable != null) 
            {
                interactables[tile.tileType].Add(tile.interactable);
            }

        }

        for (int i = 0; i < 3; i++)
        {
            if(brushesMainTileData[i].Count > 0)
            {
                mainThreadQueue.Enqueue(new ThreadInfo<Info>(ConstructChunk, new Info(brushesMainTileData[i], cmc.position, coord, order, i, false, false)));
            }
            if(brushesTransitionTilesData[i].Count > 0)
            {
                mainThreadQueue.Enqueue(new ThreadInfo<Info>(ConstructChunk, new Info(brushesTransitionTilesData[i], cmc.position, coord, order, i, true, false)));
            }

            if (useDecorations)
            {
                foreach (KeyValuePair<int, List<MeshData>> kvp in decorationsMainTileData[i])
                {
                    if (kvp.Value.Count > 0)
                    {
                        mainThreadQueue.Enqueue(new ThreadInfo<Info>(ConstructChunk, new Info(kvp.Value, cmc.position, coord, order, i, false, true)));
                    }
                }  
                foreach (KeyValuePair<int, List<MeshData>> kvp in decorationsTransitionTileData[i])
                {
                    if (kvp.Value.Count > 0)
                    {
                        mainThreadQueue.Enqueue(new ThreadInfo<Info>(ConstructChunk, new Info(kvp.Value, cmc.position, coord, order, i, true, true)));
                    }
                }
            }

            if (useInteractables)
            {
                if (interactables[i].Count > 0)
                {
                    interactThreadQueue.Enqueue(new ThreadInfo<InteracInfo>(ConstructChunk, new InteracInfo(interactables[i], cmc.position, coord, order, i)));
                }
            }
        }
    }

    void ConstructChunk(InteracInfo info)
    {
        DungeonFragment fragment = DungeonGenerator.Instance.dungeonFragments[info.fragmentOrder];
        DungeonSegment segment = DungeonGenerator.Instance.dungeon.segments[fragment.parentSegment];


        // If there is not a container for the elements of this Chunk, one will be created 
        ChunkMatricesContainer cmc;
        if (fragment.mapMatricesInfo.chunks.TryGetValue(info.chunkCoord, out cmc))
        {
            if (cmc.chunkGameObject == null)
            {
                cmc.chunkGameObject = new GameObject("Chunk(" + info.chunkCoord.tileX + ", " + info.chunkCoord.tileY + ")");
                cmc.chunkGameObject.transform.SetParent(fragment.mainObject.transform, false);
                cmc.chunkGameObject.isStatic = true;
            }
        }


        int interactablesCount = info.data.Count;
        for (int i = 0; i < interactablesCount; i++)
        {
            GameObject interactable = Instantiate(segment.interactables[info.brushId].interactables[info.data[i].decoId].decorationObject);
            interactable.transform.position = info.data[i].position + info.position;
            /*
            if (segment.interactables[info.brushId].interactables[info.data[i].decoId].useRandomRotation)
                interactable.transform.rotation = info.data[i].rotation;
            */
            interactable.transform.SetParent(cmc.chunkGameObject.transform, false);
        }
    }

    void ConstructChunk(Info info)
    {
        DungeonFragment fragment = DungeonGenerator.Instance.dungeonFragments[info.fragmentOrder];
        DungeonSegment segment = DungeonGenerator.Instance.dungeon.segments[fragment.parentSegment];


        // If there is not a container for the elements of this Chunk, one will be created 
        ChunkMatricesContainer cmc;
        if(fragment.mapMatricesInfo.chunks.TryGetValue(info.chunkCoord, out cmc))
        {
            if(cmc.chunkGameObject == null)
            {
                cmc.chunkGameObject = new GameObject("Chunk(" + info.chunkCoord.tileX + ", " + info.chunkCoord.tileY + ")");
                cmc.chunkGameObject.transform.SetParent(fragment.mainObject.transform, false);
                cmc.chunkGameObject.isStatic = true;
            }
        }


        // Individual chunk GameObject Inizialization
        GameObject go;


        // Chunk name set
        string objectName;
        if (info.brushId == 0) {
            objectName = "Ground";
        } else if (info.brushId == 1) {
            objectName = "Wall";
        } else {
            objectName = "Structure";
        }

        // Chunk segregation and Mesh creation
        if (!info.isDecoration)
        {
            if (info.isComplexMesh) // Transition Tiles (Dungeon Brush)
            {
                objectName += " Transition";
                bool hasBlueprint = InstantiateGameObject(out go, segment.brushes[info.brushId].transitionBlueprint);
                CreateComplexMesh(info.data, segment.brushes[info.brushId], go, hasBlueprint, objectName);

            } else {                // Main Tiles (Dungeon Brush)

                bool hasBlueprint = InstantiateGameObject(out go, segment.brushes[info.brushId].mainBlueprint);
                CreateSimpleMesh(info.data, segment.brushes[info.brushId], go, hasBlueprint, objectName);
            }

        } else {                    // Both Main & Transition Decorations (Dungeon Decoration)

            objectName += " Decoration";
            if (info.isComplexMesh) 
            {
                objectName += " Transition";
            }

            CreateComplexMesh(info.data, segment.decorations[info.brushId], out go, objectName);
        }


        // Chunk arrangement
        go.transform.position = info.position;
        go.transform.SetParent(cmc.chunkGameObject.transform, false);
        go.isStatic = true;
    }


    bool InstantiateGameObject(out GameObject gameObject, GameObject blueprint)
    {
        if (blueprint == null)
        {
            gameObject = new GameObject("GO", typeof(MeshRenderer), typeof(MeshFilter));

            return false;

        } else {

            gameObject = Instantiate(blueprint);

            if (gameObject.GetComponent<MeshFilter>() == null){
                gameObject.AddComponent<MeshFilter>();
            }

            if (gameObject.GetComponent<MeshRenderer>() == null){
                gameObject.AddComponent<MeshRenderer>();
            }

            return true;
        }
    }


    void CreateComplexMesh(List<MeshData> meshesData, DungeonBrush dB, GameObject containerObject, bool hasBlueprint, string name)    
    {
        int meshCount = meshesData.Count;
        CombineInstance[] combines = new CombineInstance[meshCount];


        Mesh[] meshes = new Mesh[5];
        for (int i = 0; i < 5; i++)
        {
            meshes[i] = dB.transitionTiles[i].GetComponent<MeshFilter>().sharedMesh;
        }
        for (int i = 0; i < meshCount; i++)
        {
            combines[i].subMeshIndex = 0;
            combines[i].mesh = meshes[meshesData[i].tileCompositionId];
            combines[i].transform = meshesData[i].matrix;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combines, true, true, true);



        containerObject.name = name;
        containerObject.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        containerObject.GetComponent<MeshRenderer>().sharedMaterial = dB.mainMaterial;



        if (!hasBlueprint)
        {
            if (dB.applyMeshColliderToTransitionTiles)
            {
                containerObject.AddComponent<MeshCollider>();
                containerObject.layer = dB.transitionTilesLayer;
            }
            return;
        }

        if (containerObject.GetComponent<MeshCollider>() != null)
        {
            containerObject.GetComponent<MeshCollider>().sharedMesh = null;
            containerObject.GetComponent<MeshCollider>().sharedMesh = finalMesh;
            return;
        }
        else if (dB.applyMeshColliderToTransitionTiles)
        {
            containerObject.AddComponent<MeshCollider>();
            containerObject.layer = dB.transitionTilesLayer;
            return;
        }
    }

    /* Original
    void CreateComplexMesh(List<MeshData> meshesData, DungeonDecoration dD, out GameObject containerObject, string name)
    {
        int meshCount = meshesData.Count;
        CombineInstance[] combines = new CombineInstance[meshCount];


        Mesh[] meshes;
        if (meshesData[0].tileCompositionId < 0)
        {
            meshes = new Mesh[dD.mainDecorations.Count];
            for(int i = 0; i < dD.mainDecorations.Count; i++)
            {
                meshes[i] = dD.mainDecorations[i].decorationObject.GetComponent<MeshFilter>().sharedMesh;
            }
        } else {
            meshes = new Mesh[dD.transitionsDecorations.Count];
            for (int i = 0; i < dD.transitionsDecorations.Count; i++)
            {
                meshes[i] = dD.transitionsDecorations[i].decorationObject.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        for (int i = 0; i < meshCount; i++)
        {
            combines[i].subMeshIndex = 0;
            combines[i].mesh = meshes[meshesData[i].decoId];
            combines[i].transform = meshesData[i].matrix;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combines, true, true, true);



        bool hasBlueprint;
        if (meshesData[0].tileCompositionId < 0)    // Decorations Segregation (Main & Transicion) for Blueprint Instantiation
        {
            hasBlueprint = InstantiateGameObject(out containerObject, dD.mainClusters[(meshesData[0].tileCompositionId * -1) - 1].blueprint);
        } else {
            hasBlueprint = InstantiateGameObject(out containerObject, dD.transitionsClusters[meshesData[0].tileCompositionId - 1].blueprint);
        }



        containerObject.name = name;
        containerObject.GetComponent<MeshFilter>().sharedMesh = finalMesh;

        if (meshesData[0].tileCompositionId < 0)    // Decorations Segregation (Main & Transicion) for Material Assignation
        {
            containerObject.GetComponent<MeshRenderer>().sharedMaterial = dD.mainClusters[(meshesData[0].tileCompositionId * -1) - 1].material;
        } else {
            containerObject.GetComponent<MeshRenderer>().sharedMaterial = dD.transitionsClusters[meshesData[0].tileCompositionId - 1].material;
        }



        if (hasBlueprint)
        {
            if (containerObject.GetComponent<MeshCollider>() != null)
            {
                containerObject.GetComponent<MeshCollider>().sharedMesh = null;
                containerObject.GetComponent<MeshCollider>().sharedMesh = finalMesh;
                return;
            }
        }
    }
    */

    void CreateComplexMesh(List<MeshData> meshesData, DungeonDecoration dD, out GameObject containerObject, string name)
    {
        MeshFilter[] meshes;
        List<Material> materials = new List<Material>();
        if (meshesData[0].tileCompositionId < 0)
        {
            meshes = new MeshFilter[dD.mainDecorations.Count];
            for (int i = 0; i < dD.mainDecorations.Count; i++)
            {
                meshes[i] = dD.mainDecorations[i].decorationObject.GetComponent<MeshFilter>();
                Material[] mats = meshes[i].GetComponent<MeshRenderer>().sharedMaterials;
                foreach(Material mat in mats)
                {
                    if (!materials.Contains(mat)) materials.Add(mat);
                }
            }
        }
        else
        {
            meshes = new MeshFilter[dD.transitionsDecorations.Count];
            for (int i = 0; i < dD.transitionsDecorations.Count; i++)
            {
                meshes[i] = dD.transitionsDecorations[i].decorationObject.GetComponent<MeshFilter>();
                Material[] mats = meshes[i].GetComponent<MeshRenderer>().sharedMaterials;
                foreach (Material mat in mats)
                {
                    if (!materials.Contains(mat)) materials.Add(mat);
                }
            }
        }

        // Handle Materials
        List<Mesh> submeshes = new List<Mesh>();
        foreach (Material material in materials)
        {
            List<CombineInstance> combiners = new List<CombineInstance>();
            for (int i = 0; i < meshesData.Count; i++)
            {
                MeshFilter filter = meshes[meshesData[i].decoId];
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null) continue;

                Material[] localMaterials = renderer.sharedMaterials;
                for(int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
                {
                    if (localMaterials[materialIndex] != material) continue;
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = filter.sharedMesh;
                    ci.subMeshIndex = materialIndex;
                    ci.transform = meshesData[i].matrix;
                    combiners.Add(ci);
                }
            }
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combiners.ToArray(), true);
            submeshes.Add(mesh);
        }

        int meshCount = submeshes.Count;
        CombineInstance[] combines = new CombineInstance[meshCount];

        for (int i = 0; i < meshCount; i++)
        {
            combines[i].subMeshIndex = 0;
            combines[i].mesh = submeshes[i];
            combines[i].transform = Matrix4x4.identity;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combines, false, true, true);



        bool hasBlueprint;
        if (meshesData[0].tileCompositionId < 0)    // Decorations Segregation (Main & Transicion) for Blueprint Instantiation
        {
            hasBlueprint = InstantiateGameObject(out containerObject, dD.mainClusters[(meshesData[0].tileCompositionId * -1) - 1].blueprint);
        }
        else
        {
            hasBlueprint = InstantiateGameObject(out containerObject, dD.transitionsClusters[meshesData[0].tileCompositionId - 1].blueprint);
        }



        containerObject.name = name;
        containerObject.GetComponent<MeshFilter>().sharedMesh = finalMesh;

        if (meshesData[0].tileCompositionId < 0)    // Decorations Segregation (Main & Transicion) for Material Assignation
        {
            containerObject.GetComponent<MeshRenderer>().sharedMaterial = dD.mainClusters[(meshesData[0].tileCompositionId * -1) - 1].material;
        }
        else
        {
            containerObject.GetComponent<MeshRenderer>().sharedMaterial = dD.transitionsClusters[meshesData[0].tileCompositionId - 1].material;
        }



        if (hasBlueprint)
        {
            if (containerObject.GetComponent<MeshCollider>() != null)
            {
                containerObject.GetComponent<MeshCollider>().sharedMesh = null;
                containerObject.GetComponent<MeshCollider>().sharedMesh = finalMesh;
                return;
            }
        }
    }


    void CreateSimpleMesh(List<MeshData> meshesData, DungeonBrush dB, GameObject containerObject, bool hasBlueprint, string name)
    {
        int meshCount = meshesData.Count;
        CombineInstance[] combines = new CombineInstance[meshCount];
        Mesh mesh = dB.mainTile.GetComponent<MeshFilter>().sharedMesh;

        for (int i = 0; i < meshCount; i++)
        {
            combines[i].subMeshIndex = 0;
            combines[i].mesh = mesh;
            combines[i].transform = meshesData[i].matrix;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combines, true, true, true);



        containerObject.name = name;
        containerObject.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        if (dB.useCombinedAtlas)
            containerObject.GetComponent<MeshRenderer>().sharedMaterial = dB.mainMaterial;
        else
            containerObject.GetComponent<MeshRenderer>().sharedMaterial = dB.extraMaterial;



        if (!hasBlueprint)
        {
            if (dB.applyMeshColliderToMainTile)
            {
                containerObject.AddComponent<MeshCollider>();
                containerObject.layer = dB.mainTileLayer;
            }
            return;
        }

        if (containerObject.GetComponent<MeshCollider>() != null)
        {
            containerObject.GetComponent<MeshCollider>().sharedMesh = null;
            containerObject.GetComponent<MeshCollider>().sharedMesh = finalMesh;
            return;
        }
        else if (dB.applyMeshColliderToMainTile)
        {
            containerObject.AddComponent<MeshCollider>();
            containerObject.layer = dB.mainTileLayer;
            return;
        }
    }
}

public struct Coord
{
    public int tileX;
    public int tileY;

    public Coord(int x, int y)
    {
        tileX = x;
        tileY = y;
    }
}

public struct InteracInfo
{
    public List<InteracData> data;

    public Vector3 position;
    public Coord chunkCoord;

    public int fragmentOrder;
    public int brushId;

    public InteracInfo(List<InteracData> data, Vector3 position, Coord chunkCoord, int fragmentOrder, int brushId)
    {
        this.data = data;
        this.position = position;
        this.chunkCoord = chunkCoord;

        this.fragmentOrder = fragmentOrder;
        this.brushId = brushId;
    }
}

public struct Info
{
    public List<MeshData> data;
    public Vector3 position;
    public Coord chunkCoord;

    public int fragmentOrder;    
    public int brushId;

    public bool isComplexMesh;
    public bool isDecoration;

    public Info(List<MeshData> data, Vector3 position, Coord chunkCoord, int fragmentOrder, int brushId, bool isComplexMesh, bool isDecoration)
    {
        this.data = data;
        this.position = position;
        this.chunkCoord = chunkCoord;

        this.fragmentOrder = fragmentOrder;
        this.brushId = brushId;

        this.isComplexMesh = isComplexMesh;
        this.isDecoration = isDecoration;
    }
}

struct ThreadInfo<T>
{
    public readonly Action<T> callback;
    public readonly T parameter;

    public ThreadInfo(Action<T> callback, T parameter)
    {
        this.parameter = parameter;
        this.callback = callback;
    }
}

