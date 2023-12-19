using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float radius;
    public float lifetime = 10.0f;
    [HideInInspector] public float speed;
    [HideInInspector] public float damage;

    void Update()
    {
        if (GameManager.instance.gameStopped) return;

        if (lifetime <= 0) Destroy(gameObject);
        lifetime -= Time.deltaTime;

        Vector3 startPos = transform.position;

        transform.position += transform.forward * speed * Time.deltaTime;

        Vector3 difference = transform.position - startPos;

        if (Physics.SphereCast(startPos, radius, difference, out RaycastHit hit, difference.magnitude)) {
            BodyPart part = hit.transform.GetComponent<BodyPart>();
            if (part != null) {
                part.enemy.TakeDamage(damage, part.part);
            }

            // bullet destroys regardless of hitting an enemy or not
            Destroy(gameObject);
        }
    }
}
