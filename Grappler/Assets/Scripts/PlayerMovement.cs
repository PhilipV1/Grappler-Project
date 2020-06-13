using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class PlayerMovement : MonoBehaviour
{
    private static PlayerMovement _instance;

    public static PlayerMovement Instance    { get { return _instance; } }

    private enum PlayerState
    {
        Normal,
        HookThrow,
        HookFlying
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

    //Variable for storing the player state
    private PlayerState state;

    //Movement speed multiplier for moving in x and z axis
    float moveSpeed = 10.0f;

    //Jumping and gravity variables
    float jumpHeight = 3.5f;
    float gravity = -9.81f * 2;
    float yVelocity = 0.0f;
    public float airMultiplier = 0.6f;
    Vector3 jumpDirVector;
    [SerializeField]float timeToHighestPoint;
    [SerializeField]float airTimeCounter;
    [SerializeField] float totalAirTime;

    //Debug for Vertical and Horizontal axis
    [SerializeField]float xAxis;
    [SerializeField]float zAxis;
    
    //Vector storing the velocity of the character
    [SerializeField]Vector3 velocity;

    //using the Physics.CheckSphere to see if the player is grounded. The function will only check against anything that is masked as ground
    public Transform groundSphere;
    float groundRadius = 0.6f;
    public LayerMask groundMask;
   
    //Check if the player is currently jumping or grounded
    [SerializeField]bool isJumping;
    public bool isGrounded;

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
            if (state == PlayerState.Normal)
            {
                isGrounded = CheckIfGrounded();
                GetPlayerXZVelocity();
                GetJumpVelocity();
                MovePlayer();
            }
        }
    }
    //Makes the character jump based on a formula using jump height
    void GetJumpVelocity()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            if (velocity.z > 0.07f || velocity.x > 0.07f)
            {
                Debug.LogError("Jumpboost Bug");
            }
            jumpDirVector = velocity;
            yVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
            isJumping = true;
        }
    }
    void GetPlayerXZVelocity()    {
        float xMovement = Input.GetAxis("Horizontal");
        float zMovement = Input.GetAxis("Vertical");
        xAxis = xMovement;
        zAxis = zMovement;
        if (isGrounded)
        {
            Vector3 tempVector = transform.right * xMovement + transform.forward * zMovement;
            tempVector.Normalize();
            tempVector *= moveSpeed;
            // velocity.Normalize();
            // float targetSpeed = Mathf.Min(velocity.magnitude, 1.0f);
            // velocity *= targetSpeed;
            velocity = tempVector;

        }
        else
        {

            Vector3 tempVector = transform.right * xMovement + transform.forward * zMovement;
            tempVector.Normalize();
            tempVector *= moveSpeed;
            velocity = jumpDirVector;
        }
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
}
