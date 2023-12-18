using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{
    public List<GameObject> boards = new List<GameObject>();
    public float force = 10.0f;
    public Vector2 spread = new Vector2(20.0f, 20.0f);
    public float timeToRestoreBoard = 1.5f;

    List<GameObject> removedBoards = new List<GameObject>();
    public int remainingBoards { get { return boards.Count - removedBoards.Count; } }

    List<Vector3> startingPositions = new List<Vector3>();
    List<Quaternion> startingRotations = new List<Quaternion>();

    private void Start()
    {
        for (int i = 0; i < boards.Count; i++) {
            startingPositions.Add(boards[i].transform.position);
            startingRotations.Add(boards[i].transform.rotation);
        }
    }

    public void RemoveBoard()
    {
        for (int i = 0; i < boards.Count; i++) {
            if (removedBoards.Contains(boards[i])) {
                continue;
            } else {
                removedBoards.Add(boards[i]);
                Rigidbody rb = boards[i].GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.isKinematic = false;
                    rb.AddForce(Quaternion.Euler(Random.Range(-spread.x, spread.x), Random.Range(-spread.y, spread.y), 0) * -transform.forward * force);
                }

                break;
            }
        }
    }

    public void ReplaceBoard()
    {
        if (removedBoards.Count <= 0) return;

        GameObject board = removedBoards[removedBoards.Count - 1];
        removedBoards.Remove(board);
        Rigidbody rb = board.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.isKinematic = true;
        }

        StartCoroutine(MoveBoardBackToStart(removedBoards.Count, timeToRestoreBoard));
    }

    IEnumerator MoveBoardBackToStart(int index, float duration)
    {
        Vector3 startPos = boards[index].transform.position;
        Quaternion startRot = boards[index].transform.rotation;

        float timer = 0;
        while ((timer += Time.deltaTime) <= duration) {
            float completionPercent = timer / duration;

            boards[index].transform.position = Vector3.Lerp(startPos, startingPositions[index], completionPercent);
            boards[index].transform.rotation = Quaternion.Slerp(startRot, startingRotations[index], completionPercent);

            yield return new WaitForEndOfFrame();
        }

        boards[index].transform.position = startingPositions[index];
        boards[index].transform.rotation = startingRotations[index];

        yield break;
    }
}
