using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonDecoration))]
public class DungeonDecorationEditor : Editor
{
    DungeonDecoration dd;
    SerializedObject sObjectDecoration;

    SerializedProperty mainProbability;
    SerializedProperty mainDecorations;
    SerializedProperty mainDecoClusters;
    SerializedProperty mainMeshDegeneracy;

    SerializedProperty transitionProbability;
    SerializedProperty transitionDecorations;
    SerializedProperty transitionDecoClusters;
    SerializedProperty transitionMeshDegeneracy;

    int firstWidth = 80;

    string sectionStyle = "Tooltip";


    private void OnEnable()
    {
        dd = (DungeonDecoration)target;      
        EditorUtility.SetDirty(dd);

        sObjectDecoration = new SerializedObject(target);
        mainProbability = sObjectDecoration.FindProperty("mainProbability");
        mainDecorations = sObjectDecoration.FindProperty("mainDecorations");
        mainDecoClusters = sObjectDecoration.FindProperty("mainClusters");
        mainMeshDegeneracy = sObjectDecoration.FindProperty("useDegenerateMainMeshes");

        transitionProbability = sObjectDecoration.FindProperty("transitionsProbability");
        transitionDecorations = sObjectDecoration.FindProperty("transitionsDecorations");
        transitionDecoClusters = sObjectDecoration.FindProperty("transitionsClusters");
        transitionMeshDegeneracy = sObjectDecoration.FindProperty("useDegenerateTransitionMeshes");
    }

    public override void OnInspectorGUI()
    {
        sObjectDecoration.Update();

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



        if (dd.mainClusters.Count < 1) {
            mainDecoClusters.InsertArrayElementAtIndex(0);
        }
        if (dd.transitionsClusters.Count < 1) {
            transitionDecoClusters.InsertArrayElementAtIndex(0);
        }

        firstWidth = (int)(EditorGUIUtility.currentViewWidth / 6);



        // Property Label
        EditorGUIUtility.labelWidth = 215;
        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);


        // Section 1 (Transition Decorations)
        EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("Transition Tiles Specifications", titleStyle);
            dd.useDecorationsInTransitions = EditorGUILayout.Toggle(new GUIContent("Add Decorations in Transition Tiles",
                "When enabled, this DungeonDecoration will also include decorations for the Transition Tiles."), dd.useDecorationsInTransitions);

            if (dd.useDecorationsInTransitions)
            {
                dd.excludeOverlappingDecorations = EditorGUILayout.Toggle(new GUIContent("Exclude Overlapping Decorations",
                    "When enabled, Main and Transition Decorations will not be generated on the same Tile. "), dd.excludeOverlappingDecorations);
            }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        // Section 2 (Main Decorations Segment)
        ShowDecorationList(mainProbability, mainMeshDegeneracy, mainDecorations, mainDecoClusters, "Main Decorations", titleStyle, subTitleStyle, true);


        // Section 3 (Transition Decorations Segment)
        if (dd.useDecorationsInTransitions) {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            ShowDecorationList(transitionProbability, transitionMeshDegeneracy, transitionDecorations, transitionDecoClusters, "Transition Decorations", titleStyle, subTitleStyle, false);
        }


