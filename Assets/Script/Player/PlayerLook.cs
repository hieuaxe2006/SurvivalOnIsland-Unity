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
        //get input action for look
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            lookAction = playerInput.actions["Look"];
            lookAction?.Enable();
        }
        //lock cursor at start
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
            //get vecter2
            Vector2 lookDelta = lookAction.ReadValue<Vector2>();

            xRotation += lookDelta.x * mouseSensitivity * Time.deltaTime;
            yRotation -= lookDelta.y * mouseSensitivity * Time.deltaTime;

            // Clamp vertical rotation
            yRotation = Mathf.Clamp(yRotation, -40f, 70f);
        }
    }

    private void LateUpdate()
    {
        if (camFollowPos != null)
        {
            //rotate cam
            camFollowPos.localEulerAngles = new Vector3(yRotation, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
            //reset rotation(keep y rotation to clamp vertical)
            transform.eulerAngles = new Vector3(0f, xRotation, 0f);
        }
        else
        {
            Debug.LogWarning("Camera follow position is not assigned.");
        }
    }
}
