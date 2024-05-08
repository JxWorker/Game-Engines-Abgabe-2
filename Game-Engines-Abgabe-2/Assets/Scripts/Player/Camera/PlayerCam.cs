using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEditor.Overlays;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private float xSensitivity;
    [SerializeField] private float ySensitivity;

    [SerializeField] private Transform orientation;

    private float _xRotation;
    private float _yRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float xMouse = Input.GetAxisRaw("Mouse X") * Time.deltaTime * xSensitivity;
        float yMouse = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * ySensitivity;

        _yRotation += xMouse;

        _xRotation -= yMouse;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
    }
}