        sObjectDecoration.ApplyModifiedProperties();
    }

    void ShowDecorationList(SerializedProperty globalProbability, SerializedProperty useDegeneracy, SerializedProperty decorationsList, SerializedProperty decoMaterialsClusters, string title, GUIStyle titleStyle, GUIStyle subTitleStyle, bool canRandomlyRotate)
    {
        // Section 1 (Decorations Probability and Size)
        EditorGUIUtility.labelWidth = 170;
        EditorGUILayout.BeginVertical(sectionStyle);

            EditorGUILayout.LabelField(title, titleStyle);
            globalProbability.floatValue = EditorGUILayout.FloatField(new GUIContent("Global Probability",
                "The probability of even trying to create a decoration."), globalProbability.floatValue);

            int listSize;
            listSize = decorationsList.arraySize;
            listSize = Mathf.Clamp(EditorGUILayout.IntField("Number of Decorations", listSize), 1, dd.maxDecoSize);

            useDegeneracy.boolValue = EditorGUILayout.Toggle(new GUIContent("Use Degenerate Meshes",
                "When enabled, ADG will also take into account the Transform of the given Prefabs at Tile generation.\nThis option is heavily resource-demanding, use it only for testing purposes."), useDegeneracy.boolValue);

            EditorGUILayout.Space();


            if (listSize != decorationsList.arraySize)
            {
                SerializedProperty segLast = decoMaterialsClusters.GetArrayElementAtIndex(decoMaterialsClusters.arraySize - 1);
                SerializedProperty clusterSize = segLast.FindPropertyRelative("clusterSize");

                while (listSize > decorationsList.arraySize)
                {
                    decorationsList.InsertArrayElementAtIndex(decorationsList.arraySize);
                    clusterSize.intValue++;
                }
                while (listSize < decorationsList.arraySize)
                {
                    decorationsList.DeleteArrayElementAtIndex(decorationsList.arraySize - 1);
                    clusterSize.intValue--;
                }
            }

            if (GUILayout.Button("Add Decoration"))
            {
                decorationsList.InsertArrayElementAtIndex(decorationsList.arraySize);
                SerializedProperty segLast = decoMaterialsClusters.GetArrayElementAtIndex(decoMaterialsClusters.arraySize - 1);
                SerializedProperty clusterSize = segLast.FindPropertyRelative("clusterSize");
                clusterSize.intValue++;
            }

            #region DebugInfo
                /*
                string name = "Clusters: " + clusters.arraySize;
                for (int i = 0; i < clusters.arraySize; i++)
                {
                    SerializedProperty bufferA = clusters.GetArrayElementAtIndex(i);
                    SerializedProperty clusterSizeA = bufferA.FindPropertyRelative("clusterSize");
                    name += ", Deco" + i + ": " + clusterSizeA.intValue;
                }
                Debug.Log(name);
                */
            #endregion

            // Section 2 (Decorations Segment)
            EditorGUILayout.BeginHorizontal();
                ShowMaterialClusterAndOptions(decoMaterialsClusters, 0, decorationsList.arraySize, subTitleStyle);
                //=================================================================================================================================
                int count = 0;
                int cumulative = 0;

                EditorGUILayout.BeginVertical();
                for (int i = 0; i < decorationsList.arraySize; i++)
                {
                    SerializedProperty decoRef = decorationsList.GetArrayElementAtIndex(i);

                    SerializedProperty decoObject = decoRef.FindPropertyRelative("decorationObject");
                    SerializedProperty parentCluster = decoRef.FindPropertyRelative("parentCluster");

                    SerializedProperty spawnProbability = decoRef.FindPropertyRelative("spawnProbability");
                    SerializedProperty deltaProbability = decoRef.FindPropertyRelative("deltaProbability");

                    SerializedProperty useRanRot = decoRef.FindPropertyRelative("useRandomRotation");
                    SerializedProperty rotation = decoRef.FindPropertyRelative("rotation");
                    SerializedProperty deltaH = decoRef.FindPropertyRelative("deltaH");

                    //=================================================================================================================================
                    EditorGUILayout.Space();

                    EditorGUIUtility.labelWidth = 120;
                    EditorGUILayout.BeginVertical(sectionStyle);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Decoration " + (i + 1), subTitleStyle);

                        //=================================================================================================================================
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            EditorGUILayout.BeginVertical();
                                EditorGUILayout.LabelField("Object", subTitleStyle, GUILayout.Width(firstWidth));
                                if (decoObject.objectReferenceValue)
                                {
                                    GUILayout.Label(AssetPreview.GetAssetPreview(decoObject.objectReferenceValue), GUILayout.Width(firstWidth), GUILayout.Height(firstWidth));
                                }
                                decoObject.objectReferenceValue = EditorGUILayout.ObjectField(decoObject.objectReferenceValue, typeof(GameObject), false, GUILayout.Width(firstWidth));
                            EditorGUILayout.EndVertical();

                            GUILayout.FlexibleSpace();

                            float prevProbability = 0.0f;
                            if (i != 0)
                            {
                                SerializedProperty antDecoRef = decorationsList.GetArrayElementAtIndex(i - 1);
                                SerializedProperty antProbability = antDecoRef.FindPropertyRelative("spawnProbability");
                                prevProbability = antProbability.floatValue;
                            }

                            EditorGUIUtility.labelWidth = 105;
                            EditorGUILayout.BeginVertical();
                                //GUILayout.FlexibleSpace();
                                spawnProbability.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Probability",
                                    "A representation of the overall probability of the DungeonDecoration."), spawnProbability.floatValue), prevProbability, 100);

                                float deltaProbabilityBuffer = 0.0f;
                                deltaProbabilityBuffer = spawnProbability.floatValue - prevProbability;
                                EditorGUILayout.LabelField(new GUIContent(string.Format("Actual Probability  ({0}%)", deltaProbabilityBuffer),
                                    "The actual probability of this item spawning is the probability of this decoration minus the previous one."));
                                deltaProbability.floatValue = deltaProbabilityBuffer;                    

                                EditorGUILayout.Space();

                                deltaH.floatValue = EditorGUILayout.FloatField(new GUIContent("Added Height",
                                    "Height to be added to this decoration."), deltaH.floatValue);

                                if (canRandomlyRotate)
                                {
                                    useRanRot.boolValue = EditorGUILayout.Toggle(new GUIContent("Randomly Rotate",
                                        "When enabled, the decoration will be randomly rotated on the Y axis once generated."), useRanRot.boolValue);
                                    if (!useRanRot.boolValue)
                                    {
                                        EditorGUI.indentLevel++;
                                        rotation.floatValue = EditorGUILayout.FloatField(new GUIContent("Y Rotation",
                                            "The rotation, in degrees, that will be set on the Y axis of this decoration once generated"), rotation.floatValue);
                                        EditorGUI.indentLevel--;
                                    }
                                }
                                
                                GUILayout.FlexibleSpace();
                            EditorGUILayout.EndVertical();

                            GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();

                    parentCluster.intValue = count;
                    if (count < decoMaterialsClusters.arraySize)
                    {
                        SerializedProperty clusterRef = decoMaterialsClusters.GetArrayElementAtIndex(count);
                        SerializedProperty clusterSize = clusterRef.FindPropertyRelative("clusterSize");
                        if (clusterSize.intValue - 1 + cumulative == i && i != decorationsList.arraySize - 1)
                        {
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                                ShowMaterialClusterAndOptions(decoMaterialsClusters, count + 1, decorationsList.arraySize, subTitleStyle);
                            EditorGUILayout.BeginVertical();

                            cumulative += clusterSize.intValue;
                            count++;
                        }
                    }
                }
                EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    void ShowMaterialClusterAndOptions(SerializedProperty decoMaterialsClusters, int index, int maxDeco, GUIStyle style)
    {
        SerializedProperty segCluster = decoMaterialsClusters.GetArrayElementAtIndex(index);
        SerializedProperty material = segCluster.FindPropertyRelative("material");
        SerializedProperty blueprint = segCluster.FindPropertyRelative("blueprint");
        SerializedProperty clusterSize = segCluster.FindPropertyRelative("clusterSize");

        EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(sectionStyle);
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(new GUIContent("Shared\nMaterial",
                    "The Material to be applied to the combination of decorations established by this cluster"), style, GUILayout.Width(firstWidth), GUILayout.Height(30));
                if (material.objectReferenceValue)
                {
                    Material buffer = (Material)material.objectReferenceValue;
                    if (buffer.GetTexture("_BaseMap") != null)
                    {
                        GUILayout.Label(AssetPreview.GetAssetPreview(buffer.GetTexture("_BaseMap")), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.MaxWidth(firstWidth), GUILayout.MaxHeight(firstWidth));
                    } else {
                        GUILayout.Label(AssetPreview.GetAssetPreview(buffer), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.MaxWidth(firstWidth), GUILayout.MaxHeight(firstWidth));
                    }
                }
                material.objectReferenceValue = (Material)EditorGUILayout.ObjectField(material.objectReferenceValue, typeof(Material), false, GUILayout.MaxWidth(firstWidth));


                EditorGUILayout.Space();
                EditorGUILayout.Space();


                EditorGUILayout.LabelField(new GUIContent("Blueprint",
                    "All components of this object will be copied into each Decoration Chunk generated by this cluster.\nThis option is heavily resource-demanding, use it only when absolutely necessary."), style, GUILayout.Width(firstWidth));
                blueprint.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(blueprint.objectReferenceValue, typeof(GameObject), false, GUILayout.MaxWidth(firstWidth));
                

                EditorGUILayout.Space();
                EditorGUILayout.Space();


                if (GUILayout.Button("Append a\nDecoration", GUILayout.MaxWidth(firstWidth)))
                {
                    if(clusterSize.intValue < maxDeco)
                    {
                        int value = 0;
                        if (index + 1 < decoMaterialsClusters.arraySize) {
                            value = 1;
                        } else if (index - 1 >= 0) {
                            value = -1;
                        }

                        clusterSize.intValue++;

                        SerializedProperty clusterEvaluated = decoMaterialsClusters.GetArrayElementAtIndex(index + value);
                        SerializedProperty clusterEvaluatedSize = clusterEvaluated.FindPropertyRelative("clusterSize");
                        clusterEvaluatedSize.intValue--;

                        if(clusterEvaluatedSize.intValue == 0)
                        {
                            decoMaterialsClusters.DeleteArrayElementAtIndex(index + value);
                        }
                    } 
                    else 
                    {
                        Debug.LogWarning("There are no Decorations left to Attach");
                    }
                }
                if (GUILayout.Button("Free a\nDecoration", GUILayout.MaxWidth(firstWidth)))
                {
                    if(clusterSize.intValue > 1)
                    {
                        if(index + 1 == decoMaterialsClusters.arraySize)
                        {
                            decoMaterialsClusters.InsertArrayElementAtIndex(index + 1);
                            SerializedProperty clusterNext = decoMaterialsClusters.GetArrayElementAtIndex(index + 1);
                            SerializedProperty clusterNextSize = clusterNext.FindPropertyRelative("clusterSize");
                            clusterNextSize.intValue = 0;
                        }
                        clusterSize.intValue--;

                        SerializedProperty buffer = decoMaterialsClusters.GetArrayElementAtIndex(index + 1);
                        SerializedProperty nextCluster = buffer.FindPropertyRelative("clusterSize");
                        nextCluster.intValue++;
                    }
                    else
                    {
                        Debug.LogWarning("This cluster has no Decorations left to Free");
                    }
                }

                GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
    }
}
