using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controls player movement, swimming, sinking, and water interaction.
/// Compatible with both the New Input System and Legacy Input Manager.
/// Automatically sets up Rigidbody and CapsuleCollider if missing.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Land Movement")]
    [Tooltip("Movement acceleration force on land")]
    public float moveForce = 14f;

    [Tooltip("Maximum horizontal movement speed on land (m/s)")]
    public float maxSpeed = 3.2f;

    [Tooltip("Turn speed towards movement direction")]
    public float turnSpeed = 10f;

    [Tooltip("Jump impulse on land")]
    public float jumpForce = 5.5f;

    [Tooltip("Linear drag on land")]
    public float groundDrag = 2f;

    [Header("Water Movement & Swimming")]
    [Tooltip("Horizontal swimming acceleration force in water")]
    public float waterMoveForce = 8f;

    [Tooltip("Maximum horizontal swimming speed in water (m/s)")]
    public float waterMaxSpeed = 2.0f;

    [Tooltip("Linear drag in water")]
    public float waterDrag = 2.5f;

    [Header("Sinking & Buoyancy")]
    [Tooltip("Downward sinking acceleration force when in water (negative buoyancy)")]
    public float sinkForce = 5.0f;

    [Tooltip("Maximum natural downward sinking speed in water (m/s)")]
    public float maxSinkSpeed = 1.8f;

    [Tooltip("Upward swimming acceleration force when holding Jump (Space) in water")]
    public float swimUpForce = 12f;

    [Tooltip("Maximum upward swimming speed in water (m/s)")]
    public float maxSwimSpeed = 2.2f;

    [Tooltip("Downward diving acceleration force when holding Dive (Shift/C) in water")]
    public float diveForce = 8f;

    [Tooltip("Maximum downward diving speed in water (m/s)")]
    public float maxDiveSpeed = 3.5f;

    [Header("Controls")]
    [Tooltip("Move relative to the main camera view direction")]
    public bool useCameraRelative = true;

    [Header("State (Read Only)")]
    public bool isInWater = false;
    public bool isGrounded = false;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator anim;

    private Vector2 inputMove;
    private bool inputJump;
    private bool inputDive;

    private int waterTriggerCount = 0;
    private ProjectDive.WaterVolume currentWaterVolume;

    void Awake()
    {
        // Prevent duplicate execution if attached to a child bone
        if (transform.parent != null && GetComponentInParent<PlayerMovement>() != null && GetComponentInParent<PlayerMovement>() != this)
        {
            Debug.LogWarning("[PlayerMovement] Duplicate PlayerMovement found on child object '" + gameObject.name + "'. Disabling child script.");
            enabled = false;
            return;
        }

        SetupComponents();
    }

    void Start()
    {
        SetupComponents();
        if (currentWaterVolume == null)
        {
            currentWaterVolume = Object.FindAnyObjectByType<ProjectDive.WaterVolume>();
        }
    }

    void Reset()
    {
        SetupComponents();
    }

    private void SetupComponents()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody for character movement
        rb.mass = 75f;
        rb.linearDamping = groundDrag;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Freeze X, Y, Z rotations so character stays upright
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;

        col = GetComponent<CapsuleCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider>();
        }

        // Standard humanoid capsule dimensions (~1.8m tall, 0.35m radius)
        col.center = new Vector3(0f, 0.9f, 0f);
        col.radius = 0.35f;
        col.height = 1.8f;
        col.isTrigger = false;

        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        ReadInput();

        // Handle ground jump in Update for responsive key detection
        if (inputJump && isGrounded && !isInWater && rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void ReadInput()
    {
        float x = 0f;
        float y = 0f;
        bool jump = false;
        bool dive = false;

        // 1. Check New Input System (UnityEngine.InputSystem)
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;

            if (Keyboard.current.spaceKey.isPressed) jump = true;
            if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed || Keyboard.current.cKey.isPressed) dive = true;
        }
#endif

        // 2. Fallback to Legacy Input Manager if values were not already set
#if ENABLE_LEGACY_INPUT_MANAGER
        try
        {
            if (x == 0f && y == 0f)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            }
            if (!jump && Input.GetKey(KeyCode.Space)) jump = true;
            if (!dive && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.C))) dive = true;
        }
        catch (System.InvalidOperationException)
        {
            // Legacy Input is disabled in Player Settings; ignore and rely on Input System
        }
