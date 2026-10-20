using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Rotation Parameters")]
    [Tooltip("A/D basınca sağa/sola kanat yatırma miktarı")]
    [SerializeField] private float rollControl = 25f;

    [Tooltip("W/S basınca burun kaldırma/indirme miktarı")]
    [SerializeField] private float pitchControl = 15f;

    [Tooltip("Dönüşlerin yumuşaklığı (ne kadar yüksekse o kadar seri toparlar)")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Screen Bounds (Local Limits)")]
    [SerializeField] private float xMin = -6f;
    [SerializeField] private float xMax = 6f;
    [SerializeField] private float yMin = -3.5f;
    [SerializeField] private float yMax = 3.5f;

    private Vector2 rawInput;
    private Vector3 currentVelocity;
    private Vector3 targetLocalPosition;

    void Start()
    {
        targetLocalPosition = transform.localPosition;
    }

    public void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
    }

    void Update()
    {
        ProcessTranslation();
        ProcessRotation();
    }

    private void ProcessTranslation()
    {
        // 1. Girdiye göre hedef pozisyonu güncelle
        targetLocalPosition += new Vector3(rawInput.x, rawInput.y, 0f) * moveSpeed * Time.deltaTime;

        // 2. Hedef pozisyonu belirlenen sınırlar içinde tut
        targetLocalPosition.x = Mathf.Clamp(targetLocalPosition.x, xMin, xMax);
        targetLocalPosition.y = Mathf.Clamp(targetLocalPosition.y, yMin, yMax);

        // 3. Yumuşak geçişle pozisyonu uygula
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetLocalPosition,
            ref currentVelocity,
            smoothTime
        );
    }

    private void ProcessRotation()
    {
        // Pitch: W'ye basınca burun yukarı kalksın istiyorsan -rawInput.y, uçağın burnu aşağı insin istiyorsan +rawInput.y
        float targetPitch = -rawInput.y * pitchControl;

        // Roll: A'ya basınca sola, D'ye basınca sağa yatış
        float targetRoll = -rawInput.x * rollControl;

        Quaternion targetRotation = Quaternion.Euler(targetPitch, 0f, targetRoll);

        // Slerp ile dönüşleri yumuşatarak tuşu bıraktığında geminin akıcı şekilde düzelmesini sağla
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}