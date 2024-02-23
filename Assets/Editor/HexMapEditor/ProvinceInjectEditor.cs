using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using WarGame_True.Infrastructure.HexagonGrid.Controller;
using WarGame_True.Infrastructure.Map;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.WarGameEditor.HexMapEditor {
    [CustomEditor(typeof(ProvinceInject))]
    public class ProvinceInjectEditor : Editor {


        public override async void OnInspectorGUI() {
            DrawDefaultInspector();

            ProvinceInject provinceInject = target.GetComponent<ProvinceInject>();

            //TODO����CSV��д�õ�ʡ����Ϣע�뵽�����ε�ͼ����
            GUILayout.Space(10);
            if (GUILayout.Button("���뵽��ǰ��ͼ", GUILayout.Width(160), GUILayout.Height(40))) {
                await provinceInject.ImportCSVToCurrentMap();
            }

            //TODO���������ε�ͼ���ʡ����Ϣ ������CSV�ı���
            GUILayout.Space(10);
            if (GUILayout.Button("������CSV�ı�", GUILayout.Width(160), GUILayout.Height(40))) {
                provinceInject.ExportCurrentMapToCSV();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("���csvʡ�ݴ洢", GUILayout.Width(160), GUILayout.Height(40))) {
                provinceInject.ClearMapStorage();
            }

        }
    }
}