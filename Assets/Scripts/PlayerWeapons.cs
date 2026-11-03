using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls ship weapon firing with fire rate / cooldown control,
/// particle laser emission, laser targeting, and audio feedback.
/// </summary>
public class PlayerWeapons : MonoBehaviour
{
    [Header("Lasers & Targeting")]
    [SerializeField] ParticleSystem[] lasers;
    [SerializeField] Transform targetPoint;
    [SerializeField] float targetDistance = 50f;

    [Header("Fire Rate & Cooldown")]
    [SerializeField] float fireRate = 0.15f; // Cooldown between shots/bursts
    [SerializeField] bool rapidFireActive = false;

    private bool isFiring = false;
    private float nextFireTime = 0f;

    void Awake()
    {
        // Fail-safe: locate lasers in children if not assigned in inspector
        if (lasers == null || lasers.Length == 0)
        {
            lasers = GetComponentsInChildren<ParticleSystem>();
        }

        // Fail-safe: find or create TargetPoint if missing
        if (targetPoint == null)
        {
            GameObject tp = GameObject.Find("TargetPoint");
            if (tp != null)
            {
                targetPoint = tp.transform;
            }
            else
            {
                targetPoint = new GameObject("TargetPoint").transform;
            }
        }
    }

    void Start()
    {
        foreach (ParticleSystem laser in lasers)
        {
            if (laser == null) continue;
            var collisionModule = laser.collision;
            collisionModule.enabled = true;
            collisionModule.type = ParticleSystemCollisionType.World;
            collisionModule.sendCollisionMessages = true;
            collisionModule.lifetimeLoss = 1f;

            var emission = laser.emission;
            emission.enabled = false;

            laser.Play();
        }
    }

    void Update()
    {
        MoveTargetPoint();
        AimLasers();
        ProcessFiring();
    }

    public void OnAttack(InputValue value)
    {
        isFiring = value.isPressed;
    }

    public void OnFire(InputValue value)
    {
        isFiring = value.isPressed;
    }

    private void ProcessFiring()
    {
        float currentRate = rapidFireActive ? (fireRate * 0.5f) : fireRate;

        if (isFiring && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + currentRate;

            // Trigger burst on all lasers
            foreach (ParticleSystem laser in lasers)
            {
                if (laser == null) continue;
                laser.Emit(1);
            }

            // Audio feedback
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLaser(0.35f);
            }
        }

        // Keep continuous emission module enabled only if continuous fire is desired
        foreach (ParticleSystem laser in lasers)
        {
            if (laser == null) continue;
            var emissionModule = laser.emission;
            // Laser emission is paced by fireRate
            emissionModule.enabled = isFiring;
        }
    }

    private void MoveTargetPoint()
    {
        if (targetPoint == null) return;
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector2 mousePos = Vector2.zero;
        if (Mouse.current != null)
        {
            mousePos = Mouse.current.position.ReadValue();
        }
        else
        {
            mousePos = Input.mousePosition;
        }

        Vector3 targetPointPosition = new Vector3(mousePos.x, mousePos.y, targetDistance);
        targetPoint.position = mainCam.ScreenToWorldPoint(targetPointPosition);
    }

    private void AimLasers()
    {
        if (targetPoint == null) return;

        Vector3 fireDirection = targetPoint.position - transform.position;
        if (fireDirection.sqrMagnitude < 0.001f) return;

        Quaternion rotationToTarget = Quaternion.LookRotation(fireDirection);

        foreach (ParticleSystem laser in lasers)
        {
            if (laser == null) continue;
            laser.transform.rotation = rotationToTarget;
        }
    }

    public void SetRapidFire(bool active)
    {
        rapidFireActive = active;
    }
}
