using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
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
    private bool _grounded;

    [Header("Slope Handling")] [SerializeField] private float maxSlopeAngle;
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    [Header("Orientation")] [SerializeField] private Transform orientation;
    private float _horizontalInput;
    private float _verticalInput;
    private Vector3 _moveDirection;

    [Header("Keybinds")] [SerializeField] private KeyCode jumbKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    private Rigidbody _rigidbody;

    private MovementState _state;

    private float _desiredMoveSpeed;
    private float _lastDesiredMoveSpeed;
    private MovementState _lastState;
    private bool _keepMomentum;
    private float _speedChangeFactor;

    private enum MovementState
    {
        Walking,
        Sprinting,
        Dashing,
        Air
    }

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        _grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, isGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (_state == MovementState.Walking || _state == MovementState.Sprinting)
        {
            _rigidbody.drag = groundDrag;
        }
        else
        {
            _rigidbody.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumbKey) && _readyToJump && _grounded)
        {
            _readyToJump = false;

            Jump();

            Invoke(nameof(RestJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        if (dashing)
        {
            _state = MovementState.Dashing;
            _desiredMoveSpeed = dashSpeed;
            _speedChangeFactor = dashSpeedChangeFactor;
        }
        else if (_grounded && Input.GetKey(sprintKey))
        {
            _state = MovementState.Sprinting;
            _desiredMoveSpeed = sprintSpeed;
        }
        else if (_grounded)
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
        _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

        if (OnSlope() && !_exitingSlope)
        {
            _rigidbody.AddForce(_moveSpeed * 20f * GetSlopeMoveDirection(), ForceMode.Force);

            if (_rigidbody.velocity.y > 0)
            {
                _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (_grounded)
        {
            _rigidbody.AddForce(_moveSpeed * 10f * _moveDirection.normalized, ForceMode.Force);
        }
        else if (!_grounded)
        {
            _rigidbody.AddForce(_moveSpeed * 10f * airMultiplier * _moveDirection.normalized, ForceMode.Force);
        }

        _rigidbody.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
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
}