using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ADG.Utilities.DungeonGeneratorClases;
using ADG.Utilities.MeshConstructorClases;

namespace ADG.Tools.MeshDesigner
{
    public class MeshGenerator
    {
        static int start = 1;
        static int[,] fragmentMap;
        static System.Random rng;


        public static MapMatricesInfo GenerateSiteInformation(int[,] map, DungeonFragment fragment, DungeonSegment segment, SegmentInformation segmentInformation)
        {
            fragmentMap = map;
            rng = new System.Random();
            MapMatricesInfo mapChunksPositionMatrices = new MapMatricesInfo(fragment.position);

            for (int chunkX = 0; chunkX < fragment.chunkWidth; chunkX++)
            {
                for (int chunkY = 0; chunkY < fragment.chunkHeight; chunkY++)
                {
                    mapChunksPositionMatrices.chunks.Add(new Coord(chunkX, chunkY), CreateChunk(chunkX, chunkY, fragment.chunkSize, segment,segmentInformation));
                }
            }

            return mapChunksPositionMatrices;
        }


        static ChunkMatricesContainer CreateChunk(int chunkX, int chunkY, int chunkSize, DungeonSegment segment, SegmentInformation segmentInformation)
        {
            Vector3 worldPosition = new Vector3(chunkX * chunkSize, 0, chunkY * chunkSize);
            ChunkMatricesContainer buffer = new ChunkMatricesContainer(worldPosition);

            for (int x = 0; x < chunkSize; x++)
            {
                int indexX = (chunkX * chunkSize) + x;

                for (int y = 0; y < chunkSize; y++)
                {
                    int indexY = (chunkY * chunkSize) + y;

                    TileData temp = CreateTile(indexX, indexY, x, y, segment, segmentInformation);
                    if (temp != null) {
                        buffer.tiles.Add(new Coord(x, y), temp);
                    }
                }
            }

            return buffer;
        }


