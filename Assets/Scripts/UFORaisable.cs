using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFORaisable : MonoBehaviour
{
    public float maxHeight = 5.0f;
    public float timeToRaise = 5.0f;

    int numTriggersInside = 0;
    public bool isEnteredTrigger { get { return numTriggersInside > 0; } }

    Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "VerticalMovementTrigger") {
            numTriggersInside++;

            if (numTriggersInside <= 1) {
                StartCoroutine(Raise(timeToRaise));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "VerticalMovementTrigger") {
            numTriggersInside--;
        }
    }

    IEnumerator Raise(float duration)
    {
        float timer = 0;

        while (timer < duration) {
            if (isEnteredTrigger) {
                timer += Time.deltaTime;
                if (timer > duration) timer = duration;
            } else {
                timer -= Time.deltaTime;
            }

            Vector3 targetPos = startPos;
            targetPos.y = Mathf.Lerp(startPos.y, startPos.y + maxHeight, timer / duration);

            transform.position = targetPos;

            yield return new WaitForEndOfFrame();
        }

        yield break;
    }
}
