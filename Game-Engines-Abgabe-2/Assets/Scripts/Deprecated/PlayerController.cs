using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private float health;
    [SerializeField] private GameObject player;
    // [SerializeField] private Camera camera;

    [SerializeField] private float XSensitivity;
    [SerializeField] private float YSensitivity;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");

        float yRotation = Input.GetAxis("Mouse X") * XSensitivity;
        float xRotation = Input.GetAxis("Mouse Y") * YSensitivity;

        Quaternion CharacterTargetRotation = transform.localRotation;
        CharacterTargetRotation *= Quaternion.Euler(0f, yRotation, 0f);
        
        // Quaternion CameraTargetRotation = camera.transform.localRotation;
        // CameraTargetRotation *= Quaternion.Euler(-xRotation, 0f, 0f);

        transform.localRotation =
            Quaternion.Slerp(transform.localRotation, CharacterTargetRotation, 5f * Time.deltaTime);
        
        // camera.transform.localRotation =
            // Quaternion.Slerp(camera.transform.localRotation, CameraTargetRotation, 5f * Time.deltaTime);
        
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.position += speed * 2 * Time.deltaTime * new Vector3(xDirection, 0, zDirection);
        }
        else
        {
            transform.position += speed * Time.deltaTime * new Vector3(xDirection, 0, zDirection);
        }
    }
}