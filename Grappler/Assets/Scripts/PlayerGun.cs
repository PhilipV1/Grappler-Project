using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGun : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject gunBarrel;
    public GameObject projPrefab;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            FireProjectile();
        }
    }

    void FireProjectile()
    {
        GameObject temp = projPrefab;
        temp = Instantiate(temp, gunBarrel.transform.position, this.transform.rotation);
    }
}
