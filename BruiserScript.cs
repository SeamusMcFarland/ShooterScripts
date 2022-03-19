using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BruiserScript : EnemyScript
{
    public float healthVar;
    public float speedVar;
    public bool blinker;
    float timeToBlink;

    public override void assignTraits()
    {
        SetHealth(healthVar);
        SetDamage(2f);
        SetAfterlag(0.60f);
        SetStartup(0f);
        SetBaseSpeed(speedVar);
        SetSightDistance(11f);
        SetLightSensitivity(1.1f);
        SetHearingDistance(8f);
        SetHitstun(0.2f);
        SetStrikeDistance(1.5f);
        SetVisionRange(20f);
        SetPackDistance(5f);
    }

    public override void Gore()
    {
        for (int i = 0; i < 5; i++)
            GetBloodMS().SpawnBlood(new Vector3(transform.position.x + Random.Range(-1.5f, 1.5f), transform.position.y, transform.position.z + Random.Range(-1.5f, 1.5f)));
        GetGameManagerS().PlayKillsound();
    }

    List<GameObject> tempList = new List<GameObject>();

    public override void PackReaction()
    {
        tempList = GetSameNearby();
        foreach (GameObject o in tempList)
        {
            o.transform.parent.GetComponent<BruiserScript>().DetectedPlayer();
            o.transform.parent.GetComponent<BruiserScript>().Aggroed();
        }
    }

    public override void AddedMethods()
    {
        if (blinker)
        {
            if (timeToBlink <= 0)
            {
                print("blinking");
                foreach (MeshRenderer mr in GetMrs())
                    mr.enabled = false;
                foreach (BoxCollider co in GetCos())
                    co.enabled = false;
                timeToBlink = 1.5f;
                StartCoroutine("Reappear");
            }
            else
                timeToBlink -= Time.deltaTime;
        }

    }

    IEnumerator Reappear()
    {
        yield return new WaitForSeconds(0.3f);
        foreach (MeshRenderer mr in GetMrs())
            mr.enabled = true;
        foreach (BoxCollider co in GetCos())
            co.enabled = true;
    }
}
