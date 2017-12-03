using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DepleteEnd
{
    ArrivedToGoal,
    Exhausted
}

public static class RaycastLayers
{
    public static readonly int wallCollisions;
    public static readonly int groundCollisions;
    public static readonly int normalCollisions;
    public static readonly int upRay;
    public static readonly int downRay;

    static RaycastLayers()
    {
        groundCollisions = 1 << LayerMask.NameToLayer("Ground");
        wallCollisions = 1 << LayerMask.NameToLayer("Bounds");
        normalCollisions = groundCollisions | wallCollisions;
        upRay = groundCollisions
            | 1 << LayerMask.NameToLayer("SoftTop");
        downRay = groundCollisions
            | 1 << LayerMask.NameToLayer("SoftBottom");
    }
}

public class Player : MonoBehaviour
{
    private const float SQRT_2 = 1.41421356237f;

    [Header("Energy stuff")]
    [Tooltip("Maximum energy")]
    public float m_maxEnergy = 100.0f;
    [Tooltip("Rate of energy depletion (in units/s)")]
    public float m_depletionRate = 1f;
    [Tooltip("Collectable contribution (in units/s)")]
    public float m_depletionCollectionIncrease = 0.1f;
    float _energyLeft = 0;
    float _currentDepletionRate = 0;

    public int boostCost = 1;
    public float boostCooldown = 3.0f;
    public float boostDuration = 0.8f;
    public float requiredBoostChargeTime = 0.8f;
    private float _remainingBoostCooldown = 0f;
    private float _elapsedBoost = 0f;
    bool _boosting = false;
    bool _impulseLocked = false;
    private float _elapsedBoostCharged = 0f;


    [Header("Collision tweaks")]
    public int m_vertRaysCount = 3;
    public int m_horzRaysCount = 3;
    public float m_angleLeeway = 5.0f;

    [Header("Jump configuration")]
    [Tooltip("Allow changing x-movement mid-air")]
    public bool m_steerWhileJumping;

    [Tooltip("Max. number of units the player will jump")]
    public float m_maxHeight = 5.0f;

    [Tooltip("Time to reach max height")]
    public float m_maxJumpTime = 1.0f;

    [Tooltip("Min. number of units the player will jump")]
    public float m_minHeight = 2.0f;

    [Tooltip("Max. jump length")]
    public float m_maxJumpLength = 8.0f;

    [Tooltip("Consecutive jump limit (2 => double jump)")]
    public int m_maxJumps = 1;

    [Tooltip("Grace period to recognise a jump press before hitting ground")]
    public float m_jumpInterval = 0.02f;

    [Tooltip("Max. fall speed")]
    public float m_maxFallSpeed = 30.0f;
    [Tooltip("Max. glide speed")]
    public float m_maxGlideSpeed = 30.0f;

    [Header("Walk configuration")]
    public float m_acceleration = 0.0f;
    public float m_maxSpeed = 10.0f;
    public float m_maxSpeedGliding = 4.0f;
    public float m_drag = 10.0f;
    public float m_velocityThreshold = 0.5f;
    public float m_velocityFlipThreshold = 0.5f;

    public event Action<Collider2D> FirstPlatformAfterGlide;
    public event Action<Collider2D> PlatformStepped;
    public event Action<DepleteEnd> DepletionFinished;
    public event Action GlideFinished;
    public event Action DepleteBegun;

    public event Action<bool> GameFinished;

    SpriteRenderer _rendererRef;
    SpriteRenderer _glidingRef;

    public int Collected
    {
        get; private set;
    }

    //------------------------------------
    private float m_gravity;
    private float m_initialJumpSpeed;
    private float m_jumpTermVelocity;
    //private float m_jumpTermTime;
    //------------------------------------

    private bool m_grounded;

    private bool m_jumping;
    private bool m_wasJumping;
    private int m_jumpCount;

    private bool m_facingRight;

    private bool m_falling;
    private bool m_wasFalling;

    private float m_jumpHeight;

    private Vector2 m_velocity;

    CapsuleCollider2D _colliderRef;

    private bool _canMove;
    private bool _paused;
    private Vector2 _startPos;
    private Rigidbody2D _playerBody;
    private Collider2D _endColliderRef;
    private Vector2 m_impulse;
    private float _jumpStartHeight;
    private bool _jumpMotion;
    private float _lastJumpRequest;

