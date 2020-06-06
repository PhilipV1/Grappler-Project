using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

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
        Quaternion yQuaternion = Quaternion.AngleAxis(mouseX, Vector3.right);

        //transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (PlayerMovement.Instance.isGrounded)
        {

            //Having problems with jumping. Sometimes after landing the next jump will flip the camera as if the character had a wider angle than 60 degrees away from the camera and therefore clamping it
            playerObject.transform.eulerAngles = new Vector3(playerObject.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, playerObject.rotation.eulerAngles.z);
            transform.eulerAngles = new Vector3(pitch, yaw, 0);
        }
        else
        {
     
            float angle = Vector3.Angle(playerObject.transform.forward, transform.forward);

            //Fix the mirror clamping rotation caused by the euler rotation going from 0-360
            //When jumping, clamp the cameras rotation by 60 degrees negative and positive from the players Y rotation
            //float minClamp = (playerObject.transform.rotation.eulerAngles.y + 300) % 360;
            //float maxClamp = (playerObject.transform.rotation.eulerAngles.y + 60) % 360;

            //Transform EULERANGLES TO -180/180 INSTEAD OF 0-360
            if (playerEulerY > 180)
            {
                float newEulerY = playerEulerY - 360;
                maxDegreeY = newEulerY + maxAngle;
                minDegreeY = newEulerY - maxAngle;
            }
            else
            {
                maxDegreeY = playerEulerY + maxAngle;
                minDegreeY = playerEulerY - maxAngle;
            }

            yaw = EulerConverter(yaw);
            //CAMERA FLIPS FROM CLAMPING
            yaw = Mathf.Clamp(yaw, minDegreeY, maxDegreeY);

            //if (Quaternion.Angle(yQuaternion, playerObject.transform.rotation) < maxAngle)
            //{
                
            //}

            transform.eulerAngles = new Vector3(pitch, yaw, 0);
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
}
