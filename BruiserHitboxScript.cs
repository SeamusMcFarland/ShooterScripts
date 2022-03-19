using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruiserHitboxScript : MonoBehaviour
{
    BruiserScript bruiserS;

    // Start is called before the first frame update
    void Start()
    {
        bruiserS = transform.parent.GetComponent<BruiserScript>();
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "player")
            bruiserS.SetHitboxTriggered(true);
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.tag == "player")
            bruiserS.SetHitboxTriggered(false);
    }
}