    private bool _gliding;
    public bool Gliding
    {
        get { return _gliding; }
    }

    private bool _depleting;
    public bool Depleting
    {
        get { return _depleting; }
    }

    public bool Jumping
    {
        get { return m_jumping || _jumpMotion; }
    }

    private Collider2D _lastCollider;
    private Level _levelData;
    
	// Use this for initialization
	void Start()
    {
        _levelData = FindObjectOfType<Level>();
        _rendererRef = transform.Find("View").GetComponent<SpriteRenderer>();
        _glidingRef = transform.Find("GlidingView").GetComponent<SpriteRenderer>();
        _canMove = false;
        _startPos = new Vector2(transform.position.x, transform.position.y);
        _colliderRef = GetComponent<CapsuleCollider2D>();
        _playerBody = GetComponent<Rigidbody2D>();

        _endColliderRef = GameObject.FindGameObjectWithTag("Finish").GetComponent<Collider2D>();

        CalculateJumpVars();
        DetectGround();

        m_jumping = m_wasJumping = false;
        m_jumpHeight = transform.position.y;
        _jumpMotion = false;

        m_falling = m_wasFalling = !m_grounded;

        m_facingRight = true;
        m_jumpCount = 0;

        Collected = 0;
        _energyLeft = m_maxEnergy;
        _currentDepletionRate = m_depletionRate;
        _depleting = false;

        _paused = false;

        _boosting = false;
        _impulseLocked = false;
        _elapsedBoost = -1.0f;
        _elapsedBoostCharged = -1.0f;
        _remainingBoostCooldown = -1.0f;
        SetGliding(true);
    }

    void SetGliding(bool value)
    {
        _gliding = value;
        _glidingRef.enabled = value;
    }

    // Update is called once per frame
    void Update()
    {
        if (_paused) return;

        if (_depleting)
        {
            UpdateDepleting();
        }

        m_impulse = Vector2.zero;
        m_impulse.x = _canMove ? Input.GetAxis("Horizontal") : 0f;

        m_wasFalling = m_falling;

        DetectCeiling();

        // Lateral checks:
        // Constant speed, don't do anything special
       
        DetectWalls();

        UpdateBoostTimers();
    }


    void StartBoost()
    {
        if (CanUseBoost)
        {
            _boosting = true;
            _elapsedBoost = 0.0f;
            _elapsedBoostCharged = -1.0f;
            m_jumpCount++;
            Collected -= boostCost;
            ApplyBoost();
        }
        else
        {
            _boosting = false;
            _elapsedBoost = -1.0f;
            _elapsedBoostCharged = -1.0f;
        }
        _rendererRef.color = Color.white;
        _impulseLocked = false;
        
    }

    void ApplyBoost()
    {
        m_velocity.y = 2 * m_jumpTermVelocity;
    }

    void UpdateBoostTimers()
    {
        if (_boosting)
        {
            _elapsedBoost += Time.deltaTime;
            if (_elapsedBoost >= boostDuration)
            {
                _boosting = false;
                _remainingBoostCooldown = boostCooldown;
            }
        }
        else if (_remainingBoostCooldown > 0)
        {
            _remainingBoostCooldown -= Time.deltaTime;
        }
    }

