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
    /// ���� ���������������ͼ���Ѿ�Ϊ�����ṩeditor��ڣ�����������Ϸ���������ɵĹ���
    /// </summary>
    public class HexConstructor : MonoBehaviour {
        [Tooltip("ע�⣺hexConstructor ")]
        [SerializeField] private bool ConstructorInStart = true;

        [SerializeField] private Transform originPoint;
        [SerializeField] private Vector2 Size;

        [Header("��Ϊprefab�ĸ�������")]
        [SerializeField] private Transform centerTransform;
        [SerializeField] private Transform pointTransform;
        [SerializeField] private Transform hexGridTransform;

        [Header("prefab")]
        [SerializeField] private GameObject centerPrefab;
        [SerializeField] private GameObject pointPrefab;
        [SerializeField] private GameObject hexGridPrefab;

        //��ǰ�������� ��������
        private List<Hexagon> currentHexagons = new List<Hexagon>();
        public List<Hexagon> CurrentHexagons { get => currentHexagons; private set => currentHexagons = value; }
        ////��ǰ�� ����λ�� - ʡ�� ��ӳ��
        //private Dictionary<Vector3, HexGrid> currentPosHex = new Dictionary<Vector3, HexGrid>();
        //public Dictionary<Vector3, HexGrid> CurrentPosHex {
        //    get => currentPosHex;
        //    private set => currentPosHex = value;
        //}
        //��ǰ�� ID - ʡ�� ��ӳ��
        private Dictionary<uint, HexGrid> currentIDHex = new Dictionary<uint, HexGrid>();
        public Dictionary<uint, HexGrid> CurrentIDHex {
            get => currentIDHex;
            private set => currentIDHex = value;
        }


        public void UpdateCurrentHexProvince() {
            
            //�������е�����hexgrid������CurrentHexProvince
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

            //���� Hex ������������ ���뵽������
            HexGenerator.GetInstance().ClearHexagon();
            HexGenerator.GetInstance().GenerateHexagon(10);
            //List<Hexagon> hexes = HexGenerator.GetInstance().GetHexagons();
            Dictionary<uint, Hexagon> hexeIDDic = HexGenerator.GetInstance().HexagonNumDic;

            Debug.Log("you have create " + hexeIDDic.Count.ToString() + " hex class");

            InitHexagonalGridScene(hexeIDDic);
        }

        public void InitHexagonalGridScene(Dictionary<uint, Hexagon> hexeIDDic) {
            /* // �����������
             * //��¼ ��ǰ���ɵ������ͼ
            CurrentHexagons = hexes;*/

            //������Ļ������
            Vector2 startPoint = new Vector2(originPoint.position.x, originPoint.position.y);
            Layout layout = new Layout(
                Orientation.Layout_Pointy, new Point(Size.x, Size.y), new Point(startPoint.x, startPoint.y)
            );

            //��ʼ����������
            foreach (KeyValuePair <uint, Hexagon> IDHexPair in hexeIDDic)
            {
                CreateHexagon(layout, IDHexPair.Key, IDHexPair.Value);
            }
        }

        /// <summary>
        /// ���� Hex ������ ����hex����
        /// </summary>
        private void CreateHexagon(Layout layout, uint hexID, Hexagon hex) {
            //�������
            Point center = hex.Hex_To_Pixel(layout, hex);
            Vector3 centerPoint = (Vector3)center;
            //Debug.Log("����������꣺" + centerPoint);

            /*//NOTICE:�������� ������λ�� ��ɾ
            GameObject centerObject = Instantiate(centerPrefab, centerTransform);
            centerObject.transform.position = centerPoint;*/

            //��ö���
            List<Point> vertexs = hex.Polygon_Corners(layout, hex);
            //Debug.Log("�������ζ�����Ŀ��" + vertexs.Count);

            //���������ͼ��İ뾶 ����ͼ�� �����ø������
            float R = Vector3.Distance(centerPoint, (Vector3)vertexs[0]);
            GameObject hexGridObject = Instantiate(hexGridPrefab, hexGridTransform);
            hexGridObject.transform.position = centerPoint;
            HexGrid hexGrid = hexGridObject.GetComponent<HexGrid>();
            hexGrid.InitHexGird(R, hexID, hex);

            //���뵽ӳ�䵱��
            CurrentIDHex.Add(hexID, hexGrid);

            //Debug.Log("��ǰ�����εİ뾶��" + R.ToString());

            /*//NOTICE:�����Ǵ��������θ���������� ��ɾ
            foreach (Point point in vertexs) {
                //������������ ������λ��
                Vector3 vertexVector = (Vector3)point;
                //Debug.Log("����������꣺" + vertexVector);

                //�������� ������λ��
                GameObject pointObject = Instantiate(pointPrefab, pointTransform);
                pointObject.transform.position = vertexVector;
            }*/
        }

        /// <summary>
        /// ������Ѿ����ɵ�hex object
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