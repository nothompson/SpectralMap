using UnityEngine;
using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;

[Serializable]
public class GameSettingsData
{
    //Audio
    public float masterVolume;
    public float musicVolume;
    public float soundsVolume;

    //Video
    public Resolution resolution;
    public FullScreenMode windowType;

    //Game
    public float sensitivity;
    public int fov;

    //Crosshairs
    public int crosshairIndex;
    public Vector3 crosshairScale;
    public Vector3 crosshairRotation;
    public Color crosshairColor;

    public string inputBindings;
}
