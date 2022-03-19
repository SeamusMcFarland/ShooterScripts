using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinScript : MonoBehaviour
{

    bool collected;
    MeshRenderer mr;

    // Start is called before the first frame update
    void Start()
    {
        collected = false;
        mr = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (collected)
            mr.enabled = false;
        else
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + 1f, transform.localEulerAngles.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!collected)
        {
            if (other.CompareTag("player"))
            {
                collected = true;
                GameObject.Find("Game Manager").GetComponent<GameManagerScript>().ScorePoint();
            }
        }
    }
}
