using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player damage, terrain collisions, power-up pickups,
/// and triggers the restart Coroutine flow upon game over.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Collision : MonoBehaviour
{
    [Header("Crash Effects & Timing")]
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float invulnerabilityDuration = 1.5f;

    private int currentLives;
    private bool isCrashed = false;
    private bool isInvulnerable = false;
    private Renderer[] shipRenderers;

    void Awake()
    {
        SetupColliderAndRigidbody();
        shipRenderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        currentLives = maxLives;
        if (Scoreboard.Instance != null)
        {
            Scoreboard.Instance.SetLives(currentLives);
        }
    }

    private void SetupColliderAndRigidbody()
    {
        foreach (MeshCollider meshCol in GetComponentsInChildren<MeshCollider>())
        {
            if (Application.isPlaying)
                Destroy(meshCol);
            else
                DestroyImmediate(meshCol);
        }

        BoxCollider boxCol = GetComponent<BoxCollider>();
        boxCol.isTrigger = true;
        boxCol.center = new Vector3(0.12f, -4.26f, 5.73f);
        boxCol.size = new Vector3(6f, 4f, 12f);

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
    }

    void OnTriggerEnter(Collider other)
    {
        HandleTriggerInteraction(other.gameObject);
    }

    void OnCollisionEnter(UnityEngine.Collision other)
    {
        HandleTriggerInteraction(other.gameObject);
    }

    private void HandleTriggerInteraction(GameObject other)
    {
        if (isCrashed) return;

        // Check for Power-Up Pickup
        PowerUp powerUp = other.GetComponent<PowerUp>();
        if (powerUp != null)
        {
            powerUp.Collect(gameObject);
            return;
        }

        // Ignore friendly laser triggers or target point
        if (other.name.Contains("Laser") || other.name.Contains("TargetPoint")) return;

        // Player takes damage or crashes
        TakeDamage(1, other.name.Contains("Terrain"));
    }

    public void TakeDamage(int damage = 1, bool instantDeath = false)
    {
        if (isCrashed || isInvulnerable) return;

        currentLives = instantDeath ? 0 : (currentLives - damage);

        if (Scoreboard.Instance != null)
        {
            Scoreboard.Instance.SetLives(currentLives);
        }

        if (currentLives <= 0)
        {
            StartCrashSequence();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    public void RestoreLife()
    {
        if (currentLives < maxLives)
        {
            currentLives++;
            if (Scoreboard.Instance != null)
            {
                Scoreboard.Instance.SetLives(currentLives);
            }
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHit(0.8f);
        }

        // Flashing mesh feedback
        float endTime = Time.time + invulnerabilityDuration;
        while (Time.time < endTime)
        {
            SetRenderersVisible(false);
            yield return new WaitForSeconds(0.1f);
            SetRenderersVisible(true);
            yield return new WaitForSeconds(0.1f);
        }

        SetRenderersVisible(true);
        isInvulnerable = false;
    }

    private void SetRenderersVisible(bool visible)
    {
        if (shipRenderers == null) return;
        foreach (var r in shipRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    private void StartCrashSequence()
    {
        if (isCrashed) return;
        isCrashed = true;

        Vector3 shipCenter = transform.TransformPoint(new Vector3(0.12f, -4.26f, 5.73f));

        // Explosion SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosion(1f);
        }

        // Explosion VFX
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, shipCenter, Quaternion.identity);
        }
        else
        {
            CreateSimpleExplosion(shipCenter);
        }

        // Hide ship
        SetRenderersVisible(false);

        // Disable controls
        if (GetComponent<PlayerMovement>() != null)
            GetComponent<PlayerMovement>().enabled = false;

        if (GetComponent<PlayerWeapons>() != null)
            GetComponent<PlayerWeapons>().enabled = false;

        // Trigger Game Over restart flow via Coroutine
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.TriggerGameOver();
        }
        else
        {
            FindFirstObjectByType<GameSceneManager>()?.TriggerGameOver();
        }
    }

    private void CreateSimpleExplosion(Vector3 position)
    {
        GameObject vfx = new GameObject("PlayerExplosion");
        vfx.transform.position = position;

        ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = false;
        main.duration = 1.0f;
        main.startLifetime = 0.8f;
        main.startSpeed = 25f;
        main.startSize = 2.0f;
        main.startColor = new Color(1f, 0.4f, 0f, 1f);

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 90) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;

        ps.Play();
        Destroy(vfx, 2f);
    }
}