#endif

        inputMove = new Vector2(x, y);
        inputJump = jump;
        inputDive = dive;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        UpdateWaterState();
        CheckGrounded();

        // Calculate horizontal direction from input and camera orientation
        Vector3 moveDir = Vector3.zero;

        if (useCameraRelative && Camera.main != null)
        {
            Vector3 camFwd = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camFwd.y = 0f;
            camRight.y = 0f;
            camFwd.Normalize();
            camRight.Normalize();

            moveDir = (camFwd * inputMove.y) + (camRight * inputMove.x);
        }
        else
        {
            moveDir = (transform.forward * inputMove.y) + (transform.right * inputMove.x);
        }

        if (isInWater)
        {
            ApplyWaterMovement(moveDir);
        }
        else
        {
            ApplyLandMovement(moveDir);
        }

        // Update Animator parameter if available
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            Vector3 horizVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            anim.SetFloat("Speed", horizVel.magnitude);
        }
    }

    private void ApplyWaterMovement(Vector3 moveDir)
    {
        // 1. Horizontal swimming movement
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Vector3 targetDir = moveDir.normalized;
            Vector3 horizTargetDir = new Vector3(targetDir.x, 0f, targetDir.z);
            if (horizTargetDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizTargetDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * turnSpeed);
            }

            rb.AddForce(horizTargetDir * waterMoveForce, ForceMode.Acceleration);
        }

        // 2. Vertical forces: Natural Sinking vs Swimming Up vs Diving Down
        if (inputJump)
        {
            // Actively swimming up towards water surface
            rb.AddForce(Vector3.up * swimUpForce, ForceMode.Acceleration);
        }
        else if (inputDive)
        {
            // Actively diving down deeper faster
            rb.AddForce(Vector3.down * (sinkForce + diveForce), ForceMode.Acceleration);
        }
        else
        {
            // Natural downward sinking whenever the player is in/over water
            rb.AddForce(Vector3.down * sinkForce, ForceMode.Acceleration);
        }

        // 3. Velocity clamping in water
        Vector3 vel = rb.linearVelocity;
        Vector3 horizVel = new Vector3(vel.x, 0f, vel.z);
        if (horizVel.magnitude > waterMaxSpeed)
        {
            horizVel = horizVel.normalized * waterMaxSpeed;
        }

        float vertVel = vel.y;
        float surfaceY = currentWaterVolume != null ? currentWaterVolume.SurfaceElevation : 0f;

        // Dampen upward swimming near or at the surface to prevent launching into the air
        if (transform.position.y >= surfaceY - 0.2f && vertVel > 0f)
        {
            vertVel = Mathf.MoveTowards(vertVel, 0f, Time.fixedDeltaTime * 15f);
        }
        else if (vertVel > maxSwimSpeed)
        {
            vertVel = maxSwimSpeed;
        }

        // Clamp downward vertical velocity
        if (inputDive)
        {
            if (vertVel < -maxDiveSpeed) vertVel = -maxDiveSpeed;
        }
        else
        {
            // Limit steady sinking speed
            if (vertVel < -maxSinkSpeed)
            {
                vertVel = Mathf.MoveTowards(vertVel, -maxSinkSpeed, Time.fixedDeltaTime * 8f);
            }
        }

        rb.linearVelocity = new Vector3(horizVel.x, vertVel, horizVel.z);
    }

    private void ApplyLandMovement(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Vector3 targetDir = moveDir.normalized;
            Vector3 horizTargetDir = new Vector3(targetDir.x, 0f, targetDir.z);
            if (horizTargetDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizTargetDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * turnSpeed);
            }

            rb.AddForce(horizTargetDir * moveForce, ForceMode.Acceleration);
        }

        // Limit maximum horizontal velocity on land
        Vector3 vel = rb.linearVelocity;
        Vector3 horizVel = new Vector3(vel.x, 0f, vel.z);
        if (horizVel.magnitude > maxSpeed)
        {
            horizVel = horizVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizVel.x, vel.y, horizVel.z);
        }
    }

    private void UpdateWaterState()
    {
        if (currentWaterVolume == null)
        {
            currentWaterVolume = Object.FindAnyObjectByType<ProjectDive.WaterVolume>();
        }

        bool inTrigger = waterTriggerCount > 0;
        bool isSubmergedOrOverWater = false;

        if (currentWaterVolume != null)
        {
            Bounds b = currentWaterVolume.Bounds;
            Vector3 pos = transform.position;
            float surfaceY = currentWaterVolume.SurfaceElevation;

            // Inside water's horizontal perimeter
            bool insideXZ = pos.x >= b.min.x && pos.x <= b.max.x && pos.z >= b.min.z && pos.z <= b.max.z;

            // Player is over the water or submerged if within horizontal bounds and at or below surface
            if (insideXZ && pos.y <= surfaceY + 0.15f)
            {
                isSubmergedOrOverWater = true;
            }
            // Or if capsule center/feet are below the surface within trigger
            else if (inTrigger && pos.y < surfaceY - 0.2f)
            {
                isSubmergedOrOverWater = true;
            }
        }
        else
        {
            isSubmergedOrOverWater = inTrigger;
        }

        isInWater = isSubmergedOrOverWater;

        if (rb != null)
        {
            if (isInWater)
            {
                rb.useGravity = false;
                rb.linearDamping = waterDrag;
            }
            else
            {
                rb.useGravity = true;
                rb.linearDamping = groundDrag;
            }
        }
    }

    private void CheckGrounded()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        // QueryTriggerInteraction.Ignore ensures water triggers do not register as solid ground
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, 0.25f, ~0, QueryTriggerInteraction.Ignore);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsWaterCollider(other))
        {
            waterTriggerCount++;
            var vol = other.GetComponent<ProjectDive.WaterVolume>() ?? other.GetComponentInParent<ProjectDive.WaterVolume>();
            if (vol != null) currentWaterVolume = vol;
            UpdateWaterState();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (IsWaterCollider(other))
        {
            if (currentWaterVolume == null)
            {
                currentWaterVolume = other.GetComponent<ProjectDive.WaterVolume>() ?? other.GetComponentInParent<ProjectDive.WaterVolume>();
            }
            if (!isInWater)
            {
                waterTriggerCount = Mathf.Max(1, waterTriggerCount);
                UpdateWaterState();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsWaterCollider(other))
        {
            waterTriggerCount = Mathf.Max(0, waterTriggerCount - 1);
            UpdateWaterState();
        }
    }

    private bool IsWaterCollider(Collider col)
    {
        if (col == null) return false;
        if (col.CompareTag("Water")) return true;
        if (col.gameObject.layer == LayerMask.NameToLayer("Water")) return true;
        if (col.GetComponent<ProjectDive.WaterVolume>() != null) return true;
        if (col.GetComponentInParent<ProjectDive.WaterVolume>() != null) return true;
        return false;
    }
}