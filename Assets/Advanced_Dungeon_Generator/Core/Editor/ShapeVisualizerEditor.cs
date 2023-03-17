using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShapeVisualizer))]
public class ShapeVisualizerEditor : Editor
{
    ShapeVisualizer sv;
    int textureSize = 100;

    int index;
    public string[] replace = new string[] { "Walls", "Structures", "Both" };

    int type;       
    int order;      
    
    string chain;  
    public string[] natural = new string[] { "Process Room Map", "Basic Noise Map", "Falloff Map" };
    public string[] artificial = new string[] { "Process Room Map", "Basic Noise Map", "Base Canvas Image" };
    public string[] habitacion = new string[] { "Base Canvas Image" };

    string sectionStyle = "Tooltip";
    string containerStyle = "HelpBox";

    private void OnEnable()
    {
        sv = (ShapeVisualizer)target;
        EditorUtility.SetDirty(sv);
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



        // Section 1 (Display Properties)
        EditorGUIUtility.labelWidth = 110;
        EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("Display Properties", titleStyle);
            sv.dSh = (DungeonShape)EditorGUILayout.ObjectField("DungeonShape", sv.dSh, typeof(DungeonShape), false);
            sv.canvas = (Renderer)EditorGUILayout.ObjectField("Canvas", sv.canvas, typeof(Renderer), true);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        // Section 2 (Draw Properties)
        EditorGUIUtility.labelWidth = 160;
        EditorGUILayout.BeginVertical(sectionStyle);
            sv.showShapeProperties = EditorGUILayout.Foldout(sv.showShapeProperties, "Draw Properties", true);
            if (sv.showShapeProperties)
            {
                CheckDungeonShape(titleStyle);
            
                if (chain == "Process Room Map")
                {
                    sv.useAdvanceProcessing = EditorGUILayout.Toggle("Use Advanced Processing", sv.useAdvanceProcessing);
                    if (sv.useAdvanceProcessing)
                    {
                        sv.acInfo.normalAccessGen = EditorGUILayout.Toggle("Apply Procedural Access", sv.acInfo.normalAccessGen);
                        if (sv.acInfo.normalAccessGen)
                        {
                            EditorGUI.indentLevel++;
                                sv.acInfo.entranceDirection = Mathf.Clamp(EditorGUILayout.IntField("Entrance Direction", sv.acInfo.entranceDirection), 1, 4);
                                sv.acInfo.exitDirection = Mathf.Clamp(EditorGUILayout.IntField("Exit Direction", sv.acInfo.exitDirection), 1, 4);
                            EditorGUI.indentLevel--;
                        }
                    }
                }
                

                if (chain == "Process Room Map" || chain == "Basic Noise Map")
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUIUtility.labelWidth = 110;
                    EditorGUILayout.LabelField("Randomness", titleStyle);
                        sv.seed = EditorGUILayout.IntField("Generation Seed", sv.seed);
                        sv.offset = EditorGUILayout.Vector2Field("Map Offset", sv.offset);
                }

                EditorGUILayout.Space();
            }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Section 3 (Edit Shape Parameters)
        EditorGUILayout.BeginVertical(sectionStyle);
            sv.canEditShapeParameters = EditorGUILayout.Foldout(sv.canEditShapeParameters, "Edit Shape Parameters", true);
            if (sv.canEditShapeParameters && sv.dSh != null)
            {
                EditorGUIUtility.labelWidth = 140;
                EditorGUILayout.LabelField("Shape Type", titleStyle);

                sv.dSh.generation = (SegmentGenerationType)EditorGUILayout.EnumPopup("Generation Type", sv.dSh.generation);

                if (sv.dSh.generation == SegmentGenerationType.Artificial) {
                    sv.dSh.chunkSize = 16;
                    sv.dSh.itsAPredefinedRoom = EditorGUILayout.Toggle("It's a Predefined Room?", sv.dSh.itsAPredefinedRoom);
                } else {
                    sv.dSh.itsAPredefinedRoom = false;
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();



                if (sv.dSh.generation == SegmentGenerationType.Natural)
                {
                    EditorGUIUtility.labelWidth = 135;
                    EditorGUILayout.LabelField("Generation Properties", titleStyle);
                    EditorGUILayout.Space();


                    EditorGUILayout.LabelField("Room Chunks", subTitleStyle);
                        sv.dSh.chunkSize = Mathf.Clamp(EditorGUILayout.IntField("Chunk Size", sv.dSh.chunkSize), sv.dSh.minChunkSize, sv.dSh.maxChunkSize);
                        sv.dSh.chunkWidth = Mathf.Clamp(EditorGUILayout.IntField("Room Chunks Width", sv.dSh.chunkWidth), 1, sv.dSh.maxRoomChunkWidth);
                        sv.dSh.chunkHeight = Mathf.Clamp(EditorGUILayout.IntField("Room Chunks Height", sv.dSh.chunkHeight), 1, sv.dSh.maxRoomChunkHeight);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();


                    Information(subTitleStyle);
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();


                    EditorGUIUtility.labelWidth = 95;
                    EditorGUILayout.LabelField("Falloff Map Mask", subTitleStyle);
                        sv.dSh.concentration = Mathf.Clamp(EditorGUILayout.FloatField("Concentration", sv.dSh.concentration), 0.0001f, sv.dSh.maxConcentration) ;
                        sv.dSh.remoteness = Mathf.Clamp(EditorGUILayout.FloatField("Remoteness", sv.dSh.remoteness), 0.0001f, sv.dSh.maxRemoteness);
                        EditorGUILayout.Space();
                }
                else
                {
                    if (sv.dSh.itsAPredefinedRoom) {
                        EditorGUILayout.LabelField("Room Texture", titleStyle);
                    } else {
                        EditorGUILayout.LabelField("Room Mask Texture", titleStyle);
                    }


                    EditorGUILayout.LabelField("Texture", subTitleStyle);
                    EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        EditorGUILayout.BeginVertical(containerStyle);
                            if (sv.dSh.canvasTexture != null)
                            {
                                GUILayout.Label(AssetPreview.GetAssetPreview(sv.dSh.canvasTexture), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(textureSize), GUILayout.Height(textureSize));
                            }
                            sv.dSh.canvasTexture = (Texture2D)EditorGUILayout.ObjectField(sv.dSh.canvasTexture, typeof(Texture2D), false, GUILayout.Width(textureSize));
                        EditorGUILayout.EndVertical();

                        GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();


                    if (!sv.dSh.itsAPredefinedRoom)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Generation Properties", titleStyle);
                            EditorGUILayout.Space();
                            Information(subTitleStyle);
                            EditorGUILayout.Space();
                    }
                }

            }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        sv.autoUpdate = EditorGUILayout.Toggle("AutoUpdate", sv.autoUpdate);
        if (GUI.changed && sv.autoUpdate)   
        {
            CheckDraw(subTitleStyle);
        }
        
