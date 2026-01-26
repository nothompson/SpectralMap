
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using TMPro;    

public class CrosshairColor : MonoBehaviour
{
    public Image Crosshair;
    public Slider Red;
    public Slider Green;
    public Slider Blue;
    public Slider Alpha;

    public void ChangeColor()
    {
        Crosshair.color = new Color(Red.value, Green.value, Blue.value, Alpha.value);
    }
}
