using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissile : Rocket
{
    // Update is called once per frame
    public override void Start()
    {
        player = GameObject.FindWithTag("Player");
        autoTimer -= 1f;
        base.Start();
    }
    public override void Update()
    {
        if (!reflected)
        {
            autoTimer -= Time.deltaTime;

            if (autoTimer > 0)
            {
                Vector3 dir = (player.transform.position - transform.position).normalized;

                Quaternion lookRotation = Quaternion.LookRotation(dir);

                lookRotation *= Quaternion.Euler(90f, 0f, 0f);

                Rotation(lookRotation);

                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, dir * speed * 0.5f, 5f * Time.deltaTime);
            }
            else
            {
                Explode();
            }
        }
    }

    public void Reflect()
    {
        if (reflected)
        {
            StartCoroutine(Redirect());
        }
    }
    public IEnumerator Redirect()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 dir = ray.direction.normalized;

        Quaternion lookRotation = Quaternion.LookRotation(dir);

        yield return new WaitForSeconds(0.1f);

        Rotation(lookRotation);
    }

    public void Rotation(Quaternion rotate)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, rotate, Time.deltaTime * 50f);
    }
}
