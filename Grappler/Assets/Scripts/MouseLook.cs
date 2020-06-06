using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class MouseLook : MonoBehaviour
{
    public enum CameraRotation {RotationY, RotationX }
    public CameraRotation rotationType;
    public float mouseSpeed;
    public Transform playerObject;
    public Transform cameraHolder;

    Quaternion center;

    float pitch = 0.0f;
    float yaw = 0.0f;

    float maxAngle = 60.0f;
    float maxDegreeY = 0.0f;
    float minDegreeY = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
       
        mouseSpeed = 60.0f;
        center = this.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        SetPositionToPlayerModel();

        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSpeed;
 
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSpeed;
        //xRot(pitch) needs to be assigned as -= mouseY to get the correct rotation values(yRot(yaw) does not seem to function the same)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80, 80);
        yaw += mouseX;

        float playerEulerY = playerObject.transform.rotation.eulerAngles.y;
  
        //Using quaternions
        Quaternion horizontalQ = Quaternion.AngleAxis(mouseX, playerObject.up);
        Quaternion verticalQ = Quaternion.AngleAxis(mouseY, -playerObject.right);
        Quaternion zQuaternion = Quaternion.AngleAxis(0, this.transform.forward);


        if (PlayerMovement.Instance.isGrounded)
        {
            //rotating the player according to where the camera is looking
            playerObject.transform.eulerAngles = new Vector3(playerObject.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, playerObject.rotation.eulerAngles.z);

          
            //Handle rotation around the Y axis with quaternions to be able to clamp properly while also keeping the quaternion multiplication using only one axis(no Z-axis rotation)
            Quaternion tempCamRot = this.transform.localRotation * horizontalQ;
            
            this.transform.localRotation = tempCamRot;
            this.transform.eulerAngles = new Vector3(pitch, this.transform.eulerAngles.y, 0);

        }
        else
        {
            Quaternion tempRotY = this.transform.localRotation * horizontalQ;

            if (Quaternion.Angle(playerObject.rotation, tempRotY)<maxAngle)
            {
                this.transform.localRotation = tempRotY;              
            }
            this.transform.eulerAngles = new Vector3(pitch, this.transform.eulerAngles.y, 0);
        }

    }
    void SetPositionToPlayerModel()
    {
        this.transform.position = cameraHolder.transform.position;
    }

    float CalculateAngle(float a, float b)
    {
        float retVal = 0.0f;
        if (a > b)
        {
            retVal = a - b % 360;
        }
        else
        {
            float maxDegree = 360.0f;
            float result = a - b;
            retVal = maxDegree + result;
        }

        return retVal;
    }

    float EulerConverter(float eulerAngle)
    {
        float retVal = 0.0f;

        if (eulerAngle > 180)
        {
            retVal = eulerAngle - 360;    

        }
        else if(eulerAngle < -180)
        {
            retVal = eulerAngle + 360;
        }
        else
        {
            retVal = eulerAngle;
        }
        return retVal;
    }

    void QuaternionRotation()
    {

    }
}
