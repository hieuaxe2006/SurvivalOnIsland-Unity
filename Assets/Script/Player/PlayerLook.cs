using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform camFollowPos;
    [SerializeField] private float mouseSensitivity = 1f;

    private float xRotation;
    private float yRotation;
    private PlayerInput playerInput;
    private InputAction lookAction;

    private void Start()
    {
        //Set up input
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            lookAction = playerInput.actions["Look"];
            lookAction?.Enable();
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        lookAction?.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (lookAction != null)
        {
            // Get input from new Input System
            Vector2 lookDelta = lookAction.ReadValue<Vector2>();

            // Apply sensitivity
            xRotation += lookDelta.x * mouseSensitivity;
            yRotation -= lookDelta.y * mouseSensitivity;

            // Clamp vertical rotation
            yRotation = Mathf.Clamp(yRotation, -30f, 60f);
        }   
    }

    private void LateUpdate()
    {
        if (camFollowPos != null)
        {
            // Apply rotations
            camFollowPos.localEulerAngles = new Vector3(yRotation, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
            transform.eulerAngles = new Vector3(0f, xRotation, 0f);
        }
    }
}