        static TileData CreateTile(int indexX, int indexY, int x, int y, DungeonSegment segment, SegmentInformation segmentInformation)
        {
            // The Type of the Tile in Question
            // 0 -> Ground
            // 1 -> Walls
            // 2 -> Structures
            // 3 -> Null or Empty or Void Tile
            // 4 -> Mask Tile (Texture Read Only) (Not Used in any of this)
            int tileType = Mathf.Clamp(fragmentMap[indexX + start, indexY + start], 0, 3);
            if (tileType == 3)
                return null;


            int tileComposition = segment.brushes[tileType].tileComposition;
            int transitionTileComposition = segment.brushes[tileType].transitionTileComposition;
            bool usePartialTransitionTiling = (transitionTileComposition == 2) ? true : false;

            

            TileData buffer = new TileData(tileType);

            // Standart Tiling with no Transition
            if (transitionTileComposition == 1) // Simple Tiling that does not use Transition Tiles
            {   //aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
                buffer.mainMeshes.Add(CreateBlockData(x, y, segment.brushes[tileType].mainTileAddedHeight, 0, -1, segmentInformation.brushesMainTiles[tileType], segment.brushes[tileType].useDegenerateMainMeshes));
                if (segment.useDecorations)
                {
                    int decoIdBuffer;
                    if (TryMainDecorationBlockData(segment, tileType, out decoIdBuffer))
                    {   //bbbbbbbbbbbbbbbbbbbbbbbbbbb
                        float rotation = (segment.decorations[tileType].mainDecorations[decoIdBuffer].useRandomRotation) ? rng.Next(360) : segment.decorations[tileType].mainDecorations[decoIdBuffer].rotation;
                        buffer.ornateMeshes.Add(CreateBlockData(x, y, segment.decorations[tileType].mainDecorations[decoIdBuffer].deltaH, rotation, (segment.decorations[tileType].mainDecorations[decoIdBuffer].parentCluster + 1) * -1 , decoIdBuffer, segmentInformation.decorationsMain[tileType][decoIdBuffer], segment.decorations[tileType].useDegenerateMainMeshes));
                    }
                }
                if (segment.useInteractables)
                {
                    int decoIdBuffer;
                    if (TryInteractableBlockData(segment, tileType, out decoIdBuffer))
                    {   //ccccccccccccccccccccccccccccc
                        //float rotation = (segment.interactables[tileType].interactables[decoIdBuffer].useRandomRotation) ? rng.Next(360) : segment.interactables[tileType].interactables[decoIdBuffer].rotation;
                        buffer.interactable = CreateBlockData(x, y, segment.interactables[tileType].interactables[decoIdBuffer].deltaH, decoIdBuffer);
                    }
                }
                return buffer;
            }

            int[] transitionTilesIDs;
            bool isTransitionTile = CompareNeighborsSimilarity(indexX, indexY, tileType, segment.brushes[tileType].ignoreVoidTiles, out transitionTilesIDs);

            // Basicaly the Transition Tiles Constructor
            // Or every tile constructor on Radial Tiling 
            if (isTransitionTile || tileComposition != 0)  // The Standar Tiling only uses Radial tiling on Transition Tiles
            {
                for (int i = 0; i < 4; i++)
                {
                    if (transitionTilesIDs[i] > 0 || !usePartialTransitionTiling)
                    {   //aaaaaaaaaaaaaaaaaaaaaaaa
                        buffer.mainMeshes.Add(CreateBlockData(x, y, segment.brushes[tileType].transitionTilesAddedHeight, i, transitionTilesIDs[i], segmentInformation.brushesTransitionTiles[tileType,transitionTilesIDs[i]], segment.brushes[tileType].useDegenerateTransitionMeshes));
                    }
                    if (transitionTilesIDs[i] > 0)
                    {
                        if (segment.useDecorations)
                        {
                            int decoIdBuffer;
                            if (TryTransitionDecorationBlockData(segment, tileType, out decoIdBuffer))
                            {   //bbbbbbbbbbbbbbbbbbbbbbbb
                                buffer.ornateMeshes.Add(CreateBlockData(x, y, segment.decorations[tileType].transitionsDecorations[decoIdBuffer].deltaH, i * -90, segment.decorations[tileType].transitionsDecorations[decoIdBuffer].parentCluster + 1, decoIdBuffer, segmentInformation.decorationsTransition[tileType][decoIdBuffer], segment.decorations[tileType].useDegenerateTransitionMeshes));
                            }
                        }
                    }
                }
                if (segment.useInteractables && !isTransitionTile)
                {
                    int decoIdBuffer;
                    if (TryInteractableBlockData(segment, tileType, out decoIdBuffer))
                    {   //ccccccccccccccccccccccccccccc
                        //float rotation = (segment.interactables[tileType].interactables[decoIdBuffer].useRandomRotation) ? rng.Next(360) : segment.interactables[tileType].interactables[decoIdBuffer].rotation;
                        buffer.interactable = CreateBlockData(x, y, segment.interactables[tileType].interactables[decoIdBuffer].deltaH, decoIdBuffer);
                    }
                }
            }

            // Only aplicable for Main Mesh if necessary on Transition Tiles
            // I.e. Partial Tiling 
            // The rest only covers Complete Tiling
            if(transitionTileComposition == 2 || (transitionTileComposition == 0 && !isTransitionTile && tileComposition != 1) || tileComposition == 2)    
            {   //aaaaaaaaaaaaaaaaaaaaaaaa
                buffer.mainMeshes.Add(CreateBlockData(x, y, segment.brushes[tileType].mainTileAddedHeight, 0, -1, segmentInformation.brushesMainTiles[tileType], segment.brushes[tileType].useDegenerateMainMeshes ));
            }
            if (segment.useDecorations && segment.decorations[tileType])
            {
                if (!isTransitionTile || (isTransitionTile && !segment.decorations[tileType].excludeOverlappingDecorations))
                {
                    int decoIdBuffer;
                    if (TryMainDecorationBlockData(segment, tileType, out decoIdBuffer))
                    {   //bbbbbbbbbbbbbbbbbbbbbbbb
                        float rotation = (segment.decorations[tileType].mainDecorations[decoIdBuffer].useRandomRotation) ? rng.Next(360) : segment.decorations[tileType].mainDecorations[decoIdBuffer].rotation;
                        buffer.ornateMeshes.Add(CreateBlockData(x, y, segment.decorations[tileType].mainDecorations[decoIdBuffer].deltaH, rotation, (segment.decorations[tileType].mainDecorations[decoIdBuffer].parentCluster + 1) * -1, decoIdBuffer, segmentInformation.decorationsMain[tileType][decoIdBuffer], segment.decorations[tileType].useDegenerateMainMeshes));
                    }
                }
            }
            if (segment.useInteractables)
            {
                int decoIdBuffer;
                if (TryInteractableBlockData(segment, tileType, out decoIdBuffer))
                {   //ccccccccccccccccccccccccccccc
                    //float rotation = (segment.interactables[tileType].interactables[decoIdBuffer].useRandomRotation) ? rng.Next(360) : segment.interactables[tileType].interactables[decoIdBuffer].rotation;
                    buffer.interactable = CreateBlockData(x, y, segment.interactables[tileType].interactables[decoIdBuffer].deltaH, decoIdBuffer);
                }
            }

            return buffer;
        }


        static bool CompareNeighborsSimilarity(int indexX, int indexY, int tileType, bool ignoreVoidTiles, out int[] IDs)
        {
            IDs = new int[4];
            bool isTransitionTile = false;

            float angleStart = 0;
            float angleStep = Mathf.PI / 4;

            for (int i = 0; i < 4; i++)
            {
                int buffer = 0;
                for (int j = 0; j < 3; j++)
                {
                    int x = Mathf.RoundToInt(Mathf.Cos(angleStart + angleStep * j)) + indexX;
                    int y = Mathf.RoundToInt(Mathf.Sin(angleStart + angleStep * j)) + indexY;

                    int testedTile = Mathf.Clamp(fragmentMap[x + start, y + start], 0, 3);
                    if (ignoreVoidTiles)
                    {
                        if (testedTile == tileType || testedTile == 3)
                        {
                            buffer += Mathf.Clamp(2 * j, 1, 4);
                        }
                    } else {
                        if (testedTile == tileType)
                        {
                            buffer += Mathf.Clamp(2 * j, 1, 4);
                        }
                    }
                }

                IDs[i] = GetLegalValue(buffer);
                if (IDs[i] > 0)
                    isTransitionTile = true;

                angleStart += Mathf.PI / 2;
            }

            return isTransitionTile;
        }


