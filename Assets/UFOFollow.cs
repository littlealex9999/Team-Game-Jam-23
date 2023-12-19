using System.Collections;
using UnityEngine;

public class UFOFollow : MonoBehaviour
{
    public float NormalMoveSpeed = 2f;
    public float ChaseMoveSpeed = 1f;

    public Transform moveBoundsMin;
    public Transform moveBoundsMax;

    private Transform playerTransform;
    private bool isFollowingPlayer = false;
    private bool isChasing = false;

    private void Start()
    {
        playerTransform = GameManager.instance.player.transform;

        StartCoroutine(RandomMove());
    }

    private IEnumerator RandomMove()
    {
        while (true)
        {
            if (!GameManager.instance.player.isIndoors)
            {
                if (!isFollowingPlayer)
                {
                    isFollowingPlayer = true;
                    isChasing = true; // Start chasing, so set isChasing to true
                    StartCoroutine(IncreaseChaseSpeedOverTime());
                }

                Vector3 targetPosition = playerTransform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, ChaseMoveSpeed * Time.deltaTime);
            }
            else
            {
                ChaseMoveSpeed = 1;
                isFollowingPlayer = false;
                isChasing = false; // Stop chasing, so set isChasing to false

                Vector3 targetPosition = new Vector3(
                    Random.Range(moveBoundsMin.position.x, moveBoundsMax.position.x),
                    0f,
                    Random.Range(moveBoundsMin.position.z, moveBoundsMax.position.z)
                );

                while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                {
                    if (!GameManager.instance.player.isIndoors)
                    {
                        break;
                    }

                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, NormalMoveSpeed * Time.deltaTime);
                    yield return null;
                }
            }

            yield return null;
        }
    }

    private IEnumerator IncreaseChaseSpeedOverTime()
    {
        while (isChasing) // Only increase speed when chasing
        {
            yield return new WaitForSeconds(1.5f);
            ChaseMoveSpeed += 1f; // Increase ChaseMoveSpeed by 1
        }
    }
}
