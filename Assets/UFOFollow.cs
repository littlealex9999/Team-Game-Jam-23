using System.Collections;
using UnityEngine;

public class UFOFollow : MonoBehaviour
{
    public float NormalMoveSpeed = 2f;
    public float ChaseMoveSpeed = 1f;
    public float VerticalMoveSpeed = 0.5f;
    public Transform moveBoundsMin;
    public Transform moveBoundsMax;

    private Transform playerTransform;
    private CharacterController playerController;
    private Player playerScript;
    private bool isFollowingPlayer = false;
    private bool isChasing = false;
    private bool isMovingPlayerUp = false;
    private Coroutine movePlayerUpCoroutine;
    private Coroutine stayCoroutine;
    private Coroutine randomPosCoroutine;

    private void Start()
    {
        playerTransform = GameManager.instance.player.transform;
        playerController = GameManager.instance.player.GetComponent<CharacterController>();
        playerScript = GameManager.instance.player.GetComponent<Player>();
        StartCoroutine(RandomMove());
    }

    private void Update()
    {
        if (playerTransform.position.y >= 15 && isMovingPlayerUp)
        {
            isMovingPlayerUp = false;

            if (movePlayerUpCoroutine != null)
            {
                StopCoroutine(movePlayerUpCoroutine);
            }

            if (playerScript != null)
            {
                playerScript.enabled = true;
                GameManager.instance.player.TakeDamage(200);
            }

            GameManager.instance.player.gravity = -18;
            ResetChaseState();
        }
    }

    private IEnumerator RandomMove()
    {
        while (true)
        {
            if (isMovingPlayerUp)
            {
                yield return null;
            }
            else if (!GameManager.instance.player.isIndoors)
            {
                if (!isFollowingPlayer)
                {
                    isFollowingPlayer = true;
                    isChasing = true;
                    StartCoroutine(IncreaseChaseSpeedOverTime());
                }

                Vector3 targetPosition = playerTransform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, ChaseMoveSpeed * Time.deltaTime);
            }
            else
            {
                ResetChaseState();

                if (randomPosCoroutine == null) {
                    randomPosCoroutine = StartCoroutine(MoveToRandomPosition());
                }
            }

            yield return null;
        }
    }

    private void ResetChaseState()
    {
        ChaseMoveSpeed = 1;
        isFollowingPlayer = false;
        isChasing = false;
    }

    private IEnumerator MoveToRandomPosition()
    {
        Vector3 targetPosition = new Vector3(
            Random.Range(moveBoundsMin.position.x, moveBoundsMax.position.x),
            0f,
            Random.Range(moveBoundsMin.position.z, moveBoundsMax.position.z)
        );

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, NormalMoveSpeed * Time.deltaTime);
            yield return null;
        }

        randomPosCoroutine = null;
    }

    private IEnumerator IncreaseChaseSpeedOverTime()
    {
        while (isChasing)
        {
            yield return new WaitForSeconds(1.5f);
            ChaseMoveSpeed += 1f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.instance.player.gameObject)
        {
            stayCoroutine = StartCoroutine(CheckPlayerStay());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == GameManager.instance.player.gameObject && stayCoroutine != null)
        {
            StopCoroutine(stayCoroutine);
        }
    }

    private IEnumerator CheckPlayerStay()
    {
        yield return new WaitForSeconds(5f);
        movePlayerUpCoroutine = StartCoroutine(MovePlayerUp());
    }

    private IEnumerator MovePlayerUp()
    {
        isMovingPlayerUp = true;
        float originalGravity = GameManager.instance.player.gravity;
        GameManager.instance.player.gravity = 0;

        if (playerScript != null)
        {
            playerScript.enabled = false;
        }

        while (playerTransform.position.y < 15)
        {
            Vector3 moveDirection = Vector3.up * VerticalMoveSpeed;
            playerController.Move(moveDirection * Time.deltaTime);
            yield return null;
        }

        isMovingPlayerUp = false;

        if (playerScript != null)
        {
            playerScript.enabled = true;
        }

        GameManager.instance.player.gravity = originalGravity;
    }
}
