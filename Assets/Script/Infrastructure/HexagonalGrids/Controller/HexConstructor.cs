using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [Header("作为prefab的父类物体")]
        [SerializeField] private Transform centerTransform;
        [SerializeField] private Transform pointTransform;
        [SerializeField] private Transform hexGridTransform;

        [Header("prefab")]
        [SerializeField] private GameObject centerPrefab;
        [SerializeField] private GameObject pointPrefab;
        [SerializeField] private GameObject hexGridPrefab;

        //当前的六边形 网格物体
        private List<Hexagon> currentHexagons = new List<Hexagon>();
        public List<Hexagon> CurrentHexagons { get => currentHexagons; private set => currentHexagons = value; }
        ////当前的 地理位置 - 省份 的映射
        //private Dictionary<Vector3, HexGrid> currentPosHex = new Dictionary<Vector3, HexGrid>();
        //public Dictionary<Vector3, HexGrid> CurrentPosHex {
        //    get => currentPosHex;
        //    private set => currentPosHex = value;
        //}
        //当前的 ID - 省份 的映射
        private Dictionary<uint, HexGrid> currentIDHex = new Dictionary<uint, HexGrid>();
        public Dictionary<uint, HexGrid> CurrentIDHex {
            get => currentIDHex;
            private set => currentIDHex = value;
        }


        public void UpdateCurrentHexProvince() {
            
            //根据现有的所有hexgrid，更新CurrentHexProvince
            HexGrid[] hexGrids = hexGridTransform.GetComponentsInChildren<HexGrid>();
            foreach (HexGrid hexGrid in hexGrids)
            {
                //if (CurrentPosHex.ContainsKey(hexGrid.hexPosition)) {
                //    CurrentPosHex[hexGrid.hexPosition] = hexGrid;
                //} else {
                //    CurrentPosHex.Add(hexGrid.hexPosition, hexGrid);
                //}
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
            /* // 不用在这记了
             * //记录 当前生成的网格地图
            CurrentHexagons = hexes;*/

            //生成屏幕布局类
            Vector2 startPoint = new Vector2(originPoint.position.x, originPoint.position.y);
            Layout layout = new Layout(
                Orientation.Layout_Pointy, new Point(Size.x, Size.y), new Point(startPoint.x, startPoint.y)
            );

            //初始化网格物体
            foreach (KeyValuePair <uint, Hexagon> IDHexPair in hexeIDDic)
            {
                CreateHexagon(layout, IDHexPair.Key, IDHexPair.Value);
            }
        }

        /// <summary>
        /// 根据 Hex 数据类 创建hex物体
        /// </summary>
        private void CreateHexagon(Layout layout, uint hexID, Hexagon hex) {
            //获得中心
            Point center = hex.Hex_To_Pixel(layout, hex);
            Vector3 centerPoint = (Vector3)center;
            //Debug.Log("输出中心坐标：" + centerPoint);

            /*//NOTICE:创建中心 并设置位置 勿删
            GameObject centerObject = Instantiate(centerPrefab, centerTransform);
            centerObject.transform.position = centerPoint;*/

            //获得顶点
            List<Point> vertexs = hex.Polygon_Corners(layout, hex);
            //Debug.Log("该六边形顶点数目：" + vertexs.Count);

            //获得六边形图块的半径 创建图块 并设置各项参数
            float R = Vector3.Distance(centerPoint, (Vector3)vertexs[0]);
            GameObject hexGridObject = Instantiate(hexGridPrefab, hexGridTransform);
            hexGridObject.transform.position = centerPoint;
            HexGrid hexGrid = hexGridObject.GetComponent<HexGrid>();
            hexGrid.InitHexGird(R, hexID, hex);

            //加入到映射当中
            CurrentIDHex.Add(hexID, hexGrid);

            //Debug.Log("当前六边形的半径：" + R.ToString());

            /*//NOTICE:以下是创建六边形各个顶点代码 勿删
            foreach (Point point in vertexs) {
                //创建顶点物体 并设置位置
                Vector3 vertexVector = (Vector3)point;
                //Debug.Log("输出顶点坐标：" + vertexVector);

                //创建中心 并设置位置
                GameObject pointObject = Instantiate(pointPrefab, pointTransform);
                pointObject.transform.position = vertexVector;
            }*/
        }

        /// <summary>
        /// 清楚掉已经生成的hex object
        /// </summary>
        public void ClearHexObject() {
            centerTransform.ClearObjChildren();
            pointTransform.ClearObjChildren();
            hexGridTransform.ClearObjChildren();
            //CurrentPosHex.Clear();
            CurrentIDHex.Clear();
        }

    }
}