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

            //TODO：将CSV中写好的省份信息注入到六边形地图格中
            GUILayout.Space(10);
            if (GUILayout.Button("导入到当前地图", GUILayout.Width(160), GUILayout.Height(40))) {
                await provinceInject.ImportCSVToCurrentMap();
            }

            //TODO：将六边形地图格的省份信息 导出到CSV文本中
            GUILayout.Space(10);
            if (GUILayout.Button("导出到CSV文本", GUILayout.Width(160), GUILayout.Height(40))) {
                provinceInject.ExportCurrentMapToCSV();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("清空csv省份存储", GUILayout.Width(160), GUILayout.Height(40))) {
                provinceInject.ClearMapStorage();
            }

        }
    }
}