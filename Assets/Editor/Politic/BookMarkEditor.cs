using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.WarGameEditor.PoliticEditor {
    [CustomEditor(typeof(BookMarks))]
    public class BookMarkEditor : Editor {


        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (GUILayout.Button("����FactionInfo����Faction", GUILayout.Width(160), GUILayout.Height(60))) {

            }
        }
    }
}