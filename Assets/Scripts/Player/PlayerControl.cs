using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using MovementPhysics;

public class PlayerControl : MonoBehaviour, IKnockback
{
    public CharacterController controller = null;

    [Header("References")]
    public Transform GroundCheck;
    public LayerMask GroundMask;
    public LayerMask resetMask;
    public LayerMask checkpointMask;
    public Checkpoint currentCheckpoint;
    public Transform player;
    public Transform playerCamera = null;
    public Transform attackingPoint = null;
    public Transform gun = null;
    public Transform hand = null;

    [Header("Movement Control")]
    public float mouseSensitivity = 3.5f;
    public float moveSpeed = 6.0f;
    public float jumpHeight = 8.0f;
    public float gravity = -20.0f;
    public float airAcceleration = 2.0f;
    public float friction = 5.0f;
    public float runDeacceleration = 10.0f;
    public float runAcceleration = 15.0f;
    public float queueParam = 0.5f;

    public bool confused = false;

    [Header("Collision/Surf Params")]
    public float slopeLimit = 50.0f;
    //buffer distance/cushioning for ground check
    public float GroundDistance = 0.4f;
    public bool grounded;

    [Header("Camera")]
    public AnimationCurve bounceCurve;
    public AnimationCurve jumpCurve;
    public AnimationCurve shakeCurve;

    //private variables
    private bool lockCursor = true;
    float wishSpeed;
    float wishSpeed2;
    bool jumpQueue = false;
    public bool wishJump = false;
    Vector3 vec;
    float accel;
    float drop;
    float control;
    float newSpeed;
    Vector3 moveDirection;
    Vector3 moveDirectionNorm;
    float addSpeed;
    float currentSpeed;
    float accelSpeed;
    float zspeed;
    float speed;
    float dot;
    float k;
    float slopeAngle;
    float coyoteTime = 0.1f;
    float groundTimer = 0f;
    float queueTimer = 0.0f;
    //local x rotation of camera
    float cameraPitch = 0.0f;
    float cameraTilt = 0.0f;
    [Header("For Referencing")]
    public Vector3 playerVelocity;
    private Vector3 impact;
    public bool paused = false;

    float strafingDirection;

    public float playerSpeed = 0;

    bool groundTrigger = false;

    bool jumped = false;

    float fmove;
    float smove;

    Vector3 restingPos;
    Vector3 restingRot;
    KeyCode left = KeyCode.A;
    KeyCode right = KeyCode.D;

    //end of class variables
    //--------------------------------
    #region Initialize
    void Start()
    {
        InitPlayer();
        SetResting();
    }

