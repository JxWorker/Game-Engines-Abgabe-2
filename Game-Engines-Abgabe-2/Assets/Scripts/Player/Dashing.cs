using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerCamera;
    private Rigidbody _rigidbody;
    private PlayerMovement _playerMovement;

    [Header("Dashing")] 
    [SerializeField] private float dashForce;
    [SerializeField] private float dashUpwardForce;
    [SerializeField] private float dashDuration;
    private Vector3 _delayedForceToApply;

    [Header("Cooldown")] 
    [SerializeField] private float dashCooldown;
    private float dashCooldownTimer;

    [Header("Input")] 
    [SerializeField] private KeyCode dashKeyCode = KeyCode.E;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerMovement = GetComponent<PlayerMovement>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(dashKeyCode))
        {
            Dash();
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void Dash()
    {
        if (dashCooldownTimer > 0)
        {
            return;
        }
        else
        {
            dashCooldownTimer = dashCooldown;
        }
        
        _playerMovement.dashing = true;
        
        _delayedForceToApply = orientation.forward * dashForce + orientation.up * dashUpwardForce;
        
        Invoke(nameof(DelayedDashForce), 0.025f);
        
        Invoke(nameof(ResetDash), dashDuration);
    }

    private void DelayedDashForce(Vector3 delayedForceToApply)
    {
        _rigidbody.AddForce(delayedForceToApply, ForceMode.Impulse);
    }
    
    private void ResetDash()
    {
        _playerMovement.dashing = false;
    }
}