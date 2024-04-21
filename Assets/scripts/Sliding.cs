using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{

    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    Rigidbody rb;
    PlayerController playerMovement;

    [Header("Sliding")]
    public float slideForce;
    public float maxSlideTime;
    public float slideTimer;

    float slideYScale;
    float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    float horizontalInput;
    float verticalInput;

  


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerController>();

        startYScale = playerObj.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0)) {
            StartSlide();
        }

        if (Input.GetKeyUp(slideKey) && playerMovement.isSliding) {
            StopSlide();
        }
    }

    private void FixedUpdate() {
        if (playerMovement.isSliding) {
            SlidingMovement();
        }
    }

    private void StartSlide() {
       playerMovement.isSliding = true;

       playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
       rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

       slideTimer = maxSlideTime;
    }

    private void StopSlide() {
        playerMovement.isSliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }

    private void SlidingMovement() {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(!playerMovement.onSlope() || rb.velocity.y > -0.1f) {
        rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
        slideTimer -= Time.deltaTime;
        }
        else {
        rb.AddForce(playerMovement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if(slideTimer <= 0) {
            StopSlide();
        }
    }
}
