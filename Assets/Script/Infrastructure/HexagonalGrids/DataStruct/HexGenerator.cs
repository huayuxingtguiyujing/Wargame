//using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace WarGame_True.Infrastructure.HexagonGrid.DataStruct {
    public class HexGenerator {
        #region ����ģʽ
        private static HexGenerator Instance;
        public static HexGenerator GetInstance() {
            if (Instance == null) {
                Instance = new HexGenerator();
            }
            return Instance;
        }

        public HexGenerator() {

        }
        #endregion


        // ��¼ ��ǰ�Ѿ����ɵ������ͼ

        // ����λ�� - Hexagon ��ӳ��(������)
        private Dictionary<Vector3, Hexagon> hexagonPosDic = new Dictionary<Vector3, Hexagon>();
        // ����ID - Hexagon ��ӳ��
        private Dictionary<uint, Hexagon> hexagonNumDic = new Dictionary<uint, Hexagon>();

        public Dictionary<Vector3, Hexagon> HexagonPosDic {
            get => hexagonPosDic;
            private set => hexagonPosDic = value;
        }
        public Dictionary<uint, Hexagon> HexagonNumDic {
            get => hexagonNumDic;
            private set => hexagonNumDic = value;
        }
        

        #region ��ͬ�����ɷ���
        public void ClearHexagon() {
            hexagonPosDic.Clear();
            hexagonNumDic.Clear();
        }

        /// <summary>
        /// ����ƽ���ı���״ �������μ���
        /// </summary>
        /// <param name="q1">q��������</param>
        /// <param name="q2">q������յ�</param>
        /// <param name="r1">r��������</param>
        /// <param name="r2">r������յ�</param>
        public void GenerateParallelogram(int q1, int q2, int r1, int r2) {
            uint count = 0;
            for (int q = q1; q <= q2; q++) {
                for (int r = r1; r <= r2; r++) {
                    Hexagon hexagon = new Hexagon(q, r, -q - r);
                    hexagonPosDic.Add((Vector3)hexagon, hexagon);
                    hexagonNumDic.Add((uint)count, hexagon);
                    count++;
                }
            }
        }

        /// <summary>
        /// ���� ������״ �������μ���
        /// </summary>
        /// <param name="mapSize">��ͼ�ߴ�</param>
        public void GenerateTriangle(int mapSize) {
            uint count = 0;
            for (int q = 0; q <= mapSize; q++) {
                for (int r = 0; r <= mapSize - q; r++) {
                    Hexagon hexagon = new Hexagon(q, r, -q - r);
                    hexagonPosDic.Add((Vector3)hexagon, hexagon);
                    hexagonNumDic.Add((uint)count, hexagon);
                    count++;
                }
            }
        }

        /// <summary>
        /// ���� ������״ �������μ���
        /// </summary>
        /// <param name="mapSize">��ͼ�ߴ�</param>
        public void GenerateHexagon(int mapSize) {
            uint count = 0;
            for (int q = -mapSize; q <= mapSize; q++) {
                int r1 = Mathf.Max(- mapSize, -q - mapSize);
                int r2 = Mathf.Min(mapSize, -q + mapSize);
                for (int r = r1; r <= r2; r++) {
                    Hexagon hexagon = new Hexagon(q, r, -q - r);
                    hexagonPosDic.Add((Vector3)hexagon, hexagon);
                    hexagonNumDic.Add((uint)count, hexagon);
                    count++;
                }
            }
        }

        /// <summary>
        /// ���� ������״ �������μ���
        /// </summary>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void GenerateRectangle(int top, int bottom, int left, int right) {
            uint count = 0;
            for (int r = top; r <= bottom; r++) { // pointy top
                //ִ�к����ƫ��
                int r_offset = (int)Mathf.Floor(r / 2); // or r>>1
                for (int q = left - r_offset; q <= right - r_offset; q++) {
                    Hexagon hexagon = new Hexagon(q, r, -q - r);
                    hexagonPosDic.Add((Vector3)hexagon, hexagon);
                    hexagonNumDic.Add((uint)count, hexagon);
                    count++;
                }
            }
        }
        #endregion

    }

    public enum DrawGridMethod {
        Rectangle,
        Hexagon,
        Triangle,
        Parallelogram
    }
}