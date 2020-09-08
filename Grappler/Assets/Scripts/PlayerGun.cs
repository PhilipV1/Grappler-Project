using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGun : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject gunBarrel;
    public GameObject projPrefab;
    public GameObject crosshair;
    GameObject debugLine;

    void Start()
    {
        debugLine = new GameObject("DebugLine");
        debugLine.gameObject.AddComponent<LineRenderer>();
        debugLine.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            FireProjectile();
        }
    }

    void FireProjectile()
    {
        GameObject temp = projPrefab;
        temp = Instantiate(temp, gunBarrel.transform.position, this.transform.rotation);


        //Casting a ray to see where the crosshair would hit and taking the position to aim the projectile to that point
        Ray tempRay = Camera.main.ScreenPointToRay(crosshair.transform.position);
        
        RaycastHit hit;
        if (Physics.Raycast(tempRay, out hit))
        {
            //Debug ray to see where the player hit
            //DrawLine(debugLine.GetComponent<LineRenderer>(), tempRay.origin, hit.point);
            
            temp.GetComponent<Projectile>().target = hit.point;
        }
        else
        {
            Ray crosshairRay = Camera.main.ScreenPointToRay(crosshair.transform.position);
            Vector3 rayPoint = crosshairRay.GetPoint(1000.0f);
            Vector3 projDir = (rayPoint - temp.transform.position).normalized;
            temp.GetComponent<Projectile>().target = rayPoint;
           // DrawLine(debugLine.GetComponent<LineRenderer>(), crosshairRay.origin, rayPoint);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    void DrawLine(LineRenderer line, Vector3 startPos, Vector3 endPos)
    {
        //Creating temporary primitive to access a default material
        GameObject tempPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempPrimitive.SetActive(false);
        Material diffuseMat = tempPrimitive.GetComponent<MeshRenderer>().sharedMaterial;
        Destroy(tempPrimitive);

        //Activates the debugLine gameobject when shooting
        debugLine.SetActive(true);
        //Setting material properties
        line.material = diffuseMat;
        line.widthMultiplier = 0.2f;
        line.positionCount = 2;
        //Setting the start and end point for the line
        Vector3[] lineArray = new Vector3[2];
        lineArray[0] = startPos;
        lineArray[1] = endPos;
        line.SetPositions(lineArray);
    }
}
