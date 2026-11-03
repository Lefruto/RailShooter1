using UnityEngine;
using UnityEngine.InputSystem;

public class Cross : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        transform.position = Mouse.current.position.ReadValue();
    }
}
