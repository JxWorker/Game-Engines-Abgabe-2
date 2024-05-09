using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float climbSpeed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashSpeedChangeFactor;
    [SerializeField] private float groundDrag;
    public bool dashing;
    private float _moveSpeed;

    [Header("Jump")] [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool _readyToJump = true;

    [Header("Ground Check")] [SerializeField]
    private float playerHeight;

    [SerializeField] private LayerMask isGround;
    public bool grounded;

    [Header("Slope Handling")] [SerializeField] private float maxSlopeAngle;
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    [Header("Orientation")] [SerializeField] private Transform orientation;
    private float _horizontalInput;
    private float _verticalInput;
    private Vector3 _moveDirection;

    [Header("Keybinds")] [SerializeField] private KeyCode jumbKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("References")] [SerializeField]
    private Climbing climbingScript;
    
    private Rigidbody _rigidbody;

    private MovementState _state;

    private float _desiredMoveSpeed;
    private float _lastDesiredMoveSpeed;
    private MovementState _lastState;
    private bool _keepMomentum;
    private float _speedChangeFactor;
    public bool freeze;
    public bool activeGrapple;
    private Vector3 velocityToSet;
    private bool enableMovementOnNextTouch;
    public bool climbing;
    public bool unlimited;
    public bool restricted;
    
    private enum MovementState
    {
        Walking,
        Sprinting,
        Dashing,
        Air,
        Freeze,
        Grappling,
        Climbing,
        Unlimited
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
    }
    
    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, isGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if ((_state == MovementState.Walking || _state == MovementState.Sprinting) && !activeGrapple)
        {
            _rigidbody.drag = groundDrag;
        }
        else
        {
            _rigidbody.drag = 0;
        }
        
        TextStuff();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumbKey) && _readyToJump && grounded)
        {
            _readyToJump = false;

            Jump();

            Invoke(nameof(RestJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        if (climbing)
        {
            _state = MovementState.Climbing;
            _desiredMoveSpeed = climbSpeed;
        }
        else if (freeze)
        {
            _state = MovementState.Freeze;
            _desiredMoveSpeed = 0;
            _rigidbody.velocity = Vector3.zero;
        }
        else if (activeGrapple)
        {
            _state = MovementState.Grappling;
            _desiredMoveSpeed = sprintSpeed;
        }
        else if (unlimited)
        {
            _state = MovementState.Unlimited;
            _desiredMoveSpeed = 999f;
            return;
        }
        else if (dashing)
        {
            _state = MovementState.Dashing;
            _desiredMoveSpeed = dashSpeed;
            _speedChangeFactor = dashSpeedChangeFactor;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            _state = MovementState.Sprinting;
            _desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            _state = MovementState.Walking;
            _desiredMoveSpeed = walkSpeed;
        }
        else
        {
            _state = MovementState.Air;

            if (_desiredMoveSpeed < sprintSpeed)
            {
                _desiredMoveSpeed = walkSpeed;
            }
            else
            {
                _desiredMoveSpeed = sprintSpeed;
            }
        }

        bool desiredMoveSpeedHasChanged = _desiredMoveSpeed != _lastDesiredMoveSpeed;

        if (_lastState == MovementState.Dashing)
        {
            _keepMomentum = true;
        }

        if (desiredMoveSpeedHasChanged)
        {
            if (_keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                _moveSpeed = _desiredMoveSpeed;
            }
        }
        
        _lastDesiredMoveSpeed = _desiredMoveSpeed;
        _lastState = _state;
    }

    private void MovePlayer()
    {
        if (restricted)
        {
            return;
        }
        
        if (climbingScript.exitingWall)
        {
            return;
        }
        
        if (activeGrapple)
        {
            return;
        }
        
        _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

        if (OnSlope() && !_exitingSlope)
        {
            _rigidbody.AddForce(_moveSpeed * 20f * GetSlopeMoveDirection(), ForceMode.Force);

            if (_rigidbody.velocity.y > 0)
            {
                _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (grounded)
        {
            _rigidbody.AddForce(_moveSpeed * 10f * _moveDirection.normalized, ForceMode.Force);
        }
        else if (!grounded)
        {
            _rigidbody.AddForce(_moveSpeed * 10f * airMultiplier * _moveDirection.normalized, ForceMode.Force);
        }

        _rigidbody.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrapple)
        {
            return;
        }
        
        if (OnSlope() && !_exitingSlope)
        {
            if (_rigidbody.velocity.magnitude > _moveSpeed)
            {
                _rigidbody.velocity = _rigidbody.velocity.normalized * _moveSpeed;
            }
        }
        else
        {
            Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

            if (flatVelocity.magnitude > _moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * _moveSpeed;
                _rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
            }
        }
    }

    #region Jump
    private void Jump()
    {
        _exitingSlope = true;
        
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

        _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void RestJump()
    {
        _readyToJump = true;
        
        _exitingSlope = false;
    }
    #endregion

    #region SlopeMovement
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(_desiredMoveSpeed - _moveSpeed);
        float startValue = _moveSpeed;

        float boostFactor = _speedChangeFactor;

        while (time < difference)
        {
            _moveSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        _moveSpeed = _desiredMoveSpeed;
        _speedChangeFactor = 1f;
        _keepMomentum = false;
    }
    #endregion
    
    #region GrapplingHook
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }
    
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        _rigidbody.velocity = velocityToSet;
    }
    
    public void ResetRestrictions()
    {
        activeGrapple = false;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }
    
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
                                               + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
    #endregion
    
    #region Text & Debugging

    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_mode;
    private void TextStuff()
    {
        Vector3 flatVel = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

        if (OnSlope())
            text_speed.SetText("Speed: " + Round(_rigidbody.velocity.magnitude, 1) + " / " + Round(_moveSpeed, 1));

        else
            text_speed.SetText("Speed: " + Round(flatVel.magnitude, 1) + " / " + Round(_moveSpeed, 1));

        text_mode.SetText(_state.ToString());
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    #endregion
}