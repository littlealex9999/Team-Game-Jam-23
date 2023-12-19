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

    [Header("Kill Effects")]
    public Animator animator;
    public List<GameObject> headPopEnable;
    string currentAnimState;

    [Header("Animation Jank")]
    public List<float> meleeTimes;
    public List<float> damageTimes;

    bool dead = false;
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
        if (GameManager.instance.gameStopped || dead) return;

        switch (actingState) {
            case ActingState.WALKING:
                GetPathToTarget();
                SetAnimationState("Walk", 3);
                break;

            case ActingState.DESTROYINGWALL:
                SetAnimationState("Telekinesis", 1);
                break;

            case ActingState.ATTACKING:
                SetAnimationState("Melee", 3);
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

    int SetAnimationState(string animation, int maxVal, bool retIfAlreadyPlaying = true)
    {
        if (retIfAlreadyPlaying && currentAnimState == animation) return -1;

        if (currentAnimState != null && currentAnimState != "") {
            animator.SetFloat(currentAnimState, 0);
        }

        int selectedVal;

        if (maxVal > 1) {
            selectedVal = Random.Range(1, maxVal + 1);
        } else {
            selectedVal = 1;
        }

        animator.SetFloat(animation, selectedVal);
        currentAnimState = animation;

        return selectedVal;
    }

    public void TakeDamage(float damage, BodyPart.PartType part)
    {
        //TODO: Hit anim

        switch (part) {
            case BodyPart.PartType.HEAD:
                health -= health * 2;
                GameManager.instance.AddScore(GameManager.instance.scoreOnHeadshot);
                Debug.Log("Headshot");
                break;
            case BodyPart.PartType.BODY:
            default:
                health -= damage;
                GameManager.instance.AddScore(GameManager.instance.scoreOnBullet);
                Debug.Log("Body shot");
                break;
        }

        if (health <= 0) {
            Death(part);
        }
    }

    void Death(BodyPart.PartType part)
    {
        GameManager.instance.AddScore(GameManager.instance.scoreOnKill);

        //Destroy(gameObject);
        switch (part) {
            case BodyPart.PartType.HEAD:
                for (int i = 0; i < headPopEnable.Count; i++) {
                    headPopEnable[i].SetActive(true);
                }
                break;
            case BodyPart.PartType.BODY:
            default:
                break;
        }

        SetAnimationState("Death", 3);

        dead = true;
        agent.isStopped = true;
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

        int meleeIndex = SetAnimationState("Melee", 3, false) - 1;

        // do something animation based

        bool hit = false;
        float timer = meleeTimes[meleeIndex];
        while ((timer -= Time.deltaTime) > 0) {
            if (GameManager.instance.gameStopped) yield return new WaitForEndOfFrame();

            if (!hit) {
                if (timer <= meleeTimes[meleeIndex] - damageTimes[meleeIndex]) {
                    hit = true;

                    if ((GameManager.instance.player.transform.position - transform.position).sqrMagnitude < agent.stoppingDistance * agent.stoppingDistance) {
                        DealDamage(GameManager.instance.player);
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }

        agent.isStopped = false;
        actingState = ActingState.WALKING;

        SetAnimationState("Walk", 3, false);

        yield break;
    }

    IEnumerator DestroyBoard(Board board)
    {
        if (actingState != ActingState.WALKING) yield break;

        actingState = ActingState.DESTROYINGWALL;
        if (!agent.isStopped) agent.isStopped = true;

        SetAnimationState("Telekinesis", 1, false);

        // do something animation based

        float timer = 5.0f;
        while ((timer -= Time.deltaTime) > 0) {
            yield return new WaitForEndOfFrame();
        }

        board.RemoveBoard();

        agent.isStopped = false;
        actingState = ActingState.WALKING;

        SetAnimationState("Walk", 3, false);

        yield break;
    }
}
