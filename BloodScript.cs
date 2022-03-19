using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodScript : MonoBehaviour
{
    public float[] XYdimensions;
    public Material[] bloodM;
    private int chosen;

    // Start is called before the first frame update
    void Start()
    {
        chosen = 0;
    }

    public void PlaceBlood(Vector3 pos)
    {
        chosen = Random.Range(0, 2); // choses blood sprite
        GetComponent<Renderer>().material = bloodM[chosen];
        transform.localScale = new Vector3(XYdimensions[chosen * 2], XYdimensions[chosen * 2 + 1], 1f);
        transform.position = pos;
        transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
        transform.position = new Vector3(transform.position.x + Random.Range(-.7f, .7f), Random.Range(0.0001f, 0.0010f), transform.position.z + Random.Range(-.7f, .7f));
    }

    public void PlaceBlood(Vector3 pos, int modColor)
    {
        if (modColor == 0)
            chosen = Random.Range(0, 2); // choses blood sprite
        else if (modColor == 1)
            chosen = Random.Range(3, 5);
        else if (modColor == 2)
            chosen = Random.Range(6, 8);
        else
            print("ERROR! INVALID BLOOD TYPE");
        GetComponent<Renderer>().material = bloodM[chosen];
        transform.localScale = new Vector3(XYdimensions[chosen * 2], XYdimensions[chosen * 2 + 1], 1f);
        transform.position = pos;
        transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
        transform.position = new Vector3(transform.position.x + Random.Range(-.7f, .7f), Random.Range(0.0001f, 0.0010f), transform.position.z + Random.Range(-.7f, .7f));
    }

    public void FadeBlood()
    {

    }

}
