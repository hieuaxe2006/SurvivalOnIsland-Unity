using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float gravity = -9.8f;//- de roi 
    [SerializeField] private float jump = 3f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    Vector3 velocity;
    private bool isGrounded;

    // Update is called once per frame
    void Update()
    {
        //check ground  
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)//if player touched ground and is dropping
        {
            velocity.y = -2f;//set sau 1 ti de phu hop voi map ko phang
        }
        //set move forward-back-right-left
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        //move
        Vector3 move = transform.right * x + transform.forward * z;
        characterController.Move(move * speed * Time.deltaTime);

        //if player is on ground ->jump
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            //Cong thuc van toc v^2=2hg
            velocity.y = Mathf.Sqrt(jump * -2f * gravity);//khai can va *-2 de ra duong
        }
        //roi xuong
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
