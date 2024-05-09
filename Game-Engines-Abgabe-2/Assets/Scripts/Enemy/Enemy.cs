using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

abstract public class Enemy : MonoBehaviour
{
    public int healthpoint;
    public int damage;
    public float movementSpeed;
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

    public Enemy(int pHp, int pDmg, float pMS, string pTyp)
    {
        healthpoint = pHp;
        _lastHealth = pHp;
        damage = pDmg;
        movementSpeed = pMS;
        typ = pTyp;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        // _animator = transform.GetComponent<Animator>();
    }

    void Update()
    {
        // if (!DetectedPlayer())
        // {
        //     _animator.SetBool("IsWalking", false);
        //     _animator.SetBool("IsAttacking", false);
        //     _animator.SetBool("IsTakingDamage", false);
        //     // _animator.SetBool("HasZeroHealthpoints", false);
        // }

        if (_lastHealth != healthpoint)
        {
            AdjustHealthBar();
            // _animator.SetBool("IsTakingDamage", false);
        }

        _lastHealth = healthpoint;

        if (healthpoint <= 0)
        {
            Die();
        }
        
        if (DetectedPlayer())
        {
            if (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime;
            }

            PathFinding();

            if (canAttack)
            {
                Attack();
                _animator.SetBool("IsAttacking", false);
            }
        }
        else
        {
            _animator.SetBool("IsWalking", false);
        }
    }

    public void Attack()
    {
        if (attackCooldown <= 0)
        {
            _animator.SetBool("IsAttacking", true);

            player.GetComponent<PlayerCombat>().healthpoints -= damage;
            attackCooldown = attackSpeed;
        }
        else
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    public void PathFinding()
    {
        _animator.SetBool("IsWalking", true);
        agent.SetDestination(player.transform.position);
    }

    public void Die()
    {
        _animator.SetBool("HasZeroHealthpoints", true);
        Destroy(gameObject, 5f);
    }

    public void AdjustHealthBar()
    {
        healthBar.transform.localScale = new Vector3(healthpoint / 100f, healthBar.transform.localScale.y,
            healthBar.transform.localScale.z);
        // _animator.SetBool("IsTakingDamage", true);
        _animator.SetTrigger("Damage");
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
        if (other.gameObject.CompareTag("Player")) canAttack = true;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Player")) canAttack = false;
    }
}