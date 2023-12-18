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
    public float amountToRestore;
    public float timeToInteract;

    void Interact(Player player)
    {
        switch (restorationType) {
            case RestorationType.HEALTH:
                player.HealDamage(amountToRestore);
                break;
            case RestorationType.AMMO:
                player.RestoreAmmo((int)amountToRestore);
                break;
        }
    }
}
