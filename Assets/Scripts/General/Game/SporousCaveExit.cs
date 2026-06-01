using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SporousCaveExit : MonoBehaviour
{
    public Transform blocking;
    public Transform opened; 

    public void Start()
    {
        StartCoroutine(Checking());
    }

    IEnumerator Checking()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            if(JournalManager.Instance == null) 
            {
                
            yield return null; 

            continue;

            }

            if(JournalManager.Instance.HasJournalEntry("sporouscave", 1))
            {
                transform.position = opened.position;
            }
            else
            {
                transform.position = blocking.position;
            }
        }
    }
}