using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 3f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    Vector3 velocity;
    private bool isGrounded;
    private Animator animator;

    [Header("Footstep Settings")]
    [SerializeField] private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;

    private PlayerInput playerInput;
    private InputAction move;
    private InputAction jump;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInChildren<Animator>();
        move = playerInput.actions["move"];
        jump = playerInput.actions["jump"];
        move.Enable();
        jump.Enable();
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force for uneven terrain
        }

        // Movement input
        Vector2 input = move.ReadValue<Vector2>();
        float x = input.x;
        float z = input.y;
        Vector3 movePlayer = transform.right * x + transform.forward * z;
        characterController.Move(movePlayer * speed * Time.deltaTime);

        // Animation
        float speedVelocity = characterController.velocity.magnitude;
        float speedPercent = speedVelocity / speed;
        animator.SetFloat("Speed", speedPercent);

        // Footstep sounds
        if (isGrounded && speedVelocity > 0.1f)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayFootstep();
                }
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = footstepInterval; // Reset so next step sounds immediately
        }

        // Jump
        if (jump.WasPressedThisFrame() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>Triggers the attack animation.</summary>
    public void Attack()
    {
        animator.SetTrigger("Hit");
    }
}
