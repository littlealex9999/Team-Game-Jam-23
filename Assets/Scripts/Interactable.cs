using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum RestorationType
    {
        HEALTH,
        AMMO,
        BOARD,
    }

    public RestorationType restorationType;
    public int cost;
    public float amountToRestore;
    public float timeToInteract;
    public Board board;

    public void Interact(Player player)
    {
        switch (restorationType) {
            case RestorationType.HEALTH:
                player.HealDamage(amountToRestore);
                break;
            case RestorationType.AMMO:
                player.RestoreAmmo((int)amountToRestore);
                break;
            case RestorationType.BOARD:
                board.ReplaceBoard();
                break;
        }
    }

    public bool CanInteract(Player player)
    {
        switch (restorationType) {
            case RestorationType.HEALTH:
                if (player.health < player.maxHealth) return true;
                else return false;
            case RestorationType.AMMO:
                if (player.currentAmmoHeld < player.maxAmmoClip - player.currentAmmoClip + player.maxAmmoHeld) return true;
                else return false;
            case RestorationType.BOARD:
                if (board.remainingBoards < board.boards.Count) return true;
                else return false;
        }

        // solves compiler error; should never trigger
        return false;
    }
}
