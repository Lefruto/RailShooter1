using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public enum WaveFormation
{
    HorizontalLine,
    VFormation,
    VerticalColumn,
    StaggeredPair
}

[System.Serializable]
public struct WaveDefinition
{
    public string waveName;
    public GameObject enemyPrefab;
    public int enemyCount;
    public float triggerTime;       // Timeline elapsed time to trigger wave
    public float burstInterval;     // Coroutine interval between enemy spawns
    public WaveFormation formation; // Pattern calculated via for-loop
    public float formationSpacing;
    public float enemySpeed;
    public bool weaveMotion;
}

/// <summary>
/// Manages enemy wave progression synchronized with MasterTimeline.
/// Uses arrays and for-loops for formation calculations,
/// and Coroutines for wave timing and burst spawns as specified in homework.
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Sequence")]
    [SerializeField] private WaveDefinition[] waves;

    [Header("Timeline & Player Tracking")]
    [SerializeField] private PlayableDirector masterTimeline;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float forwardSpawnDistance = 120f;

    [Header("Default Enemy Fallbacks")]
    [SerializeField] private GameObject heavyEnemyPrefab;
    [SerializeField] private GameObject scoutEnemyPrefab;

    private bool[] waveTriggered;
    private bool levelFinished = false;

    void Awake()
    {
        if (masterTimeline == null)
        {
            GameObject tlGo = GameObject.Find("MasterTimeline");
            if (tlGo != null) masterTimeline = tlGo.GetComponent<PlayableDirector>();
        }

        if (playerTransform == null)
        {
            GameObject playerGo = GameObject.Find("PlayerRig");
            if (playerGo != null) playerTransform = playerGo.transform;
        }

        // Initialize wave configurations if array is empty
        if (waves == null || waves.Length == 0)
        {
            SetupDefaultWaves();
        }

        waveTriggered = new bool[waves.Length];
    }

    void Update()
    {
        if (levelFinished) return;

        float currentTime = (masterTimeline != null && masterTimeline.playableGraph.IsValid())
            ? (float)masterTimeline.time
            : Time.timeSinceLevelLoad;

        // Check each wave trigger time using for-loop over array
        for (int i = 0; i < waves.Length; i++)
        {
            if (!waveTriggered[i] && currentTime >= waves[i].triggerTime)
            {
                waveTriggered[i] = true;
                StartCoroutine(SpawnWaveRoutine(waves[i]));
            }
        }

        // Check level win condition (end of timeline)
        if (masterTimeline != null && masterTimeline.playableGraph.IsValid())
        {
            if (currentTime >= masterTimeline.duration - 1.5f && !levelFinished)
            {
                levelFinished = true;
                if (GameSceneManager.Instance != null)
                {
                    GameSceneManager.Instance.TriggerVictory();
                }
            }
        }
    }

    /// <summary>
    /// Coroutine for wave execution and burst interval timing.
    /// </summary>
    private IEnumerator SpawnWaveRoutine(WaveDefinition wave)
    {
        GameObject prefabToSpawn = wave.enemyPrefab;
        if (prefabToSpawn == null)
        {
            prefabToSpawn = (wave.enemyCount > 4) ? scoutEnemyPrefab : heavyEnemyPrefab;
        }

        if (prefabToSpawn == null)
        {
            // Fallback: search for existing Enemy prefab
            prefabToSpawn = Resources.Load<GameObject>("Enemy");
            if (prefabToSpawn == null)
            {
                Enemy existing = FindFirstObjectByType<Enemy>();
                if (existing != null) prefabToSpawn = existing.gameObject;
            }
        }

        if (prefabToSpawn == null) yield break;

        Vector3 playerForward = playerTransform != null ? playerTransform.forward : Vector3.forward;
        Vector3 playerPos = playerTransform != null ? playerTransform.position : transform.position;
        Vector3 waveOrigin = playerPos + (playerForward * forwardSpawnDistance);

        // For-loop calculating array of formation positions
        Vector3[] spawnPositions = new Vector3[wave.enemyCount];
        for (int i = 0; i < wave.enemyCount; i++)
        {
            spawnPositions[i] = CalculateFormationOffset(wave.formation, i, wave.enemyCount, wave.formationSpacing);
        }

        // Coroutine burst spawning
        for (int i = 0; i < wave.enemyCount; i++)
        {
            Vector3 worldSpawnPos = waveOrigin + spawnPositions[i];
            Quaternion spawnRot = Quaternion.LookRotation(-playerForward);

            GameObject spawned = Instantiate(prefabToSpawn, worldSpawnPos, spawnRot, transform);

            Enemy enemy = spawned.GetComponent<Enemy>();
            if (enemy != null)
            {
                EnemyType type = (wave.enemyCount > 3) ? EnemyType.FastScout : EnemyType.HeavyDropship;
                int hp = (type == EnemyType.FastScout) ? 10 : 30;
                int score = (type == EnemyType.FastScout) ? 15 : 35;
                enemy.ConfigureEnemy(type, hp, score, wave.enemySpeed, wave.weaveMotion);
            }

            if (wave.burstInterval > 0f)
            {
                yield return new WaitForSeconds(wave.burstInterval);
            }
        }
    }

    /// <summary>
    /// Computes spatial offsets for wave patterns (arrays/for-loops).
    /// </summary>
    private Vector3 CalculateFormationOffset(WaveFormation formation, int index, int total, float spacing)
    {
        float centerOffset = (total - 1) * 0.5f;

        switch (formation)
        {
            case WaveFormation.HorizontalLine:
                return new Vector3((index - centerOffset) * spacing, 0f, 0f);

            case WaveFormation.VFormation:
                float x = (index - centerOffset) * spacing;
                float z = -Mathf.Abs(index - centerOffset) * (spacing * 0.8f);
                return new Vector3(x, 0f, z);

            case WaveFormation.VerticalColumn:
                return new Vector3(0f, (index - centerOffset) * spacing, -index * 5f);

            case WaveFormation.StaggeredPair:
                float side = (index % 2 == 0) ? -1f : 1f;
                return new Vector3(side * spacing, (index / 2) * 3f, -index * 6f);

            default:
                return Vector3.zero;
        }
    }

    private void SetupDefaultWaves()
    {
        waves = new WaveDefinition[]
        {
            new WaveDefinition
            {
                waveName = "Wave 1 - Scout V-Formation",
                enemyCount = 5,
                triggerTime = 3.5f,
                burstInterval = 0.4f,
                formation = WaveFormation.VFormation,
                formationSpacing = 12f,
                enemySpeed = 16f,
                weaveMotion = true
            },
            new WaveDefinition
            {
                waveName = "Wave 2 - Heavy Dropships",
                enemyCount = 3,
                triggerTime = 11.0f,
                burstInterval = 0.8f,
                formation = WaveFormation.HorizontalLine,
                formationSpacing = 16f,
                enemySpeed = 10f,
                weaveMotion = false
            },
            new WaveDefinition
            {
                waveName = "Wave 3 - Interceptor Pincer",
                enemyCount = 6,
                triggerTime = 18.0f,
                burstInterval = 0.35f,
                formation = WaveFormation.StaggeredPair,
                formationSpacing = 14f,
                enemySpeed = 20f,
                weaveMotion = true
            }
        };
    }
}
