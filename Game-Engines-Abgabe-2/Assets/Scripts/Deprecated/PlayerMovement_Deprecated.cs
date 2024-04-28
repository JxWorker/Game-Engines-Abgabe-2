using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement_Depreaceted : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    [SerializeField] private float speed = 7;

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        move = Input.GetKey(KeyCode.LeftShift) ?  speed * 2 * move : speed * move;
        
        _controller.Move(Time.deltaTime * move);
    }
}
