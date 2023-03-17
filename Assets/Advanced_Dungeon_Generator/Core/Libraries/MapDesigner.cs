using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ADG.Utilities.DungeonGeneratorClases;
using ADG.Utilities.DungeonShapeClases;

namespace ADG.Tools.MapDesigner
{
    // Noise is a Class dedicated to Create Base Random Noise Maps
    // The Methods of the Noise Class will usually return a Float 2D Array
    public class Noise
    {
        public static float[,] GeneratePerlinNoiseMap(int seed, Vector2 offset, NoiseInfo nI, int mapWidth, int mapHeight)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            if (nI.noiseScale <= 0)
            {
                nI.noiseScale = 0.0001f;
            }

            System.Random rng = new System.Random(seed);
            Vector2[] octaveOffset = new Vector2[nI.octaves];
            for (int i = 0; i < nI.octaves; i++)
            {
                float offsetX = rng.Next(-10000, 10000) + offset.x;
                float offsetY = rng.Next(-10000, 10000) + offset.y;
                octaveOffset[i] = new Vector2(offsetX, offsetY);
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = mapWidth / 2;
            float halfHeight = mapHeight / 2;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < nI.octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / nI.noiseScale * frequency + octaveOffset[i].x;
                        float sampleY = (y - halfHeight) / nI.noiseScale * frequency + octaveOffset[i].y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= nI.persistance;
                        frequency *= nI.lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }

        public static float[,] GenerateFalloffMap(int width, int height, float a, float b)
        {
            float[,] map = new float[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float x = (i / (float)width) * 2 - 1;
                    float y = (j / (float)height) * 2 - 1;

                    float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    map[i, j] = Evaluate(value, a, b);
                }
            }

            return map;
        }

