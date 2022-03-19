using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GameObject winText;

    int totKills;
    public int totKillsRequired;

    Rigidbody rb;

    bool noisy;
    bool noisyWall, noisyMonster;

    bool wPressed, aPressed, sPressed, dPressed;
    float rotation;

    const float BASE_ACCELERATION = 1f;
    const float MAX_SPEED = 5f;

    GameManagerScript gameManagerS;

    public GameObject[] allGuns;

    const float RELOAD_COOLDOWN = 1f;
    const float DEFAULT_GUN_COOLDOWN = 0.15f;
    const float SHOTGUN_COOLDOWN = 0.5f;
    const float MINIGUN_COOLDOWN = 0.1f;
    const float SHOTGUN_SPREAD_MOD = 0.05f;
    const float SHOTGUN_HITSTUN_MOD = 1.8f;
    const float MINIGUN_HITSTUN_MOD = 0f;

    bool minigunActive;
    int bulletCount;
    int shotgunBulletCount;
    int minigunBulletCount;
    int sbct;
    int mbct;
    RaycastHit hit;
    float damage;

    const int BULLET_MAX = 9;
    const int SHOTGUN_BULLET_MAX = 3;
    const int MINIGUN_BULLET_MAX = 30;

    float health;
    float maxHealth;
    const float BASE_SPEED = 6f;
    float healingCooldown;
    float hCool;
    float healRate;
    bool dead;
    bool locked;
    BloodManagerScript bloodMS;

    MeshRenderer[] mrs;

    bool lockLocked;

    float xDiff;
    float zDiff;
    Plane mousePlane;
    float mouseDistance;
    Vector3 mouseTarget;
    Ray mouseRay;

    // Start is called before the first frame update
    void Start()
    {

        bulletCount = BULLET_MAX;

        shotgunBulletCount = SHOTGUN_BULLET_MAX;
        minigunBulletCount = MINIGUN_BULLET_MAX;
        
        bloodMS = GameObject.Find("BloodManager").GetComponent<BloodManagerScript>();
        mrs = GetComponentsInChildren<MeshRenderer>();
        damage = 1f;
        weaponSelected = 1;
        rb = GetComponent<Rigidbody>();
        StartCoroutine("FindTheManager");
        SetGun(1);
    }

    IEnumerator FindTheManager()
    {
        yield return new WaitForSeconds(0.01f);
        gameManagerS = GameObject.Find("Game Manager").GetComponent<GameManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckDirection();
        CheckPressed();
        CheckVelocity();
        CheckAttack();
    }

    /*private void CheckDirection()
    {
        if (aPressed)
        {
            if (wPressed)
                rotation = 225f;
            else if (sPressed)
                rotation = 135f;
            else
                rotation = 180f;
        }
        else if (dPressed)
        {
            if (wPressed)
                rotation = 315f;
            else if (sPressed)
                rotation = 45f;
            else
                rotation = 0;
        }
        else if (wPressed)
            rotation = 270f;
        else if (sPressed)
            rotation = 90f;
        transform.rotation = Quaternion.Euler(0, rotation, 0);
    }*/

    private void CheckDirection()
    {
        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        mousePlane = new Plane(Vector3.up, Vector3.zero);
        if (mousePlane.Raycast(mouseRay, out mouseDistance))
        {
            mouseTarget = mouseRay.GetPoint(mouseDistance);
            xDiff = mouseTarget.x - this.transform.position.x;
            zDiff = mouseTarget.z - this.transform.position.z;
            rotation = -(Mathf.Rad2Deg * Mathf.Atan2(zDiff, xDiff));
            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }
    }

    /*private void CheckDirection()
    {
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;
        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);
            Debug.DrawLine(cameraRay.origin, pointToLook, Color.cyan);
            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }*/

