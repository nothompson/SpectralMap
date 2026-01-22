using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public GameObject Container;
    public GameObject SubContainer;

    public SpriteAnimate bagSprite;

    [SerializeField] private AnimationCurve transitionCurve;

    public bool animating = false;
    Coroutine transitionRoutine;
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
    
    public void Open()
    {
        if(animating) return;

        if(JournalManager.Instance.animating || SettingsMenu.Instance.animating) return;

        SubContainer.SetActive(false);

        Container.SetActive(true);

        bagSprite.index = bagSprite.sprites.Length - 1;

        StartTransition(true);

        StartCoroutine(bagSprite.AnimateToTarget(0, null, () =>
        {
            SubContainer.SetActive(true);
            animating = false;
        }));
        animating = true;
    }

    public void Close()
    {
        if(animating) return;

        bagSprite.index = 0;

        StartTransition(false);

        SubContainer.SetActive(false);
        StartCoroutine(bagSprite.AnimateToTarget(bagSprite.sprites.Length - 1, null, () =>
        {
            Container.SetActive(false);
            animating = false;
        }));
        animating = true;
    }

    public void StartTransition(bool intro)
    {
        if(transitionRoutine != null) {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
            }
        transitionRoutine = StartCoroutine(Transition(intro));
    }

    IEnumerator Transition(bool intro)
    {
        float t = 0f;
        float dur = intro ? 0.25f : 0.75f;

        Vector3 target = intro ? Vector3.one : Vector3.zero;
        RectTransform rect = Container.GetComponent<RectTransform>();
        Vector3 starting;

        if (intro)
        {
            rect.localScale = Vector3.zero;
            starting = Vector3.one;
        }
        else
        {
            starting = rect.localScale;
        }
        while(t < dur)
        {
            t += Time.unscaledDeltaTime;
            float time = Mathf.Clamp01(t / dur);
            float elapsed = intro ? time : 1f - time;
            float value = transitionCurve.Evaluate(elapsed);

            rect.localScale = starting * value;
            yield return null;
        }
        rect.localScale = target;
        transitionRoutine = null;
    }
}
