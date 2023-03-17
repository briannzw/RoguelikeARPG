using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonBrush))]
[CanEditMultipleObjects]
public class DungeonBrushEditor : Editor
{
    DungeonBrush db;
    int smallImageSize = 75;
    int bigImageSize = 100;

    string sectionStyle = "Tooltip";
    string containerStyle = "HelpBox";

    public string[] tilingType = new string[] { "Standard", "Radial", "Combined" };
    public string[][] transitionType = new string[][]
    {
        new string[]{ "Complete", "None", "Partial"},
        new string[]{ "Complete" },
        new string[]{ "Complete" }
    };

    private void OnEnable()
    {
        db = (DungeonBrush)target;      
        EditorUtility.SetDirty(db);

        foreach (GameObject go in db.transitionTiles)
        {
            if (go != null)
                EditorUtility.SetDirty(go);
        }
        if (db.mainTile != null)
            EditorUtility.SetDirty(db.mainTile);
        if (db.mainMaterial != null)
            EditorUtility.SetDirty(db.mainMaterial);
    }

    public override void OnInspectorGUI()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.textField) {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        GUIStyle subTitleStyle = new GUIStyle(GUI.skin.label) {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        smallImageSize = (int)(EditorGUIUtility.currentViewWidth / 7);
        bigImageSize = (int)(EditorGUIUtility.currentViewWidth / 4.5);



        // Property Label
        EditorGUIUtility.labelWidth = 140;
        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);


        // Section 1 (Tiling Composition)
        EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("Brush Tiling Composition", titleStyle);
            db.tileComposition = EditorGUILayout.Popup(new GUIContent("Tiling Type", 
                "Specifies the type of Tiling the DungeonBrush tiles will have"), db.tileComposition, tilingType);
            db.transitionTileComposition = EditorGUILayout.Popup(new GUIContent("Transition Tiling", 
                "Specifies what type of Transition the DungeonBrush tiles will have"), db.transitionTileComposition, transitionType[db.tileComposition]);

            if(db.transitionTileComposition >= transitionType[db.tileComposition].Length)
            {
                db.transitionTileComposition = 0;
            }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        // Section 2 (Main Tile)
        if (db.tileComposition == 0 || db.tileComposition == 2)
        {
            EditorGUILayout.BeginVertical(sectionStyle);
                EditorGUILayout.LabelField("Main Tile", titleStyle);
                EditorGUILayout.LabelField("Mesh", subTitleStyle);

                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.BeginVertical(containerStyle);
                        if (db.mainTile != null)
                        {
                            GUILayout.Label(AssetPreview.GetAssetPreview(db.mainTile), GUILayout.Width(bigImageSize), GUILayout.Height(bigImageSize));
                        }
                        db.mainTile = (GameObject)EditorGUILayout.ObjectField(db.mainTile, typeof(GameObject), false, GUILayout.Width(bigImageSize));
                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                

                EditorGUIUtility.labelWidth = 185;

                db.useDegenerateMainMeshes = EditorGUILayout.Toggle(new GUIContent("Use Degenerate Meshes",
                    "When enabled, ADG will also take into account the Transform of the given Prefabs at Tile generation.\nThis option is heavily resource-demanding, use it only for testing purposes."), db.useDegenerateMainMeshes);


                EditorGUILayout.Space();
                EditorGUILayout.Space();


                EditorGUILayout.LabelField("Extra Specifications", subTitleStyle);
                db.mainTileAddedHeight = EditorGUILayout.FloatField(new GUIContent("Main Tile Added Height", 
                    "Extra height to be added to the Main tiles."), db.mainTileAddedHeight);
                db.mainBlueprint = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Chunk Blueprint",
                    "All components of this object will be copied into each Main Tile Chunk.\nThis option is heavily resource-demanding, use it only when absolutely necessary."), db.mainBlueprint, typeof(GameObject), false);
                db.applyMeshColliderToMainTile = EditorGUILayout.Toggle(new GUIContent("Apply Mesh Collider at Runtime",
                    "When enabled, a Mesh Collider will be applied to each Main Tile Chunk once generated.\nIf the given blueprint already has a Mesh Collider, this option will be ignored."), db.applyMeshColliderToMainTile);

                if (db.applyMeshColliderToMainTile)
                {
                    EditorGUI.indentLevel++;
                        db.mainTileLayer = EditorGUILayout.LayerField(new GUIContent("Main Tile Layer", 
                            "Layer to be applied to the Main tiles"), db.mainTileLayer);
                    EditorGUI.indentLevel--;
                }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }


        // Section 3 (Transition Tiles)
        if (db.transitionTileComposition != 1 || db.tileComposition == 1)
        {
            EditorGUILayout.BeginVertical(sectionStyle);
                EditorGUILayout.LabelField("Transition Tiles", titleStyle);
                EditorGUILayout.LabelField("Meshes", subTitleStyle);

                EditorGUILayout.BeginHorizontal();
                    if(db.transitionTileComposition != 2)
                    {
                        db.transitionTiles[0] = DrawAndGetObject("Fill", db.transitionTiles[0], smallImageSize);
                    }
                    db.transitionTiles[1] = DrawAndGetObject("Horizontal", db.transitionTiles[1], smallImageSize);
                    db.transitionTiles[2] = DrawAndGetObject("Vertical"  , db.transitionTiles[2], smallImageSize);
                    db.transitionTiles[3] = DrawAndGetObject("Corner"    , db.transitionTiles[3], smallImageSize);
                    db.transitionTiles[4] = DrawAndGetObject("Round"     , db.transitionTiles[4], smallImageSize);
                EditorGUILayout.EndHorizontal();


                EditorGUIUtility.labelWidth = 185;

                db.useDegenerateTransitionMeshes = EditorGUILayout.Toggle(new GUIContent("Use Degenerate Meshes",
                    "When enabled, ADG will also take into account the Transform of the given Prefabs at Tile generation.\nThis option is heavily resource-demanding, use it only for testing purposes."), db.useDegenerateTransitionMeshes);


                EditorGUILayout.Space();
                EditorGUILayout.Space();


                EditorGUILayout.LabelField("Extra Specifications", subTitleStyle);
                db.transitionTilesAddedHeight = EditorGUILayout.FloatField(new GUIContent("Transition Tiles Added Height", 
                    "Extra height to be added to the Transition tiles."), db.transitionTilesAddedHeight);
                db.transitionBlueprint = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Chunk Blueprint",
                    "All components of this object will be copied into each Transition Tile Chunk.\nThis option is heavily resource-demanding, use it only when absolutely necessary."), db.transitionBlueprint, typeof(GameObject), false);
                db.applyMeshColliderToTransitionTiles = EditorGUILayout.Toggle(new GUIContent("Apply Mesh Collider at Runtime",
                    "When enabled, a Mesh Collider will be applied to each Transition Tile Chunk once generated.\nIf the given blueprint already has a Mesh Collider, this option will be ignored."), db.applyMeshColliderToTransitionTiles);

                if (db.applyMeshColliderToTransitionTiles)
                {
                    EditorGUI.indentLevel++;
                        db.transitionTilesLayer = EditorGUILayout.LayerField(new GUIContent("Transition Tiles Layer", 
                            "Layer to be applied to the Transition tiles."), db.transitionTilesLayer);
                    EditorGUI.indentLevel--;
                }
                db.ignoreVoidTiles = EditorGUILayout.Toggle(new GUIContent("Ignore Void Tiles",
                    "When enabled, the generated chuncks will not make transitions with Empty Tiles.\nActivate this option if you find artifacts around the edges of your generated rooms."), db.ignoreVoidTiles);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }


        // Section 4 (Texture Atlas)
        EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("Texture Atlas", titleStyle);
        EditorGUILayout.LabelField("Materials", subTitleStyle);


        EditorGUIUtility.labelWidth = 140;
        db.useCombinedAtlas = EditorGUILayout.Toggle(new GUIContent("Use combined Atlas",
            "When enabled, both sections will use the same material"), db.useCombinedAtlas);

            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                
                if (!db.useCombinedAtlas)
                {
                    
                    db.extraMaterial = DrawAndGetMaterial("Main Tile Material", db.extraMaterial, bigImageSize);
                    GUILayout.FlexibleSpace();
                }
                string name = (db.useCombinedAtlas) ? "Main Material" : "Transitions Material";
                db.mainMaterial = DrawAndGetMaterial(name, db.mainMaterial, bigImageSize);

                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }



    GameObject DrawAndGetObject(string name, GameObject mainObject, int size)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperCenter,
            fixedWidth = size
        };

        EditorGUILayout.BeginHorizontal(containerStyle);
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(name, style, GUILayout.Width(size));
                if (mainObject != null)
                {
                    GUILayout.Label(AssetPreview.GetAssetPreview(mainObject), GUILayout.Width(size), GUILayout.Height(size));
                }
                mainObject = (GameObject)EditorGUILayout.ObjectField(mainObject, typeof(GameObject), false, GUILayout.Width(size));
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        return mainObject;
    }

    Material DrawAndGetMaterial(string name, Material material, int size)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperCenter,
            fixedWidth = size
        };

        
        EditorGUILayout.BeginVertical(containerStyle);
            EditorGUILayout.LabelField(name, style, GUILayout.Width(size));
            if (material != null)
            {
                if(material.GetTexture("_BaseMap") != null)
                {
                    GUILayout.Label(AssetPreview.GetAssetPreview(db.mainMaterial.GetTexture("_BaseMap")), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(size), GUILayout.Height(size));//, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.MaxWidth(bigImageSize), GUILayout.MaxHeight(bigImageSize));
                } else {
                    GUILayout.Label(AssetPreview.GetAssetPreview(material), GUILayout.Width(size), GUILayout.Height(size));
                }
            }
            material = (Material)EditorGUILayout.ObjectField(material, typeof(Material), false, GUILayout.Width(size));
        EditorGUILayout.EndVertical();

        return material;
    }
}
