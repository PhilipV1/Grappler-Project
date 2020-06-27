using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
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

    //Movement speed multiplier for moving in x and z axis
    float moveSpeed = 10.0f;
    float dampSpeed = 0.3f;
    float xVelocity = 0.0f;
    float zVelocity = 0.0f;

    //Jumping and gravity variables
    float jumpHeight = 3.5f;
    float gravity = -9.81f * 2;
    float yVelocity = 0.0f;
    public float airMultiplier = 0.6f;
    Vector3 jumpDirVector;
    [SerializeField]float timeToHighestPoint;
    [SerializeField]float airTimeCounter;
    [SerializeField] float totalAirTime;

    //Grappling hook variables
    Vector3 hookPosition;
    public GameObject lineOrigin;

    //Debug for Vertical and Horizontal axis
    [SerializeField]float xAxis;
    [SerializeField]float zAxis;
    
    //Vector storing the velocity of the character
    [SerializeField]Vector3 velocity;
    private Vector3 refVelocity = Vector3.zero;//Reference vector for the Smoothdamp function

    //using the Physics.CheckSphere to see if the player is grounded. The function will only check against anything that is masked as ground
    public Transform groundSphere;
    float groundRadius = 0.6f;
    public LayerMask groundMask;
   
    //Check if the player is currently jumping or grounded
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
        airTimeCounter = 0f;
        totalAirTime = 0f;
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
                    ResetState();
                    break;
            }
                
            //if (state == PlayerState.Normal)
            //{
            //    isGrounded = CheckIfGrounded();
            //    GetPlayerXZVelocity();
            //    GetJumpVelocity();
            //    MovePlayer();
            //}
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
        }
        else
        {
            velocity += input * moveSpeed;
            //if (Vector3.Dot(input.normalized, velocity.normalized) > -0.1f)
            //{
            //     velocity += input * moveSpeed;
            //}
            //else
            //{
            //    velocity = Vector3.SmoothDamp(velocity, new Vector3(0f, velocity.y, 0f), ref refVelocity, dampSpeed);
            //}

            if (input.z == 0)
            {
                velocity.z = Mathf.SmoothDamp(velocity.z, 0f, ref zVelocity, dampSpeed);
            }
            if (input.x == 0)
            {
                velocity.x = Mathf.SmoothDamp(velocity.x, 0f, ref xVelocity, dampSpeed);
            }

            
        }
        velocity = Vector3.ClampMagnitude(velocity, 10.0f);
    }
    float TimeToHighestJumpPoint()
    {
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
            yVelocity += gravity * Time.deltaTime;
            velocity.y = yVelocity;
        }
        else
        {
            velocity.y = -0.1f * Time.deltaTime;
        }

        charController.Move(velocity * Time.deltaTime);
    }
    //Checks if the player is currently on the ground
  public bool CheckIfGrounded()
    {
        //Checking the sphere if the player is currently within the reach of being grounded
        if(Physics.CheckSphere(groundSphere.position, groundRadius, groundMask) && !isJumping)
        {    
            return true;
        }
        if (isJumping)
        {
            StartAirTimer();
            //Having two if statements in case of unnecessary computing with the CheckSphere
            if (Physics.CheckSphere(groundSphere.position, groundRadius, groundMask) && airTimeCounter >= timeToHighestPoint)
            {
                //Checking if the player touches the ground after jumping and sets isJumping to false and resets the air timer
                isJumping = false;
                jumpDirVector = Vector3.zero;
                
                totalAirTime = airTimeCounter;
                ResetAirTimer();
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
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hitInfo)){
                hookPosition = hitInfo.point;
                state = PlayerState.GrapplingShot;
                rayCastHitDebug.transform.position = hitInfo.point;
                isHooking = true;
            }
        }
    }
    private void SpawnRope()
    {
        Vector3[] positions = new Vector3[2];
        positions[0] = lineOrigin.transform.position;
        positions[1] = hookPosition;

        grapplingRope.SetPositions(positions);

    }
    private void ResetState()
    {
        if (state == PlayerState.GrapplingShot)
        {
            state = PlayerState.Normal;
            isHooking = false;
        }
    }
}
