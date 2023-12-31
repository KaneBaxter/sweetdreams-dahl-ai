using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed; // Float variable for movement speed
    public float walkSpeed; // Controls the playeer walk speed
    public float sprintSpeed; //Controls the player sprint speed

    public float groundDrag; // Float variable for groundDrag

    public float jumpForce; // Float variable jumpforce
    public float jumpCooldown; // Float variable for time between jumps
    public float airMultiplier; // Float variable for the air time speed multiplier
    bool readyToJump; // True or false if player is ready to jump



    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space; //Controls the keybinding for jump
    public KeyCode sprintKey = KeyCode.LeftShift; // Controsl the keybinding for sprint 

    [Header("Ground Check")]
    public float playerHeight; // Public float that controls player height
    public LayerMask whatIsGround; // Public LayerMask, that will recognize objects that contain layer: whatisGround
    bool grounded; // Bool that determines if player is on the ground by true and false

    public Transform orientation; //Controls player orientation

    float horizontalInput; // Horizontal sensitivity
    float verticalInput; // Vertical sensitivity

    Vector3 moveDirection; // Vector 3 for move direction 

    Rigidbody rb; // Reference to rigidbody

    public MovementState state; //Stores current player state
    public enum MovementState //Sets three states that the player can be in. These states being walking, sprinting and air
    {
        walking,
        sprinting,
        air
    }

    private void Start() // On start
    {
        rb = GetComponent<Rigidbody>(); // gives variable "rb" rigidbody
        rb.freezeRotation = true;

        readyToJump = true; // The default state for readytojump is true, meaning that on start the player can jump
    }

    private void Update() // Update every frame
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround); //Raycasts from the player body, by the distance of player height multiplied by half player height + 3

        MyInput(); // Calls function MyInput
        SpeedControl(); //Calls function SpeedControl
        StateHandler(); // CAlls the stateHandler funtion

        // handle drag
        if (grounded) //If the player is grounded give drag equal to the value of groundDrag
            rb.drag = groundDrag;
        else
            rb.drag = 0; //If the player is airborn/not on the ground do not apply ground drag
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        // Mode - Sprinting
        if(grounded && Input.GetKey(sprintKey)) //if the sprint key is being pressed, set the state of the player to sprinting and set the move speed to the set sprint speed
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }   

        //Mode - Walking     
        else if(grounded) //If the sprint key is not being held set the player statee to walking and set the movement speed to the set walk speed
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        //Mode - Air

        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
}