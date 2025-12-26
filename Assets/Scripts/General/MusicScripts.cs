using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace MusicScripts
{
    public static class MusicScript
    {
    public static float NormalizeForAutomation(float input, float minimum, float maximum)
        {
            return (1.0f / maximum) * (Mathf.Clamp(input, minimum, maximum));
        }

    public static Dictionary<Key, float> MajorScale = new Dictionary<Key, float>()
    {
        { Key.Digit1, 0.0f },
        { Key.Digit2, 2.0f },
        { Key.Digit3, 4.0f },
        { Key.Digit4, 5.0f },
        { Key.Digit5, 7.0f },
        { Key.Digit6, 9.0f },
        { Key.Digit7, 11.0f },
        { Key.Digit8, 12.0f },
        { Key.Digit9, 16.0f },
        { Key.Digit0, 19.0f },
        { Key.Minus, 21.0f },
        { Key.Equals, 23.0f}
    };

    public static Dictionary<Key, float> MinorScale = new Dictionary<Key, float>()
    {
        { Key.Digit1, 0.0f },
        { Key.Digit2, 2.0f },
        { Key.Digit3, 3.0f },
        { Key.Digit4, 5.0f },
        { Key.Digit5, 7.0f },
        { Key.Digit6, 8.0f },
        { Key.Digit7, 10.0f },
        { Key.Digit8, 12.0f }
    };

    public static void PlayNotes(FMODUnity.StudioEventEmitter instrument, Dictionary<Key, float> scale, int keyOffset)
    {
        foreach (var note in scale)
        {
            if (Keyboard.current[note.Key].wasPressedThisFrame)
            {
                var value = note.Value + keyOffset;
                instrument.Play();
                instrument.SetParameter("Pitch", value);
            }
        }
    }

    }
}
