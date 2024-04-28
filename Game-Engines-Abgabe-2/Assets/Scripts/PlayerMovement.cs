using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] 
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float groundDrag;
    private float _moveSpeed;
    
    [Header("Jump")] 
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool _readyToJump = true;

    [Header("Ground Check")] 
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask isGround;
    private bool _grounded;

    [Header("Orientation")] 
    [SerializeField] private Transform orientation;
    private float _horizontalInput;
    private float _verticalInput;
    private Vector3 _moveDirection;

    [Header("Keybinds")] 
    [SerializeField] private KeyCode jumbKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    
    private Rigidbody _rigidbody;

    private MovementState _state;
    private enum MovementState
    {
        walking,
        sprinting,
        air
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

        _rigidbody.drag = _grounded ? groundDrag : 0;
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
        if(_grounded && Input.GetKey(sprintKey))
        {
            _state = MovementState.sprinting;
            _moveSpeed = sprintSpeed;
        }
        else if (_grounded)
        {
            _state = MovementState.walking;
            _moveSpeed = walkSpeed;
        }
        else
        {
            _state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

        if (_grounded)
        {
            _rigidbody.AddForce(_moveSpeed * 10f * _moveDirection.normalized, ForceMode.Force);
        }
        else if (!_grounded)
        {
            _rigidbody.AddForce(_moveSpeed * 10f * airMultiplier * _moveDirection.normalized, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

        if (flatVelocity.magnitude > _moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * _moveSpeed;
            _rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
        }
    }

    private void Jump()
    {
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

        _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void RestJump()
    {
        _readyToJump = true;
    }
}