        static InteracData CreateBlockData(int x, int y, float h, int tileCompositionId)
        {
            Vector3 position = new Vector3(x, h, y);

            return new InteracData(position, tileCompositionId);
        }

        static MeshData CreateBlockData(int x, int y, float h, int r, int tileCompositionId, PrefabInformation prefab, bool useDegeneracy)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            Vector3 position = new Vector3(x, h, y);
            Quaternion rotation = Quaternion.Euler(0, r * -90, 0);
            Vector3 scale = Vector3.one;

            if (useDegeneracy)
            {
                if (tileCompositionId > -1)
                {
                    if (r == 0) {

                        position += new Vector3( prefab.position.x, prefab.position.y,  prefab.position.z);

                    } else if (r == 1) {

                        position += new Vector3(-prefab.position.z, prefab.position.y,  prefab.position.x);

                    } else if (r == 2) {

                        position += new Vector3(-prefab.position.x, prefab.position.y, -prefab.position.z);

                    } else {

                        position += new Vector3( prefab.position.z, prefab.position.y, -prefab.position.x);
                        
                    }

                } else {

                    position += prefab.position;

                }

                rotation = prefab.rotation * rotation;
                scale = prefab.scale;
            }

            matrix.SetTRS(position, rotation, scale);

            return new MeshData(matrix, tileCompositionId);
        }

        static MeshData CreateBlockData(int x, int y, float h, float r, int decoClusterId, int decoId, PrefabInformation prefab, bool useDegeneracy)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            Vector3 position = new Vector3(x, h, y);
            Quaternion rotation = Quaternion.Euler(0, r, 0);
            Vector3 scale = Vector3.one;

            if (useDegeneracy)
            {
                if(decoClusterId > 0)
                {
                    r = r / -90;
                    if (r == 0)
                    {

                        position += new Vector3( prefab.position.x, prefab.position.y,  prefab.position.z);

                    }
                    else if (r == 1)
                    {

                        position += new Vector3(-prefab.position.z, prefab.position.y,  prefab.position.x);

                    }
                    else if (r == 2)
                    {

                        position += new Vector3(-prefab.position.x, prefab.position.y, -prefab.position.z);

                    }
                    else
                    {

                        position += new Vector3( prefab.position.z, prefab.position.y, -prefab.position.x);

                    }

                } else {

                    position += prefab.position;
                }
                
                rotation = prefab.rotation * rotation;
                scale = prefab.scale;
            }

            matrix.SetTRS(position, rotation, scale);

            return new MeshData(matrix, decoClusterId, decoId);
        }

        static int GetLegalValue(int value)
        {
            if (value == 7)
                return 0;   //FillCube
            else if (value == 1 || value == 3)
                return 1;   //HCube
            else if (value == 4 || value == 6)
                return 2;   //VCube
            else if (value == 5)
                return 3;   //CornerCube
            else        
                return 4;   //RoundCube
        }

        
        static bool TryInteractableBlockData(DungeonSegment segment, int tileType, out int decoId)
        {
            decoId = 0;

            if (segment.interactables[tileType] == null)
                return false;

            if (rng.Next(101) > segment.interactables[tileType].globalProbability)
                return false;

            float buffer = rng.Next(100);
            for (int i = 0; i < segment.interactables[tileType].interactables.Count; i++)
            {
                if (buffer < segment.interactables[tileType].interactables[i].spawnProbability)
                {
                    decoId = i;
                    return true;
                }
            }

            return false;
        }


        static bool TryMainDecorationBlockData(DungeonSegment segment, int tileType, out int decoId)
        {
            decoId = 0;

            if (segment.decorations[tileType] == null)
                return false;

            if (rng.Next(101) > segment.decorations[tileType].mainProbability)
                return false;

            float buffer = rng.Next(100);
            for (int i = 0; i < segment.decorations[tileType].mainDecorations.Count; i++)
            {
                if (buffer < segment.decorations[tileType].mainDecorations[i].spawnProbability)
                {
                    decoId = i;
                    return true;
                }
            }

            return false;
        }

        static bool TryTransitionDecorationBlockData(DungeonSegment segment, int tileType, out int decoId)
        {
            decoId = 0;

            if (!segment.decorations[tileType])
                return false;

            if (!segment.decorations[tileType].useDecorationsInTransitions)
                return false;

            if (rng.Next(101) > segment.decorations[tileType].transitionsProbability)
                return false;

            float buffer = rng.Next(100);
            for (int i = 0; i < segment.decorations[tileType].transitionsDecorations.Count; i++)
            {
                if (buffer < segment.decorations[tileType].transitionsDecorations[i].spawnProbability)
                {
                    decoId = i;
                    return true;
                }
            }

            return false;
        }
    }

    
}
