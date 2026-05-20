using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 using UnityEngine.Events;

public class DetectObject : MonoBehaviour
{
    [SerializeField] public UnityEvent OnDetect;

    public float range;
    public string ID;
    public LayerMask targetMask;

    public bool detected = false;

    public void Start()
    {
        StartCoroutine(Scanning());
    }

    public IEnumerator Scanning()
    {
        while (!detected)
        {
            yield return new WaitForSeconds(1f);

            Collider[] hits = Physics.OverlapSphere(transform.position, range, targetMask);
            if (hits.Length != 0)
            {
                Detectable detectable = hits[0].GetComponent<Detectable>();
                if (detectable != null && detectable.ID == ID)
                {
                    OnDetect?.Invoke();
                    detected = true;
                }
            }
    }
    }
}