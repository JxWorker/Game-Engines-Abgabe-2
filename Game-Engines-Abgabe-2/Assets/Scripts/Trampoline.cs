using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 20;
    [SerializeField] private GameObject player;
    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.GetComponent<Rigidbody>().AddForce(jumpHeight * Vector3.up, ForceMode.Impulse);
        }
    }
}
