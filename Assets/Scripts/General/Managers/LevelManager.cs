using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager: MonoBehaviour 
{
    public static LevelManager Instance;

    [SerializeField] private GameObject _loaderCanvas;

    [SerializeField] private Slider _slider;

    public FMOD.Studio.EventInstance currentTrack;

    public string currentScene;

    [Header("Music")]

    [SerializeField] private SceneMusic[] tracks = new SceneMusic[0];

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public FMODUnity.EventReference sceneTrack;
    }

    public Dictionary<string, FMODUnity.EventReference> MusicDict;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            MusicDict = new Dictionary<string, FMODUnity.EventReference>();
            foreach(var i in tracks)
            {
            if(!i.sceneTrack.IsNull)
            {
                MusicDict[i.sceneName] = i.sceneTrack;
            }
        }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
        StartCoroutine(Setup());
    }
    public IEnumerator Setup()
    {
        if(AudioManager.Instance != null) yield return StartCoroutine(AudioManager.Instance.Assign());

        PlayTrack(currentScene);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public async void LoadScene(string sceneName)
    {
        if(GibsManager.Instance != null)
        {
            GibsManager.Instance.ClearPool();
        }
        
        StartCoroutine(LoadMusic(sceneName));
    }

    public IEnumerator LoadMusic(string sceneName)
    {
 
        if (currentTrack.isValid())
        {
            currentTrack.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        yield return StartCoroutine(Load(sceneName));

        yield return null;

        if(AudioManager.Instance != null){
            AudioManager.Instance.pause = 0f;
            yield return StartCoroutine(AudioManager.Instance.Init());
        }


        PlayTrack(sceneName);

    }

    private void PlayTrack(string sceneName)
    {
        if (currentTrack.isValid())
        {
            currentTrack.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentTrack.release();
        }

        if(MusicDict.TryGetValue(sceneName, out FMODUnity.EventReference track))
        {
            if(!track.IsNull)
            {


                currentTrack = FMODUnity.RuntimeManager.CreateInstance(track);

                currentTrack.setCallback(
                    AudioManager.MusicCallback, 
                    FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | 
                    FMOD.Studio.EVENT_CALLBACK_TYPE.STARTED |
                    FMOD.Studio.EVENT_CALLBACK_TYPE.RESTARTED
                );

                if(AudioManager.Instance != null)
                {
                    AudioManager.musicInfos[currentTrack] = new AudioManager.MusicInfo();
                }
                
                currentTrack.start();
            }
        }
        currentScene = sceneName;
    }

    public IEnumerator Load(string sceneName)
    {
        _loaderCanvas.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while(asyncLoad.progress < 0.9f)
        {
            Debug.Log(asyncLoad.progress);
            _slider.value = asyncLoad.progress;
            Debug.Log(_slider.value);
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        _loaderCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDestroy()
    {
        if (currentTrack.isValid())
        {
            AudioManager.musicInfos.Remove(currentTrack);
            currentTrack.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentTrack.release();
        }
    }

        void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioManager.Instance.ReInitAudio();
    }
}

