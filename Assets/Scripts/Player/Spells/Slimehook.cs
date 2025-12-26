using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slimehook : MonoBehaviour
{
    [Header("References")]
    public LayerMask targetMask;

    public LayerMask groundMask;

    public GameObject SpringObject;

    public Transform player;
    [Header("Params")]
    public float speed = 20f;

    bool slimeOn = false;

    public bool input;

    float distance;

    Rigidbody rb;

    GameObject springInstance;

    public static List<GameObject> activeSprings = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("hook instaniated");

        Spring spring = SpringObject.GetComponent<Spring>();

        //assign each prefab a rigid body   
        rb = GetComponent<Rigidbody>();
        //move based on forward direction and velocity param
        rb.linearVelocity = transform.forward * speed;
        if (springInstance != null)
        {
            Destroy(springInstance);
        }
        
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 7 || other.gameObject.layer == 8 || other.gameObject.layer == 11)
        {
            Debug.Log("slime hook hit");

            slimeOn = true;

            ContactPoint contact = other.contacts[0];

            Quaternion collisionRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);

            springInstance = Instantiate(SpringObject, contact.point, collisionRotation);
            springInstance.GetComponent<Spring>().active = slimeOn;
            springInstance.GetComponent<Spring>().player = player;

            activeSprings.Add(springInstance);
            
            Destroy(gameObject);

        }
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(transform.position, player.position);

        if (distance > 30f)
        {
            Destroy(gameObject, 0.1f);
            // slimeOn = false;
        }

        Debug.Log(slimeOn);
    }

}
