using System.Collections;
using UnityEngine;

public class UFOFollow : MonoBehaviour
{
    public float NormalMoveSpeed = 2f;
    public float ChaseMoveSpeed = 1f;
    public float VerticalMoveSpeed = 0.5f;
    public float timeToSuckPlayer = 5.0f;
    public Transform moveBoundsMin;
    public Transform moveBoundsMax;
    public AudioSource audioSource;
    private Transform playerTransform;
    private CharacterController playerController;
    private Player playerScript;
    private bool isFollowingPlayer = false;
    private bool isChasing = false;
    private bool isMovingPlayerUp = false;
    private Coroutine movePlayerUpCoroutine;
    private Coroutine stayCoroutine;
    private Coroutine randomPosCoroutine;

    [Header("Audio Clips")]
    public AudioClip UFOSound;
    

    private bool playerInside = false;
    [HideInInspector] public float suckTime = 0.0f;

    private void Start()
    {
        playerTransform = GameManager.instance.player.transform;
        playerController = GameManager.instance.player.GetComponent<CharacterController>();
        playerScript = GameManager.instance.player.GetComponent<Player>();
        StartCoroutine(RandomMove());
    }

    private void Update()
    {
        if (playerTransform.position.y >= 15 && isMovingPlayerUp) {
            isMovingPlayerUp = false;

            if (movePlayerUpCoroutine != null) {
                StopCoroutine(movePlayerUpCoroutine);
            }

            if (playerScript != null) {
                playerScript.enabled = true;
                GameManager.instance.player.TakeDamage(200);
            }

            GameManager.instance.player.gravity = -18;
            ResetChaseState();
        }
    }

    private IEnumerator RandomMove()
    {
        while (true) {
            if (isMovingPlayerUp) {
                yield return null;
            } else if (!GameManager.instance.player.isIndoors) {
                if (!isFollowingPlayer) {
                    isFollowingPlayer = true;
                    isChasing = true;
                    StartCoroutine(IncreaseChaseSpeedOverTime());
                }

                Vector3 targetPosition = playerTransform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, ChaseMoveSpeed * Time.deltaTime);
            } else {
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

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, NormalMoveSpeed * Time.deltaTime);
            yield return null;
        }

        randomPosCoroutine = null;
    }

    private IEnumerator IncreaseChaseSpeedOverTime()
    {
        while (isChasing) {
            yield return new WaitForSeconds(1.5f);
            ChaseMoveSpeed += 1f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.instance.player.gameObject) {
            playerInside = true;
            stayCoroutine = StartCoroutine(CheckPlayerStay());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == GameManager.instance.player.gameObject && stayCoroutine != null) {
            playerInside = false;
            //StopCoroutine(stayCoroutine);
        }
    }

    private IEnumerator CheckPlayerStay()
    {
        suckTime = 0;
        while (suckTime < timeToSuckPlayer && suckTime > -1.0f) {
            if (playerInside) {
                if (suckTime < 0) suckTime = 0;
                suckTime += Time.deltaTime;
            } else {
                suckTime -= Time.deltaTime;
            }

            yield return new WaitForEndOfFrame();
        }

        stayCoroutine = null;
        if (suckTime > 0) {
            movePlayerUpCoroutine = StartCoroutine(MovePlayerUp());
        }
        suckTime = 0;

        yield break;
    }

    private IEnumerator MovePlayerUp()
    {
        isMovingPlayerUp = true;
        float originalGravity = GameManager.instance.player.gravity;
        GameManager.instance.player.gravity = 0;
        audioSource.PlayOneShot(UFOSound);
        if (playerScript != null) {
            playerScript.enabled = false;
        }

        while (playerTransform.position.y < 15) {
            Vector3 moveDirection = Vector3.up * VerticalMoveSpeed;
            playerController.Move(moveDirection * Time.deltaTime);
            yield return null;
        }

        isMovingPlayerUp = false;

        if (playerScript != null) {
            playerScript.enabled = true;
        }

        GameManager.instance.player.gravity = originalGravity;
    }
}
