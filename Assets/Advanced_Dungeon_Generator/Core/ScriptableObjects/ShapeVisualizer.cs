using System;
using System.Collections.Generic;
using UnityEngine;

using ADG.Utilities.DungeonGeneratorClases;
using ADG.Utilities.DungeonShapeClases;
using ADG.Tools.MapDesigner;

public class ShapeVisualizer : MonoBehaviour
{
    [Header("Draw Mode")]
    public bool useAdvanceProcessing = true;

    [Header("Randomness")]
    public int seed;
    public Vector2 offset;

    [Header("Shape Properties")]
    public DungeonShape dSh;
    public Renderer canvas;
    public AccessInfo acInfo;
    [Space]
    public bool autoUpdate = false;

    [HideInInspector]
    public int index;
    [HideInInspector]
    public bool showShapeProperties = true;
    [HideInInspector]
    public bool canEditShapeParameters = false;

    private SegmentInformation seg;
    private bool drawZeroGrey;
    private float[,] noiseMap;
    private int[,] processedNoise;

    public System.Random rng;

    private void Start()
    {
        canvas = GetComponent<Renderer>();
    }

    public void Draw(int type, int order)
    {
        seg = new SegmentInformation(dSh);
        rng = new System.Random(seed);

        List<Coord> hotSpots;
        if (type == 0)      //For Natural Shapes
        {
            noiseMap = Noise.GeneratePerlinNoiseMap(seed, offset, dSh.noiseInfo, seg.mapWidth, seg.mapHeight);
            noiseMap = MapMerger.ApplyFalloffMask(noiseMap, seg.falloffMask);
            processedNoise = NoiseProcessor.ProcessNoiseMap(noiseMap, dSh.noiseProcessInfo, ref acInfo, useAdvanceProcessing, rng, out hotSpots);
        }
        else if (type == 1) //For Artificial Shapes
        {
            noiseMap = Noise.GeneratePerlinNoiseMap(seed, offset, dSh.noiseInfo, seg.mapWidth, seg.mapHeight);
            processedNoise = NoiseProcessor.ProcessNoiseMap(noiseMap, dSh.noiseProcessInfo, ref acInfo, false, rng, out hotSpots);

            processedNoise = MapMerger.ApplyArtificialCanvas(seg.canvasMask, processedNoise, false);
            if (useAdvanceProcessing)
            {
                processedNoise = NoiseProcessor.ProcessIntMap(processedNoise, dSh.noiseProcessInfo, ref acInfo, false, rng, out hotSpots);
                processedNoise = MapMerger.ApplyArtificialCanvas(seg.canvasMask, processedNoise, true);
            }
        }
        else                //For Rooms
        {
            processedNoise = NoiseProcessor.ProcessIntMap(seg.canvasMask, dSh.noiseProcessInfo, ref acInfo, true, rng, out hotSpots);
            drawZeroGrey = false;
        }


        if (order == 0)         //Process Room Map
        {
            drawZeroGrey = (useAdvanceProcessing) ? true : false;
            DrawNoiseMap(processedNoise, drawZeroGrey);
        }
        else if (order == 1)    //Basic Noise Map
        {
            DrawNoiseMap(noiseMap);
        }
        else if (order == 2)    //Falloff Map
        {
            DrawNoiseMap(seg.falloffMask);
        }
        else                    //Base Canvas Image
        {
            DrawNoiseMap(seg.canvasMask, false);
        }
    }

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width  = noiseMap.GetLength(0) - 2;
        int height = noiseMap.GetLength(1) - 2;

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x + 1, y + 1]);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colorMap);
        texture.Apply();

        canvas.sharedMaterial.SetTexture("_BaseMap", texture);
        //canvas.sharedMaterial.mainTexture = texture;
    }

    public void DrawNoiseMap(int[,] noiseMap, bool drawZeroGrey)
    {
        int width  = noiseMap.GetLength(0) - 2;
        int height = noiseMap.GetLength(1) - 2;

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if(noiseMap[x + 1, y + 1] > 0)
                {
                    if (noiseMap[x + 1, y + 1] == 1)
                        colorMap[y * width + x] = Color.black;  //Walls
                    else if (noiseMap[x + 1, y + 1] == 2)
                        colorMap[y * width + x] = Color.blue;   //Structures
                    else if (noiseMap[x + 1, y + 1] == 3)
                        colorMap[y * width + x] = Color.red;    //Void
                    else if (noiseMap[x + 1, y + 1] == 4)
                        colorMap[y * width + x] = Color.green;  //Canvas overlapping
                }
                else 
                {
                    if (noiseMap[x + 1, y + 1] == 0)
                    {
                        if (drawZeroGrey)
                        {
                            colorMap[y * width + x] = Color.grey;   //Hallways
                        }else{
                            colorMap[y * width + x] = Color.white;  //Hallways
                        }
                    }
                    else
                        colorMap[y * width + x] = Color.white;      //Rooms (Any negative value)
                }
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colorMap);
        texture.Apply();

        canvas.sharedMaterial.SetTexture("_BaseMap", texture);
        //canvas.sharedMaterial.mainTexture = texture;
    }
}
