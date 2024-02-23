using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.WarGameEditor.PoliticEditor {
    [CustomEditor(typeof(FactionInfo))]
    public class FactionInfoEditor : Editor {

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (GUILayout.Button("导出派系信息到文件中", GUILayout.Width(160), GUILayout.Height(60))) {

            }
        }

    }
}