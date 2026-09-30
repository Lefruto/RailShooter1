using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class thirdpersoncam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerobj;
    public Rigidbody rb;


    public float rotatinSpeed = 7f;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null) player = p.transform;
        }

        if (orientation == null && player != null)
        {
            Transform o = player.Find("Orientation");
            if (o != null) orientation = o;
        }

        if (playerobj == null && player != null)
        {
            Transform po = player.Find("PlayerObj");
            if (po != null) playerobj = po;
        }

        if (rb == null && player != null)
        {
            rb = player.GetComponent<Rigidbody>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    { 
        if (player == null || orientation == null) return;

        //rotate orientation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        if (viewDir != Vector3.zero)
            orientation.forward = viewDir.normalized;

        //rotate player object
        if (playerobj != null)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
                playerobj.forward = Vector3.Slerp(playerobj.forward, inputDir.normalized, Time.deltaTime * rotatinSpeed);
        }
    }
}
