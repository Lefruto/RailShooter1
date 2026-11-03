using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float controlSpeed = 20f;
    [SerializeField] float returnSpeed = 5f;
    [SerializeField] float xClampRange = 10f;
    [SerializeField] float yClampRange = 7f;

    [SerializeField] float rollAngle = 30f;
    [SerializeField] float pitchAngle = 30f;
    [SerializeField] float rotationSpeed = 10f;

    Vector2 movement;

    void Update()
    {
        ProcessTranslation();
        ProcessRotation();
    }

    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    private void ProcessTranslation()
    {
        if (movement == Vector2.zero)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, returnSpeed * Time.deltaTime);
            return;
        }

        float xOffset = movement.x * controlSpeed * Time.deltaTime;
        float rawXPos = transform.localPosition.x + xOffset;
        float clampedXPos = Mathf.Clamp(rawXPos, -xClampRange, xClampRange);

        float yOffset = movement.y * controlSpeed * Time.deltaTime;
        float rawYPos = transform.localPosition.y + yOffset;
        float clampedYPos = Mathf.Clamp(rawYPos, -yClampRange, yClampRange);

        transform.localPosition = new Vector3(clampedXPos, clampedYPos, 0f);
    }

    private void ProcessRotation()
    {
        float pitch = -pitchAngle * movement.y;
        float roll = -rollAngle * movement.x;

        Quaternion targetRotation = Quaternion.Euler(pitch, 0f, roll);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}