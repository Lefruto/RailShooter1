#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectDive.Editor
{
    [InitializeOnLoad]
    public static class WaterBodyBuilder
    {
        private const string InitializedKey = "ProjectDive_TeamScene_Initialized_v1";

        static WaterBodyBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool(InitializedKey, false))
                {
                    SessionState.SetBool(InitializedKey, true);
                    EnsureAllAssetsExist();
                }
            };
        }

        [MenuItem("ProjectDive/Setup Complete Team Scene (Arena + Player + Camera)", false, 0)]
        public static void MenuSetupCompleteTeamScene()
        {
            BuildCompleteTeamScene(false);
        }

        [MenuItem("ProjectDive/Setup Water Body & Test Arena in Scene", false, 1)]
        public static void MenuSetupWaterAndArena()
        {
            BuildWaterAssetsAndScene(false);
        }

        [MenuItem("ProjectDive/Create Standalone 10x10x10 Water Body in Scene", false, 2)]
        public static void MenuCreateStandaloneWaterBody()
        {
            Material waterMat = EnsureWaterMaterial();
            GameObject waterObj = CreateWaterBodyGameObject(waterMat, Vector3.zero);
            Undo.RegisterCreatedObjectUndo(waterObj, "Create Water Body");
            Selection.activeGameObject = waterObj;
            Debug.Log("[ProjectDive] Standalone 10x10x10 Water Body created in active scene!");
        }

        public static void EnsureAllAssetsExist()
        {
            EnsureDirectories();
            EnsureWaterTag();

            Material waterMat = EnsureWaterMaterial();
            Material poolTileMat = EnsurePoolTileMaterial();
            Material poolDeckMat = EnsurePoolDeckMaterial();
            Material divingBoardMat = EnsureDivingBoardMaterial();

            EnsureWaterBodyPrefab(waterMat);
            EnsureArenaPrefab(waterMat, poolTileMat, poolDeckMat, divingBoardMat);
            EnsurePlayerPrefab();
        }

        public static void BuildCompleteTeamScene(bool forceRecreate)
        {
            EnsureDirectories();
            EnsureWaterTag();

            Material waterMat = EnsureWaterMaterial();
            Material poolTileMat = EnsurePoolTileMaterial();
            Material poolDeckMat = EnsurePoolDeckMaterial();
            Material divingBoardMat = EnsureDivingBoardMaterial();

            GameObject waterPrefab = EnsureWaterBodyPrefab(waterMat);
            GameObject arenaPrefab = EnsureArenaPrefab(waterMat, poolTileMat, poolDeckMat, divingBoardMat);
            GameObject playerPrefab = EnsurePlayerPrefab();

            PopulateCompleteScene(arenaPrefab, waterPrefab, playerPrefab, forceRecreate);
        }

        public static void BuildWaterAssetsAndScene(bool forceRecreate)
        {
            BuildCompleteTeamScene(forceRecreate);
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
                AssetDatabase.CreateFolder("Assets", "Scripts");
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        private static void EnsureWaterTag()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;

            SerializedObject tagManager = new SerializedObject(assets[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            bool tagExists = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Water")
                {
                    tagExists = true;
                    break;
                }
            }

            if (!tagExists)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "Water";
                tagManager.ApplyModifiedProperties();
            }
        }

        private static Material EnsureWaterMaterial()
        {
            string path = "Assets/Materials/M_WaterBody.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");

                mat = new Material(shader);
                mat.name = "M_WaterBody";

                // URP Transparent configuration
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // 1 = Transparent
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f); // 0 = Alpha blend
                if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f); // 0 = Double-sided
                if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);

                Color waterColor = new Color(0.10f, 0.58f, 0.88f, 0.55f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", waterColor);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", waterColor);

                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);

                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = (int)RenderQueue.Transparent;

                AssetDatabase.CreateAsset(mat, path);
                AssetDatabase.SaveAssets();
            }
            return mat;
        }

        private static Material EnsurePoolTileMaterial()
        {
            string path = "Assets/Materials/M_PoolTile.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");

                mat = new Material(shader);
                mat.name = "M_PoolTile";
                Color tileColor = new Color(0.75f, 0.85f, 0.92f, 1f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tileColor);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.7f);

                AssetDatabase.CreateAsset(mat, path);
                AssetDatabase.SaveAssets();
            }
            return mat;
        }

        private static Material EnsurePoolDeckMaterial()
        {
            string path = "Assets/Materials/M_PoolDeck.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");

                mat = new Material(shader);
                mat.name = "M_PoolDeck";
                Color deckColor = new Color(0.82f, 0.80f, 0.76f, 1f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", deckColor);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.3f);

                AssetDatabase.CreateAsset(mat, path);
                AssetDatabase.SaveAssets();
            }
            return mat;
        }

        private static Material EnsureDivingBoardMaterial()
        {
            string path = "Assets/Materials/M_DivingBoard.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");

                mat = new Material(shader);
                mat.name = "M_DivingBoard";
                Color boardColor = new Color(0.95f, 0.35f, 0.15f, 1f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", boardColor);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.6f);

                AssetDatabase.CreateAsset(mat, path);
                AssetDatabase.SaveAssets();
            }
            return mat;
        }

        public static GameObject CreateWaterBodyGameObject(Material waterMat, Vector3 centerPosition)
        {
            GameObject root = new GameObject("WaterBody_10x10x10");
            root.transform.position = centerPosition;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            // Set Layer to "Water" (Layer 4)
            int waterLayer = LayerMask.NameToLayer("Water");
            root.layer = waterLayer != -1 ? waterLayer : 4;
            root.tag = "Water";

            // Add BoxCollider trigger (10x10x10 meters)
            BoxCollider col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(10f, 10f, 10f);
            col.center = Vector3.zero;

            // Add WaterVolume descriptor script
            WaterVolume volume = root.AddComponent<WaterVolume>();
            volume.dimensions = new Vector3(10f, 10f, 10f);
            volume.waterDensity = 1000f;
            volume.waterDrag = 2.5f;

            // Child 1: Water Volume Visual (Cube mesh)
            GameObject visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualCube.name = "Water_VolumeVisual";
            visualCube.transform.SetParent(root.transform, false);
            visualCube.transform.localPosition = Vector3.zero;
            visualCube.transform.localRotation = Quaternion.identity;
            visualCube.transform.localScale = new Vector3(10f, 10f, 10f);
            visualCube.layer = root.layer;

            Collider childCol = visualCube.GetComponent<Collider>();
            if (childCol != null) Object.DestroyImmediate(childCol);

            MeshRenderer mr = visualCube.GetComponent<MeshRenderer>();
            if (mr != null && waterMat != null)
            {
                mr.sharedMaterial = waterMat;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = true;
            }

            // Child 2: Water Surface Plane (Top surface at y = +5 local)
            GameObject surfacePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            surfacePlane.name = "Water_SurfacePlane";
            surfacePlane.transform.SetParent(root.transform, false);
            surfacePlane.transform.localPosition = new Vector3(0f, 5f, 0f);
            surfacePlane.transform.localRotation = Quaternion.identity;
            surfacePlane.transform.localScale = new Vector3(1f, 1f, 1f); // 10x10 meters
            surfacePlane.layer = root.layer;

            Collider planeCol = surfacePlane.GetComponent<Collider>();
            if (planeCol != null) Object.DestroyImmediate(planeCol);

            MeshRenderer planeMr = surfacePlane.GetComponent<MeshRenderer>();
            if (planeMr != null && waterMat != null)
            {
                planeMr.sharedMaterial = waterMat;
                planeMr.shadowCastingMode = ShadowCastingMode.Off;
                planeMr.receiveShadows = true;
            }

            return root;
        }

        public static GameObject EnsureWaterBodyPrefab(Material waterMat)
        {
            string path = "Assets/Prefabs/WaterBody.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject temp = CreateWaterBodyGameObject(waterMat, Vector3.zero);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
            Object.DestroyImmediate(temp);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        public static GameObject EnsureArenaPrefab(Material waterMat, Material tileMat, Material deckMat, Material boardMat)
        {
            string path = "Assets/Prefabs/WaterTestArena.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject root = new GameObject("WaterTestArena");

            // Water Body inside Arena:
            // Top surface at y = 0, depth down to y = -10. Center is at (0, -5, 0).
            GameObject waterInstance = CreateWaterBodyGameObject(waterMat, Vector3.zero);
            waterInstance.name = "WaterBody_10x10x10";
            waterInstance.transform.SetParent(root.transform, false);
            waterInstance.transform.localPosition = new Vector3(0f, -5f, 0f);

            // Pool Basin (surrounds 10x10x10 interior):
            GameObject basin = new GameObject("PoolBasin");
            basin.transform.SetParent(root.transform, false);

            // Floor: 12x12x1m slab at y = -10.5
            CreateSolidBox("PoolFloor", basin.transform, new Vector3(0f, -10.5f, 0f), new Vector3(12f, 1f, 12f), tileMat);

            // 4 Walls: interior is 10x10, depth is 10 (from y = -10 to y = 0), thickness is 1m
            CreateSolidBox("Wall_North", basin.transform, new Vector3(0f, -5f, 5.5f), new Vector3(12f, 10f, 1f), tileMat);
            CreateSolidBox("Wall_South", basin.transform, new Vector3(0f, -5f, -5.5f), new Vector3(12f, 10f, 1f), tileMat);
            CreateSolidBox("Wall_East", basin.transform, new Vector3(5.5f, -5f, 0f), new Vector3(1f, 10f, 10f), tileMat);
            CreateSolidBox("Wall_West", basin.transform, new Vector3(-5.5f, -5f, 0f), new Vector3(1f, 10f, 10f), tileMat);

            // Deck / Walkway surrounding the pool at y = 0:
            GameObject deck = new GameObject("PoolDeck");
            deck.transform.SetParent(root.transform, false);

            CreateSolidBox("Deck_North", deck.transform, new Vector3(0f, -0.25f, 7.5f), new Vector3(18f, 0.5f, 3f), deckMat);
            CreateSolidBox("Deck_South", deck.transform, new Vector3(0f, -0.25f, -7.5f), new Vector3(18f, 0.5f, 3f), deckMat);
            CreateSolidBox("Deck_East", deck.transform, new Vector3(7.5f, -0.25f, 0f), new Vector3(3f, 0.5f, 12f), deckMat);
            CreateSolidBox("Deck_West", deck.transform, new Vector3(-7.5f, -0.25f, 0f), new Vector3(3f, 0.5f, 12f), deckMat);

            // Diving Board / Jump Platform:
            GameObject divingSetup = new GameObject("DivingPlatform");
            divingSetup.transform.SetParent(root.transform, false);
            CreateSolidBox("DivingStand", divingSetup.transform, new Vector3(0f, 1.25f, -6.5f), new Vector3(1.2f, 2.5f, 1f), deckMat);
            CreateSolidBox("DivingBoard", divingSetup.transform, new Vector3(0f, 2.55f, -4.5f), new Vector3(1.2f, 0.1f, 4f), boardMat);

            // Pool Stairs / Exit Ramp on North wall:
            GameObject ladderRamp = new GameObject("PoolExitRamp");
            ladderRamp.transform.SetParent(root.transform, false);
            for (int i = 0; i < 5; i++)
            {
                float stepHeight = 2.0f;
                float stepY = -10f + (i * stepHeight) + (stepHeight * 0.5f);
                float stepZ = 4.8f - (i * 0.2f);
                CreateSolidBox($"Step_{i + 1}", ladderRamp.transform, new Vector3(4f, stepY, stepZ), new Vector3(1.5f, stepHeight, 0.4f), tileMat);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        public static GameObject EnsurePlayerPrefab()
        {
            string path = "Assets/Prefabs/Player.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            string modelPath = "Assets/Kevin Iglesias/Human Animations/Models/HumanM_Model.fbx";
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset == null)
            {
                Debug.LogWarning("[ProjectDive] Could not find HumanM_Model at " + modelPath);
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            instance.name = "Player";

            // Configure PlayerMovement
            PlayerMovement pm = instance.GetComponent<PlayerMovement>();
            if (pm == null) pm = instance.AddComponent<PlayerMovement>();

            pm.moveForce = 14f;
            pm.maxSpeed = 3.2f;
            pm.turnSpeed = 10f;
            pm.jumpForce = 5.5f;
            pm.groundDrag = 2f;

            pm.waterMoveForce = 8f;
            pm.waterMaxSpeed = 2.0f;
            pm.waterDrag = 2.5f;

            pm.sinkForce = 5.0f;
            pm.maxSinkSpeed = 1.8f;
            pm.swimUpForce = 12f;
            pm.maxSwimSpeed = 2.2f;
            pm.diveForce = 8f;
            pm.maxDiveSpeed = 3.5f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectDive] Created Player prefab at " + path);
            return prefab;
        }

        private static GameObject CreateSolidBox(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;

            if (mat != null)
            {
                MeshRenderer mr = cube.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = mat;
            }
            return cube;
        }

        private static void PopulateCompleteScene(GameObject arenaPrefab, GameObject waterPrefab, GameObject playerPrefab, bool forceRecreate)
        {
            string scenePath = "Assets/Scenes/SampleScene.unity";
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != scenePath)
            {
                activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            // 1. Water Test Arena
            GameObject existingArena = GameObject.Find("WaterTestArena");
            if (existingArena != null && forceRecreate)
            {
                Object.DestroyImmediate(existingArena);
                existingArena = null;
            }

            if (existingArena == null && arenaPrefab != null)
            {
                existingArena = (GameObject)PrefabUtility.InstantiatePrefab(arenaPrefab);
                existingArena.name = "WaterTestArena";
                existingArena.transform.position = Vector3.zero;
                existingArena.transform.rotation = Quaternion.identity;
                Undo.RegisterCreatedObjectUndo(existingArena, "Instantiate WaterTestArena");
            }
            else if (existingArena != null)
            {
                existingArena.transform.position = Vector3.zero;
            }

            // 2. Player Character
            GameObject existingPlayer = GameObject.Find("Player");
            if (existingPlayer == null) existingPlayer = GameObject.Find("HumanM_Model");

            if (existingPlayer != null && forceRecreate)
            {
                Object.DestroyImmediate(existingPlayer);
                existingPlayer = null;
            }

            if (existingPlayer == null)
            {
                if (playerPrefab != null)
                {
                    existingPlayer = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                }
                else
                {
                    string modelPath = "Assets/Kevin Iglesias/Human Animations/Models/HumanM_Model.fbx";
                    GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                    if (modelAsset != null)
                    {
                        existingPlayer = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
                    }
                }

                if (existingPlayer != null)
                {
                    existingPlayer.name = "HumanM_Model";
                    // Position on the west pool deck
                    existingPlayer.transform.position = new Vector3(-7.5f, 0.05f, 0f);
                    existingPlayer.transform.rotation = Quaternion.identity;
                    Undo.RegisterCreatedObjectUndo(existingPlayer, "Instantiate Player");
                }
            }

            // Ensure PlayerMovement is configured with tuned speed and sinking values
            if (existingPlayer != null)
            {
                PlayerMovement pm = existingPlayer.GetComponent<PlayerMovement>();
                if (pm == null) pm = existingPlayer.AddComponent<PlayerMovement>();

                pm.moveForce = 14f;
                pm.maxSpeed = 3.2f;
                pm.turnSpeed = 10f;
                pm.jumpForce = 5.5f;
                pm.groundDrag = 2f;

                pm.waterMoveForce = 8f;
                pm.waterMaxSpeed = 2.0f;
                pm.waterDrag = 2.5f;

                pm.sinkForce = 5.0f;
                pm.maxSinkSpeed = 1.8f;
                pm.swimUpForce = 12f;
                pm.maxSwimSpeed = 2.2f;
                pm.diveForce = 8f;
                pm.maxDiveSpeed = 3.5f;
            }

            // 3. Camera Setup
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0f, 6f, -14f);
                mainCam.transform.rotation = Quaternion.Euler(22f, 0f, 0f);

                var followCam = mainCam.GetComponent<ProjectDive.SmoothFollowCamera>();
                if (followCam == null) followCam = mainCam.gameObject.AddComponent<ProjectDive.SmoothFollowCamera>();

                if (existingPlayer != null)
                {
                    followCam.target = existingPlayer.transform;
                }
                followCam.offset = new Vector3(0f, 3.5f, -6f);
                followCam.positionDamping = 6f;
                followCam.rotationDamping = 10f;
            }

            if (existingPlayer != null)
            {
                Selection.activeGameObject = existingPlayer;
            }

            activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            // Also save a dedicated team scene copy as Assets/Scenes/WaterDiveScene.unity
            string dedicatedScenePath = "Assets/Scenes/WaterDiveScene.unity";
            EditorSceneManager.SaveScene(activeScene, dedicatedScenePath, true);
            AssetDatabase.SaveAssets();

            Debug.Log("[ProjectDive] Complete Team Scene assembled in: " + activeScene.name + " and saved copy to " + dedicatedScenePath);
        }
    }
}
#endif
