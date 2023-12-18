using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private void OnSceneGUI()
    {
        GameManager gameManager = (GameManager)target;

        if (gameManager.enemySpawnFocus != null) {
            Handles.DrawWireDisc(gameManager.enemySpawnFocus.position, Vector3.up, gameManager.enemySpawnDistanceMin);
            Handles.DrawWireDisc(gameManager.enemySpawnFocus.position, Vector3.up, gameManager.enemySpawnDistanceMax);
        }
    }
}
