using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCombat : MonoBehaviour
{
    public int healthpoints;
    [Header("Laser Gun")]
    [SerializeField] private int gunDamage;
    [SerializeField] private float maxGunDistance;
    [SerializeField] private LayerMask enemy;
    [SerializeField] private LineRenderer lineRenderer;
    [Header("Bomb")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float throwForce;
    [Header("KeyCodes")]
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode throwBomb = KeyCode.F;
    [Header("References")]
    [SerializeField] private Transform gunTip;
    [SerializeField] private Transform cam;
    
    private Vector3 _gunPoint;
    
    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            Attack();
        }

        if (Input.GetKeyDown(throwBomb))
        {
            GameObject bomb = Instantiate(bombPrefab, gunTip.position, Quaternion.identity);
            bomb.GetComponent<Rigidbody>().AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
        }
        
        if (healthpoints <= 0)
        {
            Die();
        }
    }
    
    private void LateUpdate()
    {
        lineRenderer.SetPosition(0, gunTip.position);
    }

    private void Attack()
    {
        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxGunDistance, enemy))
        {
            _gunPoint = hit.point;

            BasicEnemy enemey = hit.transform.GetComponent<BasicEnemy>();
            enemey.healthpoint -= gunDamage;
        }
        else
        {
            _gunPoint = cam.position + cam.forward * maxGunDistance;
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(1, _gunPoint);
        
        Invoke(nameof(RemoveLaser), 0.2f);
    }

    private void RemoveLaser()
    {
        lineRenderer.enabled = false;
    }
    
    private void Die()
    {
        SceneManager.LoadScene("Scenes/MenuScenes/MainMenu");
    }
}
