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
    /// 用于 生成六边形网格地图，已经为该类提供editor入口，不建议在游戏内运行生成的功能
    /// </summary>
    public class HexConstructor : MonoBehaviour {
        [Tooltip("注意：hexConstructor ")]
        [SerializeField] private bool ConstructorInStart = true;

        [SerializeField] private Transform originPoint;
        [SerializeField] private Vector2 Size;

        [Header("柏林噪声")]
        [SerializeField] Renderer perlinRenderer;

        PerlinGenerator perlinGenerator;

        [Header("作为prefab的父类物体")]
        [SerializeField] private Transform centerTransform;
        [SerializeField] private Transform pointTransform;
        [SerializeField] private Transform hexGridTransform;

        [Header("prefab")]
        [SerializeField] private GameObject centerPrefab;
        [SerializeField] private GameObject pointPrefab;
        [SerializeField] private GameObject hexGridPrefab;

        //当前的六边形 网格物体
        private List<Hexagon> CurrentHexagons;

        ////当前的 地理位置 - 省份 的映射
        public Dictionary<Vector3, HexGrid> currentPosHex = new Dictionary<Vector3, HexGrid>();
        public Dictionary<Vector3, HexGrid> CurrentPosHex { get => currentPosHex; private set => currentPosHex = value;}

        //当前的 ID - 省份 的映射
        public Dictionary<uint, HexGrid> currentIDHex = new Dictionary<uint, HexGrid>();
        public Dictionary<uint, HexGrid> CurrentIDHex { get => currentIDHex; private set => currentIDHex = value; }

        // 当前的河流
        [Header("河流")]
        public List<River> CurrentRiver;

        public List<HexGrid> RiverPathGenerator;


        public void UpdateCurrentHexProvince() {
            
            //根据现有的所有hexgrid，更新CurrentHexProvince
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

            //生成 Hex 六边形网格类 加入到集合中
            HexGenerator.GetInstance().ClearHexagon();
            HexGenerator.GetInstance().GenerateHexagon(10);
            //List<Hexagon> hexes = HexGenerator.GetInstance().GetHexagons();
            Dictionary<uint, Hexagon> hexeIDDic = HexGenerator.GetInstance().HexagonNumDic;

            Debug.Log("you have create " + hexeIDDic.Count.ToString() + " hex class");

            InitHexagonalGridScene(hexeIDDic);
        }

        public void InitHexagonalGridScene(Dictionary<uint, Hexagon> hexeIDDic) {
            // 不用在这记了
            //记录 当前生成的网格地图
            CurrentHexagons = hexeIDDic.Values.ToList();

            Layout layout = GetScreenLayout();

            //初始化地图网格物体
            foreach (KeyValuePair <uint, Hexagon> IDHexPair in hexeIDDic)
            {
                CreateHexagon(layout, IDHexPair.Key, IDHexPair.Value);
            }

            // 设置地图网格的邻居节点
            SetHexNeighbor(CurrentHexagons, CurrentPosHex);

            // 生成河流？

            // 生成 Mesh 网格
            RefreshHexagons();
        }

        private Layout GetScreenLayout() {
            //生成屏幕布局类
            Vector2 startPoint = new Vector2(originPoint.position.x, originPoint.position.y);
            Layout layout = new Layout(
                Orientation.Layout_Pointy, new Point(Size.x, Size.y), new Point(startPoint.x, startPoint.y)
            );
            return layout;
        }

        /// <summary>
        /// 根据 Hex 数据类 创建hex物体
        /// </summary>
        private void CreateHexagon(Layout layout, uint hexID, Hexagon hex) {
            // 创建并且初始化（基于mesh）hexgrid
            GameObject hexGridObject = Instantiate(hexGridPrefab, hexGridTransform);
            HexGrid hexGrid = hexGridObject.GetComponent<HexGrid>();
            hexGrid.InitHexGrid_Mesh(layout, hexID, hex);

            //加入到映射当中
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
                        // NOTICE: 一定要加null作为占位符，否则顺序会出错
                        CurrentPosHex[CurrentHexagons[i]].NeighbourGrids.Add(null);
                    }
                }
            }
        }

        public void RefreshHexagons() {
            //生成屏幕布局类
            Vector2 startPoint = new Vector2(originPoint.position.x, originPoint.position.y);
            Layout layout = new Layout(
                Orientation.Layout_Pointy, new Point(Size.x, Size.y), new Point(startPoint.x, startPoint.y)
            );

            // 生成 Mesh 网格(必须在设置邻居后调用)
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
        /// 生成柏林噪声纹理
        /// </summary>
        public void GeneratePerlinTexture(int width, int height, float scale) {
            if(perlinGenerator == null) {
                perlinGenerator = new PerlinGenerator();
            }
            perlinGenerator.GeneratePerlinNoise(10, 10, 1.0f);
            perlinGenerator.GenerateTexture(perlinRenderer);
            
        }

        /// <summary>
        /// 将柏林噪声扰动应用于地图网格上
        /// </summary>
        public void DisturbGridMap() {

            if (perlinGenerator == null) {
                Debug.Log("未生成perlinGenerator");
                return;
            }

            // 需要得到生成网格地图的 minPoint 和 maxPoint 才能插值
            // 进行扰动

            // TODO: 目前的扰动似乎有问题，扰动效果不是很明显
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