﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    //Setting up a singleton so that this script can be used in any other script
    private static PlayerMovement _instance;

    public static PlayerMovement Instance    { get { return _instance; } }

    private enum PlayerState
    {
        Normal,
        GrapplingShot,
        GrapplingFlying
    };
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject); 
        }
        else
        {
            _instance = this;
        }
    }

    [HideInInspector]public CharacterController charController;
    public GameObject playerCam;
    public LineRenderer grapplingRope;
    public GameObject rayCastHitDebug;

    //Variable for storing the player state
    private PlayerState state;

    //Movement speed multiplier for moving in x and z axis and damp variables
    float moveSpeed = 10.0f;
    float dampSpeed = 0.3f;
    float xVelocity = 0.0f;
    float zVelocity = 0.0f;
    //Reference vector to dampen the airmomentum vector
    private Vector3 airRefVector = Vector3.zero;

    //Jumping and gravity variables
    float jumpHeight = 3.5f;
    float gravity = -9.81f * 4;
    float yVelocity = 0.0f;
    float airMultiplier = 0.6f;
    Vector3 jumpDirVector;

    //Variable to keep momentum after releasing the hook in mid air
    public Vector3 airMomentum;
    [Header("Jump Variables")]
    [SerializeField]float timeToHighestPoint;
    [SerializeField] float airTimeCounter = 0f;
    [SerializeField]float totalAirTime = 0f;

    //Grappling hook variables
    Vector3 hookPosition;
    [Header("Grapple Hook")]
    public GameObject lineOrigin;
    Vector3[] positions;
    float maxDistance = 5000f;
    

    //Debug for Vertical and Horizontal axis
    [Header("Movement")]
    [SerializeField]float xAxis;
    [SerializeField]float zAxis;
    
    //Vector storing the velocity of the character
    [SerializeField]Vector3 velocity;
    private Vector3 refVelocity = Vector3.zero;//Reference vector for the Smoothdamp function

    //using the Physics.CheckSphere to see if the player is grounded. The function will only check against anything that is masked as ground
    [Header("Physics")]
    public Transform groundSphere;
    float groundRadius = 0.52f;
    public LayerMask groundMask;
   
    //Check if the player is currently jumping or grounded
    [Header("Boolean Checks")]
    [SerializeField]bool isJumping;
    public bool isGrounded;
    public bool isHooking;

    // Start is called before the first frame update
    void Start()
    {
        state = PlayerState.Normal;
        charController = GetComponent<CharacterController>();
        timeToHighestPoint = TimeToHighestJumpPoint();
        isJumping = false;
        positions = new Vector3[2];
        grapplingRope.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (GameManager.gameState == GameManager.GameState.Playing)
        {
            switch (state)
            {
                default:
                case PlayerState.Normal:
                    isGrounded = CheckIfGrounded();
                    GetPlayerXZVelocity();
                    GetJumpVelocity();
                    CheckGrapplingShot();
                    MovePlayer();
                    break;
                case PlayerState.GrapplingShot:
                    SpawnRope();
                    Grapple();
                    ChangeState();
                    break;
                case PlayerState.GrapplingFlying:
                    isGrounded = CheckIfGrounded();
                    GetPlayerXZVelocity();
                    MovePlayer();
                    ChangeState();
                    break;
            }
        }
    }
    //Makes the character jump based on a formula using jump height
    void GetJumpVelocity()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpDirVector = velocity;
            yVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
            isJumping = true;
        }
    }
    void GetPlayerXZVelocity()    {
        float xMovement = Input.GetAxis("Horizontal");
        float zMovement = Input.GetAxis("Vertical");
        
        //Storing the input in a vector and then make sure that the direction is relative to the object
        Vector3 input = new Vector3(xMovement, 0.0f, zMovement);
        input = transform.TransformDirection(input);
        input = Vector3.ClampMagnitude(input, 1.0f);

        //Variables for displaying in the inspector
        zAxis = input.z;
        xAxis = input.x;

        if (isGrounded)
        {
            velocity = input;       
            velocity *= moveSpeed;
            velocity = Vector3.ClampMagnitude(velocity, 10.0f);
        }
        else
        {
            velocity += input * moveSpeed;
            //Damping the movementspeed in the air to prevent a full stop when releasing movement keys
            if (input.z == 0)
            {
                velocity.z = Mathf.SmoothDamp(velocity.z, 0f, ref zVelocity, dampSpeed);
            }
            if (input.x == 0)
            {
                velocity.x = Mathf.SmoothDamp(velocity.x, 0f, ref xVelocity, dampSpeed);
            }
            velocity = Vector3.ClampMagnitude(velocity, 10.0f);
        }
    }
    float TimeToHighestJumpPoint()
    {
        //Formula for reaching the highest point in a jump
        float fVelocity = 0;
        float iVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
        float t = (fVelocity - iVelocity) / gravity;

        return t;
    }
    void StartAirTimer()
    {
        airTimeCounter += Time.deltaTime;
    }
    void ResetAirTimer()
    {
        airTimeCounter = 0.0f;
    }
    void MovePlayer()
    {
        if (!isGrounded)
        {
            ApplyGravity();
        }
        else
        {
            velocity.y = -0.1f * Time.deltaTime;
        }
        velocity += airMomentum;
        charController.Move(velocity * Time.deltaTime);
        if (airMomentum.magnitude > 0.0f)
        {       
            //Stops Damping after a certain magnitude
            airMomentum = Vector3.SmoothDamp(airMomentum, Vector3.zero, ref airRefVector, 0.1f);
        }
    }
    //Checks if the player is currently on the ground
    public bool CheckIfGrounded()
    {
        //Checking the sphere if the player is currently within the reach of being grounded
        if(!isJumping)
        {
            return Physics.CheckSphere(groundSphere.position, groundRadius, groundMask);
        }
        //Checking if grounded when in the air allowing 
        else
        {
            if (Physics.CheckSphere(groundSphere.position, groundRadius, groundMask) && !isGrounded)
            {
                isJumping = false;
                return true;
            } 
        }
        return false;
    }

    public void ResetPosition()
    {
        Vector3 worldOrigin = new Vector3(0.0f, 5.5f, 0.0f);
        velocity = Vector3.zero;
        jumpDirVector = Vector3.zero;
        charController.enabled = false;
        charController.transform.position = worldOrigin;
        charController.transform.rotation = Quaternion.identity;
        charController.enabled = true;
    }

    private void CheckGrapplingShot()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hitInfo, maxDistance, groundMask)){
                hookPosition = hitInfo.point;
                state = PlayerState.GrapplingShot;
              //  rayCastHitDebug.transform.position = hitInfo.point;
                isHooking = true;
                grapplingRope.enabled = true;
            }
        }
    }
    private void SpawnRope()
    {
        positions[0] = lineOrigin.transform.position;
        positions[1] = hookPosition;

        grapplingRope.SetPositions(positions);
    }
    private void Grapple()
    {
        airMomentum = Vector3.zero;
        positions[0] = lineOrigin.transform.position;
        //Resetting gravity each time a new grapple starts
        velocity.y = -0.1f * Time.deltaTime;

        //Setting the hookspeed direction, speed and speed multipliers
        float hookSpeedMultiplier = 3f;
        float hookSpeed = Vector3.Distance(hookPosition, charController.transform.position);
        Vector3 playerDirection = (hookPosition - charController.transform.position).normalized;
        hookSpeed = Mathf.Clamp(hookSpeed, 15f, 35f);
        
        velocity = playerDirection * hookSpeed * hookSpeedMultiplier;

        charController.Move(velocity * Time.deltaTime);
        //Releasing hook when the distance between the player and hookPosition get too close
        if (Vector3.Distance(charController.transform.position, hookPosition) < 2.0f)
        {
            state = PlayerState.Normal;
            isHooking = false;
            grapplingRope.enabled = false;
            yVelocity = -0.1f;
        }
    }
    private void ChangeState()
    {
        switch (state)
        {
            case PlayerState.Normal:
                break;
            case PlayerState.GrapplingShot:
                GrapplingShotState();
                break;
            case PlayerState.GrapplingFlying:
                GrapplingFlyingState();
                break;
            default:
                break;
        }
    }
    private void GrapplingShotState()
    {
        //When the grappling hook is released change the playerstate, disable rope visuals and reset gravity
        if (state == PlayerState.GrapplingShot && Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            state = PlayerState.GrapplingFlying;
            isHooking = false;
            grapplingRope.enabled = false;
            yVelocity = -0.1f;
            velocity.y = -0.1f * Time.deltaTime;
        }
    }

    private void GrapplingFlyingState()
    {
        //Storing momentum after releasing the hook to prevent a sudden stop in momentum
        CheckGrapplingShot();
        if (state == PlayerState.GrapplingShot) return;
        airMomentum = velocity;
        airMomentum.y = 0f;
        airMomentum = Vector3.ClampMagnitude(airMomentum, 30f);

        if (isGrounded)
        {
            state = PlayerState.Normal;
        }
    }

    private void ApplyGravity()
    {
            yVelocity += gravity * Time.deltaTime;
            velocity.y = yVelocity;
    }
    public void ResetVelocity()
    {
        velocity = Vector3.zero;
        airMomentum = Vector3.zero;
    }

    void Shoot()
    {

    }
}