        if (GUILayout.Button("Draw Map"))
        {
            CheckDraw(subTitleStyle);
        }
    }


    public void Information(GUIStyle style)
    {
        EditorGUIUtility.labelWidth = 85;
        EditorGUILayout.LabelField("Noise Properties", style);
            sv.dSh.noiseInfo.noiseScale  = Mathf.Clamp(EditorGUILayout.FloatField("Noise Scale", sv.dSh.noiseInfo.noiseScale), 0.0001f, sv.dSh.maxNoiseScale);
            sv.dSh.noiseInfo.octaves     = Mathf.Clamp(EditorGUILayout.IntField("Octaves", sv.dSh.noiseInfo.octaves), 1, sv.dSh.maxOctaves);
            sv.dSh.noiseInfo.persistance = EditorGUILayout.Slider("Persistance", sv.dSh.noiseInfo.persistance, 0, 1);
            sv.dSh.noiseInfo.lacunarity  = Mathf.Clamp(EditorGUILayout.FloatField("Lacunarity", sv.dSh.noiseInfo.lacunarity), sv.dSh.minLacunarity, sv.dSh.maxLacunarity);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


        EditorGUIUtility.labelWidth = 85;
        EditorGUILayout.LabelField("Noise Processing", style);
            EditorGUILayout.MinMaxSlider("Ground Range", ref sv.dSh.noiseProcessInfo.minFloorValue, ref sv.dSh.noiseProcessInfo.minStructureValue, 0, 1);

        EditorGUIUtility.labelWidth = 240;
            sv.dSh.noiseProcessInfo.minFloorValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Minimum Value to be Considered Ground",
                "Any entrance of the 0-1 perlin noise map below this value will be considered as a Wall"), sv.dSh.noiseProcessInfo.minFloorValue), 0, sv.dSh.noiseProcessInfo.minStructureValue);
            sv.dSh.noiseProcessInfo.minStructureValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Maximum Value to be Considered Ground",
                "Any entrance of the 0-1 perlin noise map above this value will be considered as a Structure"),  sv.dSh.noiseProcessInfo.minStructureValue), sv.dSh.noiseProcessInfo.minFloorValue, 1);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


        EditorGUIUtility.labelWidth = 225;
        EditorGUILayout.LabelField("Room Processing", style);
            sv.dSh.noiseProcessInfo.minTilesToGenWalls = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a Wall",
                "Minimum number of connected wall tiles required to be considered as an appropriate wall"), sv.dSh.noiseProcessInfo.minTilesToGenWalls);
            sv.dSh.noiseProcessInfo.minTilesToGenRooms = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a SubRoom",
                "Minimum number of connected floor tiles required to be considered as an appropriate subroom"), sv.dSh.noiseProcessInfo.minTilesToGenRooms);

            index = sv.dSh.noiseProcessInfo.failedRoomValue - 1;
            sv.dSh.noiseProcessInfo.failedRoomValue = Mathf.Clamp(EditorGUILayout.Popup(new GUIContent("Failed SubRooms will be Converted to",
                "SubRooms with less than the minimum required tiles will be Converted to this Specification"), index, replace) + 1, 1, 3);

            EditorGUILayout.Space();

            sv.dSh.noiseProcessInfo.minTilesToGenStructures = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a Structure",
                "Minimum number of connected structure tiles required to be considered as an appropriate structure"), sv.dSh.noiseProcessInfo.minTilesToGenStructures);
            sv.dSh.noiseProcessInfo.maxHallwayRadius = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Maximum Possible Size for Passages",
                "Maximum size for the passages that will connect distant subrooms"), sv.dSh.noiseProcessInfo.maxHallwayRadius), 1, sv.dSh.maxPassageSize);

        EditorGUIUtility.labelWidth = 120;
    }


    public void CheckDraw(GUIStyle style)
    {
        if (sv.canvas == null)
        {
            Debug.LogError("The room cannot be drawn due to lack of essential information (Scene Canvas is not assigned)");
            return;
        }

        if (CheckDungeonShape(style))
        {
            if (sv.dSh.generation != SegmentGenerationType.Natural)
            {
                if (sv.dSh.canvasTexture != null)
                    sv.Draw(type, order);
                else
                    Debug.LogError("The room cannot be drawn due to lack of essential information (Room Texture Missing)");
            }else{
                sv.Draw(type, order);
            }
        }
        else
        {
            Debug.LogError("The room cannot be drawn due to lack of essential information (DungeonShape is not assigned)");
        }
    }


    public bool CheckDungeonShape(GUIStyle style)
    {
        if (sv.dSh != null)
        {
            if (sv.dSh.generation == SegmentGenerationType.Natural)      //Natural
            {
                EditorGUILayout.LabelField("Natural Shape Options", style);
                sv.index = Mathf.Clamp(EditorGUILayout.Popup("Draw Mode", sv.index, natural), 0, 2);
                chain = natural[sv.index];
                type = 0;
            }
            else if (sv.dSh.itsAPredefinedRoom != true)                  //Artificial, No Habitation 
            {
                EditorGUILayout.LabelField("Artificial Shape Options", style);
                sv.index = Mathf.Clamp(EditorGUILayout.Popup("Draw Mode", sv.index, artificial), 0, 2);
                chain = artificial[sv.index];
                type = 1;
            }
            else                                                    //Artificial & Habitation
            {
                EditorGUILayout.LabelField("Room Shape Options", style);
                sv.index = Mathf.Clamp(EditorGUILayout.Popup("Draw Mode", sv.index, habitacion), 0, 0);
                chain = habitacion[sv.index];
                type = 2;
            }

            if (chain == "Process Room Map")
                order = 0;
            else if (chain == "Basic Noise Map")
                order = 1;
            else if (chain == "Falloff Map")
                order = 2;
            else   //chain == "Base Canvas Image"
                order = 3;

            return true;
        }
        else
            return false;
    }
}
