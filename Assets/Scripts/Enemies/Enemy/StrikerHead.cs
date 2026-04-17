using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerHead : MonoBehaviour
{
    [SerializeField] Enemy enemy;

    void Update()
    {
        if(enemy.player != null)
        {
            transform.LookAt(enemy.player);
        }
    }
}