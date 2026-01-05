using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTriggerButton : MonoBehaviour
{
    // Start is called before the first frame update
    public virtual void TriggerSceneChange(string sceneName)
    {
        LevelManager.Instance.LoadScene(sceneName);
    }
}
