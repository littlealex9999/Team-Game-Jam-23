using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    private void OnSceneGUI()
    {
        Player player = (Player)target;
        CharacterController controller = player.GetComponent<CharacterController>();

        Vector3 center = player.transform.position - player.transform.up * (controller.height / 2 + player.groundedCheckBuffer);

        Handles.DrawWireDisc(center, Vector3.up, controller.radius);
        Handles.DrawWireDisc(center, Vector3.right, controller.radius);
        Handles.DrawWireDisc(center, Vector3.forward, controller.radius);
    }
}
