using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public float health = 100;
    public float damage = 10;

    [Space]
    public float destroyBoardsDistance = 5.0f;

    NavMeshAgent agent;

    enum ActingState
    {
        WALKING,
        DESTROYINGWALL,
        ATTACKING,
    }

    ActingState actingState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (GameManager.instance.gameStopped) return;

        switch (actingState) {
            case ActingState.WALKING:
                GetPathToTarget();
                break;

            case ActingState.DESTROYINGWALL:

                break;

            case ActingState.ATTACKING:

                break;
        }

    }

    void GetPathToTarget()
    {
        if (GameManager.instance.player != null) {
            agent.SetDestination(GameManager.instance.player.transform.position);

            if (agent.remainingDistance <= agent.stoppingDistance) {
                StartCoroutine(Attack());
            } else if (GameManager.instance.player.isIndoors) {
                // layermask 7 is the "Boards" layer
                Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, destroyBoardsDistance, 1 << 7);

                for (int i = 0; i < nearbyColliders.Length; i++) {
                    Board b = nearbyColliders[i].GetComponent<Board>();
                    if (b != null) {
                        if (b.remainingBoards > 0) {
                            StartCoroutine(DestroyBoard(b));
                        }
                    }
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        GameManager.instance.AddScore(GameManager.instance.scoreOnBullet);

        if (health <= 0) {
            Death();
        }
    }

    void Death()
    {
        GameManager.instance.AddScore(GameManager.instance.scoreOnKill);

        Destroy(gameObject);
    }

    public void DealDamage(Player player)
    {
        player.TakeDamage(damage);
    }

    IEnumerator Attack()
    {
        if (actingState != ActingState.WALKING) yield break;

        actingState = ActingState.ATTACKING;
        if (!agent.isStopped) agent.isStopped = true;

        // do something animation based

        float timer = 1.0f;
        while ((timer -= Time.deltaTime) > 0) {
            yield return new WaitForEndOfFrame();
        }

        if ((GameManager.instance.player.transform.position - transform.position).sqrMagnitude < agent.stoppingDistance * agent.stoppingDistance) {
            DealDamage(GameManager.instance.player);
        }

        agent.isStopped = false;
        actingState = ActingState.WALKING;

        yield break;
    }

    IEnumerator DestroyBoard(Board board)
    {
        if (actingState != ActingState.WALKING) yield break;

        actingState = ActingState.DESTROYINGWALL;
        if (!agent.isStopped) agent.isStopped = true;

        // do something animation based

        float timer = 5.0f;
        while ((timer -= Time.deltaTime) > 0) {
            yield return new WaitForEndOfFrame();
        }

        board.RemoveBoard();

        agent.isStopped = false;
        actingState = ActingState.WALKING;

        yield break;
    }
}