    void InitPlayer()
    {
        controller = GetComponent<CharacterController>();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void SetResting()
    {
        restingPos = hand.localPosition;
        restingRot = hand.localEulerAngles;
    }

    void Update()
    {
        MovementInputs();
        groundedCheck();
        Jump();
        PlayerCamera();
        Movement();
        currentCheckpoint.saveCheckpoint();
    }

    #endregion


    #region Camera Juice
    void PlayerCamera()
    {
        if (!paused)
        {
            tilt();
            //rotate entire player capsule on the y axis, camera moves along as child object
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            //inverse y axis and clamp to avoid flipping
            cameraPitch -= mouseDelta.y * mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);
            if (confused)
            {
                cameraTilt *= -1f;
            }
            playerCamera.localEulerAngles = new Vector3(cameraPitch, 0, cameraTilt);

            attackingPoint.localEulerAngles = new Vector3(cameraPitch, 0, cameraTilt);

            hand.localEulerAngles = new Vector3(restingRot.x + cameraTilt, restingRot.y, restingRot.z);


            if (grounded && !groundTrigger)
            {
                StartCoroutine(bounce());
            }
            groundTrigger = grounded;

            if (playerVelocity.y < -10 && !jumped)
            {
                StartCoroutine(jumpBounce());
            }

            transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);
        }
    }

    public void tilt()
    {
        float strafeSpeed = Mathf.Abs(Vector3.Dot(playerVelocity, transform.right));

        float tiltAmount = 5f * strafeSpeed * 0.05f;
        if (Input.GetKey(left))
        {
            cameraTilt = Mathf.SmoothStep(cameraTilt, tiltAmount, Time.deltaTime * 10f);
        }
        else if (Input.GetKey(right))
        {
            cameraTilt = Mathf.SmoothStep(cameraTilt, -tiltAmount, Time.deltaTime * 10f);
        }
        else
        {
            cameraTilt = Mathf.SmoothStep(cameraTilt, 0f, Time.deltaTime * 10f);
        }
    }
    public IEnumerator jumpBounce()
    {
        float dur = 3f;
        float t = 0f;

        while (t < dur && !grounded)
        {
            t += Time.deltaTime;
            float elapsed = t / dur;
            float jumpAmp = jumpCurve.Evaluate(elapsed);
            hand.localPosition = restingPos * jumpAmp;
            jumped = true;
            yield return null;
        }
    }
    public IEnumerator bounce()
    {
        float dur = 0.75f;
        float t = 0f;
        Vector3 currentPos = hand.localPosition;

        while (t < dur)
        {
            jumped = false;
            t += Time.deltaTime;
            float elapsed = t / dur;
            float bounceAmp = bounceCurve.Evaluate(elapsed);
            hand.localPosition = restingPos * bounceAmp;
            yield return null;
        }
        hand.localPosition = restingPos;
    }

    public void applyShake(float shakeDur, float mult)
    {
        StartCoroutine(shake(shakeDur, mult));
    }

    public IEnumerator shake(float shakeDur, float mult)
    {
        Vector3 startingPos = playerCamera.localPosition;
        float t = 0f;
        while (t < shakeDur)
        {
            t += Time.deltaTime;
            float shakeAmp = shakeCurve.Evaluate(t / shakeDur);
            playerCamera.localPosition = startingPos + Random.insideUnitSphere * shakeAmp * mult;
            yield return null;
        }
        playerCamera.localPosition = startingPos;
    }

    #endregion

    #region Movement
    void Movement()
    {

        if (grounded)
        {
            groundMove();
        }
        else if (!grounded)
        {
            airMove();
        }

        if (impact.magnitude > 0.1f)
        {
            playerVelocity += impact;
            //reset so it doesnt accumulate
            impact = Vector3.zero;
        }

        playerSpeed = playerVelocity.magnitude;

        controller.Move(playerVelocity * Time.deltaTime);
    }

    void groundedCheck()
    {
        grounded = false;
        //test to see if touching any collisions given ground mask
        if (Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask))
        {
            //now see what the normals are of the surface
            //raycast to find normals of surface
            //in order, 1. check origin point of ray, 2. choose which component of vector to test, 3. out will be filled with data of raycast if hit (how we get normals)
            //4. maximum distance of ray, and 5. what to check ray against
            if (Physics.Raycast(GroundCheck.position, Vector3.down, out RaycastHit hit, GroundDistance + 0.5f, GroundMask))
            {
                //if hit is filled with data, then look to see what the angle is of the collision
                slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                //slope limit defines what is still "flat" ground and what can be surfed on
                if (slopeAngle <= slopeLimit)
                {
                    grounded = true;
                    if (playerVelocity.y < 0)
                    {
                        playerVelocity.y = -2f;
                    }
                    //"coyote time" allows a buffer period after leaving collider to still jump
                    groundTimer = coyoteTime;
                    if (hit.collider.CompareTag("MovingPlatform"))
                    {
                        Vector3 platformVelocity = hit.collider.GetComponent<MovingPlatform>().platformVelocity;
                        Vector3 projectedVelocity = Vector3.ProjectOnPlane(playerVelocity, hit.normal) + platformVelocity;
                        Debug.Log(projectedVelocity);
                        playerVelocity = platformVelocity;
                    }
                }
                else
                {
                    // SURFING: Slope too steep to walk on
                    // Clip velocity to slide along the surface
                    Vector3 clipped = playerVelocity;
                    clipVelocity(playerVelocity, hit.normal, ref clipped, 1f);
                    playerVelocity = clipped;

                    // Apply reduced gravity along slope for smooth sliding
                    float slopeGravityMultiplier = 0.5f; // Adjust for "stickiness"
                    Vector3 slopeGravity = Vector3.ProjectOnPlane(Physics.gravity, hit.normal);
                    playerVelocity += slopeGravity * slopeGravityMultiplier * Time.deltaTime;

                    // Keep you from falling through slope
                    if (playerVelocity.y < -2f)
                    {
                        playerVelocity.y = -2f;
                    }
                }
            }
        }

        if (Physics.CheckSphere(GroundCheck.position, GroundDistance, resetMask))
        {
            currentCheckpoint.Reset();
        }

        if (!grounded)
        {
            groundTimer -= Time.deltaTime;
        }
    }

    public void clipVelocity(Vector3 input, Vector3 normal, ref Vector3 output, float overbounce = 1f)
    {
        var angle = normal.y;
        var backoff = Vector3.Dot(input, normal) * overbounce;

        for (int i = 0; i < 3; i++)
        {
            var change = normal[i] * backoff;
            output[i] = input[i] - change;
        }

        float adjust = Vector3.Dot(output, normal);
        if (adjust < 0.0f)
        {
            output -= (normal * adjust);
        }
    }

    public void reflectVelocity(RaycastHit hit, Vector3 normal)
    {
        Vector3 clipped = playerVelocity;
        clipVelocity(playerVelocity, normal, ref clipped, 1f);
        playerVelocity = clipped;
    }

    void Jump()
    {
        //if grounded and jump iwnput, then jump
        if (Input.GetButtonDown("Jump"))
        {
            jumpQueue = true;
        }
        //if queued, then start ticking down timer
        if (jumpQueue && queueTimer >= 0)
        {
            queueTimer -= Time.deltaTime;
        }

        //timers up, reset
        if (queueTimer <= 0)
        {
            jumpQueue = false;
            queueTimer = queueParam;
        }
        // //add to jump queue 
        if ((grounded || groundTimer > 0f) && jumpQueue)
        {
            wishJump = true;
            jumpQueue = false;
            groundTimer = 0f;
        }
        //apply jump
        if(grounded && wishJump)
        {
            playerVelocity.y = jumpHeight;
            wishJump = false;
        }
    }

    public void MovementInputs()
    {
        fmove = Input.GetAxisRaw("Vertical");
        smove = Input.GetAxisRaw("Horizontal");

        if (confused)
        {
            fmove *= -1f;
            smove *= -1f;
        }
    }

    public void airMove()
    {
        playerVelocity = MovementFunctions.AirMovement(playerVelocity, playerCamera, fmove, smove, moveSpeed, airAcceleration);

        playerVelocity.y += gravity * Time.deltaTime;

    }

    public void groundMove()
    {
        applyFriction(1f);

        playerVelocity = MovementFunctions.GroundMovement(
            playerVelocity,
            transform,
            fmove, smove,
            moveSpeed,
            runAcceleration
        );

        // if (wishJump)
        // {
        //     playerVelocity.y = jumpHeight;
        //     wishJump = false;
        // }
    }

    public void applyFriction(float t)
    {
        playerVelocity = MovementFunctions.ApplyFriction(t, playerVelocity, friction, runDeacceleration);
    }

    public void addKnockback(Vector3 force)
    {
        impact += force;
    }

    #endregion
}
