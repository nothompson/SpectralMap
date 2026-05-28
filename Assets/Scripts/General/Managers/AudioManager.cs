    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using MusicScripts;
    using System;
    using System.Runtime.InteropServices;

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("References")]
        public GameObject player;
        public PlayerControlRigid pc;
        public HP hp;
        public LayerMask enemyMask;
        public UIPulse heartbeat;

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

        public event Action <int,int> OnBeat;
        public event Action <int> OnBar;

        [Header("Event Instances")]

        public FMOD.Studio.EventInstance ConfigInstance;

        public FMOD.Studio.EventInstance TextInstance;

        public FMOD.Studio.EventInstance BagInstance;

        [Header("Rhythm")]
        public bool InBeatWindow;
        public float BeatWindowSize;
        public float BeatDur;
        private float beatTimer = 0f;

        private float OpeningWindow;
        private float ClosingWindow; 
        private float Buffer = 0.002f;

        public class MusicInfo
        {
            public int bar;
            public int beat;
            public float tempo;
            public int position;
        }

        public static Dictionary <FMOD.Studio.EventInstance, AudioManager.MusicInfo> musicInfos = new Dictionary<FMOD.Studio.EventInstance, AudioManager.MusicInfo>();

        
            //private variables

            bool zonechange = false;
            public bool combat = false;
            bool overrideCombat = false;
            public float pause = 0f;
            float combatvalue = 0.0f;

            float drumvalue = 0.0f;
            float bassvalue = 0.0f;

            float drumtimer = 0.0f;
            float basstimer = 0.0f;

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
            OnBeat += BeatWindow;
        }
        private void OnDestroy()
        {
            OnBeat -= BeatWindow;
        }

        public void EnableCombat()
        {
            combat = true;
            overrideCombat = true;
        }
        public void DisableCombat()
        {
            combat = false;
            overrideCombat = false;
        }

        public void RegisterPlayer(GameObject input)
        {
            player = input;
            pc = player.GetComponent<PlayerControlRigid>();
            hp = player.GetComponent<HP>();
            heartbeat = player.GetComponentInChildren<UIPulse>();

            StopAllCoroutines();
            StartCoroutine(CombatCheck());
            StartCoroutine(ZoneCheck());
        }
    
        public void ReInitAudio()
        {
            StartCoroutine(Init());
        }

        public IEnumerator Init()
    {
            StopCoroutine(ZoneCheck());
            StopCoroutine(CombatCheck());
            yield return StartCoroutine(Assign());
            ReInitParams();
            StartCoroutine(ZoneCheck());
            StartCoroutine(CombatCheck());
            yield return null;
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

        public static FMOD.RESULT MusicCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instanceptr, IntPtr paramptr)
        {
            FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instanceptr);

            if(type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
            {

                if(!musicInfos.TryGetValue(instance, out MusicInfo info)) return FMOD.RESULT.OK;

                var mus = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES) Marshal.PtrToStructure(paramptr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));

                info.bar = mus.bar;
                info.beat = mus.beat;
                info.tempo = mus.tempo;
                info.position = mus.position;

                if(Instance != null)
                {
                    Instance.QueueBeatEvent(mus.bar, mus.beat);
                }
            }
            return FMOD.RESULT.OK;
        }

        private Queue <(int bar, int beat)> beatQueue = new Queue<(int bar, int beat)>();

        private void QueueBeatEvent(int bar, int beat)
        {
            lock (beatQueue)
            {
                beatQueue.Enqueue((bar,beat));
            }
        }

        void BeatWindow(int bar, int beat)
        {
            MusicInfo info = GetMusicInfo();
            if(info != null && info.tempo > 0)
            {
                //how many beats per second
                BeatDur = 60f / info.tempo;
            }

            OpeningWindow = BeatWindowSize * 0.5f;
            ClosingWindow = BeatWindowSize * 0.5f;

            beatTimer = Time.unscaledTime;
        }

        void CheckBeatWindow()
        {
            if(beatTimer > 0f && BeatDur > 0f)
            {
                float LastBeat = Time.unscaledTime - beatTimer;
                float NextBeat = BeatDur - LastBeat;

                bool close = LastBeat <= ClosingWindow + Buffer;
                bool open = NextBeat <= OpeningWindow + Buffer && NextBeat > -1f * Buffer;

                InBeatWindow = close || open;
            }
            else
            {
                InBeatWindow = false;
            }
        }

        public bool IsOnBeat()
        {
            return InBeatWindow;

        }


        void Update()
        {
            ProcessTempo();

            CheckBeatWindow();

            ZoneUpdate();
            DrumAndBass();
            ParamUpdate();
        }

        void ProcessTempo()
        {
            lock (beatQueue)
            {
                while(beatQueue.Count > 0)
                {
                    var(bar,beat) = beatQueue.Dequeue();

                    MusicInfo info = GetMusicInfo();
   
                    OnBeat?.Invoke(bar,beat);

                    if(beat == 1)
                    {
                        OnBar?.Invoke(bar);
                    }

                    if(pc.playerSpeed >= 40f)
                    {
                        if(heartbeat != null)
                            {
                                StartCoroutine(heartbeat.Pulse(BeatDur * 0.95f));
                            }
                    }
                    if(pc.playerSpeed < 40f && pc.playerSpeed >= 15f){
                        if(beat == 1 || beat == 3)
                        {
                            if(heartbeat != null)
                            {
                                StartCoroutine(heartbeat.Pulse(BeatDur * 1.95f));
                            }
                        }
                    }
                    if(pc.playerSpeed < 15f)
                    {
                        if(beat == 1)
                        {
                            if(heartbeat != null)
                                {
                                    StartCoroutine(heartbeat.Pulse(BeatDur * 3.95f));
                                }
                        }
                    }

                    if(ReloadManager.Instance.reloading && ReloadManager.Instance.ReloadSprite != null)
                    {
                        StartCoroutine(ReloadManager.Instance.Pulse());
                    }

                }
            }
        }

        public MusicInfo GetMusicInfo()
        {
            if(LevelManager.Instance.currentTrack.isValid()){

                if(musicInfos.TryGetValue(LevelManager.Instance.currentTrack, out MusicInfo info))
                {
                    return info;
                }
            }
            return null;
        }

        void ZoneUpdate()
        {
            var track = LevelManager.Instance.currentTrack;
            if(!track.isValid()) return; 
            for(int i = 0; i < musicZones.Count; i++){
                if(musicZoneFlag[i]){
                    musicZoneParam[i] = Mathf.MoveTowards(musicZoneParam[i], 1.0f, transitionSpeed * Time.unscaledDeltaTime);
                }
                else{
                    musicZoneParam[i] = Mathf.MoveTowards(musicZoneParam[i], 0.0f, transitionSpeed * Time.unscaledDeltaTime);
                }
                string zone = "Zone" + (i+1).ToString();
                LevelManager.Instance.currentTrack.setParameterByName(zone, musicZoneParam[i]);
            }
        }

        void DrumAndBass()
        {
            if(!LevelManager.Instance.currentTrack.isValid()) return;
            if(zonechange){
                drumtimer += Time.unscaledDeltaTime;
                basstimer += Time.unscaledDeltaTime;
                if(drumtimer >= drumdel){
                    drumvalue = Mathf.MoveTowards(drumvalue, 1.0f, transitionSpeed * Time.unscaledDeltaTime);
                }
                if(basstimer >= bassdel){
                    bassvalue = Mathf.MoveTowards(bassvalue, 1.0f, transitionSpeed * Time.unscaledDeltaTime);
                }
                LevelManager.Instance.currentTrack.setParameterByName("Drums", drumvalue);
                LevelManager.Instance.currentTrack.setParameterByName("Bass", bassvalue);

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

            if (!LevelManager.Instance.currentTrack.isValid()) return;

            if(LevelManager.Instance.currentScene != "MainMenu"){

            float normalizeSpeed = MusicScript.NormalizeForAutomation(pc.playerSpeed, 0f, 70f);
            LevelManager.Instance.currentTrack.setParameterByName("WetDryMusic", normalizeSpeed);

            float normalizeXVel = MusicScript.NormalizeForAutomation(pc.rb.linearVelocity.x, -10f, 10f);
            float XVel = Mathf.Abs(normalizeXVel);
            LevelManager.Instance.currentTrack.setParameterByName("XVel", XVel);

            float normalizePositiveYVel = MusicScript.NormalizeForAutomation(pc.rb.linearVelocity.y, 0f, 10f);
            float pYVel = Mathf.Abs(normalizePositiveYVel);
            LevelManager.Instance.currentTrack.setParameterByName("+YVel", pYVel);

            float normalizeNegativeYVel = MusicScript.NormalizeForAutomation(pc.rb.linearVelocity.y, -20f, 0f);
            float nYVel = Mathf.Abs(normalizeNegativeYVel);
            LevelManager.Instance.currentTrack.setParameterByName("-YVel", nYVel);

            float normalizeZVel = MusicScript.NormalizeForAutomation(pc.rb.linearVelocity.z, -10f, 10f);
            float ZVel = Mathf.Abs(normalizeZVel);
            LevelManager.Instance.currentTrack.setParameterByName("ZVel", ZVel);

            float hpParam = 1f - (hp.currentHP / hp.maxHP);
            LevelManager.Instance.currentTrack.setParameterByName("HP", hpParam);

            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GlobalPause", pause);

            if (combat)
            {
                combatvalue = Mathf.MoveTowards(combatvalue, 1.0f, transitionSpeed * Time.unscaledDeltaTime);
                LevelManager.Instance.currentTrack.setParameterByName("InCombat", combatvalue);
            }
            else
            {
                combatvalue = Mathf.MoveTowards(combatvalue, 0.0f, transitionSpeed * Time.unscaledDeltaTime);
                LevelManager.Instance.currentTrack.setParameterByName("InCombat", combatvalue);
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
                if(!overrideCombat){
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

            int searchTime = 30;
            int frame = 0;

            GameObject[] zones = null;

            while(frame < searchTime)
            {
                zones = GameObject.FindGameObjectsWithTag("MusicZone");

                if(zones.Length > 0) break;

                frame++;
                yield return null;
            }

            if(zones == null || zones.Length == 0)
            {
                musicZoneFlag = new List<bool>();
                musicZoneParam = new List<float>();
                yield break;
            }
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
            if(musicZones == null || musicZones.Count < 1)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }
            if(player == null){
                yield return new WaitForSeconds(1f);
                continue;
            }
                
                int foundZone = -1;
                for(int i = 0; i < musicZones.Count; i++){
                    var zone = musicZones[i];
                    if(zone == null) continue;
                    bool inZone = zone.bounds.Contains(player.transform.position);
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

        public void Land()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.playerLand);
        }

        public void Hurt()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.playerHurt);
        }

        public void ReloadSuccess()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.ReloadSuccess);
        }
        public void ReloadFailure()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.ReloadFailure);
        }
        public void ReloadTick()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.ReloadTick);
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

        public void Type()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.Typing);
        }

        public void Pop()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.Pop);
        }

         public void Apop()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.Apop);
        }

        public void WindSlice()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.WindSlice);
        }

        public void RisingTexture()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.RisingTexture);
        }

           public void TransitionTexture()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.TransitionTexture);
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
        
        public void JournalNext()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.JournalNext);
        }

        public void JournalPrevious()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.JournalPrevious);
        }

        public void JournalEntry()
        {
            FMODUnity.RuntimeManager.PlayOneShot(sounds.JournalEntry);
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

        public void BodybagOpen()
        {
            if(BagInstance.isValid())
            {
            BagInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            BagInstance.release();
            }

            BagInstance = FMODUnity.RuntimeManager.CreateInstance(sounds.BodybagOpen);
            BagInstance.start();
            
        }

        public void BodybagClose()
        {
            if(BagInstance.isValid())
            {
            BagInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            BagInstance.release();
            }

            BagInstance = FMODUnity.RuntimeManager.CreateInstance(sounds.BodybagClose);
            BagInstance.start();
            
        }

    }