using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform camFollowPos;
    [SerializeField] private float mouseSensitivity = 1f;

    public float xRotation;
    public float yRotation;
    private PlayerInput playerInput;
    private InputAction lookAction;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            lookAction = playerInput.actions["Look"];
            lookAction?.Enable();
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Lock cursor and enable look
    private void OnEnable()
    {
        lookAction?.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Unlock cursor and disable look
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
            Vector2 lookDelta = lookAction.ReadValue<Vector2>();

            xRotation += lookDelta.x * mouseSensitivity * Time.deltaTime;
            yRotation -= lookDelta.y * mouseSensitivity * Time.deltaTime;

            // Clamp vertical rotation
            yRotation = Mathf.Clamp(yRotation, -30f, 60f);
        }
    }

    private void LateUpdate()
    {
        if (camFollowPos != null)
        {
            camFollowPos.localEulerAngles = new Vector3(yRotation, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
            transform.eulerAngles = new Vector3(0f, xRotation, 0f);
        }
    }
}
