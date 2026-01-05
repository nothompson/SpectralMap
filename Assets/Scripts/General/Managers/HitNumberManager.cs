using UnityEngine;

public class HitNumberManager : MonoBehaviour
{
    public static HitNumberManager Instance;
    void Awake()
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

    public void DisplayHitNumber(float damage, Transform transform)
    {
        
    }
}
