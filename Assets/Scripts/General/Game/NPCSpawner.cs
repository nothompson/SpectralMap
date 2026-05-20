using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npc;
    public void Start()
    {
        StartCoroutine(Spawn());
    }
    
    public IEnumerator Spawn()
    {
        while (true)
        {
        GameObject instance = Instantiate(npc, transform);
        yield return new WaitForSeconds(10f);
        Destroy(instance);
        }
    }
}