float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    private void CheckPressed()
    {
        if (Input.GetKeyDown(KeyCode.W))
            wPressed = true;
        if (Input.GetKeyUp(KeyCode.W))
            wPressed = false;
        if (Input.GetKeyDown(KeyCode.A))
            aPressed = true;
        if (Input.GetKeyUp(KeyCode.A))
            aPressed = false;
        if (Input.GetKeyDown(KeyCode.S))
            sPressed = true;
        if (Input.GetKeyUp(KeyCode.S))
            sPressed = false;
        if (Input.GetKeyDown(KeyCode.D))
            dPressed = true;
        if (Input.GetKeyUp(KeyCode.D))
            dPressed = false;
    }

    private void CheckVelocity()
    {
        if (Mathf.Abs(rb.velocity.z) < MAX_SPEED)
        {
            if (wPressed)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z + BASE_ACCELERATION);
            if (sPressed)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z - BASE_ACCELERATION);
        }

        if (Mathf.Abs(rb.velocity.x) < MAX_SPEED)
        {
            if (dPressed)
                rb.velocity = new Vector3(rb.velocity.x + BASE_ACCELERATION, rb.velocity.y, rb.velocity.z);
            if (aPressed)
                rb.velocity = new Vector3(rb.velocity.x - BASE_ACCELERATION, rb.velocity.y, rb.velocity.z);
        }


    }

    bool cooldown;
    float gunCooldown;
    int weaponSelected;
    float startupLag;



    private void CheckAttack()
    {
        if (Input.GetMouseButtonDown(0) && cooldown == false && gunCooldown <= 0) // MAIN WEAPON
        {
            if (weaponSelected == 1)
                FireDefaultGun();
            else if (weaponSelected == 2)
                FireShotgun();
            else if (weaponSelected == 3)
            {
                minigunActive = true;
            }
            else
                print("ERROR! INVALID WEAPON SELECTED in CHECKATTACK");
        }
        else if (Input.GetKeyDown(KeyCode.R) && cooldown == false && weaponSelected != 0) // RELOAD
        {
            if ((bulletCount < BULLET_MAX && weaponSelected == 1) || (shotgunBulletCount < SHOTGUN_BULLET_MAX && weaponSelected == 2) || (shotgunBulletCount < MINIGUN_BULLET_MAX && weaponSelected == 3))
                ReloadGun();
        }
        else if ((Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.E)) && cooldown == false) // SWITCH WEAPONS
        {
            SwitchWeapons();
        }

        if (startupLag > 0)
            startupLag -= Time.deltaTime;

        if (Input.GetMouseButtonUp(0))
            minigunActive = false;

        if (minigunActive && cooldown == false && gunCooldown <= 0)
            FireMinigun();
        
        if(gunCooldown >= 0)
            gunCooldown -= Time.deltaTime;
    }


    private void FireDefaultGun() // for default gun
    {
        if (bulletCount > 0)
        {
            gunCooldown = DEFAULT_GUN_COOLDOWN;
            cooldown = true;
            bulletCount--;
            Bullet(0);
            Gunshot();
            StartCoroutine("EndDelay", gunCooldown);
        }
        else
            ReloadGun();
    }

    private void FireShotgun()
    {
        if (shotgunBulletCount > 0)
        {
            cooldown = true;
            gunCooldown = SHOTGUN_COOLDOWN;
            shotgunBulletCount--;
            for (float i = -2f; i <= 2f; i++) //fires 5 bullets with i spread
                Bullet(i); // note that angleMod is also used for iterating over spark effects
            Gunshot();
            StartCoroutine("EndDelay", gunCooldown);
        }
        else
            ReloadGun();
    }

    private void FireMinigun() // for default gun
    {
        if (minigunBulletCount > 0)
        {
            gunCooldown = MINIGUN_COOLDOWN;
            cooldown = true;
            minigunBulletCount--;
            Bullet(Random.Range(-0.8f, 0.8f));
            Gunshot();
            StartCoroutine("EndDelay", gunCooldown);
        }
        else
        {
            minigunActive = false;
            ReloadGun();
        }
    }

    private void Gunshot() // occurs reguardless of what is hit
    {
        if (noisy == false) // prevents multi-shots from causing it to play multiple times at once.
        {
            if (weaponSelected == 1)
                gameManagerS.PlayGunshot();
            else if (weaponSelected == 2)
                gameManagerS.PlayShotgun();
            else if (weaponSelected == 3)
                gameManagerS.PlayMinigun();
            else
                print("ERROR! INVALID WEAPON TYPE IN GUNSHOT");
        }
        noisy = true;
        StartCoroutine("EndShotLight");
    }

    IEnumerator EndShotLight() // turns off light produced by gunshot after 0.05 seconds
    {
        yield return new WaitForSeconds(0.05f);
        noisy = false;
        noisyWall = false;
        noisyMonster = false;
    }

    private void Bullet(float angleMod) // handles individual bullets from gunshots
    {
        print("firing bullet");
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z), transform.right + (Vector3.Cross(transform.right, Vector3.up.normalized) * SHOTGUN_SPREAD_MOD * angleMod), Color.red, 100f);
        //for (float i = -2f; i <= 2f; i++)
        //    Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z), transform.right + (Vector3.Cross(transform.right, Vector3.up.normalized) * SHOTGUN_SPREAD_MOD * i), Color.blue, 100f);
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z), transform.right + (Vector3.Cross(transform.right, Vector3.up.normalized) * SHOTGUN_SPREAD_MOD * angleMod), out hit))
        {
            print("I hit: " + hit.transform.name);

            if (!hit.transform.CompareTag("player"))
            {
                if (hit.transform.CompareTag("wall"))
                {
                    if (noisyWall == false) // prevents multi-shots from causing it to play multiple times at once.
                        gameManagerS.PlayBulletClang();
                    noisyWall = true;
                }
                else if (hit.transform.CompareTag("bruiserhurtbox"))
                {
                    if (noisyMonster == false) // prevents multi-shots from causing it to play multiple times at once.
                        gameManagerS.PlayBulletStrike();
                    noisyMonster = true;
                    if (weaponSelected == 1)
                        hit.collider.gameObject.transform.parent.GetComponent<BruiserScript>().Hit(damage);
                    else if (weaponSelected == 2)
                        hit.collider.gameObject.transform.parent.GetComponent<BruiserScript>().Hit(damage, SHOTGUN_HITSTUN_MOD);
                    else if (weaponSelected == 3)
                        hit.collider.gameObject.transform.parent.GetComponent<BruiserScript>().Hit(damage, MINIGUN_HITSTUN_MOD);
                    else
                        print("INVALID WEAPON TYPE in BULLET");
                }
            }

        }
        else
            print("missed!");
    }

    private void ReloadGun()
    {
        if (weaponSelected == 1)
        {
            gameManagerS.PlayReload();
            bulletCount = BULLET_MAX;
        }
        else if (weaponSelected == 2)
        {
            if (sbct > SHOTGUN_BULLET_MAX - shotgunBulletCount)
            {
                gameManagerS.PlayReload();
                shotgunBulletCount = SHOTGUN_BULLET_MAX;
                sbct -= (SHOTGUN_BULLET_MAX - shotgunBulletCount);
            }
            else if (sbct == 0)
            {
                gameManagerS.PlayClick();
            }
            else
            {
                gameManagerS.PlayReload();
                shotgunBulletCount += sbct;
                sbct = 0;
            }
        }
        else if (weaponSelected == 3)
        {
            if (mbct > MINIGUN_BULLET_MAX - minigunBulletCount)
            {
                gameManagerS.PlayReload();
                minigunBulletCount = MINIGUN_BULLET_MAX;
                mbct -= (MINIGUN_BULLET_MAX - minigunBulletCount);
            }
            else if (mbct == 0)
            {
                gameManagerS.PlayClick();
            }
            else
            {
                gameManagerS.PlayReload();
                minigunBulletCount += mbct;
                mbct = 0;
            }
        }
        else
            print("ERROR! INVALID WEAPON TYPE in RELOAD");
        cooldown = true;
        StartCoroutine("EndDelay", RELOAD_COOLDOWN);
    }

    IEnumerator EndDelay(float time)
    {
        yield return new WaitForSeconds(time);
        if (!dead)
        {
                if (!lockLocked)
                    locked = false;
            
            cooldown = false;
        }
    }

    private void SwitchWeapons()
    {
        if (weaponSelected == 1)
        {
            weaponSelected = 2;
            damage = 1f;
        }
        else if (weaponSelected == 2)
        {
            weaponSelected = 3;
            damage = 1f;
        }
        else if (weaponSelected == 3)
        {
            weaponSelected = 1;
            damage = 0.5f;
        }
        else
            print("ERORR! INVALID WEAPON TYPE in mouse input 2");

        SetGun(weaponSelected);
        gameManagerS.PlaySwitchWeapons();
        cooldown = true;
        StartCoroutine("EndDelay", 0.2f);
    }

    public void SetGun(int t)
    {
        if (t == 1)
        {
            allGuns[0].SetActive(true);
            allGuns[1].SetActive(false);
            allGuns[2].SetActive(false);
        }
        else if (t == 2)
        {
            allGuns[0].SetActive(false);
            allGuns[1].SetActive(true);
            allGuns[2].SetActive(false);
        }
        else if (t == 3)
        {
            allGuns[0].SetActive(false);
            allGuns[1].SetActive(false);
            allGuns[2].SetActive(true);
        }
        else
            print("ERROR! INVALID GUN TYPE");
    }

    public void Hit(float dam)
    {
        if (dead == false)
        {
            health -= dam;
            hCool = healingCooldown;
            bloodMS.SpawnBlood(transform.position);
            gameManagerS.PlayPlayerHit();
            if (health <= 0)
            {
                Death();
            }
        }
    }

    private void Death()
    {
        gameManagerS.PlayPlayerKill();
        dead = true;
        gameManagerS.Lose();
        SetAllToDead();
        locked = true;
        rb.velocity = new Vector3(0, 0, 0);
        for (int i = 0; i < 6; i++)
            bloodMS.SpawnBlood(new Vector3(transform.position.x + Random.Range(-1.5f, 1.5f), transform.position.y, transform.position.z + Random.Range(-1.5f, 1.5f)));
        foreach(MeshRenderer mr in mrs)
            mr.enabled = false;
        GetComponent<BoxCollider>().enabled = false;
    }

    public void SetAllToDead()
    {
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("bruiserhurtbox"))
            o.transform.parent.GetComponent<BruiserScript>().PlayerDead();
    }

    private float Get2DDistance(Transform t1, Transform t2)
    {
        return Mathf.Sqrt(Mathf.Pow(t1.position.x - t2.position.x, 2f) + Mathf.Pow(t1.position.z - t2.position.z, 2f));
    }
    public bool GetNoisy()
    {
        return noisy;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("shotgunammo"))
        {
            sbct += 8;
            other.transform.gameObject.SetActive(false);
        }
        else if (other.CompareTag("minigunammo"))
        {
            mbct += 50;
            other.transform.gameObject.SetActive(false);
        }
        else if (other.CompareTag("explosives"))
        {
            for (float i = 0; i < 360f; i++)
            {
                Bullet(i);
            }
            gameManagerS.PlayBoom();
            other.transform.gameObject.SetActive(false);
        }
    }

    public void IncreaseKill()
    {
        totKills++;
        if (totKills >= totKillsRequired)
            WinLevel();
    }

    public void WinLevel()
    {
        StartCoroutine("FlashColor");
        StartCoroutine("DelayEnd");
    }

    IEnumerator FlashColor()
    {
        foreach (MeshRenderer mr in mrs)
        {
            mr.material.color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        }
        yield return new WaitForSeconds(0.1f);
        StartCoroutine("FlashColor");
    }

    IEnumerator DelayEnd()
    {
        yield return new WaitForSeconds(2f);
        if (totKillsRequired == 5)
            gameManagerS.Win();
        else
            winText.SetActive(true);
    }

}
