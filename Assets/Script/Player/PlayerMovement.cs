using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float gravity = -9.8f;//- de roi 
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
        //get component
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInChildren<Animator>();
        //get actions and enable
        move = playerInput.actions["move"];
        jump = playerInput.actions["jump"];
        move.Enable();
        jump.Enable();
    }
    // Update is called once per frame
    void Update()
    {
        //check ground  
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded && velocity.y < 0)//if player touched ground and is dropping
        {
            velocity.y = -2f;//set sau 1 ti de phu hop voi map ko phang
        }
        //set move forward-back-right-left
        Vector2 input = move.ReadValue<Vector2>();
        float x = input.x;
        float z = input.y;//z to forward and use y input
        //move
        Vector3 movePlayer = transform.right * x + transform.forward * z;
        characterController.Move(movePlayer * speed * Time.deltaTime);
        //Anm move
        float speedVelocity = characterController.velocity.magnitude;//get real velocoty
        float speedPercent = speedVelocity / speed;//get percent velocity
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

        //if player is on ground ->jump
        // Use Input System jump action instead of legacy Input.GetKeyDown
        if (jump.WasPressedThisFrame() && isGrounded)
        {
            // Apply jump using physics formula v = sqrt(2 * g * h)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        //roi xuong
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    //anm attack
    public void Attack()
    {
        animator.SetTrigger("Hit");
    }    
    
}
