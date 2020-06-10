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


    float pitch = 0.0f;

    float maxAngle = 85.0f;



    // Start is called before the first frame update
    void Start()
    {
        mouseSpeed = 60.0f;
    }

    // Update is called once per frame
    void Update()
    {
        SetPositionToPlayerModel();

        //Getting the movement input for X and Y axis
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSpeed;
        //xRot(pitch) needs to be assigned as -= mouseY to get the correct rotation values(yRot(yaw) does not seem to function the same)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80, 80);


        float playerEulerY = playerObject.transform.rotation.eulerAngles.y;
  
        //Using quaternions
        Quaternion horizontalQ = Quaternion.AngleAxis(mouseX, playerObject.up);

        if (PlayerMovement.Instance.isGrounded)
        {
            //rotating the player according to where the camera is looking
            playerObject.transform.eulerAngles = new Vector3(playerObject.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, playerObject.rotation.eulerAngles.z);

            //Handle rotation around the Y axis with quaternions to be able to clamp properly while also keeping the quaternion multiplication using only one axis(preventing Z-axis rotation)
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



}
