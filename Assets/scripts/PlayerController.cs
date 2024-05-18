using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    //Make the rigib body speed drag the speed cap down, but respects the contrains of state cap speed
    [Header("HUD")]
    public Image damageMultiplierBar; 

    [Header("Sounds")]
    public AudioSource footstepSound;
    public AudioSource sprintSound;
    public AudioSource jumpSound;
    public AudioSource slideSound;

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
    private Coroutine healthCoroutine;

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
    
    float healthRegen = 5;
    bool exitingSlope;

    Vector3 moveDirection;

    Rigidbody rb;

    public Health health;

    public float damageMultiplier = 1f;
    
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
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 2f, ground);
        
        myInput();  
        SpeedControl();
        stateHandler();
        manageSound();

         // handle drag
        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0; 

        if (health.HealthPlayer < 200) {
            if (rb.velocity.magnitude > 1f)
            health.HealthPlayer += ((healthRegen / rb.velocity.magnitude) * Time.deltaTime); 
            else health.HealthPlayer += 8 * Time.deltaTime;
        }

        if (damageMultiplier <= 30) {
            if (rb.velocity.magnitude > 12f) 
            damageMultiplier += (rb.velocity.magnitude * Time.deltaTime) / 10;
            else damageMultiplier = 1;

            if (damageMultiplier > 30) damageMultiplier = 30;
        }
        Debug.Log(damageMultiplier);
        damageMultiplierBar.fillAmount = damageMultiplier / 30f;
    }

    void manageSound() {
       if (rb.velocity.magnitude > 1f) {
        switch (state) {
            case playerState.Walking:
                footstepSound.enabled = true;
                sprintSound.enabled = false;
                slideSound.enabled = false;
                break;
            case playerState.Sprinting:
                footstepSound.enabled = false;
                sprintSound.enabled = true;
                slideSound.enabled = false;
                break;
            case playerState.Sliding:
                footstepSound.enabled = false;
                sprintSound.enabled = false;
                slideSound.enabled = true;
                break;
            default:
                footstepSound.enabled = false;
                sprintSound.enabled = false;
                slideSound.enabled = false;
                break;
            }
        } else {
            footstepSound.enabled = false;
            sprintSound.enabled = false;
            slideSound.enabled = false;
        }
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

   
    
    void myInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            jumpSound.Play();
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
