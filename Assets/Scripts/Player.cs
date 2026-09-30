using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float smoothTime = 0.1f;

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
        // Başlangıç pozisyonunu hedef olarak belirle
        targetLocalPosition = transform.localPosition;
    }

    // New Input System: Player Input bileşeni Send Messages modundayken bu fonksiyonu tetikler
    public void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
    }

    void Update()
    {
        // 1. Girdiye göre hedef pozisyonu güncelle
        targetLocalPosition += new Vector3(rawInput.x, rawInput.y, 0f) * moveSpeed * Time.deltaTime;

        // 2. Hedef pozisyonu belirlenen sınırlar (Bounds) içinde tut
        targetLocalPosition.x = Mathf.Clamp(targetLocalPosition.x, xMin, xMax);
        targetLocalPosition.y = Mathf.Clamp(targetLocalPosition.y, yMin, yMax);

        // 3. SmoothDamp ile mevcut pozisyondan hedef pozisyona yumuşak geçiş sağla
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetLocalPosition,
            ref currentVelocity,
            smoothTime
        );
    }
}