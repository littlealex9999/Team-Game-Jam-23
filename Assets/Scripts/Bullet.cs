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
        if (lifetime <= 0) Destroy(gameObject);
        lifetime -= Time.deltaTime;

        Vector3 startPos = transform.position;

        transform.position += transform.forward * speed * Time.deltaTime;

        if (Physics.SphereCast(startPos, radius, transform.position - startPos, out RaycastHit hit)) {

        }
    }
}
