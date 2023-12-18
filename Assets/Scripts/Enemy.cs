using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public float health = 100;
    public float damage = 10;

    NavMeshAgent agent;
    bool attacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (GameManager.instance.player != null) {
            agent.SetDestination(GameManager.instance.player.transform.position);

            if (agent.remainingDistance <= agent.stoppingDistance) {
                StartCoroutine(Attack());
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) {
            Destroy(gameObject);
        }
    }

    public void DealDamage(Player player)
    {
        player.TakeDamage(damage);
    }

    IEnumerator Attack()
    {
        if (attacking) yield break;

        attacking = true;

        // do something animation based

        DealDamage(GameManager.instance.player);

        attacking = false;

        yield break;
    }
}
