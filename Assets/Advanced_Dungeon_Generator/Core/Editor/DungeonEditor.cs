using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using ADG.Utilities.DungeonGeneratorClases;

[CustomEditor(typeof(Dungeon))]
public class DungeonEditor : Editor
{
    Dungeon d;
    SerializedObject sObjectDungeon;
    SerializedProperty segments;
    SerializedProperty useDeco;
    SerializedProperty useInterac;
    int listSize;

    bool gloabUseDecorations;
    DungeonBrush[] brushes = new DungeonBrush[3];
    DungeonDecoration[] decorations = new DungeonDecoration[3];
    DungeonInteractables[] interactables = new DungeonInteractables[3];

    string sectionStyle = "Tooltip";


    private void OnEnable()
    {
        d = (Dungeon)target;
        EditorUtility.SetDirty(d);

        sObjectDungeon = new SerializedObject(target);
        segments = sObjectDungeon.FindProperty("segments");
        useDeco = sObjectDungeon.FindProperty("hasDecorations");
        useInterac = sObjectDungeon.FindProperty("hasInteractables");
    }

    public override void OnInspectorGUI()
    {
        sObjectDungeon.Update();

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

        

        if (d.segments.Count < 1)
        {
            segments.InsertArrayElementAtIndex(0);
            SerializedProperty segZero = segments.GetArrayElementAtIndex(0);

            SerializedProperty dBrushes = segZero.FindPropertyRelative("brushes");
            dBrushes.InsertArrayElementAtIndex(0);
            dBrushes.InsertArrayElementAtIndex(1);
            dBrushes.InsertArrayElementAtIndex(2);
            brushes[0] = (DungeonBrush)dBrushes.GetArrayElementAtIndex(0).objectReferenceValue;
            brushes[1] = (DungeonBrush)dBrushes.GetArrayElementAtIndex(1).objectReferenceValue;
            brushes[2] = (DungeonBrush)dBrushes.GetArrayElementAtIndex(2).objectReferenceValue;

            SerializedProperty dDecorations = segZero.FindPropertyRelative("decorations");
            dDecorations.InsertArrayElementAtIndex(0);
            dDecorations.InsertArrayElementAtIndex(1);
            dDecorations.InsertArrayElementAtIndex(2);
            decorations[0] = (DungeonDecoration)dDecorations.GetArrayElementAtIndex(0).objectReferenceValue;
            decorations[1] = (DungeonDecoration)dDecorations.GetArrayElementAtIndex(1).objectReferenceValue;
            decorations[2] = (DungeonDecoration)dDecorations.GetArrayElementAtIndex(2).objectReferenceValue;

            SerializedProperty dInteractables = segZero.FindPropertyRelative("interactables");
            dInteractables.InsertArrayElementAtIndex(0);
            dInteractables.InsertArrayElementAtIndex(1);
            dInteractables.InsertArrayElementAtIndex(2);
            interactables[0] = (DungeonInteractables)dInteractables.GetArrayElementAtIndex(0).objectReferenceValue;
            interactables[1] = (DungeonInteractables)dInteractables.GetArrayElementAtIndex(1).objectReferenceValue;
            interactables[2] = (DungeonInteractables)dInteractables.GetArrayElementAtIndex(2).objectReferenceValue;
        }


        // Property Label
        EditorGUIUtility.labelWidth = 170;
        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);


        // Section 1 (Rooms Loading)
        EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("Room Loading", titleStyle);
            d.hidePredecesors = EditorGUILayout.Toggle(new GUIContent("Hide Predecessors",
                "When enabled, the rooms that are no longer visible on screen will be hidden"), d.hidePredecesors);
            d.useArtificialAsigment = EditorGUILayout.Toggle(new GUIContent("Use Artificial Loading",
                "When enabled, the user will define the proper load and distribution of the Dungeon Rooms, otherwise the Dungeon Generator will do it on its own"), d.useArtificialAsigment);
            d.useProceduralAsigment = !d.useArtificialAsigment;
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        // Section 2 (Global Attributes)
        EditorGUIUtility.labelWidth = 120;
        EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("Global Attributes", titleStyle);