    private bool DetectCeiling()
    {
        if (m_velocity.y > 0.0f)
        {
            float distance = _colliderRef.size.y * 0.5f + m_velocity.y * Time.deltaTime;
            bool foundCeil = false;
            Vector2 origin = (Vector2)transform.position + _colliderRef.offset;
            origin.x -= _colliderRef.size.x * 0.5f;
            float delta = _colliderRef.size.x / (m_vertRaysCount - 1);
            RaycastHit2D[] upRays = new RaycastHit2D[m_vertRaysCount];

            int lastIdx = -1;
            float minDistance = Mathf.Infinity;
            for (int i = 0; i < m_vertRaysCount; ++i)
            {
                upRays[i] = Physics2D.Raycast(origin, Vector2.up, distance, RaycastLayers.upRay);
                if (upRays[i].collider != null)
                {
                    foundCeil = true;
                    if (upRays[i].distance < minDistance)
                    {
                        minDistance = upRays[i].distance;
                        lastIdx = i;
                    }

                }
                origin.x += delta;
            }

            if (foundCeil)
            {
                //transform.Translate(Vector2.up * (upRays[lastIdx].distance - _colliderRef.size.y));
                m_velocity.y = 0.0f;
                return true;
            }
        }
        return false;
    }
    private bool DetectWalls()
    {
        float absVelocity = Mathf.Abs(m_velocity.x);
        int signVelocity = m_velocity.x > 0 ? 1 : (m_velocity.x < 0) ? -1 : 0;
        float collH = _colliderRef.size.y;
        float collW = _colliderRef.size.x;
        float collHW = collW * 0.5f;
        float collHH = collH * 0.5f;

        RaycastHit2D[] raycasts = new RaycastHit2D[m_horzRaysCount];

        if (absVelocity > m_velocityThreshold)
        {
            Vector2 origin = (Vector2)transform.position + _colliderRef.offset;
            Vector2 direction = m_velocity.x > 0 ? Vector2.right : Vector2.left;

            const float heightScale = 0.8f;
            origin.y += collHH; // the collider is already offset in the y axis

            float rayDelta = (collH * heightScale) / (m_horzRaysCount - 1);
            float rayDistance = (collHW + absVelocity * Time.deltaTime) * 1.1f;
            Vector3 pos = transform.position;
            float lastFraction = 0.0f;
            int numHits = 0;
            for (int i = 0; i < m_horzRaysCount; ++i)
            {
                //Debug.DrawRay(origin, direction * rayDistance, Color.cyan, 0.4f);
                raycasts[i] = Physics2D.Raycast(origin, direction, rayDistance, RaycastLayers.wallCollisions);
                if (raycasts[i].collider != null)
                {
                    if (lastFraction > 0.0f)
                    {
                        float angle = Vector2.Angle(raycasts[i].point - raycasts[i - 1].point, Vector2.right);
                        if (Mathf.Abs(angle - 90.0f) < m_angleLeeway)
                        {
                            //transform.Translate(direction * (raycasts[i].distance - collHW));
                            //transform.position = pos;

                            bool newFacingRight = m_velocity.x > 0.0f || (m_facingRight && Mathf.Abs(m_velocity.x) < m_velocityThreshold);
                            if ((newFacingRight && !m_facingRight) || (!newFacingRight && m_facingRight))
                            {
                                Vector3 scale = transform.localScale;
                                m_facingRight = newFacingRight;
                                scale.x = Mathf.Abs(scale.x) * (m_facingRight ? 1 : -1);
                                transform.localScale = scale;
                            }

                            m_velocity.x = 0.0f;
                            return true;
                        }
                    }
                    numHits++;
                    lastFraction = raycasts[i].fraction;
                }
                origin.y -= rayDelta;
            }
        }
        return false;
    }

    private void DetectGround()
    {
        if (!m_grounded && !m_falling) return;

        bool foundGround = false;
        Vector2 origin = (Vector2)transform.position + _colliderRef.offset;
        origin.x -= _colliderRef.size.x * 0.5f;
        RaycastHit2D[] raycasts = new RaycastHit2D[m_vertRaysCount];
        float minDistance = Mathf.Infinity;
        int idx = -1;
        Collider2D bestCollider = null;

        float rayDelta = _colliderRef.size.x / (m_vertRaysCount - 1);
        float rayDistance = _colliderRef.size.y * 0.55f;
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = true;
        filter.layerMask = RaycastLayers.groundCollisions;
        RaycastHit2D[] results = new RaycastHit2D[1];             

        for (int i = 0; i < m_vertRaysCount; ++i)
        {
            //-Debug.DrawRay(origin, Vector2.down * rayDistance, Color.green, 0.4f);
            int numResults = Physics2D.Raycast(origin, Vector2.down, filter, results, rayDistance);
            if (numResults > 0)
            {
                raycasts[i] = results[0];
                if (raycasts[i].collider != null)
                {
                    foundGround = true;
                    if (raycasts[i].distance < minDistance)
                    {
                        minDistance = raycasts[i].distance;
                        idx = i;
                        bestCollider = raycasts[i].collider;
                    }
                }
            }
            origin.x += rayDelta;
        }
        if (foundGround)
        {
            //transform.Translate(Vector2.down * (minDistance - _colliderRef.size.y * 0.5f));

            m_velocity.y = 0.0f;
            m_jumpCount = 0;
            m_grounded = true;
            m_falling = false;
            _jumpMotion = false;
            if (_gliding && bestCollider == _endColliderRef)
            {
                SetGliding(false);
                if (GlideFinished != null)
                {
                    GlideFinished();
                }
            }

            if (!_gliding)
            {
                if (bestCollider.CompareTag("Respawn"))
                { 
                    FinaliseDepleting(DepleteEnd.ArrivedToGoal);
                }
                else
                {
                    if (bestCollider != _endColliderRef)
                    {
                        PlatformStepped(bestCollider);
                        if (_lastCollider == _endColliderRef && FirstPlatformAfterGlide != null)
                        {
                            FirstPlatformAfterGlide(bestCollider);
                        }
                    }
                }                
            }
            _lastCollider = bestCollider;
        }
        else
        {
            m_grounded = false;
        }

    }

