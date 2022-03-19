using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveScript : MonoBehaviour
{

    public float max, min;
    public bool isXAxis;

    bool goingDown;

    float speed;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        speed = 4f;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (goingDown)
        {
            if (isXAxis)
            {
                if (transform.position.x < min)
                    goingDown = false;
                else
                    rb.velocity = new Vector3(-speed, 0, 0);
            }
            else
            {
                if (transform.position.z < min)
                    goingDown = false;
                else
                    rb.velocity = new Vector3(0, 0, -speed);
            }
        }
        else
        {
            if (isXAxis)
            {
                if (transform.position.x > max)
                    goingDown = true;
                else
                    rb.velocity = new Vector3(speed, 0, 0);
            }
            else
            {
                if (transform.position.z > max)
                    goingDown = true;
                else
                
                    rb.velocity = new Vector3(0, 0, speed);
                
            }
        }
    }
}
