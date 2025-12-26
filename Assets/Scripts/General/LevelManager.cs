using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager: MonoBehaviour 
{
    public static LevelManager Instance;

    [SerializeField] private GameObject _loaderCanvas;

    [SerializeField] private Slider _slider;

    public FMODUnity.StudioEventEmitter currentTrack;

    public string currentScene;

    [Header("Music")]

    [SerializeField] private SceneMusic[] tracks = new SceneMusic[0];

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public FMODUnity.StudioEventEmitter sceneTrack;
    }

    public Dictionary<string, FMODUnity.StudioEventEmitter> MusicDict;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            MusicDict = new Dictionary<string, FMODUnity.StudioEventEmitter>();
            foreach(var i in tracks)
            {
            if(i.sceneTrack != null)
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
        if(currentTrack != null && currentTrack.IsPlaying())
        {
            var eventInstance = currentTrack.EventInstance;
            if (eventInstance.isValid())
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        yield return StartCoroutine(Load(sceneName));

        PlayTrack(sceneName);

    }

    private void PlayTrack(string sceneName)
    {
        if(currentTrack != null)
        {
            currentTrack.Stop();
            currentTrack = null;
        }

        if(MusicDict.TryGetValue(sceneName, out FMODUnity.StudioEventEmitter track))
        {
            if(track != null)
            {
                currentTrack = track;
                if(AudioManager.Instance != null)
                {
                    AudioManager.Instance.ReInitAudio();
                }
                currentTrack.Play();
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
}
