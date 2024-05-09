using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float duration;
    public float maxDistance;
    public float explosionForce;
    public int damage;
    public LayerMask interactiveMask;
    
    void Start()
    {
        Invoke(nameof(Explode), duration);
    }

    void Explode()
    {
        // Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        // GetComponent<AudioSource>().Play();

        Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistance, interactiveMask);
        foreach (var collider in colliders)
        {
            // collider.GetComponent<Rigidbody>()
                // .AddExplosionForce(explosionForce, transform.position, maxDistance, 1, ForceMode.Impulse);
            collider.transform.GetComponent<BasicEnemy>().healthpoint -= damage;
        }

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        
        Destroy(gameObject, 2f);
    }
}
