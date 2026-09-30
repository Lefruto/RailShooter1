using UnityEngine;

namespace ProjectDive
{
    /// <summary>
    /// Smooth follow camera designed for third-person player tracking during walking, diving, and swimming.
    /// Follows the player character through land and underwater navigation.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("ProjectDive/Smooth Follow Camera")]
    public class SmoothFollowCamera : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The player transform to follow")]
        public Transform target;

        [Header("Follow Settings")]
        [Tooltip("Camera offset relative to player (X: Lateral, Y: Height, Z: Distance behind)")]
        public Vector3 offset = new Vector3(0f, 3.5f, -6f);

        [Tooltip("Follow position smoothing factor")]
        public float positionDamping = 6f;

        [Tooltip("Look rotation smoothing factor")]
        public float rotationDamping = 10f;

        [Tooltip("Target look height above the player's root position")]
        public float lookHeightOffset = 1.2f;

        void Start()
        {
            AutoAssignTarget();
        }

        /// <summary>
        /// Attempts to find the PlayerMovement component in the scene if target is not set.
        /// </summary>
        public void AutoAssignTarget()
        {
            if (target == null)
            {
                var player = Object.FindAnyObjectByType<PlayerMovement>();
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        void LateUpdate()
        {
            if (target == null)
            {
                AutoAssignTarget();
                if (target == null) return;
            }

            // Desired camera position
            Vector3 desiredPosition = target.position + offset;

            // Smooth position interpolation
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionDamping);

            // Look towards target body center
            Vector3 lookTarget = target.position + Vector3.up * lookHeightOffset;
            Vector3 lookDir = (lookTarget - transform.position).normalized;

            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRot = Quaternion.LookRotation(lookDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotationDamping);
            }
        }
    }
}
