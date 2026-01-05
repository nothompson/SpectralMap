using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class FeedManager : MonoBehaviour
{

    [System.Serializable]
    public class FeedEntry
    {
        public string text;
        public float lifeTime;

        public FeedEntry(string t, float decay)
        {
            text = t;
            lifeTime = decay;
        }
    }

    public static FeedManager Instance;

    public GameObject canvas; 

    public TMP_Text tmp;

    private SpriteText Text;

    public float DecayTime;

    List<FeedEntry> outputs = new List<FeedEntry>();

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

        Text = tmp.GetComponent<SpriteText>();
    }

    public void Update()
    {
        if(outputs.Count < 1) return;

        float dt = Time.deltaTime;

        bool hasChanged = false;

        for (int i = outputs.Count - 1; i >= 0; i--)
        {
            outputs[i].lifeTime -= dt;
            if(outputs[i].lifeTime <= 0f)
            {
                outputs.RemoveAt(i);
                hasChanged = true;
            }
        }

        if (hasChanged)
        {
            BuildFeed();
        }
    }

    public void AddToFeed(string input)
    {
        outputs.Add(new FeedEntry(input,DecayTime));

        BuildFeed();

        AudioManager.Instance.JournalEntry();
    }

    public void BuildFeed()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for(int i = 0; i < outputs.Count; i++)
        {
            sb.Append(outputs[i].text);
            sb.AppendLine();
            sb.AppendLine();
        }

        Text.input = sb.ToString();
        Text.Refresh();
    }

}
