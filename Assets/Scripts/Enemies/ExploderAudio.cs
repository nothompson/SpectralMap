using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploderAudio : EnemyAudio
{
    // Start is called before the first frame update

    ExploderEnemy exploder; 

    bool bounced = false;

    bool exploded = false;

    
    public FMODUnity.StudioEventEmitter bounce;

    
    public FMODUnity.StudioEventEmitter explode;
    public override void Start()
    {
        exploder = enemy.GetComponent<ExploderEnemy>();
        base.Start();
    }

    public override void Update()
    {
        base.Update();
        Bounced();
        Explode();
    }

    public void Bounced()
    {
        if(exploder.bounced && !bounced)
        {
            bounce.Play();
        }
        bounced = exploder.bounced;
    }

    public void Explode()
    {
        if(exploder.exploded && !exploded)
        {
            explode.Play();
        }
        exploded = exploder.exploded;
    }

}
