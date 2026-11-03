using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    HeavyDropship,
    FastScout,
    Boss
}

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Classification & Stats")]
    [SerializeField] EnemyType enemyType = EnemyType.HeavyDropship;
    [SerializeField] int hitPoints = 30;
    [SerializeField] int maxHitPoints = 30;
    [SerializeField] int scoreValue = 20;
    [SerializeField] float moveSpeed = 0f; // Autonomous flight speed towards player rail
    [SerializeField] bool weaveMotion = false; // Sinusoidal waving behavior for scouts

    [Header("Hit & Death Effects")]
    [SerializeField] GameObject hitVFX;
    [SerializeField] GameObject destroyedVFX;
    [SerializeField] GameObject powerUpPrefab;
    [Range(0f, 1f)] [SerializeField] float powerUpDropRate = 0.25f;

    private Scoreboard scoreboard;
    private bool isDead = false;
    private Renderer[] enemyRenderers;
    private Color[] originalColors;
    private float spawnTime;
    private Vector3 initialPosition;

    void Awake()
    {
        SetupCollider();
        enemyRenderers = GetComponentsInChildren<Renderer>();
        CacheOriginalColors();
    }

    void Start()
    {
        scoreboard = FindFirstObjectByType<Scoreboard>();
        spawnTime = Time.time;
        initialPosition = transform.position;
        if (maxHitPoints < hitPoints) maxHitPoints = hitPoints;
    }

    void Update()
    {
        if (isDead) return;

        // Custom flight behavior for dynamically spawned enemies
        if (moveSpeed > 0f)
        {
            float elapsed = Time.time - spawnTime;
            Vector3 movement = -transform.forward * moveSpeed * Time.deltaTime;

            if (weaveMotion)
            {
                movement += transform.right * Mathf.Sin(elapsed * 4f) * 6f * Time.deltaTime;
            }

            transform.position += movement;
        }
    }

    private void CacheOriginalColors()
    {
        if (enemyRenderers == null || enemyRenderers.Length == 0) return;
        List<Color> colors = new List<Color>();
        foreach (var r in enemyRenderers)
        {
            if (r.material != null && r.material.HasProperty("_BaseColor"))
                colors.Add(r.material.GetColor("_BaseColor"));
            else if (r.material != null && r.material.HasProperty("_Color"))
                colors.Add(r.material.color);
        }
        originalColors = colors.ToArray();
    }

    private void SetupCollider()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.isTrigger = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (isDead) return;

        hitPoints--;

        // Hit Feedback: SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHit(0.5f);
        }

        // Hit Feedback: Visual hit flash
        StartCoroutine(HitFlashRoutine());

        // Hit Feedback: VFX sparks
        CreateHitSparks();

        ProcessHit();
    }

    private void CreateHitSparks()
    {
        if (hitVFX != null)
        {
            Instantiate(hitVFX, transform.position, Quaternion.identity);
            return;
        }

        // Procedural sparks VFX
        GameObject sparks = new GameObject("HitSparks");
        sparks.transform.position = transform.position;
        ParticleSystem ps = sparks.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.duration = 0.2f;
        main.startLifetime = 0.15f;
        main.startSpeed = 8f;
        main.startSize = 0.35f;
        main.startColor = new Color(1f, 0.9f, 0.2f, 1f);

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 12) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        ps.Play();
        Destroy(sparks, 0.4f);
    }

    private IEnumerator HitFlashRoutine()
    {
        if (enemyRenderers == null) yield break;

        foreach (var r in enemyRenderers)
        {
            if (r != null && r.material != null)
            {
                if (r.material.HasProperty("_BaseColor"))
                    r.material.SetColor("_BaseColor", Color.white);
                else if (r.material.HasProperty("_Color"))
                    r.material.color = Color.white;
            }
        }

        yield return new WaitForSeconds(0.06f);

        if (enemyRenderers == null) yield break;
        int idx = 0;
        foreach (var r in enemyRenderers)
        {
            if (r != null && r.material != null && originalColors != null && idx < originalColors.Length)
            {
                if (r.material.HasProperty("_BaseColor"))
                    r.material.SetColor("_BaseColor", originalColors[idx]);
                else if (r.material.HasProperty("_Color"))
                    r.material.color = originalColors[idx];
            }
            idx++;
        }
    }

    private void ProcessHit()
    {
        if (hitPoints > 0) return;

        isDead = true;

        // Increase score on UI
        if (scoreboard != null)
        {
            scoreboard.IncreaseScore(scoreValue);
        }

        // Audio feedback on kill
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosion(0.7f);
        }

        // Death VFX
        if (destroyedVFX != null)
        {
            Instantiate(destroyedVFX, transform.position, Quaternion.identity);
        }
        else
        {
            CreateExplosionVFX(transform.position);
        }

        // Optional Power-up drop
        TryDropPowerUp();

        Destroy(gameObject);
    }

    private void TryDropPowerUp()
    {
        if (powerUpPrefab != null && Random.value < powerUpDropRate)
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
        }
    }

    private void CreateExplosionVFX(Vector3 position)
    {
        GameObject vfx = new GameObject("EnemyExplosion");
        vfx.transform.position = position;

        ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = false;
        main.duration = 0.8f;
        main.startLifetime = 0.6f;
        main.startSpeed = (enemyType == EnemyType.FastScout) ? 14f : 22f;
        main.startSize = (enemyType == EnemyType.FastScout) ? 0.9f : 1.6f;
        main.startColor = (enemyType == EnemyType.FastScout)
            ? new Color(0.2f, 0.8f, 1f, 1f)
            : new Color(1f, 0.45f, 0f, 1f);

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;

        ps.Play();
        Destroy(vfx, 1.2f);
    }

    public void ConfigureEnemy(EnemyType type, int hp, int score, float speed, bool weave)
    {
        enemyType = type;
        hitPoints = hp;
        maxHitPoints = hp;
        scoreValue = score;
        moveSpeed = speed;
        weaveMotion = weave;
    }
}
