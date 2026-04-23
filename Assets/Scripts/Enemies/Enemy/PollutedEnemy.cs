using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using MovementPhysics;

public class PollutedEnemy : Enemy
{
    public ModularBody modularBody;

    public override void Start()
    {
        base.Start();
        Load();
    }

    public async void Load()
    {
        await modularBody.LoadRandomParts();
    }
}