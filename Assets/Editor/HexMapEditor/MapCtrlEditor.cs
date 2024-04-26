using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WarGame_True.Infrastructure.HexagonGrid.Controller;
using WarGame_True.Infrastructure.HexagonGrid.DataStruct;
using WarGame_True.Infrastructure.Map.Controller;

namespace WarGame_True.WarGameEditor.HexMapEditor {
    [CustomEditor(typeof(MapController))]
    public class MapCtrlEditor : Editor {


        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            MapController mapController = (MapController)target;

            // �����Ӵֵ��ı���ʽ
            GUIStyle boldTextStyle = new GUIStyle() {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
            };
            EditorGUILayout.LabelField("��ͼģʽ�л�:", boldTextStyle);
            GUILayout.Space(6);

            if (GUILayout.Button("һ��ģʽ")) {
                mapController.InitMap();
                mapController.ChangeMapMode(MapMode.Normal);
            }

            GUILayout.Space(6);

            if (GUILayout.Button("����ģʽ")) {
                mapController.InitMap();
                mapController.ChangeMapMode(MapMode.Terrain);
            }

            
        }

    }
}