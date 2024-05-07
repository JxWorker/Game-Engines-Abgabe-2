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

    public Enemy(int pHp, int pDmg, float pMS, string pTyp)
    {
        healthpoint = pHp;
        damage = pDmg;
        movementSpeed = pMS;
        typ = pTyp;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        //player = player.GetComponent<Player>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;
        PathFinding();
        if (canAttack)
        {
            Attack();
        }
        Die();
    }
    
    public void Attack()
    {
        if (attackCooldown <= 0)
        {
            //player.hp = player.hp - damage;
            attackCooldown = attackSpeed;
        }
        else
        {
            attackCooldown -= Time.deltaTime;
        }
    }
    
    public void PathFinding()
    {
        agent.SetDestination(player.transform.position);
    }
    
    public void Die()
    {
        if (healthpoint <= 0)
        {
            Destroy(this.gameObject);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player) canAttack = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player) canAttack = false;
    }
}