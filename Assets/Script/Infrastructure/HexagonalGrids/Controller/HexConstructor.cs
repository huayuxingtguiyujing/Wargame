using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using WarGame_True.Infrastructure.HexagonGrid.DataStruct;
using WarGame_True.Infrastructure.HexagonGrid.MapObject;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Utils;

namespace WarGame_True.Infrastructure.HexagonGrid.Controller {
    /// <summary>
    /// ���� ���������������ͼ���Ѿ�Ϊ�����ṩeditor��ڣ�����������Ϸ���������ɵĹ���
    /// </summary>
    public class HexConstructor : MonoBehaviour {
        [Tooltip("ע�⣺hexConstructor ")]
        [SerializeField] private bool ConstructorInStart = true;

        [SerializeField] private Transform originPoint;
        [SerializeField] private Vector2 Size;

        [Header("��������")]
        [SerializeField] Renderer perlinRenderer;

        PerlinGenerator perlinGenerator;

        [Header("��Ϊprefab�ĸ�������")]
        [SerializeField] private Transform centerTransform;
        [SerializeField] private Transform pointTransform;
        [SerializeField] private Transform hexGridTransform;

        [Header("prefab")]
        [SerializeField] private GameObject centerPrefab;
        [SerializeField] private GameObject pointPrefab;
        [SerializeField] private GameObject hexGridPrefab;

        //��ǰ�������� ��������
        private List<Hexagon> CurrentHexagons;

        ////��ǰ�� ����λ�� - ʡ�� ��ӳ��
        public Dictionary<Vector3, HexGrid> currentPosHex = new Dictionary<Vector3, HexGrid>();
        public Dictionary<Vector3, HexGrid> CurrentPosHex { get => currentPosHex; private set => currentPosHex = value;}

        //��ǰ�� ID - ʡ�� ��ӳ��
        public Dictionary<uint, HexGrid> currentIDHex = new Dictionary<uint, HexGrid>();
        public Dictionary<uint, HexGrid> CurrentIDHex { get => currentIDHex; private set => currentIDHex = value; }

        // ��ǰ�ĺ���
        [Header("����")]
        public List<River> CurrentRiver;

        public List<HexGrid> RiverPathGenerator;


        public void UpdateCurrentHexProvince() {
            
            //�������е�����hexgrid������CurrentHexProvince
            HexGrid[] hexGrids = hexGridTransform.GetComponentsInChildren<HexGrid>();
            foreach (HexGrid hexGrid in hexGrids)
            {
                if (CurrentPosHex.ContainsKey(hexGrid.hexPosition)) {
                    CurrentPosHex[hexGrid.hexPosition] = hexGrid;
                } else {
                    CurrentPosHex.Add(hexGrid.hexPosition, hexGrid);
                }

                if (CurrentIDHex.ContainsKey(hexGrid.HexID)) {
                    CurrentIDHex[hexGrid.HexID] = hexGrid;
                } else {
                    CurrentIDHex.Add(hexGrid.HexID, hexGrid);
                }
                //Debug.Log(hexGrid.hexCanvas == null);
            }

            //Debug.Log(CurrentHexProvince.Count);
        }

        private void Start() {
            if (!ConstructorInStart) {
                return;
            }

            //���� Hex ������������ ���뵽������
            HexGenerator.GetInstance().ClearHexagon();
            HexGenerator.GetInstance().GenerateHexagon(10);
            //List<Hexagon> hexes = HexGenerator.GetInstance().GetHexagons();
            Dictionary<uint, Hexagon> hexeIDDic = HexGenerator.GetInstance().HexagonNumDic;

            Debug.Log("you have create " + hexeIDDic.Count.ToString() + " hex class");

            InitHexagonalGridScene(hexeIDDic);
        }

        public void InitHexagonalGridScene(Dictionary<uint, Hexagon> hexeIDDic) {
            // �����������
            //��¼ ��ǰ���ɵ������ͼ
            CurrentHexagons = hexeIDDic.Values.ToList();

            Layout layout = GetScreenLayout();

            //��ʼ����ͼ��������
            foreach (KeyValuePair <uint, Hexagon> IDHexPair in hexeIDDic)
            {
                CreateHexagon(layout, IDHexPair.Key, IDHexPair.Value);
            }

            // ���õ�ͼ������ھӽڵ�
            SetHexNeighbor(CurrentHexagons, CurrentPosHex);

            // ���ɺ�����

            // ���� Mesh ����
            RefreshHexagons();
        }

        private Layout GetScreenLayout() {
            //������Ļ������
            Vector2 startPoint = new Vector2(originPoint.position.x, originPoint.position.y);
            Layout layout = new Layout(
                Orientation.Layout_Pointy, new Point(Size.x, Size.y), new Point(startPoint.x, startPoint.y)
            );
            return layout;
        }

