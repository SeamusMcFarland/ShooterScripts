using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    GameObject player;
    PlayerScript playerS;
    bool aggro;
    bool lightDetected;
    float playerDistance;
    bool playerSeen;
    bool locked;
    GameObject bloodM;
    BloodManagerScript bloodMS;
    Vector3 playerSP;
    bool hitboxTriggered;
    GameObject[] audioM;
    const int AUDIO_NUM = 2;

    float xDiff;
    float zDiff;
    float rotation;
    bool rotationLock;

    float health;
    float maxHealth;
    float damage;
    float afterlag;
    float startup;
    float baseSpeed;
    float sightDistance;
    float lightSensitivity;
    float hearingDistance;
    float hitstun;
    float strikeDistance;
    float visionRange; // angle at which the monster recieves a light-sensitive vision bonus (is only half of the angle)
    float packDistance; // reactionary distance to be aggroed by another monster of the same type

    float passiveX;
    float passiveZ;

    bool active;
    bool dead;
    bool striking;

    const float BEGIN_SLOW_DISTANCE = 5f;
    NavMeshAgent nma;
    bool speedDisabled;
    Vector3 savedPosition;
    float stuckTimeout; // once reaches a certain point without changing position, will de-aggro.

    RaycastHit hit; //pack detection variables
    bool validHit; //
    GameObject hitObject; //
    List<GameObject> targets = new List<GameObject>(); //
    int removedNum; //

    public bool omniscient; // will "see" player through walls after aggroed

    GameManagerScript gameManagerS;

    MeshRenderer[] mrs;
    BoxCollider[] cos;

    //TO CHANGE IF MONSTER ADDED: AcidScript, FireScript, PlayerScript (bullet), ExplosiveBarrelScript

    // Start is called before the first frame update
    void Start()
    {
        setupVars();
        assignTraits();
        StartCoroutine("FindTheManager");
    }

    IEnumerator FindTheManager()
    {
        yield return new WaitForSeconds(0.01f);
        gameManagerS = GameObject.Find("Game Manager").GetComponent<GameManagerScript>();
    }

    private void setupVars()
    {
        mrs = GetComponentsInChildren<MeshRenderer>();
        cos = GetComponentsInChildren<BoxCollider>();
        speedDisabled = false;
        rotationLock = false;
        nma = GetComponent<NavMeshAgent>();
        striking = false;
        dead = false;
        active = true;
        GetComponent<NavMeshAgent>().updateUpAxis = false;
        player = GameObject.FindGameObjectWithTag("player");
        playerS = player.GetComponent<PlayerScript>();
        aggro = false;
        playerSeen = false;
        locked = false;
        bloodM = GameObject.Find("BloodManager");
        bloodMS = bloodM.GetComponent<BloodManagerScript>();
        playerSP = transform.position;
        hitboxTriggered = false;
        passiveX = transform.position.x;
        passiveZ = transform.position.y;
        lightDetected = false;

    }

    public virtual void assignTraits()
    {
        health = 1f;
        damage = 1f;
        afterlag = 1;
        startup = 0f;
        baseSpeed = 1f;
        sightDistance = 1f;
        lightSensitivity = 1f; // is a multiplier
        hearingDistance = 2f;
        hitstun = 1f;
        strikeDistance = 2f;
        visionRange = 20f;
        packDistance = 4f;
    }

    // Update is called once per frame
    void Update()
    {
        AddedMethods();

        if (dead)
        {
            transform.position = new Vector3(500f, transform.position.y, 500f);
        }
        if (active)
        {
            if (!rotationLock)
                CheckDirection();
            CheckPlayerDistance();
            if (!locked)
            {
                if (!aggro)
                {
                    if (playerS.GetNoisy() && playerDistance < hearingDistance)
                    {
                        DetectedPlayer();
                        Aggroed();
                    }

                    if (PlayerWithinSight())
                    {
                        if (lightDetected && playerDistance < sightDistance * lightSensitivity)
                        {
                            DetectedPlayer();
                            Aggroed();
                            playerSeen = true;

                        }
                        else if (playerDistance < sightDistance)
                        {
                            DetectedPlayer();
                            Aggroed();
                            playerSeen = true;
                        }
                    }

                }
                else // if aggro true
                {
                    if (Mathf.Pow(Mathf.Pow(savedPosition.x - transform.position.x, 2f) + Mathf.Pow(savedPosition.z - transform.position.z, 2f), 0.5f) > 3f)
                    {
                        stuckTimeout = 0;
                        savedPosition = transform.position;
                    }
                    else
                    {
                        stuckTimeout += Time.deltaTime;
                        if (stuckTimeout > 3f) // will de-aggro if in same place for specified seconds
                        {
                            DeAggro();
                        }
                    }

                    if (playerDistance < strikeDistance)
                    {
                        Attack();
                    }
                    if (!speedDisabled)
                        SetSpeed();
                    if (PlayerWithinSight() == true)
                    {
                        playerSeen = true;
                        DetectedPlayer();
                    }
                    else
                    {
                        playerSeen = false;
                    }
                    if (Mathf.Pow(Mathf.Pow(playerSP.x - transform.position.x, 2f) + Mathf.Pow(playerSP.z - transform.position.z, 2f), 0.5f) < 1f && playerSeen == false)
                    {
                        DeAggro();
                    }
                }
            }
            GetComponent<NavMeshAgent>().destination = playerSP;
        }
    }

    private void DeAggro()
    {
        playerSeen = false;
        aggro = false;
        playerSP = transform.position;
        savedPosition = transform.position;
        nma.velocity = new Vector3(0, 0, 0);
    }

    private void SetSpeed()
    {
        if (striking)
            GetComponent<NavMeshAgent>().speed = 0f;
        else
        {
            if (playerDistance > BEGIN_SLOW_DISTANCE)
                GetComponent<NavMeshAgent>().speed = baseSpeed;
            else
                GetComponent<NavMeshAgent>().speed = baseSpeed * Mathf.Pow(playerDistance / BEGIN_SLOW_DISTANCE, 0.05f);
        }
    }

    public virtual void AddedMethods()
    {

    }

    public void CheckDirection()
    {
        if (aggro && !locked)
        {
            if (playerSeen || omniscient)
            {
                xDiff = player.transform.position.x - transform.position.x;
                zDiff = player.transform.position.z - transform.position.z;
            }
            else
            {
                xDiff = transform.position.x - passiveX;
                zDiff = transform.position.z - passiveZ;
                passiveX = transform.position.x;
                passiveZ = transform.position.z;
            }
            rotation = Mathf.Rad2Deg * Mathf.Atan2(zDiff, xDiff);
            transform.rotation = Quaternion.Euler(90, -rotation, 0);
        }
    }

    public void DetectedPlayer()
    {
        playerSP = player.transform.position; // sets position to be traveled to
    }

    private void CheckPlayerDistance()
    {
        playerDistance = Mathf.Pow(Mathf.Pow(this.transform.position.x - player.transform.position.x, 2) + Mathf.Pow(this.transform.position.z - player.transform.position.z, 2), 0.5f);
    }

    public void Aggroed()
    {
        if (active && !dead && !locked)
        {
            savedPosition = transform.position;
            stuckTimeout = 0;
            if (!aggro)
            {
                aggro = true; // prevents potential infinite loop
                PackReaction();
            }
            aggro = true;
        }
    }

    public virtual void PackReaction()
    {

    }

    List<GameObject> tempMList = new List<GameObject>();

    public List<GameObject> GetSameNearby() // checks for nearby monsters of same type
    {
        removedNum = 0;
        tempMList = GetTargets();
        int savInt = tempMList.Count;
        for (int i = 0; i < savInt; i++)
        {
            if (Get2DDistance(tempMList[i - removedNum].transform, transform) > packDistance) //checks if too far away
            {
                tempMList.RemoveAt(i - removedNum);
                removedNum++;
            }
            else if (!tempMList[i - removedNum].transform.CompareTag(transform.tag + "hurtbox")) //checks if of different object type
            {
                tempMList.RemoveAt(i - removedNum);
                removedNum++;
            }
        }

        return tempMList;

    }

    public List<GameObject> GetTargets() // gathers all surrounding objects
    {
        targets.Clear();

        for (int i = 0; i < 10; i++) // first quadrent
        {
            IncludeIfUnique((1f * (10 - i) / 10), (1f * i / 10), i);
        }
        for (int i = 0; i < 10; i++) // second quadrent
        {
            IncludeIfUnique(-(1f * (10 - i) / 10), (1f * i / 10), i);
        }
        for (int i = 0; i < 10; i++) // third quadrent
        {
            IncludeIfUnique(-(1f * (10 - i) / 10), -(1f * i / 10), i);
        }
        for (int i = 0; i < 10; i++) // fourth quadrent
        {
            IncludeIfUnique(-(1f * (10 - i) / 10), -(1f * i / 10), i);
        }
        return targets;
    }

    private void IncludeIfUnique(float x, float z, int i) // adds if not already in list
    {
        if (Physics.Raycast(new Vector3(transform.position.x, 0.5f, transform.position.z), new Vector3(x, 0, z), out hit))
        {
            hitObject = hit.transform.gameObject;
            validHit = true;
            foreach (GameObject t in targets)
                if (t == hitObject)
                    validHit = false;
            if (validHit)
                targets.Add(hitObject);
        }
    }

    private bool PlayerWithinSight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit))
        {
            if (hit.transform.CompareTag("player"))
                return true;
            else
                return false;
        }
        else
            return false;
    }

    public void Hit(float dam)
    {
        if (!dead)
        {
            print("I'M HIT!");
            DetectedPlayer();
            Aggroed();
            health -= dam;
            locked = true;
            playerSeen = false;
            if (!speedDisabled)
                GetComponent<NavMeshAgent>().speed = 0f;
            bloodMS.SpawnBlood(transform.position);
            if (health <= 0f)
                Death();
            else
                StartCoroutine("EndLocked", 1f);
        }
    }

    public void Hit(float dam, float hitstunMod) // special call for hitstun-modifying hits
    {
        if (!dead)
        {
            DetectedPlayer();
            Aggroed();
            health -= dam;
            locked = true;
            playerSeen = false;
            if (!speedDisabled)
                GetComponent<NavMeshAgent>().speed = 0f;
            bloodMS.SpawnBlood(transform.position);
            if (health <= 0f)
                Death();
            else
                StartCoroutine("EndLocked", hitstunMod);
        }
    }

    void Death()
    {
        Gore();
        nma.Warp(new Vector3(150f, transform.position.y, 150f));
        aggro = false;
        locked = true;
        active = false;
        dead = true;
        playerS.IncreaseKill();
    }

    public virtual void Gore()
    {
        for (int i = 0; i < 5; i++)
            bloodMS.SpawnBlood(new Vector3(transform.position.x + Random.Range(-1.5f, 1.5f), transform.position.y, transform.position.z + Random.Range(-1.5f, 1.5f)));
        gameManagerS.PlayKillsound();
    }

    IEnumerator EndLocked(float hitstunMod) // will automatically get free even if shot again while in hitstun after first hitstun is over
    {
        yield return new WaitForSeconds(hitstun * hitstunMod);
        if (!dead && !striking)
        {
            locked = false;
        }
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "player")
        {
            Attack();
        }
    }*/

    void Attack()
    {
        if (locked == false)
        {
            locked = true;
            striking = true;
            nma.velocity = new Vector3(0, 0, 0);
            StartCoroutine("Startup");
        }
    }

    IEnumerator Startup()
    {
        yield return new WaitForSeconds(startup);
        if (active && !dead)
        {
            StartCoroutine("Strike");
        }
    }

    IEnumerator Strike()
    {
        if (active && !dead)
        {
            if (hitboxTriggered == true)
                playerS.Hit(damage);
            yield return new WaitForSeconds(afterlag);
            locked = false;
            striking = false;
        }
    }

    public void SetHitboxTriggered(bool t)
    {
        hitboxTriggered = t;
    }

    public void PlayerDead()
    {
        active = false;
        locked = true;
        playerSeen = false;
    }

    public void Respawn(Transform t)
    {
        print("respawn called at: " + Time.time);
        lightDetected = false;
        striking = false;
        active = true;
        locked = false;
        dead = false;
        health = maxHealth;
        print("health: " + health);
        print("max health: " + maxHealth);

        DeAggro();
        transform.position = t.position;
        GetComponent<NavMeshAgent>().destination = t.position;
        GetComponent<NavMeshAgent>().Warp(t.position); // took me a while to figure out that you have to use WARP and not a regular transform.position Vector assignment
        playerSP = t.position;
        savedPosition = transform.position;
        StartCoroutine("CheckAgain", t);

        print("position: " + transform.position);
        print("saved: " + savedPosition);
        print("sp: " + playerSP);
    }

    IEnumerator CheckAgain(Transform t)
    {
        yield return new WaitForSeconds(0.1f);
        transform.position = t.position;
        StartCoroutine("CheckAgain", t);
    }


    //getters and setters onward


    public bool GetPlayerSeen()
    {
        return playerSeen;
    }

    public float GetStrikeDistance()
    {
        return strikeDistance;
    }

    public void SetStrikeDistance(float s)
    {
        strikeDistance = s;
    }

    public float GetHealth()
    {
        return health;
    }

    public void SetHealth(float h)
    {
        if (health == 0)
            maxHealth = h;
        health = h;
    }

    public float GetDamage()
    {
        return damage;
    }

    public void SetDamage(float d)
    {
        damage = d;
    }

    public float GetAfterlag()
    {
        return afterlag;
    }

    public void SetAfterlag(float a)
    {
        afterlag = a;
    }

    public float GetStartup()
    {
        return startup;
    }

    public void SetStartup(float s)
    {
        startup = s;
    }

    public float GetBaseSpeed()
    {
        return baseSpeed;
    }

    public void SetBaseSpeed(float b)
    {
        baseSpeed = b;
    }

    public float GetSightDistance()
    {
        return sightDistance;
    }

    public void SetSightDistance(float s)
    {
        sightDistance = s;
    }

    public void SetLightSensitivity(float ls)
    {
        lightSensitivity = ls;
    }

    public float GetHearingDistance()
    {
        return hearingDistance;
    }

    public void SetHearingDistance(float h)
    {
        hearingDistance = h;
    }

    public float GetHitstun()
    {
        return hitstun;
    }

    public void SetHitstun(float h)
    {
        hitstun = h;
    }

    public void SetVisionRange(float vr)
    {
        visionRange = vr;
    }

    public BloodManagerScript GetBloodMS()
    {
        return bloodMS;
    }


    public void SetPackDistance(float n)
    {
        packDistance = n;
    }

    public float Get2DDistance(Transform t1, Transform t2)
    {
        return Mathf.Sqrt(Mathf.Pow(t1.position.x - t2.position.x, 2f) + Mathf.Pow(t1.position.z - t2.position.z, 2f));
    }


    public bool GetAggro()
    {
        return aggro;
    }

    public float GetRotation()
    {
        return rotation;
    }

    public void SetRotationLock(bool b)
    {
        rotationLock = b;
    }


    public bool GetActive()
    {
        return active;
    }

    public NavMeshAgent GetNavMeshAgent()
    {
        return nma;
    }


    public bool GetDead()
    {
        return dead;
    }

    public GameObject GetPlayer()
    {
        return player;
    }

    public PlayerScript GetPlayerScript()
    {
        return playerS;
    }

    public bool GetLocked()
    {
        return locked;
    }

    public void SetSpeedDisabled(bool b)
    {
        speedDisabled = b;
    }
    public GameManagerScript GetGameManagerS()
    {
        return gameManagerS;
    }

    public NavMeshAgent GetNMA()
    {
        return nma;
    }

    public BoxCollider[] GetCos()
    {
        return cos;
    }

    public MeshRenderer[] GetMrs()
    {
        return mrs;
    }
}
