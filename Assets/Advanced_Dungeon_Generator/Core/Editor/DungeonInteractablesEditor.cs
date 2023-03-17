using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using ADG.Utilities.DungeonDecorationClases;


[CustomEditor(typeof(DungeonInteractables))]
public class DungeonInteractablesEditor : Editor
{
    DungeonInteractables di;
    SerializedObject sObjectInteractable;
    SerializedProperty interactables;
    int listSize; 


    int firstWidth = 80;

    string sectionStyle = "Tooltip";


    private void OnEnable()
    {
        di = (DungeonInteractables)target;
        EditorUtility.SetDirty(di);

        sObjectInteractable = new SerializedObject(target);
        interactables = sObjectInteractable.FindProperty("interactables");
    }

    public override void OnInspectorGUI()
    {
        sObjectInteractable.Update();

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





        // Property Label
        EditorGUIUtility.labelWidth = 215;
        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);



        
        EditorGUIUtility.labelWidth = 160;
        EditorGUILayout.BeginVertical(sectionStyle);


            EditorGUILayout.LabelField("Dungeon Interactables", titleStyle);
            di.globalProbability = EditorGUILayout.FloatField(new GUIContent("Global Probability",
                "The probability of even trying to create a decoration."), di.globalProbability);

            
            listSize = interactables.arraySize;
            listSize = Mathf.Clamp(EditorGUILayout.IntField("Number of Interactables", listSize), 1, di.maxInteractablesSize);
            
            if (listSize < 1)
                listSize = 1;

            if (listSize != interactables.arraySize)
            {
                while (listSize > interactables.arraySize)
                {
                    interactables.InsertArrayElementAtIndex(interactables.arraySize);
                }
                while (listSize < interactables.arraySize)
                {
                    interactables.DeleteArrayElementAtIndex(interactables.arraySize - 1);
                }
            }

            if (GUILayout.Button("Add Interactable"))
            {
                di.interactables.Add(new Decoration());
            }
            EditorGUILayout.Space();



        // Section 2 (Dungeon Interactables)
        EditorGUILayout.BeginVertical();
        for (int i = 0; i < interactables.arraySize; i++)
        {
            SerializedProperty interRef = interactables.GetArrayElementAtIndex(i);

            SerializedProperty interObject = interRef.FindPropertyRelative("decorationObject");

            SerializedProperty spawnProbability = interRef.FindPropertyRelative("spawnProbability");
            SerializedProperty deltaProbability = interRef.FindPropertyRelative("deltaProbability");

            SerializedProperty useRanRot = interRef.FindPropertyRelative("useRandomRotation");
            SerializedProperty rotation = interRef.FindPropertyRelative("rotation");
            SerializedProperty deltaH = interRef.FindPropertyRelative("deltaH");

            //=================================================================================================================================
            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 120;
            EditorGUILayout.BeginVertical(sectionStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Interactable " + (i + 1), subTitleStyle);

                //=================================================================================================================================
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Object", subTitleStyle, GUILayout.Width(firstWidth));
                        if (interObject.objectReferenceValue)
                        {
                            GUILayout.Label(AssetPreview.GetAssetPreview(interObject.objectReferenceValue), GUILayout.Width(firstWidth), GUILayout.Height(firstWidth));
                        }
                        interObject.objectReferenceValue = EditorGUILayout.ObjectField(interObject.objectReferenceValue, typeof(GameObject), false, GUILayout.Width(firstWidth));
                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    float prevProbability = 0.0f;
                    if (i != 0)
                    {
                        SerializedProperty antDecoRef = interactables.GetArrayElementAtIndex(i - 1);
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
                        
                        /*
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
                        */

                        GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();



        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();



        sObjectInteractable.ApplyModifiedProperties();
    }
}
