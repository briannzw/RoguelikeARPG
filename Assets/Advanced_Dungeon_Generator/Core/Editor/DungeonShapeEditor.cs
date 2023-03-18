﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonShape))]
public class DungeonShapeEditor : Editor
{
    DungeonShape ds;
    int textureSize = 100;

    int index;
    public string[] replace = new string[] { "Walls", "Structures", "Both" };

    string sectionStyle = "Tooltip";
    string containerStyle = "HelpBox";

    private void OnEnable()
    {
        ds = (DungeonShape)target;
        EditorUtility.SetDirty(ds);
    }

    public override void OnInspectorGUI()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.textField)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        GUIStyle subTitleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        textureSize = (int)(EditorGUIUtility.currentViewWidth / 6);



        // Property Label
        EditorGUIUtility.labelWidth = 170;
        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);


        // Section 1 (Generation Type)
        EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUIUtility.labelWidth = 140;
            EditorGUILayout.LabelField("Shape Type", titleStyle);

            ds.generation = (SegmentGenerationType)EditorGUILayout.EnumPopup("Generation Type", ds.generation);

            if (ds.generation == SegmentGenerationType.Artificial) {
                ds.chunkSize = 16;
                ds.itsAPredefinedRoom = EditorGUILayout.Toggle(new GUIContent("Is it a Predefined Room?",
                    "When enabled, the masking process will be ignored, and all the Dungeon Rooms generated by this Dungeon Shape will be copies of the given image"), ds.itsAPredefinedRoom);
                ds.prioritizeImageFidelity = EditorGUILayout.Toggle(new GUIContent("Prioritize Image Fidelity",
                    "When enable, priority will be given to copying the shape of the given Canvas in the Dungeon Rooms rather than their functionality"), ds.prioritizeImageFidelity);
            } else {
                ds.itsAPredefinedRoom = false;
            }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        // Section 2 (Generation Properties)
        EditorGUILayout.BeginVertical(sectionStyle);
        if (ds.generation == SegmentGenerationType.Natural)
        {
            EditorGUIUtility.labelWidth = 135;
            EditorGUILayout.LabelField("Generation Properties", titleStyle);
            EditorGUILayout.Space();


            EditorGUILayout.LabelField("Room Chunks", subTitleStyle);
                ds.chunkSize = Mathf.Clamp(EditorGUILayout.IntField("Chunk Size", ds.chunkSize), ds.minChunkSize, ds.maxChunkSize);
                ds.chunkWidth = Mathf.Clamp(EditorGUILayout.IntField("Room Chunks Width", ds.chunkWidth), 1, ds.maxRoomChunkWidth);
                ds.chunkHeight = Mathf.Clamp(EditorGUILayout.IntField("Room Chunks Height", ds.chunkHeight), 1, ds.maxRoomChunkHeight);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();


            Information(subTitleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            EditorGUIUtility.labelWidth = 95;
            EditorGUILayout.LabelField("Falloff Map Mask", subTitleStyle);
                ds.concentration = Mathf.Clamp(EditorGUILayout.FloatField("Concentration", ds.concentration), 0.0001f, ds.maxConcentration);
                ds.remoteness = Mathf.Clamp(EditorGUILayout.FloatField("Remoteness", ds.remoteness), 0.0001f, ds.maxRemoteness);
                EditorGUILayout.Space();
        }
        else
        {
            if (ds.itsAPredefinedRoom) {
                EditorGUILayout.LabelField("Room Texture", titleStyle);
            } else {
                EditorGUILayout.LabelField("Room Mask Texture", titleStyle);
            }


            EditorGUILayout.LabelField("Texture", subTitleStyle);
            EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical(containerStyle);
                    if (ds.canvasTexture != null)
                    {
                        GUILayout.Label(AssetPreview.GetAssetPreview(ds.canvasTexture), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(textureSize), GUILayout.Height(textureSize));
                    }
                    ds.canvasTexture = (Texture2D)EditorGUILayout.ObjectField(ds.canvasTexture, typeof(Texture2D), false, GUILayout.Width(textureSize));
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();


            if (!ds.itsAPredefinedRoom)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                EditorGUILayout.Space();


                EditorGUILayout.BeginVertical(sectionStyle);
                EditorGUILayout.LabelField("Generation Properties", titleStyle);
                    EditorGUILayout.Space();
                    Information(subTitleStyle);
                    EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndVertical();
    }

    public void Information(GUIStyle style)
    {
        EditorGUIUtility.labelWidth = 85;
        EditorGUILayout.LabelField("Noise Properties", style);
            ds.noiseInfo.noiseScale  = Mathf.Clamp(EditorGUILayout.FloatField("Noise Scale", ds.noiseInfo.noiseScale), 0.0001f, ds.maxNoiseScale);
            ds.noiseInfo.octaves     = Mathf.Clamp(EditorGUILayout.IntField("Octaves", ds.noiseInfo.octaves), 1, ds.maxOctaves);
            ds.noiseInfo.persistance = EditorGUILayout.Slider("Persistance", ds.noiseInfo.persistance, 0, 1);
            ds.noiseInfo.lacunarity  = Mathf.Clamp(EditorGUILayout.FloatField("Lacunarity", ds.noiseInfo.lacunarity), ds.minLacunarity, ds.maxLacunarity);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


        EditorGUIUtility.labelWidth = 85;
        EditorGUILayout.LabelField("Noise Processing", style);
            EditorGUILayout.MinMaxSlider("Ground Range", ref ds.noiseProcessInfo.minFloorValue, ref ds.noiseProcessInfo.minStructureValue, 0, 1);

        EditorGUIUtility.labelWidth = 240;
            ds.noiseProcessInfo.minFloorValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Minimum Value to be Considered Ground",
                "Any entrance of the 0-1 perlin noise map below this value will be considered as a Wall"), ds.noiseProcessInfo.minFloorValue), 0, ds.noiseProcessInfo.minStructureValue);
            ds.noiseProcessInfo.minStructureValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Maximum Value to be Considered Ground",
                "Any entrance of the 0-1 perlin noise map above this value will be considered as a Structure"), ds.noiseProcessInfo.minStructureValue), ds.noiseProcessInfo.minFloorValue, 1);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


        EditorGUIUtility.labelWidth = 225;
        EditorGUILayout.LabelField("Room Processing", style);
            ds.noiseProcessInfo.minTilesToGenWalls = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a Wall",
                "Minimum number of connected wall tiles required to be considered as an appropriate wall"), ds.noiseProcessInfo.minTilesToGenWalls);
            ds.noiseProcessInfo.minTilesToGenRooms = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a SubRoom",
                "Minimum number of connected ground tiles required to be considered as an appropriate subroom"), ds.noiseProcessInfo.minTilesToGenRooms);
            
            index = ds.noiseProcessInfo.failedRoomValue - 1;
            ds.noiseProcessInfo.failedRoomValue = Mathf.Clamp(EditorGUILayout.Popup(new GUIContent("Failed SubRooms will be Converted to",
                "SubRooms with less than the minimum required tiles will be Converted to this Specification"), index, replace) + 1, 1, 3);

        EditorGUILayout.Space();

            ds.noiseProcessInfo.minTilesToGenStructures = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a Structure",
                "Minimum number of connected structure tiles required to be considered as an appropriate structure"), ds.noiseProcessInfo.minTilesToGenStructures);
            ds.noiseProcessInfo.maxHallwayRadius = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Maximum Possible Size for Passages",
                "Maximum size for the passages that will connect distant subrooms"), ds.noiseProcessInfo.maxHallwayRadius), 1, ds.maxPassageSize);

        EditorGUIUtility.labelWidth = 120;
    }
}