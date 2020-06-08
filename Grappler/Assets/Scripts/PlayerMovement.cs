using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class PlayerMovement : MonoBehaviour
{
    private static PlayerMovement _instance;

    public static PlayerMovement Instance    { get { return _instance; } }


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

    //Movement speed multiplier for moving in x and z axis
    float moveSpeed = 10.0f;

    //Jumping speed and gravity multipliers
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
    float groundRadius = 0.7f;
    public LayerMask groundMask;
   
    //Check if the player is currently jumping or grounded
    [SerializeField]bool isJumping;
    public bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
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
            isGrounded = CheckIfGrounded();
            GetPlayerXZVelocity();
            GetJumpVelocity();
            MovePlayer();
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
    void GetPlayerXZVelocity()
    {
        float xMovement = Input.GetAxis("Horizontal");
        float zMovement = Input.GetAxis("Vertical");
        xAxis = xMovement;
        zAxis = zMovement;
        if (isGrounded)
        {
            velocity = transform.right * xMovement * moveSpeed * Time.deltaTime + transform.forward * zMovement * moveSpeed * Time.deltaTime;
        }
        else
        {
            if (zMovement < -0.1f)
            {
                zMovement = -0.1f;
            }
            Vector3 tempVector = transform.right * xMovement * moveSpeed * Time.deltaTime + transform.forward * zMovement *moveSpeed * Time.deltaTime;
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
            velocity.y = yVelocity * Time.deltaTime;
        }
        else
        {
            velocity.y = -0.1f * Time.deltaTime;
        }

        charController.Move(velocity);
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
}
