using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateScript : MonoBehaviour
{
    List<GameObject> pressingObjects = new List<GameObject>();
    MeshRenderer mr;
    public Material redM, greenM;
    bool wasActive;

    public int PRESSURE_TYPE; // functions as constant during game, though can be changed in unity editor.
    public GameObject objectAppear; // for type 1 

    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        wasActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (pressingObjects.Count < 1)
        {
            if (wasActive)
                DeactivateTask();
            wasActive = false;
            mr.material = redM;
            transform.localScale = new Vector3(transform.localScale.x, 0.3f, transform.localScale.z);
        }
        else
        {
            if (!wasActive)
                ActivateTask();
            wasActive = true;
            mr.material = greenM;
            transform.localScale = new Vector3(transform.localScale.x, 0.1f, transform.localScale.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("ground"))
            pressingObjects.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("ground"))
            pressingObjects.Remove(other.gameObject);
    }

    private void ActivateTask()
    {
        if (PRESSURE_TYPE == 1)
        {
            objectAppear.SetActive(true);
        }
        else
        {
            print("ERROR! INVALID PRESSURE_TYPE ON ACTIVATE/DEACTIVATE!");
        }
    }

    private void DeactivateTask()
    {
        if (PRESSURE_TYPE == 1)
        {
            objectAppear.SetActive(false);
        }
        else
        {
            print("ERROR! INVALID PRESSURE_TYPE ON ACTIVATE/DEACTIVATE!");
        }
    }

}