            EditorGUI.indentLevel++;
            d.showGlobalSettings = EditorGUILayout.Foldout(d.showGlobalSettings, "Show Global Attributes", true);

            if (d.showGlobalSettings)
            {
                //=================================================================================================================================
                EditorGUIUtility.labelWidth = 140;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Dungeon Brush", EditorStyles.boldLabel);

                d.useGlobalBrush = EditorGUILayout.Toggle(new GUIContent("Use Global Brush",
                    "When enable, these Brushes will be assigned to all Dungeon Segments"), d.useGlobalBrush);
                if (d.useGlobalBrush)
                {
                    SerializedProperty dBrushes = segments.GetArrayElementAtIndex(0).FindPropertyRelative("brushes");

                    brushes[0] = (DungeonBrush)dBrushes.GetArrayElementAtIndex(0).objectReferenceValue;
                    brushes[1] = (DungeonBrush)dBrushes.GetArrayElementAtIndex(1).objectReferenceValue;
                    brushes[2] = (DungeonBrush)dBrushes.GetArrayElementAtIndex(2).objectReferenceValue;
                }

                if (d.useGlobalBrush)
                {
                    EditorGUI.indentLevel++;
                        EditorGUIUtility.labelWidth = 140;
                        brushes[0] = ShowGetAndValidateObject("Ground Brush", brushes[0]);
                        brushes[1] = ShowGetAndValidateObject("Walls Brush", brushes[1]);
                        brushes[2] = ShowGetAndValidateObject("Structures Brush", brushes[2]);
                    EditorGUI.indentLevel--;
                }
            //=================================================================================================================================
                EditorGUIUtility.labelWidth = 180;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Dungeon Decorations", EditorStyles.boldLabel);
            
                useDeco.boolValue = EditorGUILayout.Toggle(new GUIContent("Enable Decorations",
                    "When enabled, you can add Decorations to the Dungeon"), useDeco.boolValue);
                if (useDeco.boolValue)
                {
                    EditorGUI.indentLevel++;
                    d.useGlobalDecoration = EditorGUILayout.Toggle(new GUIContent("Use Global Decoration",
                        "When enable, these Decorations will be assigned to all Dungeon Segments"), d.useGlobalDecoration);
                    if (d.useGlobalDecoration)
                    {
                        SerializedProperty dDecorations = segments.GetArrayElementAtIndex(0).FindPropertyRelative("decorations");

                        decorations[0] = (DungeonDecoration)dDecorations.GetArrayElementAtIndex(0).objectReferenceValue;
                        decorations[1] = (DungeonDecoration)dDecorations.GetArrayElementAtIndex(1).objectReferenceValue;
                        decorations[2] = (DungeonDecoration)dDecorations.GetArrayElementAtIndex(2).objectReferenceValue;
                    }

                    if (d.useGlobalDecoration)
                    {
                        EditorGUI.indentLevel++;
                            EditorGUIUtility.labelWidth = 180;
                            decorations[0] = ShowGetAndValidateObject("Ground Decoration", decorations[0]);
                            decorations[1] = ShowGetAndValidateObject("Walls Decoration", decorations[1]);
                            decorations[2] = ShowGetAndValidateObject("Structures Decoration", decorations[2]);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                //=================================================================================================================================

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Dungeon Interactables", EditorStyles.boldLabel);

                useInterac.boolValue = EditorGUILayout.Toggle(new GUIContent("Enable Interactables",
                    "When enabled, you can add Interactables to the Dungeon"), useInterac.boolValue);
                if(useInterac.boolValue)
                {
                    EditorGUI.indentLevel++;
                    d.useGlobalInteractables = EditorGUILayout.Toggle(new GUIContent("Use Global Interactables",
                        "When enable, these Interactables will be assigned to all Dungeon Segments"), d.useGlobalInteractables);

                    if (d.useGlobalInteractables)
                    {
                        SerializedProperty dInteractables = segments.GetArrayElementAtIndex(0).FindPropertyRelative("interactables");

                        interactables[0] = (DungeonInteractables)dInteractables.GetArrayElementAtIndex(0).objectReferenceValue;
                        interactables[1] = (DungeonInteractables)dInteractables.GetArrayElementAtIndex(1).objectReferenceValue;
                        interactables[2] = (DungeonInteractables)dInteractables.GetArrayElementAtIndex(2).objectReferenceValue;
                    }

                    if (d.useGlobalInteractables)
                    {
                        EditorGUI.indentLevel++;
                            EditorGUIUtility.labelWidth = 180;
                            interactables[0] = ShowGetAndValidateObject("Ground Interactables", interactables[0]);
                            interactables[1] = ShowGetAndValidateObject("Walls Interactables", interactables[1]);
                            interactables[2] = ShowGetAndValidateObject("Structures Interactables", interactables[2]);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
            //=====================================================================================================================================
            EditorGUI.indentLevel--;
            }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        // Section 3 (Dungeon Segments)
        EditorGUIUtility.labelWidth = 160;
        EditorGUILayout.BeginVertical(sectionStyle);
            
            EditorGUILayout.LabelField("Dungeon Segments", titleStyle);
            listSize = segments.arraySize;
            listSize = Mathf.Clamp(EditorGUILayout.IntField("Number of Segments", listSize), 1, d.maxDungeonSegments);

            if (listSize < 1)
                listSize = 1;

            if (listSize != segments.arraySize)
            {
                while (listSize > segments.arraySize)
                {
                    segments.InsertArrayElementAtIndex(segments.arraySize);
                }
                while (listSize < segments.arraySize)
                {
                    segments.DeleteArrayElementAtIndex(segments.arraySize - 1);
                }
            }

            if (GUILayout.Button("Add Segment"))
            {
                d.segments.Add(new DungeonSegment());
            }
            EditorGUILayout.Space();


        for (int i = 0; i < segments.arraySize; i++)
        {
            SerializedProperty segRef = segments.GetArrayElementAtIndex(i);

            SerializedProperty rooms = segRef.FindPropertyRelative("rooms");
            SerializedProperty shape = segRef.FindPropertyRelative("shape");
            SerializedProperty useBorder = segRef.FindPropertyRelative("useBorder");
            SerializedProperty borderSize = segRef.FindPropertyRelative("borderSize");

            SerializedProperty useExtra = segRef.FindPropertyRelative("useExtra");
            SerializedProperty iterateExtra = segRef.FindPropertyRelative("iterateExtra");
            SerializedProperty extra = segRef.FindPropertyRelative("extra");
            SerializedProperty extraPos = segRef.FindPropertyRelative("extraPos");

            SerializedProperty dBrushes = segRef.FindPropertyRelative("brushes");
            SerializedProperty dBg = dBrushes.GetArrayElementAtIndex(0);
            SerializedProperty dBw = dBrushes.GetArrayElementAtIndex(1);
            SerializedProperty dBs = dBrushes.GetArrayElementAtIndex(2);

            SerializedProperty segUseDeco = segRef.FindPropertyRelative("useDecorations");
            SerializedProperty dDecorations = segRef.FindPropertyRelative("decorations");
            SerializedProperty dDg = dDecorations.GetArrayElementAtIndex(0);
            SerializedProperty dDw = dDecorations.GetArrayElementAtIndex(1);
            SerializedProperty dDs = dDecorations.GetArrayElementAtIndex(2);

            SerializedProperty segUseInterac = segRef.FindPropertyRelative("useInteractables");
            SerializedProperty dInteractables = segRef.FindPropertyRelative("interactables");
            SerializedProperty dIg = dInteractables.GetArrayElementAtIndex(0);
            SerializedProperty dIw = dInteractables.GetArrayElementAtIndex(1);
            SerializedProperty dIs = dInteractables.GetArrayElementAtIndex(2);

            //=================================================================================================================================
            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 135;
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("Segment " + (i + 1), subTitleStyle);

            //=================================================================================================================================
            rooms.intValue = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Segment Rooms", 
                "The number of Dungeon Rooms in this Segment"), rooms.intValue), 1, d.maxSegmentRooms);
            shape.objectReferenceValue = ShowGetAndValidateObject("Segment Shape", (DungeonShape)shape.objectReferenceValue);

            if (shape.objectReferenceValue)
            {
                SerializedObject buffer = new SerializedObject(shape.objectReferenceValue);
                SerializedProperty chunckSize = buffer.FindProperty("chunkSize");
                
                useBorder.boolValue = EditorGUILayout.Toggle(new GUIContent("Use Room Border", 
                    "The Dungeon Rooms of this Segment will have a smart border, very useful to avoid showing the raw edges of the Dungeon Room"), useBorder.boolValue);
                if (useBorder.boolValue)
                {
                    EditorGUI.indentLevel++;
                    borderSize.intValue = EditorGUILayout.IntSlider("Border Size", borderSize.intValue, 1, chunckSize.intValue);
                    EditorGUI.indentLevel--;
                }
            }
            //=================================================================================================================================
            useExtra.boolValue = EditorGUILayout.Toggle(new GUIContent("Use Additional Objects",
                "Will instantiate a given GameObject in each Dungeon Room of this Segment"), useExtra.boolValue);
            if (useExtra.boolValue)
            {
                EditorGUI.indentLevel++;
                    extra.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Prefab",
                        "The GameObject to be instantiated with the Dungeon Room"), extra.objectReferenceValue, typeof(GameObject), false);
                    extraPos.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("Position",
                        "Local position of the GameObject relative to its Dungeon Room position (Lower Left Corner)"), extraPos.vector3Value);
                    iterateExtra.boolValue = EditorGUILayout.Toggle(new GUIContent("Iterate Game Object",
                        "When enable, the given GameObject will be instantiated at each of the key points of the Dungeon Room, very useful if you want to define spawners"), iterateExtra.boolValue);

