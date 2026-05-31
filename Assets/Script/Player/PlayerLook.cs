using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform camFollowPos;//follow object camera
    [SerializeField] private float mouseSensitivity = 1f;//do nhay
    //lay do xoay theo cach use player input
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
            lookAction = playerInput.actions["Look"];//call action look da set trong player input component
            lookAction?.Enable();//kich hoat action
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;//shiftlock
    }
    //ham shiftlock(use for dialogue)
    private void OnEnable()
    {
        lookAction?.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //ham thoat shiftlock
    public void OnDisable()
    {
        lookAction?.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (lookAction != null && Cursor.lockState == CursorLockMode.Locked)
        {
            // Get input from new Input System
            Vector2 lookDelta = lookAction.ReadValue<Vector2>();//doc look

            // Apply sensitivity
            xRotation += lookDelta.x * mouseSensitivity *Time.deltaTime;
            yRotation -= lookDelta.y * mouseSensitivity *Time.deltaTime;

            // limit vertical rotation
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
