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

            if (GUILayout.Button("������ϵ��Ϣ���ļ���", GUILayout.Width(160), GUILayout.Height(60))) {

            }
        }

    }
}