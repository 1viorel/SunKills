using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("HUD")]
    public Text speedText;

    [Header("Movement")]
    float movementSpeed;
    float desiredSpeed;
    float lastDesiredSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float slideSpeed;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    float crouchYScale = 0.5f;
    float startYScale;

    [Header("Slope Handling")]
    public float maxSlopeAngle;

    RaycastHit slopeHit;

    [Header("Ground Check")]
    float playerHeight;
    public LayerMask ground;
    bool grounded;

    [Header("Keybinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    bool exitingSlope;

    Vector3 moveDirection;

    Rigidbody rb;

    
    enum playerState
    {
        Walking,
        Sprinting,
        Airborne,
        Crounching,
        Sliding
    }
    playerState state;

    public bool isSliding;

    // Start is called before the first frame update
    void Start() {

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        playerHeight = transform.localScale.y;
        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update() {

        Debug.Log(state);
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 2f, ground);
        
        myInput();  
        SpeedControl();
        stateHandler();
        UpdateSpeedHUD();

         // handle drag
        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0; 

    }

    void stateHandler() {
        
        if (isSliding) {
            state = playerState.Sliding;
           
            if(onSlope() && rb.velocity.y < 0.1f) desiredSpeed = slideSpeed;
            else desiredSpeed = movementSpeed;
        }

        else if (Input.GetKey(crouchKey)) {
            state = playerState.Crounching;
            desiredSpeed = crouchSpeed;
        }

        else if(grounded && Input.GetKey(sprintKey)) {
            state = playerState.Sprinting;
            desiredSpeed = sprintSpeed;
        }

        else if(grounded) {
            state = playerState.Walking;
            desiredSpeed = walkSpeed;
        }
        else {
            state = playerState.Airborne;
        }

        //Complicated math stuff, its for speed transitions between states
        if(Mathf.Abs(desiredSpeed - lastDesiredSpeed) > 4f && movementSpeed != 0) {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else movementSpeed = desiredSpeed;
        
        //keepSpeedInCheck();
        lastDesiredSpeed = desiredSpeed;       
    }

    void keepSpeedInCheck() {

        if(rb.velocity.magnitude > movementSpeed) rb.velocity = rb.velocity.normalized * movementSpeed;
    }

    void UpdateSpeedHUD() {
    
        float currentSpeed = rb.velocity.magnitude;
        speedText.text = "Speed: " + currentSpeed.ToString("F2");
    }
    
    void myInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if(Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        
        if(Input.GetKeyUp(crouchKey)) transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }


    //complicated math speed transition stuff, keeping momentum
    private IEnumerator SmoothlyLerpMoveSpeed() {
        float t = 0;
        float difference = Mathf.Abs(desiredSpeed - movementSpeed);
        float startValue = movementSpeed;

       
        while (t < difference)
        {
            movementSpeed = Mathf.Lerp(startValue, desiredSpeed, t / difference);

            if (onSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                t += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                t += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        movementSpeed = desiredSpeed;
    }

    void movePlayer() {

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
       
        //ob slope
        if(onSlope()  && !exitingSlope) {
            rb.AddForce(GetSlopeMoveDirection(moveDirection)*movementSpeed *20f, ForceMode.Force);
            
            if(rb.velocity.y>0) rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        else if(grounded) rb.AddForce(moveDirection.normalized * movementSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded) rb.AddForce(moveDirection.normalized * movementSpeed * 10f * airMultiplier, ForceMode.Force);

        //slope gravity
        rb.useGravity = !onSlope();
    }

    void FixedUpdate() {

        movePlayer();
    }

     private void SpeedControl() {

        if(onSlope() && !exitingSlope) keepSpeedInCheck();
            
        else {
             Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if(flatVel.magnitude > movementSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * movementSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump() {

        exitingSlope = true;
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {

        readyToJump = true;
        exitingSlope = false;
    }

    public bool onSlope() {

        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 2f)) {

           float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
           return angle < maxSlopeAngle && angle !=0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction) {

        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
   
}