        /// <summary>
        /// ���� Hex ������ ����hex����
        /// </summary>
        private void CreateHexagon(Layout layout, uint hexID, Hexagon hex) {
            // �������ҳ�ʼ��������mesh��hexgrid
            GameObject hexGridObject = Instantiate(hexGridPrefab, hexGridTransform);
            HexGrid hexGrid = hexGridObject.GetComponent<HexGrid>();
            hexGrid.InitHexGrid_Mesh(layout, hexID, hex);

            //���뵽ӳ�䵱��
            CurrentIDHex.Add(hexID, hexGrid);
            CurrentPosHex.Add(hexGrid.HexIntPosition, hexGrid);
        }

        private void SetHexNeighbor(List<Hexagon> CurrentHexagons, Dictionary<Vector3, HexGrid> CurrentPosHex) {
            for (int i = 0; i < CurrentHexagons.Count; i++) {
                for (int j = 0; j < 6; j++) {
                    HexDirection direction = (HexDirection)j;
                    Hexagon neighbor = CurrentHexagons[i].Hex_Neighbor(direction);
                    if (CurrentPosHex.ContainsKey(CurrentHexagons[i]) && CurrentPosHex.ContainsKey(neighbor)) {
                        //CurrentPosHex[CurrentHexagons[i]].NeighbourGrids[j] = CurrentPosHex[neighbor];
                        CurrentPosHex[CurrentHexagons[i]].NeighbourGrids.Add(CurrentPosHex[neighbor]);
                    } else {
                        // NOTICE: һ��Ҫ��null��Ϊռλ��������˳������
                        CurrentPosHex[CurrentHexagons[i]].NeighbourGrids.Add(null);
                    }
                }
            }
        }

        public void RefreshHexagons() {
            //������Ļ������
            Vector2 startPoint = new Vector2(originPoint.position.x, originPoint.position.y);
            Layout layout = new Layout(
                Orientation.Layout_Pointy, new Point(Size.x, Size.y), new Point(startPoint.x, startPoint.y)
            );

            // ���� Mesh ����(�����������ھӺ����)
            foreach (KeyValuePair<uint, HexGrid> IDHexPair in CurrentIDHex) {
                IDHexPair.Value.DrawHexGridMesh(layout, IDHexPair.Value.hexagon);
            }
        }

        public void ClearHexObject() {
            centerTransform.ClearObjChildren();
            pointTransform.ClearObjChildren();
            hexGridTransform.ClearObjChildren();
            CurrentPosHex.Clear();
            CurrentIDHex.Clear();
        }


        public void GenerateRiver() {
            if(CurrentRiver == null) {
                CurrentRiver = new List<River>();
            }
            CurrentRiver.Add(new River(RiverPathGenerator));
            //RiverPathGenerator.Clear();
        }


        /// <summary>
        /// ���ɰ�����������
        /// </summary>
        public void GeneratePerlinTexture(int width, int height, float scale) {
            if(perlinGenerator == null) {
                perlinGenerator = new PerlinGenerator();
            }
            perlinGenerator.GeneratePerlinNoise(10, 10, 1.0f);
            perlinGenerator.GenerateTexture(perlinRenderer);
            
        }

        /// <summary>
        /// �����������Ŷ�Ӧ���ڵ�ͼ������
        /// </summary>
        public void DisturbGridMap() {

            if (perlinGenerator == null) {
                Debug.Log("δ����perlinGenerator");
                return;
            }

            // ��Ҫ�õ����������ͼ�� minPoint �� maxPoint ���ܲ�ֵ
            // �����Ŷ�

            // TODO: Ŀǰ���Ŷ��ƺ������⣬�Ŷ�Ч�����Ǻ�����
            foreach (KeyValuePair<uint, HexGrid> IDHexPair in CurrentIDHex) {

                //Debug.Log(IDHexPair.Value.vertices.Count);
                for (int i = 0; i < IDHexPair.Value.vertices.Count; i++) {
                    Vector3 truePostion = IDHexPair.Value.vertices[i] + IDHexPair.Value.transform.position;
                    Vector4 sample = perlinGenerator.SampleNosie(truePostion);
                    IDHexPair.Value.vertices[i] = new Vector3(
                        IDHexPair.Value.vertices[i].x,
                        IDHexPair.Value.vertices[i].y,
                        IDHexPair.Value.vertices[i].z + sample.z - 0.5f
                    );
                    //Debug.Log(IDHexPair.Value.vertices[i]);
                }
                IDHexPair.Value.RefreshMesh();

            }
        }

    }
}