    void LateUpdate()
    {
        if (m_velocity.magnitude > m_velocityFlipThreshold * SQRT_2)
        {
            bool newFacingRight = m_velocity.x > 0.0f || (m_facingRight && Mathf.Abs(m_velocity.x) < m_velocityFlipThreshold);
            if ((newFacingRight && !m_facingRight) || (!newFacingRight && m_facingRight))
            {
                Vector3 scale = transform.localScale;
                m_facingRight = newFacingRight;
                scale.x = Mathf.Abs(scale.x) * (m_facingRight ? 1 : -1);
                transform.localScale = scale;
            }
        }
    }

    void FixedUpdate()
    {
        if (_paused) return;

        DetectGround();

        // Y-movement
        if (!m_grounded)
        {
            if (_gliding)
            {
                m_velocity.y = -m_maxGlideSpeed;
            }
            else
            {
                m_velocity.y -= m_gravity * Time.deltaTime;
                if (m_velocity.y < -m_maxFallSpeed)
                {
                    m_velocity.y = -m_maxFallSpeed;
                }
            }
        }

        if (_boosting)
        {
            ApplyBoost();
        }
        else
        {
            bool jumpReleased = Input.GetButtonUp("Jump") && _canMove && !_gliding;
            bool jumpJustPressed = Input.GetButtonDown("Jump") && _canMove && !_gliding;
            bool jumpPressed = Input.GetButton("Jump") && _canMove && !_gliding;

            if (jumpReleased)
            {
                if (m_velocity.y > m_jumpTermVelocity)
                {
                    m_velocity.y = m_jumpTermVelocity;
                }
            }
            else
            {
                m_wasJumping = m_jumping;
                m_jumping = jumpPressed;
                if (m_jumping)
                {
                    _lastJumpRequest = Time.time;
                    bool multiJump = m_jumping && !m_wasJumping && m_jumpCount < m_maxJumps;
                    bool groundJump = ((m_jumping && !m_wasJumping) || (Time.time - _lastJumpRequest < m_jumpInterval)) && m_grounded;

                    if (!_boosting && (groundJump || multiJump))
                    {
                        BeginJumping();
                    }
                }
                else
                {
                    bool boostReleased = Input.GetButtonUp("Fire1") && _canMove && !_gliding;
                    bool boostJustPressed = Input.GetButtonDown("Fire1") && _canMove && !_gliding;
                    bool boostPressed = Input.GetButton("Fire1") && _canMove && !_gliding;
                    if (boostReleased)
                    {
                        if (_elapsedBoostCharged > requiredBoostChargeTime)
                        {
                            StartBoost();
                        }
                        else
                        {
                            _elapsedBoostCharged = -1.0f;
                        }
                        _impulseLocked = false;
                    }
                    else if (boostPressed)
                    {
                        if (CanUseBoost && _elapsedBoostCharged < 0f)
                        {
                            if (Mathf.Abs(m_impulse.x) < 0.1f)
                            {
                                //_impulseLocked = true;
                            }
                            _elapsedBoostCharged = 0.0f;
                        }
                        else if (_elapsedBoostCharged >= 0)_elapsedBoostCharged += Time.deltaTime;
                        if (_elapsedBoostCharged >= requiredBoostChargeTime)
                        {
                            _rendererRef.color = Color.yellow;
                        }
                    }
                }
            }
        }
        m_falling = m_velocity.y < 0.0f;
        _jumpMotion = _jumpMotion && (m_velocity.y > 0f || (m_falling && transform.position.y > _jumpStartHeight));

        float oldVelocity = m_velocity.x;
        float maxSpeed = _gliding ? m_maxSpeedGliding : m_maxSpeed;
        if (Mathf.Approximately(m_acceleration, 0.0f))
        {
            m_velocity.x = m_impulse.x * maxSpeed;
        }
        else // Accelerated movement. Factor drag, too.
        {
            bool directionChange = Mathf.Sign(m_velocity.x) != Mathf.Sign(m_impulse.x) && !Mathf.Approximately(m_impulse.x, 0.0f);
            if (directionChange /*|| inputVector.Equals(Vector2.zero) */)
            {
                m_velocity.x = 0.0f;
            }

            if (Mathf.Abs(m_impulse.x) < 0.1f)
            {
                int oldSign = m_velocity.x > 0 ? 1 : (m_velocity.x < 0) ? -1 : 0;
                if (Mathf.Abs(m_velocity.x) > m_velocityThreshold)
                {
                    int dragDirection = ((m_velocity.x > 0.0f) ? -1 : (m_velocity.x < 0.0f) ? 1 : 0);
                    float dragAmount = m_drag * Time.deltaTime;
                    Debug.Log("Applying drag: velocity: " + m_velocity.x + ", DRAG: " + dragAmount + ", impulse: " + m_impulse.x);
                    m_velocity.x += dragAmount * dragDirection;
                }

                int newSign = ((m_velocity.x > 0.0f) ? -1 : (m_velocity.x < 0.0f) ? 1 : 0);
                if (Mathf.Abs(m_velocity.x) < m_velocityThreshold || newSign != oldSign)
                {
                    m_velocity.x = 0.0f;
                }
            }
            else
            {
                m_velocity.x += m_acceleration * m_impulse.x * Time.deltaTime;
                if (Mathf.Abs(m_velocity.x) >= maxSpeed)
                {
                    // Unity Math's sign returns 1 if 0, which we don't want here :/
                    m_velocity.x = maxSpeed * ((m_velocity.x > 0.0f) ? 1 : (m_velocity.x < 0.0f) ? -1 : 0);
                }
            }
        }

        if (_impulseLocked)
        {
            m_velocity.x = 0;
        }
        Vector2 pos = _playerBody.position + m_velocity * Time.deltaTime;
        _playerBody.position = pos;

   }

