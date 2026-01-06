using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using MovementPhysics;

public class PlayerControlRigid : MonoBehaviour, IKnockback
{
    [Header("References")]
    public Rigidbody rb;
    public Transform GroundCheck;
    public LayerMask GroundMask;
    public LayerMask resetMask;
    public LayerMask checkpointMask;
    public Checkpoint currentCheckpoint;
    public Transform player;
    public Transform playerCamera = null;
    public Transform attackingPoint = null;
    public Transform gun = null;
    public Transform[] hands = null;
    public GameObject YawPivot; 

    [Header("Movement Control")]
    public float moveSpeed = 6.0f;
    public float jumpHeight = 8.0f;
    public float gravity = -20.0f;
    public float airAcceleration = 2.0f;
    public float friction = 5.0f;
    public float runDeacceleration = 10.0f;
    public float runAcceleration = 15.0f;
    public float queueParam = 0.5f;
    public float pogoTime = 0.15f;

    public float syncTime = 0.1f;

    public bool confused = false;

    [Header("Collision/Surf Params")]
    //buffer distance/cushioning for ground check
    public float GroundDistance = 0.1f;
    public bool grounded;
    public bool surfing;

    public bool reset;
    public bool groundedLastFrame = false;

    public bool surfCast;
    public RaycastHit surfHit;
    

    [Header("Camera")]
    public AnimationCurve bounceCurve;
    public AnimationCurve jumpCurve;
    public AnimationCurve shakeCurve;

    //private variables
    public bool lockCursor = true;
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
    float coyoteTime = 0.25f;
    float groundTimer = 0f;
    float queueTimer = 0.0f;
    //local x rotation of camera
    float cameraPitch = 0.0f;
    float cameraTilt = 0.0f;

