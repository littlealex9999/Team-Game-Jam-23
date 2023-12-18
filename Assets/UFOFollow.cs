using System.Collections;
using UnityEngine;

public class UFOFollow : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float squareSize = 5f;

    private Transform playerTransform;
    private bool isFollowingPlayer = false;

    private void Start()
    {
        playerTransform = GameManager.instance.player.transform; // Assuming GameManager has a reference to the player

        // Start the movement coroutine
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
                    // If the player is not indoors and we are not following them, start following
                    isFollowingPlayer = true;
                }

                // Move towards the player's position
                Vector3 targetPosition = playerTransform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
            else
            {
                // If the player is indoors, stop following the player
                isFollowingPlayer = false;

                // Generate a random target position within the square area
                Vector3 targetPosition = new Vector3(
                    Random.Range(-squareSize / 2, squareSize / 2),
                    0f,
                    Random.Range(-squareSize / 2, squareSize / 2)
                );

                // Smoothly move the object towards the random position
                while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }
            }

            yield return null;
        }
    }
}
