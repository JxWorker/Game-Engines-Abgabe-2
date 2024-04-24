using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private float health;
    [SerializeField] private GameObject player;
    [SerializeField] private Camera camera;

    [SerializeField] private float sensitivity;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");

        float xRotation = Input.GetAxis("Mouse X");
        float yRotation = Input.GetAxis("Mouse Y");

        // camera rotates around the player
        transform.RotateAround(player.transform.position, -Vector3.up, xRotation * sensitivity);
        transform.RotateAround(Vector3.zero, transform.right, yRotation * sensitivity);

        // camera.transform.Rotate( xRotation * sensitivity * -camera.transform.up); //instead if you dont want the camera to rotate around the player
        // camera.transform.Rotate(yRotation * sensitivity * camera.transform.right); //if you don't want the camera to rotate around the player
        
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