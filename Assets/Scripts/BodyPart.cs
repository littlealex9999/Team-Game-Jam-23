using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    public Enemy enemy;

    public enum PartType
    {
        BODY,
        HEAD,
    }

    public PartType part;

    private void Start()
    {
        if (enemy == null) enemy = GetComponentInParent<Enemy>();
    }
}
