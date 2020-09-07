using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    float projSpeed = 150f;
    Vector3 force;
    Vector3 startPos;
    private Rigidbody rb;
    float currentTime = 0f;
    float totalLifeTime = 3f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = this.transform.position;
        MoveProjectile();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= totalLifeTime)
        {
            Destroy(gameObject);
        }
    }
    void MoveProjectile()
    {
        force =  this.transform.forward * projSpeed;
        rb.AddForce(force, ForceMode.Impulse);
    }

}
