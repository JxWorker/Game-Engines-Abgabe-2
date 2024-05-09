using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement _playerMovement;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    [FormerlySerializedAs("lr")] public LineRenderer lineRenderer;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 _grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float _grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    private bool _grappling;

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            StartGrapple();
        }

        if (_grapplingCdTimer > 0)
        {
            _grapplingCdTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (_grappling)
        {
            lineRenderer.SetPosition(0, gunTip.position);
        }
    }

    private void StartGrapple()
    {
        if (_grapplingCdTimer > 0)
        {
            return;
        }

        _grappling = true;

        _playerMovement.freeze = true;

        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            _grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            _grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(1, _grapplePoint);
    }

    private void ExecuteGrapple()
    {
        _playerMovement.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = _grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0)
        {
            highestPointOnArc = overshootYAxis;
        }

        _playerMovement.JumpToPosition(_grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        _playerMovement.freeze = false;

        _grappling = false;

        _grapplingCdTimer = grapplingCd;

        lineRenderer.enabled = false;
    }
}
