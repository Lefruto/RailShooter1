using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

[InitializeOnLoad]
public static class SetupHomeworkScene
{
    static SetupHomeworkScene()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool("HomeworkSetupRunOnce", false))
            {
                SessionState.SetBool("HomeworkSetupRunOnce", true);
                SetupComplete();
            }
        };
    }

    [MenuItem("Tools/Setup Homework Complete")]
    public static void SetupComplete()
    {
        Debug.Log(">>> Starting Complete Homework Setup for Ali (Rail Shooter) <<<");

        // 1. Import TMP Essential Resources if missing
        ImportTMPEssentials();

        // 2. Ensure Prefabs directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // 3. Create PowerUp Prefab
        GameObject powerUpPrefab = SetupPowerUpPrefab();

        // 4. Create Scout Enemy Prefab (Enemy Type 2)
        GameObject scoutPrefab = SetupScoutEnemyPrefab(powerUpPrefab);

        // 5. Update Heavy Enemy Prefab (Enemy Type 1)
        GameObject heavyPrefab = SetupHeavyEnemyPrefab(powerUpPrefab);

        // 6. Setup Scene GameObjects & Canvas HUD
        SetupSceneObjects(heavyPrefab, scoutPrefab);

        // Save Scene and Assets
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(">>> Homework Setup Successfully Completed! Everything is configured and saved. <<<");
    }

    private static void ImportTMPEssentials()
    {
        string[] packages = System.IO.Directory.GetFiles("Library/PackageCache", "TMP Essential Resources.unitypackage", System.IO.SearchOption.AllDirectories);
        if (packages.Length > 0)
        {
            AssetDatabase.ImportPackage(packages[0], false);
            Debug.Log("Imported TMP Essential Resources from package cache.");
        }
    }

    private static GameObject SetupPowerUpPrefab()
    {
        string path = "Assets/Prefabs/PowerUp.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "PowerUp";
        go.transform.localScale = Vector3.one * 2.5f;

        SphereCollider sc = go.GetComponent<SphereCollider>();
        sc.isTrigger = true;

        PowerUp pu = go.AddComponent<PowerUp>();

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = new Color(0f, 1f, 1f, 1f);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0f, 1f, 1f, 1f));
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0f, 0.8f, 1f) * 2f);
        }
        AssetDatabase.CreateAsset(mat, "Assets/Prefabs/PowerUp_Mat.mat");
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject SetupScoutEnemyPrefab(GameObject powerUpPrefab)
    {
        string path = "Assets/Prefabs/ScoutEnemy.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        string modelPath = "Assets/thirdpartyassets/Imported Assets/Spaceships/Models/Fighter_01_Neutral.fbx";
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (model == null)
        {
            modelPath = "Assets/thirdpartyassets/Imported Assets/Spaceships/Models/Fighter_08_Red.fbx";
            model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        }

        GameObject go;
        if (model != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(model);
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        go.name = "ScoutEnemy";
        go.transform.localScale = Vector3.one * 1.5f;

        BoxCollider box = go.GetComponent<BoxCollider>();
        if (box == null) box = go.AddComponent<BoxCollider>();
        box.isTrigger = false;
        box.size = new Vector3(4f, 2f, 4f);

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy == null) enemy = go.AddComponent<Enemy>();
        enemy.ConfigureEnemy(EnemyType.FastScout, 10, 15, 18f, true);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject SetupHeavyEnemyPrefab(GameObject powerUpPrefab)
    {
        string path = "Assets/Prefabs/Enemy.prefab";
        GameObject heavy = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (heavy != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(heavy);
            Enemy enemy = instance.GetComponent<Enemy>();
            if (enemy == null) enemy = instance.AddComponent<Enemy>();
            enemy.ConfigureEnemy(EnemyType.HeavyDropship, 30, 35, 10f, false);
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }
        return heavy;
    }

    private static void SetupSceneObjects(GameObject heavyPrefab, GameObject scoutPrefab)
    {
        // 1. AudioManager
        GameObject audioMgr = GameObject.Find("AudioManager");
        if (audioMgr == null)
        {
            audioMgr = new GameObject("AudioManager");
            audioMgr.AddComponent<AudioManager>();
        }

        // 2. GameSceneManager
        GameObject sceneMgr = GameObject.Find("Game Scene Manager") ?? GameObject.Find("GameSceneManager");
        if (sceneMgr == null)
        {
            sceneMgr = new GameObject("Game Scene Manager");
        }
        if (sceneMgr.GetComponent<GameSceneManager>() == null)
        {
            sceneMgr.AddComponent<GameSceneManager>();
        }

        // 3. Canvas & Hierarchy HUD Elements
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        Scoreboard scoreboard = canvas.GetComponent<Scoreboard>();
        if (scoreboard == null)
        {
            scoreboard = canvas.gameObject.AddComponent<Scoreboard>();
        }

        // Build ScoreText child
        Transform scoreTransform = canvas.transform.Find("ScoreText");
        TextMeshProUGUI scoreTmp;
        if (scoreTransform == null)
        {
            GameObject scoreGo = new GameObject("ScoreText");
            scoreGo.layer = 5; // UI Layer
            scoreGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = scoreGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(30f, -30f);
            rt.sizeDelta = new Vector2(300f, 50f);

            scoreTmp = scoreGo.AddComponent<TextMeshProUGUI>();
            scoreTmp.fontSize = 28;
            scoreTmp.fontStyle = FontStyles.Bold;
            scoreTmp.color = new Color(0.2f, 1f, 0.45f, 1f);
            scoreTmp.text = "SCORE: 0000";
        }
        else
        {
            scoreTmp = scoreTransform.GetComponent<TextMeshProUGUI>();
        }

        // Build LivesText child
        Transform livesTransform = canvas.transform.Find("LivesText");
        TextMeshProUGUI livesTmp;
        if (livesTransform == null)
        {
            GameObject livesGo = new GameObject("LivesText");
            livesGo.layer = 5; // UI Layer
            livesGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = livesGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-30f, -30f);
            rt.sizeDelta = new Vector2(300f, 50f);

            livesTmp = livesGo.AddComponent<TextMeshProUGUI>();
            livesTmp.fontSize = 28;
            livesTmp.fontStyle = FontStyles.Bold;
            livesTmp.alignment = TextAlignmentOptions.TopRight;
            livesTmp.color = new Color(1f, 0.35f, 0.35f, 1f);
            livesTmp.text = "LIVES: 3 ★ ★ ★";
        }
        else
        {
            livesTmp = livesTransform.GetComponent<TextMeshProUGUI>();
        }

        // Assign to Scoreboard SerializedObject
        SerializedObject sbSo = new SerializedObject(scoreboard);
        SerializedProperty scoreProp = sbSo.FindProperty("scoreboardText");
        SerializedProperty livesProp = sbSo.FindProperty("livesText");
        if (scoreProp != null && scoreTmp != null) scoreProp.objectReferenceValue = scoreTmp;
        if (livesProp != null && livesTmp != null) livesProp.objectReferenceValue = livesTmp;
        sbSo.ApplyModifiedProperties();

        // 4. Enemy Wave Spawner on "Enemy Maneger"
        GameObject enemyManager = GameObject.Find("Enemy Maneger") ?? GameObject.Find("Enemy Manager");
        if (enemyManager == null)
        {
            enemyManager = new GameObject("Enemy Maneger");
        }

        WaveSpawner spawner = enemyManager.GetComponent<WaveSpawner>();
        if (spawner == null)
        {
            spawner = enemyManager.AddComponent<WaveSpawner>();
        }

        SerializedObject spawnerSo = new SerializedObject(spawner);
        SerializedProperty heavyProp = spawnerSo.FindProperty("heavyEnemyPrefab");
        SerializedProperty scoutProp = spawnerSo.FindProperty("scoutEnemyPrefab");
        if (heavyProp != null && heavyPrefab != null) heavyProp.objectReferenceValue = heavyPrefab;
        if (scoutProp != null && scoutPrefab != null) scoutProp.objectReferenceValue = scoutPrefab;
        spawnerSo.ApplyModifiedProperties();
    }

    [MenuItem("Tools/Build Windows Standalone")]
    public static void BuildWindowsPlayer()
    {
        Debug.Log(">>> Starting Standalone Windows Build for RailShooter_Ali_Build... <<<");
        string buildFolder = "Build/RailShooter_Ali_Build";
        string buildExe = buildFolder + "/RailShooter.exe";
        System.IO.Directory.CreateDirectory(buildFolder);

        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = buildExe;
        options.target = BuildTarget.StandaloneWindows64;
        options.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log(">>> Build Completed with Result: " + report.summary.result + " <<<");
    }
}
