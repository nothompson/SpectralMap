using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAudio : MonoBehaviour
{
    [SerializeField] private NPCSounds sound;
    private FMOD.Studio.EventInstance eventInstance;

    public bool init = false;

    public bool random = false;

    public float minWait = 3f;

    public float maxWait = 8f;
    

    void Start()
    {
        if(sound == null) return;
        
        eventInstance = FMODUnity.RuntimeManager.CreateInstance(sound.sound);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventInstance, gameObject);

        if (init)
        {
            Play();
        }

        if (random)
        {
            StartCoroutine(IdlePlay());
        }
    }

    public void Play()
    {
        if (eventInstance.isValid())
        {
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        eventInstance.release();
        }
        
        eventInstance = FMODUnity.RuntimeManager.CreateInstance(sound.sound);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventInstance, gameObject);

        if (sound.looping)
        {
            eventInstance.start();
        }
        else
        {
            FMODUnity.RuntimeManager.PlayOneShotAttached(sound.sound, gameObject);
        }
    }

    public void Stop()
    {
        if (eventInstance.isValid())
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    public void OnDestroy()
    {
        if (eventInstance.isValid())
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }

    public IEnumerator IdlePlay()
    {
        while (true)
        {
            float wait = Random.Range(minWait,maxWait);

            FMODUnity.RuntimeManager.PlayOneShotAttached(sound.sound, gameObject);

            yield return new WaitForSeconds(wait);
        }
    }
}
