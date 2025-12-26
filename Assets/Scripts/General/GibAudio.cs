using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GibAudio : MonoBehaviour
{

    public FMODUnity.EventReference gibLand;

    private bool landed;

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if(landed) return;

        if (collision.gameObject.layer == 19) return;

        landed = true;

        FMODUnity.RuntimeManager.PlayOneShot(gibLand,transform.position);
    }

    private void OnEnable()
    {
        landed = false;
        StartCoroutine(ResetLanded());
    }

    public IEnumerator ResetLanded()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb!= null)
        {
            while(true){
            if (rb.linearVelocity.magnitude > 3f)
            {
                landed = false;
            }
            yield return new WaitForSeconds(0.25f);
            }
        }
    }
}
