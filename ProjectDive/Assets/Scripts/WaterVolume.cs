using UnityEngine;

namespace ProjectDive
{
    /// <summary>
    /// Represents a 10x10x10m (or custom-sized) physical water volume.
    /// Provides spatial and physical reference queries (surface height, depth, density, bounds)
    /// for external swimming or buoyancy systems.
    /// NOTE: This script does NOT implement swimming logic.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    [AddComponentMenu("ProjectDive/Water Volume")]
    public class WaterVolume : MonoBehaviour
    {
        [Header("Dimensions (Meters)")]
        [Tooltip("Size of the water volume in meters (X: Width, Y: Depth, Z: Length)")]
        public Vector3 dimensions = new Vector3(10f, 10f, 10f);

        [Header("Physical Reference Properties")]
        [Tooltip("Standard water density in kg/m^3 (approx 1000 for fresh water)")]
        public float waterDensity = 1000f;

        [Tooltip("Reference linear drag factor inside water")]
        public float waterDrag = 2f;

        [Tooltip("Reference angular drag factor inside water")]
        public float waterAngularDrag = 1f;

        [Header("Editor Gizmos")]
        public bool showGizmos = true;
        public Color volumeColor = new Color(0f, 0.7f, 1f, 0.25f);
        public Color wireColor = new Color(0f, 0.8f, 1f, 0.9f);
        public Color surfaceColor = new Color(0.2f, 0.9f, 1f, 0.5f);

        private BoxCollider _boxCollider;

        public BoxCollider BoxCollider
        {
            get
            {
                if (_boxCollider == null)
                    _boxCollider = GetComponent<BoxCollider>();
                return _boxCollider;
            }
        }

        /// <summary>
        /// Global Y position of the water surface.
        /// </summary>
        public float SurfaceElevation
        {
            get
            {
                if (BoxCollider != null)
                    return BoxCollider.bounds.max.y;
                return transform.position.y + (dimensions.y * 0.5f);
            }
        }

        /// <summary>
        /// Global Y position of the bottom of the water body.
        /// </summary>
        public float BottomElevation
        {
            get
            {
                if (BoxCollider != null)
                    return BoxCollider.bounds.min.y;
                return transform.position.y - (dimensions.y * 0.5f);
            }
        }

        /// <summary>
        /// Total depth of the water volume.
        /// </summary>
        public float Depth => SurfaceElevation - BottomElevation;

        /// <summary>
        /// Global bounding box of this water body.
        /// </summary>
        public Bounds Bounds => BoxCollider != null ? BoxCollider.bounds : new Bounds(transform.position, dimensions);

        private void Reset()
        {
            SyncTriggerCollider();
        }

        private void OnValidate()
        {
            SyncTriggerCollider();
        }

        private void Awake()
        {
            SyncTriggerCollider();
        }

        /// <summary>
        /// Configures the BoxCollider to be a trigger matching the volume dimensions.
        /// </summary>
        public void SyncTriggerCollider()
        {
            if (BoxCollider != null)
            {
                BoxCollider.isTrigger = true;
                BoxCollider.size = dimensions;
                BoxCollider.center = Vector3.zero;
            }
        }

        /// <summary>
        /// Returns true if the world position is inside this water volume.
        /// </summary>
        public bool Contains(Vector3 worldPosition)
        {
            return Bounds.Contains(worldPosition);
        }

        /// <summary>
        /// Returns the submerged depth at a given world position.
        /// Returns 0 if the position is above the water surface or outside the XZ boundary.
        /// </summary>
        public float GetSubmergedDepth(Vector3 worldPosition)
        {
            if (!Contains(worldPosition)) return 0f;
            return Mathf.Max(0f, SurfaceElevation - worldPosition.y);
        }

        /// <summary>
        /// Returns the normalized immersion ratio (0 = at or above surface, 1 = at or below bottom)
        /// for a given world position.
        /// </summary>
        public float GetNormalizedDepth(Vector3 worldPosition)
        {
            if (!Contains(worldPosition)) return 0f;
            float total = Depth;
            if (total <= 0.0001f) return 0f;
            return Mathf.Clamp01((SurfaceElevation - worldPosition.y) / total);
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Vector3 center = transform.position;
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(center, dimensions);

            // Water surface highlight
            Vector3 surfaceCenter = center + Vector3.up * (dimensions.y * 0.5f);
            Gizmos.color = surfaceColor;
            Gizmos.DrawCube(surfaceCenter, new Vector3(dimensions.x, 0.02f, dimensions.z));
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = transform.position;
            Gizmos.color = volumeColor;
            Gizmos.DrawCube(center, dimensions);
        }
    }
}
