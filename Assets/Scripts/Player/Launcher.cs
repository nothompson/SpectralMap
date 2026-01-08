using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Launcher : MonoBehaviour
{
    [Header("References")]
    public PlayerControlRigid player;
    public MagicManagement playerMagic;
    public Transform attackPoint;
    public Animator rightHandAnim;
    public Animator leftHandAnim;
    public Transform hand;
    public ManaUI mana;
    public GameObject blastCanvas;

    [Header("Spell Prefabs")]
    public GameObject Fireball;
    public GameObject blast;
    public GameObject Windblast;
    public GameObject Slimehook;
    private GameObject SlimehookInstance;

    [Header("Audio")]
    public FMODUnity.StudioEventEmitter fireSound;
    public FMODUnity.StudioEventEmitter cantShootYet;

    [Header("General")]
    public float shootMultiplier;
    public float costMultiplier;
    public float damageMultiplier;
    public float forceMultiplier;

    public AnimationCurve recoil;

    [Header("Fireball")]
    public float fireballSpeed;
    float firingSpeed;
    float costToShoot;

    bool shooting, readyToShoot;

    float shootTimer = 0f;

    bool punching;

    bool fireball, wind, slime, slimeOn, slimeOff;

    bool reset;

    Vector3 restingPos;

    [Header("Debugging")]
    public int spell; 
    public float autoDestroyTimer = 10f;

    public bool allowInvoke = true;

    float errorCooldown = 0f;
    float errorTimer = 0.5f;

    bool cooldown = false;

    void Start()
    {
        spell = 1;
        readyToShoot = true;

        restingPos = hand.localPosition;

    }

    void Update()
    {
        MyInput();
        soundUpdate();

        if(shootTimer > 0f)
        {
            shootTimer -= Time.deltaTime;
        }

              if(shootTimer <= 0f && cooldown)
            {
                cooldown = false;
                // rightHandAnim.SetTrigger("Ready");
            }
    }

    private void soundUpdate()
    {
        float normalizedMagic = playerMagic.magicPoints / playerMagic.maximumMagic;
        fireSound.SetParameter("WetDryRocket", 1.0f - normalizedMagic);
        
        //error sound if out of magic
        if (readyToShoot && InputManager.Instance.inputs.Player.Fire.IsPressed() && playerMagic.magicPoints < costToShoot)
        {
            errorCooldown -= Time.deltaTime;
            if(errorCooldown <=0){
                mana.Error();
                cantShootYet.Play();
                errorCooldown = errorTimer;
            }
        }
        
        if(readyToShoot && InputManager.Instance.inputs.Player.Fire.triggered && playerMagic.magicPoints < costToShoot)
        {
            mana.Error();
            cantShootYet.Play();
        }
    }

    private void MyInput()
    {
        if(!player.paused){
            //left click. if yes trigger bool
            // shooting = InputManager.Instance.inputs.Player.Fire.triggered;
            if (InputManager.Instance.inputs.Player.Fire.IsPressed())
            {
                shooting = true;
            }
            else
            {
                shooting = false;
            }

        if (shootTimer <= 0f && shooting && playerMagic.magicPoints >= costToShoot)
        {
            Shoot();
        }

        punching = InputManager.Instance.inputs.Player.AltFire.triggered;
        if (punching)
        {
            Punch();
        }

        // if (allowInvoke)
        // {
        //     fireball = Input.GetKeyDown(KeyCode.Alpha1);
        //     if (fireball)
        //     {
        //         spell = 1;

        //     }
        //     wind = Input.GetKeyDown(KeyCode.Alpha2);
        //     if (wind)
        //     {
        //         spell = 2;
        //     }
        //     slime = Input.GetKeyDown(KeyCode.Alpha3);
        //     if (slime)
        //     {
        //         spell = 3;
        //     }
        // }
            SpellManager();
        }
    }

    private void SpellManager()
    {
        if (spell == 1)
        {
            firingSpeed = 0.66f / shootMultiplier;
            costToShoot = 10f * costMultiplier;
        }
        if (spell == 2)
        {
            firingSpeed = 1.5f / shootMultiplier;
            costToShoot = 33f * costMultiplier;
        }
        if (spell == 3) {
            firingSpeed = 3f / shootMultiplier;
            costToShoot = 50f * costMultiplier;
        }
    }


    private void Shoot()
    {
        // readyToShoot = false;

        cooldown = true;

        shootTimer = firingSpeed;
        //raycast to see where rocket will land
        rightHandAnim.SetTrigger("Fire");
        if (spell == 1)
        {
            ShootFireball(attackPoint);
        }
        if (spell == 2)
        {
            ShootWindblast(attackPoint);
        }
        if (spell == 3)
        {
            ShootSlimeHook(attackPoint);
        }
        //create rocket at the point of attacking

        //magic drain
        playerMagic.magicPoints -= costToShoot;
        playerMagic.justUsed = true;
        playerMagic.regenTimer = playerMagic.magicBufferTime;

        // if (allowInvoke)
        // {
        //     Invoke("ResetShot", firingSpeed);
        //     allowInvoke = false;
        // }
    }

    private void Punch()
    {
        leftHandAnim.SetTrigger("Punch");
    }

    private void ShootFireball(Transform attackPoint)
    {
        // StartCoroutine(ShowFireballCanvas());
        GameObject rocketInstance = Instantiate(Fireball, attackPoint.position, attackPoint.rotation);
        //pass in references to rocket prefab
        rocketInstance.GetComponent<Fireball>().player = player.transform;
        rocketInstance.GetComponent<Fireball>().damageMultiplier = damageMultiplier;
        rocketInstance.GetComponent<Fireball>().forceMultiplier = forceMultiplier;
        rocketInstance.GetComponent<Fireball>().speed = fireballSpeed;

        fireSound.Play();
        //if shot out to space, clear up instantiated objects after a timer
        Destroy(rocketInstance, autoDestroyTimer);
    }

    private void ShootWindblast(Transform attackPoint)
    {
        if (!player.grounded)
        {
            player.rb.linearVelocity = new Vector3(0f, 30f, 0f);
        }
        else
        {
            GameObject windblastInstance = Instantiate(Windblast, attackPoint.position, attackPoint.rotation);
            windblastInstance.GetComponent<Windblast>().player = player.transform;
            windblastInstance.GetComponent<Windblast>().attackPoint = attackPoint;
        }

    }

    private void ShootSlimeHook(Transform attackPoint)
    {
        Debug.Log("shooting hook");

        // Raycast ray = Physics.Raycast();

        // RaycastHit hit;

        SlimehookInstance = Instantiate(Slimehook, attackPoint.position, attackPoint.rotation);

        SlimehookInstance.GetComponent<Slimehook>().player = player.transform;
    }

    private void ResetShot()
    {
        rightHandAnim.SetTrigger("Ready");
    }

    public IEnumerator Recoil()
    {
        float dur = firingSpeed;
        float t = 0;

        while (t < dur)
        {
            t += Time.deltaTime;
            float elapsed = t / dur;
            float recoilAmp = recoil.Evaluate(elapsed);
            hand.localPosition = new Vector3(restingPos.x, restingPos.y, restingPos.z * recoilAmp);
            yield return null;
        }
        hand.localPosition = restingPos;
    }

    public IEnumerator ShowFireballCanvas()
    {
        if(blastCanvas != null)
        {
            blastCanvas.SetActive(true);

            Image image = blastCanvas.GetComponentInChildren<Image>();
            if(image != null){
                RectTransform imageRect = image.GetComponent<RectTransform>();

                imageRect.localRotation = Quaternion.Euler(0,0, Random.Range(0f,360f));
            }
            yield return new WaitForSeconds(0.125f);

            blastCanvas.SetActive(false);
        }
        
    }
}
