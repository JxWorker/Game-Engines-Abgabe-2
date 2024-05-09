using UnityEngine;
using UnityEngine.AI;

abstract public class Enemy : MonoBehaviour
{
    public int healthpoint;
    public int damage;
    public float attackSpeed;
    public float attackCooldown;
    public string typ;
    public bool canAttack;
    public GameObject player;
    public NavMeshAgent agent;
    public GameObject healthBar;
    public Animator _animator;
    public float maxDetectionDistance;
    public LayerMask playerMask;
    private int _lastHealth;
    private bool _isDead = false;

    public Enemy(int pHp, int pDmg, string pTyp)
    {
        healthpoint = pHp;
        _lastHealth = pHp;
        damage = pDmg;
        typ = pTyp;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        
        healthBar.transform.localScale = new Vector3(healthpoint / 100f, healthBar.transform.localScale.y,
            healthBar.transform.localScale.z);
    }

    void Update()
    { 
        if (_lastHealth != healthpoint)
        {
            AdjustHealthBar();
        }

        _lastHealth = healthpoint;

        if (healthpoint <= 0)
        {
            Die();
        }
        
        if (DetectedPlayer() && !_isDead)
        {
            if (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime;
            }

            PathFinding();

            if (canAttack)
            {
                Attack();
            }
        }
        else
        {
            _animator.SetBool("IsWalking", false);
        }
    }

    private void Attack()
    {
        if (attackCooldown <= 0)
        {
            _animator.SetTrigger("Attack");
            player.GetComponent<PlayerCombat>().healthpoints -= damage;
            attackCooldown = attackSpeed;
        }
        else
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    private void PathFinding()
    {
        _animator.SetBool("IsWalking", true);
        agent.SetDestination(player.transform.position);
    }

    private void Die()
    {
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _isDead = true;
        _animator.SetTrigger("Death");
        Destroy(gameObject, 2f);
    }

    private void AdjustHealthBar()
    {
        _animator.SetTrigger("Damage");
        healthBar.transform.localScale = new Vector3(healthpoint / 100f, healthBar.transform.localScale.y,
            healthBar.transform.localScale.z);
    }

    private bool DetectedPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxDetectionDistance, playerMask);

        if (colliders.Length == 1)
        {
            return true;
        }

        return false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canAttack = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canAttack = false;
        }
    }
}