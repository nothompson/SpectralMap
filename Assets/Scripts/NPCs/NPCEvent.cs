using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCEvent : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private NPCAudio sound;
    public void StartAudio()
    {
        sound.Play();
    }

    public void StopAudio()
    {
        sound.Stop();
    }
}