        static float Evaluate(float value, float a, float b)
        {
            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow((b - b * value), a));
        }
    }


    // NoiseProcessor is a Class dedicated to Transform the Base Randon Noise Maps to actual Dungeon Maps
    // The Methods of the NoiseProcessor Class will usually return an Int 2D Array
    public class NoiseProcessor
    {
        static int[,] map;
        static int mapWidth;
        static int mapHeight;

        public static int[,] ProcessNoiseMap(float[,] noise, NoiseProcessInfo nPI, ref AccessInfo acInfo, bool useAdvanceProcessing, System.Random rng, out List<Coord> hotSpots)
        {
            hotSpots = new List<Coord>();

            int width = noise.GetLength(0);
            int height = noise.GetLength(1);

            int[,] noiseEvaluated = new int[width, height];

            // The ProcessNoise Method will always transform the Gradient Noise values to Concrete Tile values.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (noise[x, y] <= nPI.minFloorValue)
                    {
                        noiseEvaluated[x, y] = 1;        //Number given to Wall Tiles (Set as Black in the Visualizer)
                    }
                    else
                    {
                        if (noise[x, y] <= nPI.minStructureValue)
                            noiseEvaluated[x, y] = 0;    //Number given to Ground Tiles, specifically, path tiles (Set as Grey in the Visualizer) 
                        else
                            noiseEvaluated[x, y] = 2;    //Number given to Structure Tiles (Set as Blue in the Visualizer)  
                    }
                }
            }

            // If requested, the new Tile Map will also have Advanced Processing applied to it.
            // The Advanced processing of the Tile Map will:
            // -> Define an Validate the allowed Tile clusters
            // -> Connect all the Ground Tile clusters
            // -> Define the access that the Map has with the other Dungeon Maps
            if (useAdvanceProcessing)
            {
                mapWidth = width;
                mapHeight = height;
                map = noiseEvaluated;

                List<Room> rooms = ProcessRooms(nPI.minTilesToGenWalls, nPI.minTilesToGenRooms, nPI.minTilesToGenStructures, nPI.failedRoomValue);
                if (rooms.Count == 0) {
                    Debug.LogWarning("It is Impossible to make a Proper Room with the given Specifications");
                    return new int[width, height];
                }

                List<Coord> paths = CreatePaths(rooms);
                JoinRooms(rooms, paths, nPI.maxHallwayRadius, acInfo, rng);


                foreach(Room room in rooms)
                {
                    hotSpots.Add(room.center);
                }
                return map;
            }
            else
            {
                return noiseEvaluated;
            }
        }

        public static int[,] ProcessIntMap(int[,] noise, NoiseProcessInfo nPI, ref AccessInfo acInfo, bool itsAPredefinedRoom, System.Random rng, out List<Coord> hotSpots)
        {
            hotSpots = new List<Coord>();

            int width = noise.GetLength(0);
            int height = noise.GetLength(1);

            mapWidth = width;
            mapHeight = height;
            map = new int[mapWidth, mapHeight];

            // The ProcessIntMap Method works pretty much the same as ProcessNoiseMap Method
            //   The only difference is that instead of working with Gradient Noise values,
            //   it works with Tile values that have already been set. 
            // The ProcessIntMap Method is designed to work with Artificial Dungeon Shapes. 
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    map[i, j] = noise[i, j];
                }
            }

            // In case the Dungeon Shape is a predefined room, only accesses to the Map itself will be created
            if (!itsAPredefinedRoom)
            {
                List<Room> rooms = ProcessRooms(nPI.minTilesToGenWalls, nPI.minTilesToGenRooms, nPI.minTilesToGenStructures, nPI.failedRoomValue);
                if (rooms.Count == 0) {
                    Debug.LogWarning("It is impossible to make a proper room with the given specifications");
                    return new int[width, height];
                }

                List<Coord> paths = CreatePaths(rooms);
                JoinRooms(rooms, paths, nPI.maxHallwayRadius, acInfo, rng);

                foreach (Room room in rooms)
                {
                    hotSpots.Add(room.center);
                }
                return map;
            }
            else
            {
                CreateAccesses(new List<Coord>(), new List<Coord>(), 1, acInfo, rng);
            }

            return map;
        }

        public static int[,] GenerateMapFromTexture(Texture2D canvas, ref Vector3 position)
        {
            Color32 bufferColor;
            Color32[] pixelsColors = canvas.GetPixels32();

            int mapWidth  = canvas.width  + 2;   //Map Legal values
            int mapHeight = canvas.height + 2;

            int[,] intMap = new int[mapWidth, mapHeight];
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    intMap[x, y] = 1;
                }
            }

            // The GenerateMapFromTexture Method will convert a given Texture to a Concrete Tile Map by the following color values
            for (int y = 1; y < mapHeight - 1; y++)
            {
                for (int x = 1; x < mapWidth - 1; x++)
                {
                    bufferColor = pixelsColors[(x - 1) + (y - 1) * (mapWidth - 2)];
                    if (bufferColor.a == 0)
                    {
                        intMap[x, y] = 3;    // Empty Tiles or Void Tiles (Only set by an Alpha of zero)
                    }
                    else if (bufferColor == Color.green)
                    {
                        intMap[x, y] = 4;    // Tile to Overwrite with Noise Map Generation
                    }
                    else if (bufferColor == Color.black)
                    {
                        intMap[x, y] = 1;    // Wall
                    }
                    else if (bufferColor == Color.white)
                    {
                        intMap[x, y] = 0;    // Ground
                    }
                    else if (bufferColor == Color.blue)
                    {
                        intMap[x, y] = 2;    // Structures
                    }
                    else if (bufferColor == Color.red)
                    {
                        intMap[x, y] = -1;
                        position = new Vector3(x, 0, y);  // Spawn (Not implemented)
                    }
                }
            }

            return intMap;
        }

        static void DrawCircle(Coord c, int r, int filling)
        {
            // The DrawCircle Method will transform all tiles that are as close to the radius r of the Coordinate c into the given filler

            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        int realX = c.tileX + x;
                        int realY = c.tileY + y;
                        if (IsInMapRange(realX, realY))
                        {
                            map[realX, realY] = filling;
                        }
                    }
                }
            }
        }

        static List<Coord> GetLine(Coord from, Coord to)
        {
            // The GetLine Method will return all the Tiles that lie between the given Coordinates

            List<Coord> line = new List<Coord>();

            int x = from.tileX;
            int y = from.tileY;

            int dx = to.tileX - from.tileX;
            int dy = to.tileY - from.tileY;

            bool inverted = false;
            int step = (int)Mathf.Sign(dx);
            int gradientStep = (int)Mathf.Sign(dy);

            int longest = Mathf.Abs(dx);
            int shortest = Mathf.Abs(dy);

            if (longest < shortest)
            {
                inverted = true;
                longest = Mathf.Abs(dy);
                shortest = Mathf.Abs(dx);

                step = (int)Mathf.Sign(dy);
                gradientStep = (int)Mathf.Sign(dx);
            }

            int gradientAcumulation = longest / 2;
            for (int i = 0; i < longest; i++)
            {
                line.Add(new Coord(x, y));

                if (inverted) {
                    y += step;
                } else {
                    x += step;
                }

                gradientAcumulation += shortest;
                if (gradientAcumulation >= longest) 
                {
                    if (inverted) {
                        x += gradientStep;
                    } else {
                        y += gradientStep;
                    }

                    gradientAcumulation -= longest;
                }
            }

            return line;
        }

        static void CreateMapEntry(int orientation, out Coord entrance, out Coord exit, System.Random rng, bool needVerification, Coord A, Coord B)
        {
            // The CreateMapEntry Method will try to find the closest Ground Tiles to a specific Side of the Map.
            // The CreateMapEntry Method will also try to avoid searching for Tiles in a Certain area, if requested to do so
            // The CreateMapEntry Method will return one of these entries to the Map

            int startX;  int endX;  int incX;
            int startY;  int endY;  int incY;

            List<Coord> entries = new List<Coord>();
            List<Coord> outputs = new List<Coord>();


            if (orientation == 1)   // From Top to Bottom 
            {
                startX = 0;              endX = mapWidth;   incX = 1;
                startY = mapHeight - 1;  endY = 0;          incY = -1;

                for (int y = startY; y > endY; y += incY)
                {
                    for (int x = startX; x < endX; x += incX)
                    {
                        if (needVerification)
                        {
                            if (x > A.tileX && x < B.tileX && y > A.tileY && y < B.tileY)
                                continue;
                        }

                        if (map[x, y] < 1)
                        {
                            endY = y;
                            entries.Add(new Coord(x, y));
                            outputs.Add(new Coord(x, startY));
                        }
                    }
                }
            }
            else if (orientation == 2)  // From Right to Left
            {
                startX = mapWidth - 1;   endX = 0;          incX = -1;
                startY = mapHeight - 1;  endY = 0;          incY = -1;

                for (int x = startX; x > endX; x += incX)
                {
                    for (int y = startY; y > endY; y += incY)
                    {
                        if (needVerification)
                        {
                            if (x > A.tileX && x < B.tileX && y > A.tileY && y < B.tileY)
                                continue;
                        }

                        if (map[x, y] < 1)
                        {
                            endX = x;
                            entries.Add(new Coord(x, y));
                            outputs.Add(new Coord(startX, y));
                        }
                    }
                }
            }
            else if (orientation == 3)  // From Bottom to Top 
            {
                startX = 0;              endX = mapWidth;   incX = 1;
                startY = 0;              endY = mapHeight;  incY = 1;

                for (int y = startY; y < endY; y += incY)
                {
                    for (int x = startX; x < endX; x += incX)
                    {
                        if (needVerification)
                        {
                            if (x > A.tileX && x < B.tileX && y > A.tileY && y < B.tileY)
                                continue;
                        }

                        if (map[x, y] < 1)
                        {
                            endY = y;
                            entries.Add(new Coord(x, y));
                            outputs.Add(new Coord(x, startY));
                        }
                    }
                }
            }
            else    //From Left to Right 
            {
                startX = 0;              endX = mapWidth; incX = 1;
                startY = mapHeight - 1;  endY = 0; incY = -1;

                for (int x = startX; x < endX; x += incX)
                {
                    for (int y = startY; y > endY; y += incY)
                    {
                        if (needVerification)
                        {
                            if (x > A.tileX && x < B.tileX && y > A.tileY && y < B.tileY)
                                continue;
                        }

                        if (map[x, y] < 1)
                        {
                            endX = x;
                            entries.Add(new Coord(x, y));
                            outputs.Add(new Coord(startX, y));
                        }
                    }
                }
            }

            int n = rng.Next(0, entries.Count);
            entrance = entries[n];
            exit = outputs[n];
        }

        static void CreateAccesses(List<Coord> start, List<Coord> end, int maxHallwayRadius, AccessInfo aI, System.Random rng)
        {
            // The CreateAccess Method will create a line of ground tiles between each pair of given Coordinates
            // The CreateAccess Method will also create accesses for the Map itself

            int count = 0;
            Coord entranceStart = new Coord();   Coord entranceEnd = new Coord();
            Coord exitStart     = new Coord();   Coord exitEnd     = new Coord();

            Coord maskStart = new Coord();
            Coord maskEnd   = new Coord();
            bool needVerification = false;

            int sideDivider = 3;

            if (aI.hasEntrance && aI.normalAccessGen)
            {
                CreateMapEntry(aI.entranceDirection, out entranceStart, out entranceEnd, rng, needVerification, maskStart, maskEnd);
                start.Add(entranceStart); end.Add(entranceEnd);
                aI.entranceEnd = new Vector2(entranceStart.tileX, entranceStart.tileY);
                count++;

                // Code to prevent Overlaping of the Map Entrance and Exit in the same Corner 
                int chuckSizeX = (mapWidth - 2) / sideDivider;
                int chuckSizeY = (mapHeight - 2) / sideDivider;
                int chunkX = entranceStart.tileX / chuckSizeX;
                int chunkY = entranceStart.tileY / chuckSizeY;

                maskStart = new Coord(chunkX * chuckSizeX, chunkY * chuckSizeY);
                maskEnd   = new Coord(((chunkX + 1) * chuckSizeX) + 2, ((chunkY + 1) * chuckSizeY) + 2);

                if (chunkX == 0 || chunkX == sideDivider - 1 || chunkY == 0 || chunkY == sideDivider - 1)
                    needVerification = true;
            }

            if (aI.hasExit && aI.normalAccessGen)
            {
                CreateMapEntry(aI.exitDirection, out exitStart, out exitEnd, rng, needVerification, maskStart, maskEnd);
                start.Add(exitStart); end.Add(exitEnd);
                aI.exitStart = new Vector2(exitStart.tileX, exitStart.tileY);
                count++;
            }


            int size = start.Count;
            for (int j = 0; j < size; j++)
            {
                List<Coord> line = GetLine(start[j], end[j]);
                foreach (Coord coord in line)
                {
                    if (j < size - count)
                    {
                        int radio = rng.Next(1, maxHallwayRadius + 1);
                        DrawCircle(coord, radio, 0);
                    } else {
                        map[coord.tileX, coord.tileY] = 0;     // The Map Entries and Exits are Concrete
                    }
                }
            }
        }

        static void JoinRooms(List<Room> rooms, List<Coord> paths, int maxHallwayRadius, AccessInfo aI, System.Random rng)
        {
            // The JoinRooms Method will take the paths already created by the CreatePaths Method
            // and will try to link those rooms by the shortest path between their "Key Points"

            List<Coord> start = new List<Coord>();
            List<Coord> end = new List<Coord>();

            foreach (Coord coord in paths)
            {
                int minDistance = int.MaxValue;

                Coord bufferA = new Coord(0, 0);
                Coord bufferB = new Coord(0, 0);
                foreach (Coord coordA in rooms[coord.tileX].keyPoints)
                {
                    foreach (Coord coordB in rooms[coord.tileY].keyPoints)
                    {
                        int distance = (coordB.tileX - coordA.tileX) * (coordB.tileX - coordA.tileX) + (coordB.tileY - coordA.tileY) * (coordB.tileY - coordA.tileY);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            bufferA.tileX = coordA.tileX;
                            bufferA.tileY = coordA.tileY;
                            bufferB.tileX = coordB.tileX;
                            bufferB.tileY = coordB.tileY;
                        }
                    }
                }

                start.Add(bufferA);
                end.Add(bufferB);
            }

            CreateAccesses(start, end, maxHallwayRadius, aI, rng);
        }

        static List<Coord> CreatePaths(List<Room> rooms)
        {
            // The CreatePaths Method will try to join the rooms together, always using the shortest path between the "Centers" of the rooms
            //   The CreatePaths Method is sequential, it starts with one room and joins it with the nearest one,
            //   then takes that room, joins it with the nearest room that is still isolated and repeats
            // The CreatePaths Method then returns the sets of the rooms to be joined in the form of Coordinates

            int numberOfRooms = rooms.Count;
            int[] thisRoomIsIsolated = new int[numberOfRooms];
            for (int i = 0; i < numberOfRooms; i++)
                thisRoomIsIsolated[i] = 1;
            
            int pathsNeeded = numberOfRooms - 1;
            List<Coord> paths = new List<Coord>();

            bool endSearchIsDone = false;
            int currentRoom = 0;
            thisRoomIsIsolated[0] = 0;

            for (int i = 0; i < pathsNeeded; i++)
            {
                int minDistance = int.MaxValue;
                Coord pathBuffer = new Coord(0, 0);

                for (int j = 0; j < numberOfRooms; j++)
                {
                    // To avoid strange paths, the last room is joined to the nearest room, instead of the next room in the sequence
                    if (i != pathsNeeded - 1) {
                        if (thisRoomIsIsolated[j] == 0)
                            continue;
                    } else {
                        if (!endSearchIsDone) {
                            for (int k = 0; k < numberOfRooms; k++)
                            {
                                if (thisRoomIsIsolated[k] == 1)
                                {
                                    currentRoom = k;
                                    endSearchIsDone = true;
                                }
                            }
                        }
                        if (currentRoom == j)
                            continue;
                    }

                    int xA = rooms[currentRoom].center.tileX;
                    int yA = rooms[currentRoom].center.tileY;
                    int xB = rooms[j].center.tileX;
                    int yB = rooms[j].center.tileY;

                    int distance = (xB - xA) * (xB - xA) + (yB - yA) * (yB - yA);
                    if (distance < minDistance)
                    {
                        minDistance = distance;

                        pathBuffer.tileX = currentRoom;
                        pathBuffer.tileY = j;
                    }
                }

                thisRoomIsIsolated[pathBuffer.tileY] = 0;
                paths.Add(pathBuffer);
                currentRoom = pathBuffer.tileY;
            }

            return paths;
        }

        static List<Room> ProcessRooms(int wallMinSize, int roomMinSize, int structMinSize, int failedHabValue)
        {
            // The GetRegions Method will return a List of all the clusters from a specific Tile; i.e., a list of "Regions" 
            // These clusters are represented as a List of Coordinates
            // If one of these clusters does not meet the Requirements to be valid, its Map values will be replaced.

            List<List<Coord>> wallRegions = GetRegions(1);
            foreach (List<Coord> region in wallRegions)
            {
                if (region.Count < wallMinSize)
                {
                    foreach (Coord cor in region)
                    {
                        map[cor.tileX, cor.tileY] = 0;
                    }
                }
            }

            List<List<Coord>> structRegions = GetRegions(2);
            foreach (List<Coord> region in structRegions)
            {
                if (region.Count < structMinSize)
                {
                    foreach (Coord cor in region)
                    {
                        map[cor.tileX, cor.tileY] = 0;
                    }
                }
            }

            // Each tile has a value assigned to it on the map
            // The value of a Ground Tile is 0
            // However, any negative value is also considered as a Ground Tile.
            // That's because each cluster of Ground Tiles, is also considered as a mini-room, with its own identifier. 
            // The paths linking these mini-rooms will have a value of 0.
            List<List<Coord>> roomRegions = GetRegions(0);
            List<Room> survivingRooms = new List<Room>();
            int roomId = -1;

            foreach (List<Coord> region in roomRegions)
            {
                if (region.Count < roomMinSize)
                {
                    int buffer;
                    if(failedHabValue < 3)
                    {
                        buffer = failedHabValue;
                    } else {
                        buffer = map[region[0].tileX - 1, region[0].tileY];
                    }

                    foreach (Coord cor in region)
                    {
                        map[cor.tileX, cor.tileY] = buffer;
                    }
                }
                else
                {
                    // Each valid Room has to be connected to the others in a whole.
                    //   One method would be to separate all the tiles that are at the edge of the rooms and try to
                    //   compare them with those of all the other rooms to find the shortest path between them. 
                    // However, this method will try to find SOME key points in the room and will only compare those points. 
                    int roomSize = region.Count;
                    Coord approxRoomCenter = new Coord(0, 0);

                    // First we try to find the "center" of the room
                    foreach (Coord cor in region)
                    {
                        approxRoomCenter.tileX += cor.tileX;
                        approxRoomCenter.tileY += cor.tileY;

                        map[cor.tileX, cor.tileY] = roomId;
                    }
                    approxRoomCenter.tileX = approxRoomCenter.tileX / roomSize;
                    approxRoomCenter.tileY = approxRoomCenter.tileY / roomSize;

                    // Then we make a High Approximation of how far away we can start to evaluate tiles from the room.
                    int roomLengthApprox = (int)Mathf.Sqrt(roomSize);
                    List<Coord> edges = new List<Coord>();

                    // We decide how many samples of these "Key Points" we want to take.
                    int samples = 8;

                    // And then we evaluate, starting from as far away as possible and approaching the center.
                    // This is to take into account any odd shapes the rooms may take. 
                    for (int i = 0; i < samples; i++)
                    {
                        float angle = (2 * Mathf.PI / samples) * i;
                        float x = Mathf.Cos(angle);
                        float y = Mathf.Sin(angle);
                        float m = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                        for (int j = roomLengthApprox; j > 0; j--)
                        {
                            float tileX = (j * x) / m;
                            float tileY = (j * y) / m;

                            int testTileX = approxRoomCenter.tileX + (int)tileX;
                            int testTileY = approxRoomCenter.tileY + (int)tileY;
                            if (IsInMapRange(testTileX, testTileY))
                            {
                                if (map[testTileX, testTileY] == roomId)
                                {
                                    edges.Add(new Coord(testTileX, testTileY));
                                    continue;
                                }
                            }
                        }
                    }

                    survivingRooms.Add(new Room(region, roomId, roomSize, approxRoomCenter, edges));
                    roomId--;
                }
            }

            return survivingRooms;
        }

        static List<List<Coord>> GetRegions(int tileType)
        {
            List<List<Coord>> regions = new List<List<Coord>>();
            int[,] mapFlags = new int[mapWidth, mapHeight];

            // The GetRegions Method works with the help of a new 2D Array called MapFlags
            // Each time a (x, y) Coordinate does not have a flag AND matches the given Tile, the GetRegionTiles Method will be called
            // The GetRegionsTiles Method will return a cluster of all the SameTileType Coordinates that are next to that (x, y) Coordinate and next to each other
            // Then each Coordinate that belongs to that cluster will be flagged 

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                    {
                        List<Coord> newRegion = GetRegionTiles(x, y);
                        regions.Add(newRegion);

                        foreach (Coord cor in newRegion)
                        {
                            mapFlags[cor.tileX, cor.tileY] = 1;
                        }
                    }
                }
            }

            return regions;
        }

        static List<Coord> GetRegionTiles(int startX, int startY)
        {
            List<Coord> tiles = new List<Coord>();
            int[,] mapFlags = new int[mapWidth, mapHeight];
            int tileType = map[startX, startY];

            Queue<Coord> queue = new Queue<Coord>();
            queue.Enqueue(new Coord(startX, startY));
            mapFlags[startX, startY] = 1;

            // The GetRegionTiles Method will return a List of all the SameTileType Coordinates that are next to each other, i.e., a cluster, i.e., a "Region"
            // The GetRegionTiles Method tries to find Tiles of the same type that are:
            // -> Not the same as others before (That's why there is a flag map)
            // -> Inside the Map Range 
            // -> Next to each other (not diagonally). 
            // And stops trying when it can't find any more.
            while (queue.Count > 0)
            {
                Coord tile = queue.Dequeue();
                tiles.Add(tile);

                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (IsInMapRange(x, y) && (x == tile.tileX || y == tile.tileY))
                        {
                            if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                            {
                                mapFlags[x, y] = 1;
                                queue.Enqueue(new Coord(x, y));
                            }
                        }
                    }
                }
            }

            return tiles;
        }

        static bool IsInMapRange(int x, int y)
        {
            return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
        }

        struct Room
        {
            public int id;
            public List<Coord> tiles;
            public int size;
            public Coord center;
            public List<Coord> keyPoints;

            public Room(List<Coord> roomTiles, int ID, int roomSize, Coord center, List<Coord> keyPoints)
            {
                id = ID;
                tiles = roomTiles;
                size = roomSize;
                this.center = center;
                this.keyPoints = keyPoints;
            }
        }

    }


    // MapMerger is a Class dedicated to combine several maps into one.
    // The Methods of the MapMerger Class will usually return an Int 2D Array
    public class MapMerger
    {
        public static int[,] ApplyMargenMap(int[,] noiseMap, DungeonFragment thisSite, DungeonFragment nextSite, DungeonFragment prevSite)
        {
            int start = 1;

            int thisSiteX = (thisSite.chunkWidth + 2) * thisSite.chunkSize;
            int thisSiteY = (thisSite.chunkHeight + 2) * thisSite.chunkSize;
            int[,] map = new int[thisSiteX + 2, thisSiteY + 2];

            //Room Border Creation
            for (int chunckX = 0; chunckX < thisSite.chunkWidth + 2; chunckX++)
            {
                for (int chunckY = 0; chunckY < thisSite.chunkHeight + 2; chunckY++)
                {
                    if (chunckX == 0 || chunckX == thisSite.chunkWidth + 1 || chunckY == 0 || chunckY == thisSite.chunkHeight + 1)
                    {
                        int startX = (chunckX == 0) ? thisSite.chunkSize - thisSite.borderSize : 0;
                        int startY = (chunckY == 0) ? thisSite.chunkSize - thisSite.borderSize : 0;
                        int endX = (chunckX == thisSite.chunkWidth + 1) ? thisSite.borderSize : thisSite.chunkSize;
                        int endY = (chunckY == thisSite.chunkHeight + 1) ? thisSite.borderSize : thisSite.chunkSize;

                        int dX = chunckX * thisSite.chunkSize;
                        for (int x = 0; x < thisSite.chunkSize; x++)
                        {
                            int dY = chunckY * thisSite.chunkSize;
                            for (int y = 0; y < thisSite.chunkSize; y++)
                            {
                                if (x >= startX && x < endX && y >= startY && y < endY)
                                {
                                    map[start + dX + x, start + dY + y] = 1;    // Wall Tile
                                }
                                else
                                {
                                    map[start + dX + x, start + dY + y] = 3;    // Empty Tile, Void Tile
                                }
                            }
                        }
                    }
                }
            }

            // int thisSiteSizeX = thisSite.chunkWidth * thisSite.chunkSize;
            // int thisSiteSizeY = thisSite.chunkHeight * thisSite.chunkSize;

            int nextSiteSizeX = nextSite.chunkWidth * nextSite.chunkSize;
            int nextSiteSizeY = nextSite.chunkHeight * nextSite.chunkSize;

            int prevSiteSizeX;
            int prevSiteSizeY;
            if (prevSite.useBorder)
            {
                prevSiteSizeX = (prevSite.chunkWidth - 2) * prevSite.chunkSize;
                prevSiteSizeY = (prevSite.chunkHeight - 2) * prevSite.chunkSize;
            }
            else
            {
                prevSiteSizeX = (prevSite.chunkWidth) * prevSite.chunkSize;
                prevSiteSizeY = (prevSite.chunkHeight) * prevSite.chunkSize;
            }


            // Overlapping Removal
            if (nextSite.acInfo != null)
            {
                // Borders are only created on same height passages 
                if (nextSite.position.y == thisSite.position.y)
                {
                    float xof = nextSite.position.x - thisSite.position.x;
                    float yof = nextSite.position.z - thisSite.position.z;

                    xof += thisSite.chunkSize + start;
                    yof += thisSite.chunkSize + start;

                    int xo = Mathf.Clamp((int)xof, 0, thisSiteX + 2);
                    int yo = Mathf.Clamp((int)yof, 0, thisSiteY + 2);

                    int xf = Mathf.Clamp((int)xof + nextSiteSizeX, 0, thisSiteX + 2);
                    int yf = Mathf.Clamp((int)yof + nextSiteSizeY, 0, thisSiteY + 2);
                    
                    // Debug.Log("Fo = (" + xo + ", " + yo + ") Ff = (" + xf + ", " + yf + ")");

                    for (int x = xo; x < xf; x++)
                    {
                        for (int y = yo; y < yf; y++)
                        {
                            map[x, y] = 3;     // Removed (Empty Tile)
                        }
                    }
                }
            }


            if (prevSite.acInfo != null)
            {
                // Borders are only created on same height passages 
                if (prevSite.position.y == thisSite.position.y)
                {
                    float xof = prevSite.position.x - thisSite.position.x;
                    float yof = prevSite.position.z - thisSite.position.z;

                    int headstart = (prevSite.useBorder) ? prevSite.chunkSize : 0;

                    xof += headstart;
                    yof += headstart;

                    int borderSize = (prevSite.useBorder) ? prevSite.borderSize : 0;
                    
                    xof -= borderSize;
                    yof -= borderSize;

                    xof += thisSite.chunkSize + start;
                    yof += thisSite.chunkSize + start;

                    int xo = Mathf.Clamp((int)xof, 0, thisSiteX + 2);
                    int yo = Mathf.Clamp((int)yof, 0, thisSiteY + 2);

                    int xf = Mathf.Clamp((int)xof + prevSiteSizeX + (borderSize * 2), 0, thisSiteX + 2);
                    int yf = Mathf.Clamp((int)yof + prevSiteSizeY + (borderSize * 2), 0, thisSiteY + 2);

                    // Debug.Log("Fo = (" + xo + ", " + yo + ") Ff = (" + xf + ", " + yf + ")");

                    for (int x = xo; x < xf; x++)
                    {
                        for (int y = yo; y < yf; y++)
                        {
                            map[x, y] = 3;     // Removed (Empty Tile)
                        }
                    }
                }
            }

            // Map Filling
            int sizeX = thisSite.chunkSize;
            int sizeY = thisSite.chunkSize;
            int maxX = thisSite.chunkWidth * thisSite.chunkSize;
            int maxY = thisSite.chunkHeight * thisSite.chunkSize;
            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    map[start + sizeX + x, start + sizeY + y] = noiseMap[start + x, start + y];
                }
            }

            return map;
        }

        public static float[,] ApplyFalloffMask(float[,] noiseMap, float[,] falloffMap)
        {
            int width = noiseMap.GetLength(0);
            int height = noiseMap.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
            }

            return noiseMap;
        }

        public static int[,] ApplyArtificialCanvas(int[,] canvas, int[,] noiseMap, bool cleanCanvas)
        {
            int width = noiseMap.GetLength(0);
            int height = noiseMap.GetLength(1);

            if (!cleanCanvas)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (canvas[x, y] < 4)  //Allow modification
                        {
                            noiseMap[x, y] = canvas[x, y];
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (canvas[x, y] < 4)  //Allow modification
                        {
                            if (canvas[x, y] == 0)
                                noiseMap[x, y] = -1;
                            else
                                noiseMap[x, y] = canvas[x, y];
                        }
                    }
                }
            }

            return noiseMap;
        }
    }

}