    float cameraYaw = 0.0f;
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
    public bool RocketJumped = false;
    public bool CanPogo = false;
    public bool Synced = false;
    public bool StartSyncTimer = false;
    public int syncHits = 0;
    float pogoTimer = 0f;
    public float syncTimer = 0f;
    bool StartPogoTimer = false;
    public bool syncResult = false;
    Vector3[] restingPos;
    Vector3[] restingRot;

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
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        AudioManager.Instance.RegisterPlayer(gameObject);
    }

    void SetResting()
    {
        restingPos = new Vector3[hands.Length];
        restingRot = new Vector3[hands.Length];

        for(int i = 0; i < hands.Length; i++){
            restingPos[i] = hands[i].localPosition;
            restingRot[i] = hands[i].localEulerAngles;
        }
    }

    void Update()
    {
        MovementInputs();
        PlayerCamera();
    }

    void FixedUpdate()
    {
        CalculateVelocity();
        MovementFunctions.ApplyVelocity(playerVelocity, ref rb);
        currentCheckpoint.saveCheckpoint();
    }

    void CalculateVelocity()
    {
        GroundedCheck();
        SurfCheck();
        Jump();
        Movement();
        Tricks();
        groundedLastFrame = grounded;
    }

    #endregion


    #region Camera Juice
    void PlayerCamera()
    {

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!paused)
        {
            lockCursor = true;
            tilt();
            //rotate entire player capsule on the y axis, camera moves along as child object
            Vector2 mouseDelta = InputManager.Instance.inputs.Player.Look.ReadValue<Vector2>();

            cameraYaw += mouseDelta.x * SettingsMenu.Instance.mouseSensitivity;

            YawPivot.transform.localEulerAngles = new Vector3(0f, cameraYaw, 0f);

            // transform.Rotate(Vector3.up * mouseDelta.x * SettingsMenu.Instance.mouseSensitivity);

            //inverse y axis and clamp to avoid flipping
            cameraPitch -= mouseDelta.y * SettingsMenu.Instance.mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

            if (confused)
            {
                cameraTilt *= -1f;
            }
            playerCamera.localEulerAngles = new Vector3(cameraPitch, 0f, cameraTilt);
            attackingPoint.localEulerAngles = new Vector3(cameraPitch, 0f, cameraTilt);

            for(int i = 0; i < hands.Length; i++){
                hands[i].localEulerAngles = new Vector3(restingRot[i].x + cameraTilt, restingRot[i].y, restingRot[i].z);
            }


            if (grounded && !groundTrigger)
            {
                StartCoroutine(bounce());
                AudioManager.Instance.Land();
            }
            groundTrigger = grounded;

            if (playerVelocity.y < -10 && !jumped)
            {
                StartCoroutine(jumpBounce());
            }

        }
        else
        {
            lockCursor = false;
        }
    }

    public void tilt()
    {
        float strafeSpeed = Mathf.Abs(Vector3.Dot(playerVelocity, transform.right));

        float tiltAmount = 5f * strafeSpeed * 0.05f;
        if (InputManager.Instance.inputs.Player.Move.ReadValue<Vector2>().x < -0.1f)
        {
            cameraTilt = Mathf.SmoothStep(cameraTilt, tiltAmount, Time.deltaTime * 10f);
        }
        else if (InputManager.Instance.inputs.Player.Move.ReadValue<Vector2>().x > 0.1f)
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
            for(int i = 0; i < hands.Length; i++)
            {
                hands[i].localPosition = new Vector3(restingPos[i].x,restingPos[i].y * jumpAmp, restingPos[i].z);
            }
            jumped = true;
            yield return null;
        }
    }
    public IEnumerator bounce()
    {
        float dur = 0.75f;
        float t = 0f;

        while (t < dur)
        {
            jumped = false;
            t += Time.deltaTime;
            float elapsed = t / dur;
            float bounceAmp = bounceCurve.Evaluate(elapsed);
            for(int i = 0; i < hands.Length; i++)
            {
                hands[i].localPosition = new Vector3(restingPos[i].x,restingPos[i].y * bounceAmp, restingPos[i].z);
            }
            yield return null;
        }
        for(int i = 0; i < hands.Length; i++)
            {
                hands[i].localPosition = restingPos[i];
            }
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
            t += Time.unscaledDeltaTime;
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

        if(reset){
            currentCheckpoint.Reset();
        }

        bool landed = grounded && !groundedLastFrame;

        surfing = surfCast && MovementFunctions.CanSurf(surfHit);

        if (grounded)
        {
            groundMove();
            if (RocketJumped)
            {
                RocketJumped = false;
            }
            if (StartPogoTimer)
            {
                StartPogoTimer = false;
            }
            if (CanPogo)
            {
                CanPogo = false;
            }
            
            pogoTimer = 0f;

            if(landed && TrickManager.Instance.Score > 0 && !TrickManager.Instance.completed)
            {
                TrickManager.Instance.StartComboTimer();
            }

        }
        else
        {
            airMove();

            StartPogoTimer = true;
        }

        if (surfing)
        {
            grounded = false;
            surfMove();
        }

        if (impact.magnitude > 0.1f)
        {
            playerVelocity += impact;
            //reset so it doesnt accumulate
            impact = Vector3.zero;
        }

        playerSpeed = rb.linearVelocity.magnitude;
    }

    void GroundedCheck()
    {
        grounded = MovementFunctions.GroundedCheck(
            GroundCheck,
            GroundDistance,
            GroundMask,
            ref playerVelocity,
            ref groundTimer,
            coyoteTime
        );

        reset = MovementFunctions.ResetCheck(
            GroundCheck,
            GroundDistance,
            resetMask
        );
    }

    public void SurfCheck()
    {
        surfCast = Physics.Raycast(GroundCheck.position, Vector3.down, out surfHit, GroundDistance + 1.5f, GroundMask);
    }

    void Jump()
    {
        //if grounded and jump iwnput, then jump
        if (InputManager.Instance.inputs.Player.Jump.triggered)
        {
            jumpQueue = true;
        }
        //if queued, then start ticking down timer
        if (jumpQueue && queueTimer >= 0)
        {
            queueTimer -= Time.fixedDeltaTime;
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

    public void Tricks()
    {
        if (StartPogoTimer)
        {
            pogoTimer+= Time.fixedDeltaTime;
            if(pogoTimer > pogoTime)
            {
                pogoTimer = pogoTime;
                CanPogo = true;
                StartPogoTimer = false;
            }
        }

        if (StartSyncTimer)
        {
            syncTimer+= Time.fixedDeltaTime;
            if(syncTimer >= syncTime)
            {
                if (syncResult)
                {
                    if(syncHits > 1)
                    {
                        TrickManager.Instance.Sync(syncHits);
                    }
                    else
                    {
                        TrickManager.Instance.AddTrick("RocketJump");
                    }
                }

                StartSyncTimer = false;
                syncResult = false;
                syncHits = 0;
                syncTimer = 0f;
            }
        }
    }

    public void MovementInputs()
    {
        Vector2 move = InputManager.Instance.inputs.Player.Move.ReadValue<Vector2>();
        fmove = move.y;
        smove = move.x;

        if (confused)
        {
            fmove *= -1f;
            smove *= -1f;
        }
    }

    public void airMove()
    {
        playerVelocity = MovementFunctions.AirMovement(
            playerVelocity,
            YawPivot.transform,
            fmove, smove,
            moveSpeed,
            airAcceleration
        );
    }

    public void groundMove()
    {
        applyFriction(1f);

        playerVelocity = MovementFunctions.GroundMovement(
            playerVelocity,
            YawPivot.transform,
            fmove, smove,
            moveSpeed,
            runAcceleration
        );
    }

    public void surfMove()
    {

        Vector3 normal = surfHit.normal;

        Vector3 alongSlope = Vector3.ProjectOnPlane(playerVelocity, normal);
        playerVelocity = alongSlope;

        Vector3 downhill = Vector3.Cross(Vector3.Cross(normal, Vector3.down), normal).normalized;
        playerVelocity += downhill * MovementFunctions.Gravity.magnitude * Time.fixedDeltaTime;

        Vector3 moveInput = YawPivot.transform.TransformDirection(new Vector3(0,0,fmove));
        Vector3 wishVel = Vector3.ProjectOnPlane(moveInput.normalized * moveSpeed, normal);
        playerVelocity = MovementFunctions.Accelerate(playerVelocity, wishVel.normalized, wishVel.magnitude, runAcceleration);

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