                EditorGUI.indentLevel--;
            }
            //=================================================================================================================================
            if (d.useArtificialAsigment)
            {
                SerializedProperty displacement = segRef.FindPropertyRelative("artificialDisplacement");

                EditorGUILayout.Space();

                EditorGUIUtility.labelWidth = 140;
                EditorGUILayout.LabelField("Artificial Loading Attributes", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                displacement.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("Room Offset",
                    "The amount of displacement a Room of this segment has, relative to the previous one.\n" +
                    "To clarify, this action moves the Global Position of this Room from the Global Position of the previous one " + 
                    "(The position of a Room starts at its Lower Left Corner)."), displacement.vector3Value);

                SerializedProperty useLoader = segRef.FindPropertyRelative("useLoader");
                useLoader.boolValue = EditorGUILayout.Toggle(new GUIContent("Create Loader",
                    "Will instantiate a custom loader in all the rooms of the segment"), useLoader.boolValue);

                if (useLoader.boolValue)
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty loaderAngle = segRef.FindPropertyRelative("loaderAngle");
                    loaderAngle.floatValue = EditorGUILayout.FloatField(new GUIContent("Loader Angle",
                        "The Y angle at which the Room Size Loader will be"), loaderAngle.floatValue);

                    SerializedProperty loaderPos = segRef.FindPropertyRelative("deltaLoaderPos");
                    loaderPos.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("Loader Offset",
                        "The amount of displacement the Room Size Loader will have, relative to its Room position"), loaderPos.vector3Value);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            //=================================================================================================================================
            if (d.useGlobalBrush)
            {
                dBg.objectReferenceValue = brushes[0];
                dBw.objectReferenceValue = brushes[1];
                dBs.objectReferenceValue = brushes[2];
            }else{
                EditorGUILayout.Space();

                EditorGUIUtility.labelWidth = 140;
                EditorGUILayout.LabelField("Brushes", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                dBg.objectReferenceValue = ShowGetAndValidateObject("Ground Brush"    , (DungeonBrush)dBg.objectReferenceValue);
                dBw.objectReferenceValue = ShowGetAndValidateObject("Walls Brush"     , (DungeonBrush)dBw.objectReferenceValue);
                dBs.objectReferenceValue = ShowGetAndValidateObject("Structures Brush", (DungeonBrush)dBs.objectReferenceValue);

                EditorGUI.indentLevel--;
            }
            //=================================================================================================================================
            if (d.useGlobalDecoration)
            {
                dDg.objectReferenceValue = decorations[0];
                dDw.objectReferenceValue = decorations[1];
                dDs.objectReferenceValue = decorations[2];
            }else{
                if (useDeco.boolValue)
                {
                    EditorGUILayout.Space();

                    EditorGUIUtility.labelWidth = 150;
                    EditorGUILayout.LabelField("Decorations", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    dDg.objectReferenceValue = ShowGetAndValidateObject("Ground Decoration"    , (DungeonDecoration)dDg.objectReferenceValue);
                    dDw.objectReferenceValue = ShowGetAndValidateObject("Walls Decoration"     , (DungeonDecoration)dDw.objectReferenceValue);
                    dDs.objectReferenceValue = ShowGetAndValidateObject("Structures Decoration", (DungeonDecoration)dDs.objectReferenceValue);

                    EditorGUI.indentLevel--;
                }
            }
            segUseDeco.boolValue = useDeco.boolValue;
            //=================================================================================================================================
            if (d.useGlobalInteractables)
            {
                dIg.objectReferenceValue = interactables[0];
                dIw.objectReferenceValue = interactables[1];
                dIs.objectReferenceValue = interactables[2];
            }else{
                if(useInterac.boolValue)
                {
                    EditorGUILayout.Space();

                    EditorGUIUtility.labelWidth = 150;
                    EditorGUILayout.LabelField("Interactables", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    dIg.objectReferenceValue = ShowGetAndValidateObject("Ground Interactable"    , (DungeonInteractables)dIg.objectReferenceValue);
                    dIw.objectReferenceValue = ShowGetAndValidateObject("Walls Interactable"     , (DungeonInteractables)dIw.objectReferenceValue);
                    dIs.objectReferenceValue = ShowGetAndValidateObject("Structures Interactable", (DungeonInteractables)dIs.objectReferenceValue);

                    EditorGUI.indentLevel--;
                }
            }
            segUseInterac.boolValue = useInterac.boolValue;
            //===================================================================================================================================
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        sObjectDungeon.ApplyModifiedProperties();
    }


    DungeonBrush ShowGetAndValidateObject(string name, DungeonBrush brush)
    {
        brush = (DungeonBrush)EditorGUILayout.ObjectField(name, brush, typeof(DungeonBrush), false);
        if (GUI.changed)
        {
            if (brush)
            {
                if (!ADG.Tools.SecurityCheck.ValidateDungeonComponent(brush))
                {
                    brush = null;
                }
            }
        }
        
        return brush;
    }

    DungeonDecoration ShowGetAndValidateObject(string name, DungeonDecoration deco)
    {
        deco = (DungeonDecoration)EditorGUILayout.ObjectField(name, deco, typeof(DungeonDecoration), false);
        if (GUI.changed)
        {
            if (deco)
            {
                if (!ADG.Tools.SecurityCheck.ValidateDungeonComponent(deco))
                {
                    deco = null;
                }
            }
        }

        return deco;
    }

    DungeonInteractables ShowGetAndValidateObject(string name, DungeonInteractables interactable)
    {
        interactable = (DungeonInteractables)EditorGUILayout.ObjectField(name, interactable, typeof(DungeonInteractables), false);
        if (GUI.changed)
        {
            if (interactable)
            {
                if (!ADG.Tools.SecurityCheck.ValidateDungeonComponent(interactable))
                {
                    interactable = null;
                }
            }
        }

        return interactable;
    }

    DungeonShape ShowGetAndValidateObject(string name, DungeonShape shape)
    {
        shape = (DungeonShape)EditorGUILayout.ObjectField(name, shape, typeof(DungeonShape), false);
        if (GUI.changed)
        {
            if (shape)
            {
                if (!ADG.Tools.SecurityCheck.ValidateDungeonComponent(shape))
                {
                    shape = null;
                }
            }
        }

        return shape;
    }
}
