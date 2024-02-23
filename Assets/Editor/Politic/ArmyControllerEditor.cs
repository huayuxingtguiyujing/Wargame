using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.WarGameEditor.PoliticEditor {
    [CustomEditor(typeof(ArmyController))]
    public class ArmyControllerEditor : Editor {

        ArmyController armyController;



        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            armyController = target.GetComponent<ArmyController>();

            GUILayout.Label("调试军队控制器");

            GUILayout.Space(10);
            if (GUILayout.Button("随机创建军队", GUILayout.Width(160), GUILayout.Height(40))) {
                armyController.CreateArmy_Random();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("移除军队单位*1", GUILayout.Width(160), GUILayout.Height(40))) {

            }

            GUILayout.Space(10);
            if (GUILayout.Button("删除所有军队", GUILayout.Width(160), GUILayout.Height(40))) {
                armyController.RemoveArmy_All();
            }
        }

    }
}