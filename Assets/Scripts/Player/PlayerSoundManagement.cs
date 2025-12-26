using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicScripts;
public class PlayerSoundManagement : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    public LayerMask enemyMask;
    public List<BoxCollider> musicZones;
    public List<float> musicZoneParam;
    public List<bool> musicZoneFlag;

    [SerializeField] float zonecheckdel = 1f;
    [SerializeField] float combatcheckdel = 1f;
    [SerializeField] float transitionSpeed = 0.25f;
    [SerializeField] float drumdel = 16.0f;
    [SerializeField] float bassdel = 32.0f;

    HP hp;
    PlayerControlRigid playerControl;

    bool zonechange = false;

    bool combat = false;

    float pause = 0f;
    float combatvalue = 0.0f;

    float drumvalue = 0.0f;
    float bassvalue = 0.0f;

    float drumtimer = 0.0f;
    float basstimer = 0.0f;

    [Header("FMOD Events")]
    public FMODUnity.StudioEventEmitter backgroundMusic;
    public FMODUnity.StudioEventEmitter directionalFiring;
    public FMODUnity.StudioEventEmitter playerLand;

    public FMODUnity.StudioEventEmitter playerHurt;

    void Start()
    {
        playerControl = player.GetComponent<PlayerControlRigid>();
        hp = player.GetComponent<HP>();

        musicZoneFlag = new List<bool>(new bool[musicZones.Count]);
        musicZoneParam = new List<float>(new float[musicZones.Count]);

        StartCoroutine(CombatCheck());
        StartCoroutine(ZoneCheck());
    }

    // // Update is called once per frame
    void Update()
    {
        ZoneUpdate();
        ParamUpdate();
        PitchBend();
        DrumAndBass();
    }

    void ZoneUpdate()
    {
        for(int i = 0; i < musicZones.Count; i++){
            if(musicZoneFlag[i]){
                musicZoneParam[i] = Mathf.MoveTowards(musicZoneParam[i], 1.0f, transitionSpeed * Time.unscaledDeltaTime);
            }
            else{
                musicZoneParam[i] = Mathf.MoveTowards(musicZoneParam[i], 0.0f, transitionSpeed * Time.unscaledDeltaTime);
            }
            string zone = "Zone" + (i+1).ToString();
            backgroundMusic.SetParameter(zone, musicZoneParam[i]);
        }
    }

    void DrumAndBass()
    {
        if(zonechange){
            drumtimer += Time.unscaledDeltaTime;
            basstimer += Time.unscaledDeltaTime;
            if(drumtimer >= drumdel){
                drumvalue = Mathf.MoveTowards(drumvalue, 1.0f, transitionSpeed * Time.unscaledDeltaTime);
            }
            if(basstimer >= bassdel){
                bassvalue = Mathf.MoveTowards(bassvalue, 1.0f, transitionSpeed * Time.unscaledDeltaTime);
            }
            backgroundMusic.SetParameter("Drums", drumvalue);
            backgroundMusic.SetParameter("Bass", bassvalue);

            if (drumvalue >= 1.0f && bassvalue >= 1.0f)
            {
                zonechange = false;
            }
        }
        else{
            drumvalue = Mathf.MoveTowards(drumvalue, 0.0f, transitionSpeed * Time.unscaledDeltaTime);
            bassvalue = Mathf.MoveTowards(bassvalue, 0.0f, transitionSpeed * Time.unscaledDeltaTime);
        }
    }

    public void ParamUpdate()
    {

        // float normalizedSpeed = 0.01f * Mathf.Clamp(playerSpeed, 0f, 100f);
        float normalizeSpeed = MusicScript.NormalizeForAutomation(playerControl.playerSpeed, 0f, 70f);
        backgroundMusic.SetParameter("WetDryMusic", normalizeSpeed);

        // float normalizeXVel = 0.02f * Mathf.Clamp(playerVelocity.x, 0f, 50f);
        float normalizeXVel = MusicScript.NormalizeForAutomation(playerControl.rb.linearVelocity.x, -10f, 10f);
        float XVel = Mathf.Abs(normalizeXVel);
        backgroundMusic.SetParameter("XVel", XVel);
        directionalFiring.SetParameter("XVel", XVel);

        // float normalizeYVel = 0.02f * Mathf.Clamp(playerVelocity.y, 0f, 50f);
        float normalizePositiveYVel = MusicScript.NormalizeForAutomation(playerControl.rb.linearVelocity.y, 0f, 10f);
        float pYVel = Mathf.Abs(normalizePositiveYVel);
        backgroundMusic.SetParameter("+YVel", pYVel);

        float normalizeNegativeYVel = MusicScript.NormalizeForAutomation(playerControl.rb.linearVelocity.y, -20f, 0f);
        float nYVel = Mathf.Abs(normalizeNegativeYVel);
        backgroundMusic.SetParameter("-YVel", nYVel);

        // float normalizeZVel = 0.02f * Mathf.Clamp(playerVelocity.z, 0f, 50f);
        float normalizeZVel = MusicScript.NormalizeForAutomation(playerControl.rb.linearVelocity.z, -10f, 10f);
        float ZVel = Mathf.Abs(normalizeZVel);
        backgroundMusic.SetParameter("ZVel", ZVel);
        directionalFiring.SetParameter("ZVel", ZVel);

        float hpParam = 1f - (hp.currentHP / hp.maxHP);
        backgroundMusic.SetParameter("HP", hpParam);

        if (playerControl.paused)
        {
            pause = 1f;
        }
        else
        {
            pause = 0f;
        }

        backgroundMusic.SetParameter("Paused", pause);

        if (combat)
        {
            combatvalue = Mathf.MoveTowards(combatvalue, 1.0f, transitionSpeed * Time.unscaledDeltaTime);
            backgroundMusic.SetParameter("InCombat", combatvalue);
        }
        else
        {
            combatvalue = Mathf.MoveTowards(combatvalue, 0.0f, transitionSpeed * Time.unscaledDeltaTime);
            backgroundMusic.SetParameter("InCombat", combatvalue);
        }
    }

    public void PitchBend()
    {
        if (!playerControl.grounded && playerControl.playerSpeed >= 20)
        {
            backgroundMusic.SetParameter("JumpPitch", 1.0f);
        }
        else
        {
            backgroundMusic.SetParameter("JumpPitch", 0.0f);
        }
    }

    public IEnumerator CombatCheck()
    {
        while (true)
        {
            Collider[] enemyCheck = Physics.OverlapSphere(playerControl.transform.position, 30f, enemyMask);
            if (enemyCheck.Length > 0)
            {
                if(enemyCheck[0].gameObject.GetComponentInParent<Enemy>().engage)
                    combat = true;
            }
            else {
            if (combat)
            {
                yield return new WaitForSeconds(2f);
                    combat = false;
            } 
            }
         yield return new WaitForSeconds(combatcheckdel);
        }
    }

    public IEnumerator ZoneCheck()
    {
        int lastActive = -1;

        while(true)
        {
            int foundZone = -1;
            for(int i = 0; i < musicZones.Count; i++){
                bool inZone = musicZones[i].bounds.Contains(player.transform.position);
                if(inZone){
                    foundZone = i;
                    break;
                }
            }

            if(foundZone != -1 && foundZone != lastActive){
                for(int i = 0; i < musicZones.Count; i++){
                    musicZoneFlag[i] = (i == foundZone);
                }
                zonechange = true;
                drumtimer = 0f;
                basstimer = 0f;
                lastActive = foundZone;
            }
            yield return new WaitForSeconds(zonecheckdel);
        }
    }
}
