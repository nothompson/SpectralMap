using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicManagement : MonoBehaviour
{
    [Header("Magic Management")]
    public float magicPoints = 100f;

    public float maximumMagic = 100f;

    public float magicBufferTime = 2f;

    public float regenTimer = 0;

    public float regenSpeed = 30;

    public bool justUsed = false;

    public void magicRegen()
    {
        if (justUsed && regenTimer > 0)
        {
            regenTimer -= Time.deltaTime;
        }
        if (regenTimer <= 0 && magicPoints < maximumMagic)
        {
            justUsed = false;
            magicPoints += Time.deltaTime * regenSpeed;
        }
        if (magicPoints == maximumMagic)
        {
            regenTimer = magicBufferTime;
        }
        if (magicPoints > maximumMagic)
        {
            magicPoints = maximumMagic;
        }
    }

    public void Drain(float x)
    {
        if (PlayerManager.Instance.Forgiveness)
        {
            float r = Random.Range(0f,1f);
            if(r <= 0.25f)
            {
                return;
            }
        }
        magicPoints -= x;

        Bounds();
    }

    public void Replenish(float x)
    {
        magicPoints +=x;

        Bounds();
    }
    
    public void Bounds()
    {
        if(magicPoints <= 0f)
        {
            magicPoints = 0f;
        }

        if (magicPoints >= maximumMagic)
        {
            magicPoints = maximumMagic;
        }
    }
}
