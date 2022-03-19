using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinOrbScript : MonoBehaviour
{
    public GameObject youWinP;
    bool winning;

    // Start is called before the first frame update
    void Start()
    {
        winning = false;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!winning)
        {
            if (other.CompareTag("player"))
            {
                winning = true;
                print("winning");
                Instantiate(youWinP, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.Euler(-90f, 0, 0), transform);
                GameObject.Find("Game Manager").GetComponent<GameManagerScript>().Win();
            }
        }
    }
}
