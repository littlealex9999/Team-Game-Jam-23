using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum RestorationType
    {
        HEALTH,
        AMMO,
    }

    public RestorationType restorationType;
    public int cost;
    public float timeToInteract;


}
