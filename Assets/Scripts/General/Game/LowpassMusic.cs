    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using MusicScripts;
    using System;
    using System.Runtime.InteropServices;
public class LowpassMusic : MonoBehaviour
{
    public void Pause()
    {
        AudioManager.Instance.lowpass = 1f;
    }

    public void Unpause()
    {
        AudioManager.Instance.lowpass =0f;
    }
}