using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicScripts;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("References")]
    public GameObject player;
    public PlayerControlRigid pc;
    public HP hp;
    public LayerMask enemyMask;

    [Header("Parameters")]
    [SerializeField] float zonecheckdel = 1f;
    [SerializeField] float combatcheckdel = 1f;
    [SerializeField] float transitionSpeed = 0.25f;
    [SerializeField] float drumdel = 16.0f;
    [SerializeField] float bassdel = 32.0f;

    [Header("Zones")]
    public List<BoxCollider> musicZones;
    public List<float> musicZoneParam;
    public List<bool> musicZoneFlag;

    public UserSounds sounds;

    //private variables

    bool zonechange = false;
    bool combat = false;
    float pause = 0f;
    float combatvalue = 0.0f;

    float drumvalue = 0.0f;
    float bassvalue = 0.0f;

    float drumtimer = 0.0f;
    float basstimer = 0.0f;

    public FMOD.Studio.EventInstance ConfigInstance;

    public FMOD.Studio.EventInstance TextInstance;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitChecks();
    }

    private void InitChecks()
    {
        if(player != null)
        {
            StartCoroutine(CombatCheck());
            StartCoroutine(ZoneCheck());
        }
    }

    public void RegisterPlayer(GameObject input)
    {
        player = input;
        pc = player.GetComponent<PlayerControlRigid>();
        hp = player.GetComponent<HP>();
    }
 
    public void ReInitAudio()
    {
        StopAllCoroutines();

        AssignZones();

        InitChecks();

        ReInitParams();
    }

    private void ReInitParams()
    {
        zonechange = false;
        combat = false;
        pause = 0f;
        combatvalue = 0.0f;

        drumvalue = 0.0f;
        bassvalue = 0.0f;

        drumtimer = 0.0f;
        basstimer = 0.0f;

        for(int i = 0; i < musicZoneFlag.Count; i++)
        {
            musicZoneFlag[i] = false;
            musicZoneParam[i] = 0f;
        }
    }

    public void Land()
    {
        FMODUnity.RuntimeManager.PlayOneShot(sounds.playerLand);
    }

    public void Hurt()
    {
        FMODUnity.RuntimeManager.PlayOneShot(sounds.playerHurt);
    }

    public void UIOpen()
    {
        FMODUnity.RuntimeManager.PlayOneShot(sounds.UIOpen);
    }

    
    public void UIClose()
    {
        FMODUnity.RuntimeManager.PlayOneShot(sounds.UIClose);
    }

    public void UIClick()
    {
        FMODUnity.RuntimeManager.PlayOneShot(sounds.UIClick);
    }

    public void StartConfigHover()
    {
        ConfigInstance = FMODUnity.RuntimeManager.CreateInstance(sounds.ConfigHover);
        ConfigInstance.start();
    }

    public void StopConfigHover()
    {
        if(ConfigInstance.isValid()){
        ConfigInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        ConfigInstance.release();
        }
    }

    public void JournalOpen()
    {
        FMODUnity.RuntimeManager.PlayOneShot(sounds.JournalOpen);
    }

    public void JournalClose()
    {
        FMODUnity.RuntimeManager.PlayOneShot(sounds.JournalClose);
    }


    public void TextOpen()
    {
        if(TextInstance.isValid())
        {
        TextInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        TextInstance.release();
        }

        TextInstance = FMODUnity.RuntimeManager.CreateInstance(sounds.TextOpen);
        TextInstance.start();
    }

    public void TextClose()
    {
        if(TextInstance.isValid())
        {
        TextInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        TextInstance.release();
        }

        TextInstance = FMODUnity.RuntimeManager.CreateInstance(sounds.TextClose);
        TextInstance.start();

    }

    void Update()
    {
       ZoneUpdate();
       DrumAndBass();
       ParamUpdate();
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
            LevelManager.Instance.currentTrack.SetParameter(zone, musicZoneParam[i]);
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
            LevelManager.Instance.currentTrack.SetParameter("Drums", drumvalue);
            LevelManager.Instance.currentTrack.SetParameter("Bass", bassvalue);

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
        if(player!= null){
            if(LevelManager.Instance.currentScene != "MainMenu"){
        float normalizeSpeed = MusicScript.NormalizeForAutomation(pc.playerSpeed, 0f, 70f);
        LevelManager.Instance.currentTrack.SetParameter("WetDryMusic", normalizeSpeed);

        float normalizeXVel = MusicScript.NormalizeForAutomation(pc.rb.linearVelocity.x, -10f, 10f);
        float XVel = Mathf.Abs(normalizeXVel);
        LevelManager.Instance.currentTrack.SetParameter("XVel", XVel);

        float normalizePositiveYVel = MusicScript.NormalizeForAutomation(pc.rb.linearVelocity.y, 0f, 10f);
        float pYVel = Mathf.Abs(normalizePositiveYVel);
        LevelManager.Instance.currentTrack.SetParameter("+YVel", pYVel);

        float normalizeNegativeYVel = MusicScript.NormalizeForAutomation(pc.rb.linearVelocity.y, -20f, 0f);
        float nYVel = Mathf.Abs(normalizeNegativeYVel);
        LevelManager.Instance.currentTrack.SetParameter("-YVel", nYVel);

        float normalizeZVel = MusicScript.NormalizeForAutomation(pc.rb.linearVelocity.z, -10f, 10f);
        float ZVel = Mathf.Abs(normalizeZVel);
        LevelManager.Instance.currentTrack.SetParameter("ZVel", ZVel);

        float hpParam = 1f - (hp.currentHP / hp.maxHP);
        LevelManager.Instance.currentTrack.SetParameter("HP", hpParam);

        if (pc.paused)
        {
            pause = 1f;
        }
        else
        {
            pause = 0f;
        }

        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GlobalPause", pause);

        if (combat)
        {
            combatvalue = Mathf.MoveTowards(combatvalue, 1.0f, transitionSpeed * Time.unscaledDeltaTime);
            LevelManager.Instance.currentTrack.SetParameter("InCombat", combatvalue);
        }
        else
        {
            combatvalue = Mathf.MoveTowards(combatvalue, 0.0f, transitionSpeed * Time.unscaledDeltaTime);
            LevelManager.Instance.currentTrack.SetParameter("InCombat", combatvalue);
        }
        }
        }
    }

    public IEnumerator CombatCheck()
    {
        while (true)
        {
            if(player == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            Collider[] enemyCheck = Physics.OverlapSphere(player.transform.position, 30f, enemyMask);
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

    public void AssignZones()
    {
        StartCoroutine(Assign());
    }

    public IEnumerator Assign()
    {
        yield return null;

        musicZones.Clear();

        GameObject[] zones = GameObject.FindGameObjectsWithTag("MusicZone");

        var sorted = new List<(string name, BoxCollider collider)>();

        foreach(var zone in zones)
        {
            BoxCollider bc = zone.GetComponent<BoxCollider>();

            if(bc != null)
            {
                string name = zone.name;
                sorted.Add((name,bc));
            }
        }

        sorted.Sort ((x, y) =>
        {
           int a = GetZoneNumber(x.name);
           int b = GetZoneNumber(y.name);
           return a.CompareTo(b);
        });

        foreach(var zone in sorted)
        {
            musicZones.Add(zone.collider);
        }

        musicZoneFlag = new List<bool>(new bool[musicZones.Count]);
        musicZoneParam = new List<float>(new float[musicZones.Count]);
    }

    private int GetZoneNumber(string name)
    {
        int index = name.IndexOf("Zone") + 4;
        return int.Parse(name.Substring(index));
    }

    public IEnumerator ZoneCheck()
    {
        int lastActive = -1;

        while(true)
        {
        if(musicZones.Count < 1|| musicZones == null)
            {
                AssignZones();
                yield return new WaitForSeconds(0.5f);
                continue;
            }
        if(player == null){
            yield return new WaitForSeconds(1f);
            continue;
        }
            
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
