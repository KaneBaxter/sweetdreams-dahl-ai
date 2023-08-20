using UnityEngine;
using UnityEngine.Windows;

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
    public KeyCode crouchKey = KeyCode.LeftControl; // Controls the keybinding for crouching

    [Header("Crouching")]
    public float crouchSpeed; //Movement speed when crouching
    public float crouchYScale; //how far player crouchs 
    private float startYScale; //The starting player height

    [Header("Ground Check")]
    public float playerHeight; // Public float that controls player height
    public LayerMask whatIsGround; // Public LayerMask, that will recognize objects that contain layer: whatisGround
    bool grounded; // Bool that determines if player is on the ground by true and false

    [Header("Slope handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

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
        crouching,
        air
    }

    private void Start() // On start
    {
        rb = GetComponent<Rigidbody>(); // gives variable "rb" rigidbody
        rb.freezeRotation = true;

        readyToJump = true; // The default state for readytojump is true, meaning that on start the player can jump

        startYScale = transform.localScale.y;
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
        horizontalInput = UnityEngine.Input.GetAxisRaw("Horizontal");
        verticalInput = UnityEngine.Input.GetAxisRaw("Vertical");

        // when to jump
        if (UnityEngine.Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

//-----------------------------------------------------------------------------------------------

        // Start crouch
        if (UnityEngine.Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }   

        // Stop crouch
        if (UnityEngine.Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
//-----------------------------------------------------------------------------------------------
    }

    private void StateHandler()
    {
        //Mode - Crouching
        if (UnityEngine.Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && UnityEngine.Input.GetKey(sprintKey)) //if the sprint key is being pressed, set the state of the player to sprinting and set the move speed to the set sprint speed
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        //Mode - Walking     
        else if (grounded) //If the sprint key is not being held set the player statee to walking and set the movement speed to the set walk speed
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

        // On slope 
        if(onSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
        }

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // Turn gravity off while on slope
        rb.useGravity = !onSlope();
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

    private bool onSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit,playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}