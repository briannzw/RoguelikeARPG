using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    DungeonGenerator dg;

    string sectionStyle = "Tooltip";

    private void OnEnable()
    {
        dg = (DungeonGenerator)target;
        EditorUtility.SetDirty(dg);
    }

    public override void OnInspectorGUI()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.textField)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };



        // Section 1 (Dungeon)
        EditorGUIUtility.labelWidth = 120;
        EditorGUILayout.BeginVertical(sectionStyle);
        EditorGUILayout.LabelField("Dungeon Generation", titleStyle);
            dg.dungeon = ShowGetAndValidateObject("Dungeon", dg.dungeon);
            EditorGUILayout.Space();


        // Section 2 (Loader)
        dg.usePrefabLoader = !EditorGUILayout.Toggle(new GUIContent("Use in build Loader",
            "If checked, the dungeon generator will create the standard loaders of the dungeon on its own, otherwise the user will specify the loader to use"), !dg.usePrefabLoader);

        EditorGUI.indentLevel++;
            if (!dg.usePrefabLoader) 
            { 
                dg.loaderLayer = EditorGUILayout.LayerField(new GUIContent("Loader Layer",
                "The layer that will be assigned to all Dungeon Loaders"), dg.loaderLayer);
            }
            else
            {
                dg.loader = ShowGetAndValidateObject("Prefab Loader", dg.loader);

                EditorGUIUtility.labelWidth = 180;
                dg.roomsToLoadAtStart = EditorGUILayout.IntField(new GUIContent("Rooms to Load at the Start",
                    "The number of rooms that will be loaded at the start of the dungeon creation (2 by default)"), dg.roomsToLoadAtStart);
                dg.roomsLoadOffset = EditorGUILayout.IntField(new GUIContent("Rooms Loading Offset", 
                    "When the loader triggers, the room to load will be the number of the current one, plus this number (1 by default)"), dg.roomsLoadOffset);
                dg.destroyFarBackRooms = EditorGUILayout.Toggle(new GUIContent("Destroy Far Back Rooms",
                    "Rooms that are too far back will be destroyed (True by default)"), dg.destroyFarBackRooms);
                if (dg.destroyFarBackRooms)
                {
                    EditorGUI.indentLevel++;
                        dg.roomDestructionOffset = EditorGUILayout.IntField(new GUIContent("Room Destruction Offset",
                        "When the loader triggers, the room to destroy will be the current one minus the Room Loading Offset and this number (2 by default)"), dg.roomDestructionOffset);
                    EditorGUI.indentLevel--;
                }
            }
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();


        // Section 3 (Randomness)
        EditorGUIUtility.labelWidth = 120;
        dg.useRandomSeed = EditorGUILayout.Toggle("Use Random Seed", dg.useRandomSeed);
        if (!dg.useRandomSeed)
        {
            EditorGUI.indentLevel++;
            dg.seed = EditorGUILayout.IntField("Dungeon Seed", dg.seed);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();


        // Section 4 (Start)
        dg.generateDungeonAtStart = EditorGUILayout.Toggle("Generate at Start", dg.generateDungeonAtStart);
        if (!dg.generateDungeonAtStart)
        {
            if(GUILayout.Button("Create Dungeon"))
            {
                dg.DungeonStartTrigger();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();


        // Section 5 (Debug Options)
        dg.showDebugOptions = EditorGUILayout.Foldout(dg.showDebugOptions, "Dungeon Debug Options", true);
        if (dg.showDebugOptions)
        {
            if (dg.dungeon)
            {
                EditorGUILayout.LabelField("Room Loading", EditorStyles.boldLabel);

                dg.canGenerateSites = EditorGUILayout.Toggle(new GUIContent("Use Debug Loading",
                    "Gives you the option to load a specific room from the dungeon"), dg.canGenerateSites);

                if (dg.canGenerateSites)
                {
                    EditorGUI.indentLevel++;
                    dg.siteToCharge = Mathf.Clamp(EditorGUILayout.IntField("Room to Load", dg.siteToCharge), 0, dg.dungeonFragmentsAmount - 1);
                    if (GUILayout.Button("Create Room"))
                    {
                        dg.DebugGenerateDungeonFragment();
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUIUtility.labelWidth = 165;
                EditorGUILayout.LabelField("Rooms Information", EditorStyles.boldLabel);
                base.OnInspectorGUI();
            }
        }
    }

    Dungeon ShowGetAndValidateObject(string name, Dungeon dungeon)
    {
        dungeon = (Dungeon)EditorGUILayout.ObjectField(name, dungeon, typeof(Dungeon), false);
        if (GUI.changed)
        {
            if (dungeon)
            {
                if (!ADG.Tools.SecurityCheck.ValidateDungeonComponent(dungeon))
                {
                    dungeon = null;
                }
            }
        }

        return dungeon;
    }

    GameObject ShowGetAndValidateObject(string name, GameObject loader)
    {
        loader = (GameObject)EditorGUILayout.ObjectField(name, loader, typeof(GameObject), false);
        if (GUI.changed)
        {
            if (loader)
            {
                if (loader.GetComponent<DungeonLoader>() == null)
                {
                    Debug.LogError("The selected Dungeon Loader cannot be accepted (It does not have a DungeonLoader component attached)");
                    loader = null;
                }
            }
        }

        return loader;
    }
}
