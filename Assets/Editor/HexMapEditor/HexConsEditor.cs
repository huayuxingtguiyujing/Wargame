using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using WarGame_True.Infrastructure.HexagonGrid.Controller;
using WarGame_True.Infrastructure.HexagonGrid.DataStruct;

namespace WarGame_True.WarGameEditor.HexMapEditor {
    [CustomEditor(typeof(HexConstructor))]
    public class HexConsEditor : Editor {

        int mapSize;

        Vector2 Q1AndQ2;
        Vector2 Q3AndQ4;

        Vector2 LeftAndRight = new Vector2(-20, 21);
        Vector2 TopAndBottom = new Vector2(-24, 24);

        int mapSizeHex;

        Vector2Int noiseWH = new Vector2Int(20, 20);
        float noiseScale = 1.0f;


        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            HexConstructor hexConstructor = (HexConstructor)target;

            // �հ�����
            GUILayout.Space(15);

            // �����Ӵֵ��ı���ʽ
            GUIStyle boldTextStyle = new GUIStyle() {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
            };
            EditorGUILayout.LabelField("�༭�����湦��:", boldTextStyle);

            GUILayout.Space(6);

            if (GUILayout.Button("�������")) {
                hexConstructor.ClearHexObject();
                HexGenerator.GetInstance().ClearHexagon();
            }
            if (GUILayout.Button("���ƺ���")) {
                hexConstructor.GenerateRiver();
            }
            if (GUILayout.Button("ˢ������")) {
                hexConstructor.RefreshHexagons();
            }
           
            GUILayout.Space(6);

            GUILayout.Space(6);

            //inspector���� ���ֻ��Ʒ�ʽ���ṩ��ť�ӿ�
            GUILayout.Label("�����������С");
            mapSize = EditorGUILayout.IntField(mapSize, GUILayout.Width(80));
            if (GUILayout.Button("��������������")) {
                hexConstructor.ClearHexObject();
                HexGenerator.GetInstance().ClearHexagon();
                HexGenerator.GetInstance().GenerateTriangle(mapSize);
                //List<Hexagon> hexes = HexGenerator.GetInstance().GetHexagons();
                hexConstructor.InitHexagonalGridScene(HexGenerator.GetInstance().HexagonNumDic);
            }

            Q1AndQ2 = EditorGUILayout.Vector2Field("Left-Right", Q1AndQ2);
            Q3AndQ4 = EditorGUILayout.Vector2Field("Top-Bottom", Q3AndQ4);
            if (GUILayout.Button("����ƽ���ı�������")) {
                hexConstructor.ClearHexObject();
                HexGenerator.GetInstance().ClearHexagon();
                HexGenerator.GetInstance().GenerateParallelogram(
                    (int)Q1AndQ2.x, (int)Q1AndQ2.y,
                    (int)Q3AndQ4.x, (int)Q3AndQ4.y
                );
                //List<Hexagon> hexes = HexGenerator.GetInstance().GetHexagons();
                hexConstructor.InitHexagonalGridScene(HexGenerator.GetInstance().HexagonNumDic);
            }

            LeftAndRight = EditorGUILayout.Vector2Field("Left-Right", LeftAndRight);
            TopAndBottom = EditorGUILayout.Vector2Field("Top-Bottom", TopAndBottom);
            if (GUILayout.Button("���ƾ�������")) {
                hexConstructor.ClearHexObject();
                HexGenerator.GetInstance().ClearHexagon();
                HexGenerator.GetInstance().GenerateRectangle(
                    (int)TopAndBottom.x, (int)TopAndBottom.y,
                    (int)LeftAndRight.x, (int)LeftAndRight.y
                );
                //List<Hexagon> hexes = HexGenerator.GetInstance().GetHexagons();
                hexConstructor.InitHexagonalGridScene(HexGenerator.GetInstance().HexagonNumDic);
            }

            GUILayout.Label("�����������С");
            mapSizeHex = EditorGUILayout.IntField(mapSizeHex, GUILayout.Width(80));
            if (GUILayout.Button("��������������")) {
                hexConstructor.ClearHexObject();
                HexGenerator.GetInstance().ClearHexagon();
                HexGenerator.GetInstance().GenerateHexagon(mapSizeHex);
                //List<Hexagon> hexes = HexGenerator.GetInstance().GetHexagons();
                hexConstructor.InitHexagonalGridScene(HexGenerator.GetInstance().HexagonNumDic);
            }

            GUILayout.Space(12);
            GUILayout.Label("��ͼ����Ŷ�");
            GUILayout.Space(6);
            noiseWH = EditorGUILayout.Vector2IntField("Noise Width Height", noiseWH);
            GUILayout.Space(2);
            GUILayout.Label("������ģ");
            noiseScale = EditorGUILayout.FloatField(noiseScale, GUILayout.Width(80));
            if (GUILayout.Button("���ɰ�������")) {
                hexConstructor.GeneratePerlinTexture(noiseWH.x, noiseWH.y, noiseScale);
            }

            if (GUILayout.Button("Ӧ�ð�������")) {
                hexConstructor.DisturbGridMap();
            }


        }
    }
}