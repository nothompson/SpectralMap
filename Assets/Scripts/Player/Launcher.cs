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
    public GameObject BlastAnchor;
    public Grapple grapple;

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

    private bool grappleSuccess;

    [Header("Fireball")]
    public float fireballSpeed;
    float firingSpeed;
    float costToShoot;

    bool shooting, readyToShoot;

    float shootTimer = 0f;

    bool grappling;

    bool fireball, wind, slime, slimeOn, slimeOff;

    bool reset;

    Vector3 restingPos;

    private ParticleSystem.EmitParams blastParams;

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

        blastParams = new ParticleSystem.EmitParams();

    }

    void Update()
    {
        if(DeathManager.PlayerDead) return;
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

        blastParams.velocity = BlastAnchor.transform.forward;
        ProjectileParticleManager.Instance.FireballBlast.transform.position = BlastAnchor.transform.position;
    }

    private void soundUpdate()
    {
        if(player.paused) return;
        
        float normalizedMagic = playerMagic.magicPoints / playerMagic.maximumMagic;
        fireSound.SetParameter("WetDryRocket", 1.0f - normalizedMagic);

        
        //error sound if out of magic
        if (readyToShoot && InputManager.Instance.inputs.Player.Fire.IsPressed() && playerMagic.magicPoints < 10f * costMultiplier)
        {
            errorCooldown -= Time.deltaTime;
            if(errorCooldown <=0){
                SpellError();
                errorCooldown = errorTimer;
            }
        }
        
        if(readyToShoot && InputManager.Instance.inputs.Player.Fire.triggered && playerMagic.magicPoints < 10f * costMultiplier)
        {
            SpellError();
        }

        // if(grappling && playerMagic.magicPoints < costToShoot && !grapple.grappleActive && !grapple.releasing)
        // {
        //     mana.Error();
        //     cantShootYet.Play();
        // }
    }

    public void SpellError(bool drain = false)
    {
        mana.Error();
        cantShootYet.Play();
        if (drain)
        {
            playerMagic.magicPoints -= 25f;
            if(playerMagic.magicPoints <= 0f)
            {
                playerMagic.magicPoints = 0f;
            }
        }
    }

    private void MyInput()
    {
        if(player.ensared) return;

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

        if (shootTimer <= 0f && shooting && playerMagic.magicPoints >= 10f * costMultiplier)
        {
            if(ReloadManager.Instance.reloading) ReloadManager.Instance.StopReload();
            Shoot();
        }

        grappling = InputManager.Instance.inputs.Player.AltFire.triggered;
        if (grappling)
        { 
            if(!ReloadManager.Instance.reloading) TryGrapple(); 
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
            firingSpeed = 0.66f / shootMultiplier;
            costToShoot = 20f * costMultiplier;
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
        
        ShootFireball(attackPoint);

        //create rocket at the point of attacking

        //magic drain
        playerMagic.magicPoints -= 10f * costMultiplier;
        playerMagic.justUsed = true;
        playerMagic.regenTimer = playerMagic.magicBufferTime;

    }

    private void TryGrapple()
    {
        if(!grapple.grappleActive){
            if(playerMagic.magicPoints >= 20f * costMultiplier){
            grapple.TryGrapple(attackPoint, ref grappleSuccess);
            if (grappleSuccess)
            {
                leftHandAnim.SetTrigger("Fire");
                playerMagic.magicPoints -= 20f * costMultiplier;
            }
            }
            else
            {
                mana.Error();
                cantShootYet.Play();
            }
        }
        else
        {
            grapple.Release();
        }
    }

    private void ShootFireball(Transform attackPoint)
    {
        // StartCoroutine(ShowFireballCanvas());
        GameObject rocketInstance = Instantiate(Fireball, attackPoint.position, attackPoint.rotation);
        //pass in references to rocket prefab
        rocketInstance.GetComponent<Fireball>().player = player.transform;
        rocketInstance.GetComponent<Fireball>().damageMultiplier = PlayerManager.Instance.DamageMultiplier;
        rocketInstance.GetComponent<Fireball>().forceMultiplier = PlayerManager.Instance.ForceMultiplier;
        rocketInstance.GetComponent<Fireball>().speed = fireballSpeed;

        fireSound.Play();
        //if shot out to space, clear up instantiated objects after a timer
        Destroy(rocketInstance, autoDestroyTimer);


        ProjectileParticleManager.Instance.FireballBlast.Emit(blastParams, 50);
        ProjectileParticleManager.Instance.FireballBlast.Play();

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

    //



    // neither of these are being used
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
