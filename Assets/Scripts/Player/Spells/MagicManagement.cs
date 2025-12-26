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

    // Update is called once per frame
    void Update()
    {
        magicRegen();
    }
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
}