    private void CalculateJumpVars()
    {
        m_gravity = 2 * m_maxHeight / (m_maxJumpTime * m_maxJumpTime);
        m_initialJumpSpeed = Mathf.Sqrt(2 * m_gravity * m_maxHeight);

        m_jumpTermVelocity = Mathf.Sqrt(m_initialJumpSpeed * m_initialJumpSpeed - 2 * m_gravity * (m_maxHeight - m_minHeight));
    }

    public void Pause(bool paused)
    {
        _paused = paused;
    }

    public void EnableMovement(bool enabled)
    {
        _canMove = enabled;
    }

    public void Reset()
    {
        _playerBody.position = _startPos;
        m_velocity = Vector2.zero;
        _energyLeft = m_maxEnergy;
        _currentDepletionRate = m_depletionRate;
        Collected = 0;
        _depleting = false;
        _canMove = true;
        SetGliding(true);
        _boosting = false;
        _impulseLocked = false;
        _elapsedBoost = -1.0f;
        _elapsedBoostCharged = -1.0f;
        _remainingBoostCooldown = -1.0f;
        _rendererRef.color = Color.white;
    }

    public void OnCollected(Collectable item)
    {
        Collected += item.amount;
        if (_depleting)
        {
            //_currentDepletionRate += m_depletionCollectionIncrease;
        }
    }

    public void BeginJumping()
    {
        _jumpStartHeight = transform.position.y;
        _jumpMotion = true;
        m_velocity.y = m_initialJumpSpeed;
        m_jumpCount++;
        if (!_depleting)
        {
            BeginDepleting();
        }
    }

    public void BeginDepleting()
    {
        _currentDepletionRate = m_depletionRate; //+ Collected * m_depletionCollectionIncrease;
        _depleting = true;
        if (DepleteBegun != null)
        {
            DepleteBegun();
        }
    }

    public void UpdateDepleting()
    {
    }

    public void FinaliseDepleting(DepleteEnd type)
    {
        _depleting = false;
        // do stuff
        EnableMovement(false);
        DepletionFinished(type);
        GameFinished(Collected >= _levelData.required);
    }

    public bool CanUseBoost
    {
        get
        {
            return !_gliding && _canMove && !_boosting && Collected >= boostCost && (_remainingBoostCooldown < 0f);
        }
    }

}
