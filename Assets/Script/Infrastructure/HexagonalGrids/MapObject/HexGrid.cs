
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.HexagonGrid.DataStruct;
using WarGame_True.Infrastructure.Map.Provinces;
using static Unity.Burst.Intrinsics.X86;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace WarGame_True.Infrastructure.HexagonGrid.MapObject {
    /// <summary>
    /// HexGrid ��ͼ�����UI�����࣬ ��֧��x yƽ��
    /// </summary>
    public class HexGrid : MonoBehaviour {

        [Header("����")]
        public MeshFilter meshFilter;
        public Mesh hexMesh;
        
        public Color GridColor;
        public float GridHigh;

        public List<Vector3> vertices;
        public List<int> triangles;
        public List<UnityEngine.Color> colors;
        public List<Vector3> terrainTypes;

        // innerRatio ���ڲ��������εİ뾶��ռ����
        float innerRatio = 0.7f;

        #region �������
        [Header("����")]
        public bool hasRiverIn = false;

        public HexDirection RiverInDir;

        public bool hasRiverOut = false;

        public HexDirection RiverOutDir;

        public void SetRiverInPath(HexGrid pre) {
            if (pre == null) {
                return;
            }

            int inDir = GetNeighborIndex(pre);

            if (inDir >= 0) {
                hasRiverIn = true;
                RiverInDir = (HexDirection)Mathf.Min(inDir, 5);
            }
        }

        public void SetRiverOutPath(HexGrid next) {
            if (next == null) {
                return;
            }

            int outDir = GetNeighborIndex(next);

            if (outDir >= 0) {
                hasRiverOut = false;
                RiverOutDir = (HexDirection)Mathf.Min(outDir, 5);
            }
        }

        public bool HasUncompletedRiver() {
            return (hasRiverIn && !hasRiverOut) || (!hasRiverIn && hasRiverOut);
        }

        public bool HasCompletedRiver() {
            return hasRiverIn && hasRiverOut;
        }

        #endregion

        [Header("�ھ�")]
        public List<HexGrid> NeighbourGrids = new List<HexGrid>();  // idx���ھӷ�λ��Ӧ

        public int GetNeighborIndex(HexGrid hexGrid) {
            for (int i = 0; i < NeighbourGrids.Count; i ++) {
                if (NeighbourGrids[i] == hexGrid) {
                    return i;
                }
            }
            return -1;
        }

        public HexGrid GetNeighbor(int idx) {
            if (idx < 0) {
                idx = 0;
            }
            if (idx >= NeighbourGrids.Count) {
                return null;
            }
            return NeighbourGrids[idx];
        }

        public HexGrid GetNeighbor(HexDirection direction) {
            int idx = (int)direction;
            return GetNeighbor(idx);
        }


        [Header("------------�ֽ���-------------")]
        [Header("����ID")]
        public uint HexID;

        public Vector3 hexPosition;
        private Vector3Int hexIntPosition = Vector3Int.zero;
        public Vector3Int HexIntPosition {
            get {
                hexIntPosition.x = Mathf.RoundToInt(hexPosition.x);
                hexIntPosition.y = Mathf.RoundToInt(hexPosition.y);
                hexIntPosition.z = Mathf.RoundToInt(hexPosition.z);
                return hexIntPosition;
            }
        }

        public Hexagon hexagon;

        [SerializeField] SpriteRenderer BGSpriteRenderer;           // ��ͼ�������ײ�
        [SerializeField] SpriteRenderer interactSpriteBaseRenderer; // ���ֽ������ܣ�����ʾ�����ߣ�
        [SerializeField] SpriteRenderer interactSpriteRenderer;     // ���ֽ������ܣ����ƶ���
        [SerializeField] SpriteRenderer occupySpriteRenderer;       // ��ʾռ��ĵ�ͼ����
        [SerializeField] SpriteRenderer mouseInteractSpriteRenderer;// �������Ч��

        [Header("��ͬ״̬�µ�����ͼƬ")]
        [SerializeField] Sprite normalGridSprite;
        [SerializeField] Sprite mouseOnGridSprite;
        [SerializeField] Sprite choosenGridSprite;
        [SerializeField] Sprite movingGridSprite;
        [SerializeField] Sprite withdrawingGridSprite;
        [SerializeField] Sprite supplyLineGridSprite;

        [Header("����λ��")]
        [SerializeField] GameObject CanvasPrefab;
        [SerializeField] GameObject SphereObject;

        [Header("����ui")]
        public HexCanvas hexCanvas;


        #region HexGrid ��ʼ��
        public void InitHexGird(float HexR, uint hexID, Hexagon coordinate) {
            HexID = hexID;
            hexPosition = coordinate;
            //��Ӧ ������ �Ĵ�С,�޸Ŀ��
            float xSize = HexR * Mathf.Sqrt(3);
            float ySize = HexR * 2;

            InitHexGrid(xSize, ySize, coordinate);
        }

        public void InitHexGrid_Mesh(Layout layout, uint hexID, Hexagon hex) {
            HexID = hexID;
            hexPosition = hex;
            hexagon = hex;

            // ͨ�� Mesh ����ʼ������
            hexMesh = new Mesh();
            meshFilter = GetComponentInChildren<MeshFilter>();
            if (meshFilter != null) meshFilter.mesh = hexMesh;
            hexMesh.name = "Hex Mesh";

            vertices = new List<Vector3>();
            triangles = new List<int>();
            colors = new List<Color>();
            terrainTypes = new List<Vector3>();
            //ListPool<Vector3>.Add(); // TODO: unity�ٷ��ĳػ�����

            // ���Hex������
            Point center = hex.Hex_To_Pixel(layout, hex);
            Vector3 centerPoint = (Vector3)center;
            transform.position = centerPoint;

            //��ö��㣬������������
            List<Point> vertexs = hex.Polygon_Corners(layout, hex);
            //foreach (Vector3 vertex in vertexs) {
            //    GameObject sphere = Instantiate(SphereObject, transform.parent);
            //    sphere.transform.localPosition = vertex;
            //}

            //���������ͼ��İ뾶 ����ͼ�� �����ø������
            float R = Vector3.Distance(centerPoint, (Vector3)vertexs[0]);

            // ������ɫ
            GridColor = GetGridColor_Random();
            // ���ø߶�
            GridHigh = GetGridHigh_Random();
            // TODO: ���ú���

            // ����mesh
            // NOTE: ��ΪҪ�����ɫ������Ҫ��������hex�ھ�֮���ٵ��ã��ʷŵ�hexConstructor��
            //DrawHexGridMesh(layout, hex);

            // TODO: ����R����� ��С��
            // NOTE: ʹ��scale�ᵼ�������С�쳣
            float xSize = R * Mathf.Sqrt(3);
            float ySize = R * 2;
            InitHexGrid(xSize, ySize, centerPoint);

        }

        private void InitHexGrid(float xSize, float ySize, Vector3 coordinate) {

            //ȷ����x-yƽ����
            transform.rotation = Quaternion.identity;

            Vector2 spriteSize = BGSpriteRenderer.sprite.bounds.size;
            Vector3 currentScale = transform.localScale;
            uint hexID = (uint)HexID;

            float xScale = xSize / spriteSize.x * currentScale.x;
            float yScale = ySize / spriteSize.y * currentScale.y;

            transform.localScale = new Vector3(xScale, yScale, 0);
            // TODO: ��Ҫ��mesh�Ĵ�С��Ӱ��

            // ��ʼ�� ʡ�ݸ���ui
            hexCanvas = Instantiate(CanvasPrefab, transform.parent).GetComponent<HexCanvas>();
            // ��������ʾ�Լ���ID
            hexCanvas.InitHexCanvas(xSize, ySize, hexID, transform.position);
            // ��������,��������ʾ�Լ���λ��
            //hexCanvas.InitHexCanvas(xSize, ySize, coordinate, transform.position);

        }

        #endregion

        public void SetCenterText(string centerText, bool setHexID = false) {
            if (setHexID) {
                // setHexIDΪtrueʱ�����������ı�Ϊ HexID����������centerText
                hexCanvas.SetCenterText(HexID.ToString());
            } else {
                hexCanvas.SetCenterText(centerText);
            }
        }

        #region ���� ״̬�ı�
        public void SetHexGridActive() {
            mouseInteractSpriteRenderer.sprite = mouseOnGridSprite;
        }

        public void SetHexGridChoose() {
            mouseInteractSpriteRenderer.sprite = choosenGridSprite;
        }

        public void SetHexGridNormal() {
            mouseInteractSpriteRenderer.sprite = normalGridSprite;
        }

        public void SetHexGridAsMovePath() {
            //Debug.Log("you have set it!");
            interactSpriteRenderer.sprite = movingGridSprite;
        }

        public void SetHexGridAsWithdrawPath() {
            //Debug.Log("you have set it!");
            interactSpriteRenderer.sprite = withdrawingGridSprite;
        }

        public void SetHexGridNoInteract() {
            interactSpriteRenderer.sprite = normalGridSprite;
        }

        public void SetHexGridOccupied(Color controllerColor) {
            controllerColor.a = 0.6f;
            occupySpriteRenderer.color = controllerColor;
            occupySpriteRenderer.gameObject.SetActive(true);
        }

        public void SetHexGridUnoccupied() {
            occupySpriteRenderer.gameObject.SetActive(false);
        }

        /// <summary>
        /// ��������ĵ�ɫ�����ڱ�ʾ���ι�������ε�������Ϣ
        /// </summary>
        /// <param name="girdBGColor">Ҫ���õ���ɫ</param>
        /// <param name="activeBG">�رյ�ɫ</param>
        public void SetHexGridBG(Color girdBGColor, bool activeBG = true) {
            BGSpriteRenderer.gameObject.SetActive(activeBG);
            BGSpriteRenderer.color = girdBGColor;
        }

        public void SetHexGridSupplyLine() {
            interactSpriteBaseRenderer.sprite = supplyLineGridSprite;
        }

        public void SetHexGridNoInterBase() {
            // NOTICE: interactSpriteBaseRenderer ���� interactSprite �� ��һ��
            interactSpriteBaseRenderer.sprite = normalGridSprite;
        }

        #endregion

        #region ʡ��ui��ʾ

        public void ShowProvinceName(string name) {
            if (hexCanvas == null) {
                Debug.LogError("û�����ӵ�hexcanvas");
                return;
            }
            hexCanvas.ShowProvinceName(name);
        }

        public void ShowOccupyProcess(int maxDay, int currentProcess) {
            //Debug.Log(maxDay + "_" + currentProcess);
            //Debug.Log(hexCanvas == null);
            hexCanvas.ShowOccupyProcess(maxDay, currentProcess);
        }

        public void HideOccupyProcess() {
            hexCanvas.HideOccupyProcess();
        }

        public void ShowRecruitProcess(uint maxProcess, uint lastProcess, string curTaskNum) {
            int curProcess = (int)maxProcess - (int)lastProcess;
            hexCanvas.ShowRecruitProcess((int)maxProcess, curProcess, curTaskNum);
        }

        public void HideRecruitProcess() {
            hexCanvas.HideRecruitProcess();
        }

        public void ShowBuildProcess(uint maxProcess, uint lastProcess, string curTaskNum) {
            int curProcess = (int)maxProcess - (int)lastProcess;
            hexCanvas.ShowBuildProcess((int)maxProcess, curProcess, curTaskNum);
        }

        public void HideBuildProcess() {
            hexCanvas.HideBuildProcess();
        }

        #endregion

        #region ����Mesh ����Hex�ر�

        private bool ShouldDivideStair(float highGap) {
            return highGap <= 3 && highGap > 0;
        }

        public void RefreshMesh() {
            hexMesh.vertices = vertices.ToArray();
            hexMesh.triangles = triangles.ToArray();
            hexMesh.RecalculateNormals();
            //hexMesh.SetColors(colors);
            hexMesh.SetUVs(1, terrainTypes);
        }

        public void DrawHexGridMesh(Layout layout, Hexagon hex) {
            hexMesh.Clear();
            vertices.Clear();
            colors.Clear();
            triangles.Clear();

            // ��ã��ڲ��������ε���������
            // NOTICE: Ҫ��GameObject�������趨 Mesh �����ĺͶ��㣬������Ƶ��������ƫ��
            Vector3 centerPoint = new Vector3(0, 0, GridHigh);

            List<Point> innerVertexs = new List<Point>();
            List<Point> outterVertexs = new List<Point>();
            // �����α��ϵĵ㣬��һ��Tupleֵ���� �� �ڶ����ھ� ����ı��ϵ�������
            List<Tuple<Point, Point>> edgeVertexs = new List<Tuple<Point, Point>>();
            for (int i = 0; i < 6; i++) {
                int next = i + 1 >= 6 ? 0 : i + 1;
                int next2 = next + 1 >= 6 ? 0 : next + 1;

                Point offset = hex.Hex_Corner_Offset(layout, i);
                Point nextOffset = hex.Hex_Corner_Offset(layout, next);

                // ��������ھӵĸ�
                float high_i = GetNeighbor(i) == null ? 0 : GetNeighbor(i).GridHigh;
                float high_next = GetNeighbor(next) == null ? 0 : GetNeighbor(next).GridHigh;
                float high_next2 = GetNeighbor(next2) == null ? 0 : GetNeighbor(next2).GridHigh;

                // ���λ�ȡ �ڲ����㡢�ⲿ���㡢��һ���ⶥ��
                Point innerVertex = new Point(offset.x * innerRatio, offset.y * innerRatio, GridHigh);
                Point outterVertex = new Point(offset.x, offset.y, (GridHigh + high_i + high_next) / 3);
                Point nextOutterVertex = new Point(nextOffset.x, nextOffset.y, (GridHigh + high_i + high_next2) / 3);

                // �� ��ǰ����ھ� �ཻ�ı��ϵĵ㣬�����õ�ĸ߶�Ϊ ������ �� �ھ� ��ƽ��
                Point edgeVertex_Mid = (outterVertex + nextOutterVertex) / 2;
                //* (1 - innerRatio)
                Point edgeVertex_Low = outterVertex + (edgeVertex_Mid - outterVertex) * (1 - innerRatio);
                edgeVertex_Low.z = (high_next + GridHigh) / 2;
                // * (innerRatio)
                Point edgeVertex_High = edgeVertex_Mid + (nextOutterVertex - edgeVertex_Mid) * innerRatio;
                edgeVertex_High.z = (high_next + GridHigh) / 2;


                innerVertexs.Add(innerVertex);
                outterVertexs.Add(outterVertex);
                edgeVertexs.Add(new Tuple<Point, Point>(edgeVertex_Low, edgeVertex_High));
            }

            // ���� ������ ����
            for (int i = 0; i < 6; i++) {
                int j = i + 1 >= 6 ? 0 : i + 1;
                int k = j + 1 >= 6 ? 0 : j + 1;

                // ��ɾ: 
                // ���� �������� �� �������Σ��� 6 ����
                // NOTICE: ע�ⷨ�������������ζ��㴫���˳��
                //AddTriangle(centerPoint, innerVertexs[i], innerVertexs[j]);
                //AddTriangleColor(GridColor, GridColor, GridColor);
                //Vector3 types;
                //types.x = types.y = types.z = 1;
                //AddTriangleTerrainTypes(types);
                //AddTriangleTerrainTypes(types);
                //AddTriangleTerrainTypes(types);
                //AddTriangleTerrainTypes(types);

                // ϸ����Ŀ �� 3
                int lerpNum = 3;

                // ���� �������� �� �������Σ��� 6 ����
                // ÿ���������� Ҫϸ��Ϊ3����������
                DrawSubdivisionTriangle(
                    centerPoint, innerVertexs[j], innerVertexs[i], 
                    GridColor, GridColor, GridColor,
                    lerpNum
                );

                // �����ھӵ���ɫ j - �����ⶥ�㹲�е��ھ�; i - ��һ���ⶥ��Ķ����ھ�; k - ~
                float highNeigh_i = GetNeighbor(i) == null ? GridHigh : GetNeighbor(i).GridHigh;
                float highNeigh_j = GetNeighbor(j) == null ? GridHigh : GetNeighbor(j).GridHigh;
                float highNeigh_k = GetNeighbor(k) == null ? GridHigh : GetNeighbor(k).GridHigh;
                Color color_i = GetNeighbor(i) == null ? Color.white : GetNeighbor(i).GridColor;
                Color color_j = GetNeighbor(j) == null ? Color.white : GetNeighbor(j).GridColor;
                Color color_k = GetNeighbor(k) == null ? Color.white : GetNeighbor(k).GridColor;

                // ����Χ�����ھӵĸ߶Ȳ�
                float highGap_i = Mathf.Abs(highNeigh_i - GridHigh);
                float highGap_j = Mathf.Abs(highNeigh_j - GridHigh);
                float highGap_k = Mathf.Abs(highNeigh_k - GridHigh);

                Color outterVertext_i_color = (GridColor + color_i + color_j) / 3;
                Color outterVertext_j_color = (GridColor + color_j + color_k) / 3;
                Color edgeVertex_color = (GridColor + color_j) / 2;

                if (ShouldDivideStair(highGap_j)) {
                    // �߶Ȳ���һ����Χ�ڣ��ֳ�����
                    DrawStairsQuad(color_j, edgeVertex_color, 
                        edgeVertexs[i].Item1, edgeVertexs[i].Item2, innerVertexs[i], innerVertexs[j]);

                    // ����ʽ���Ե������������
                    DrawStairsTriangle_Left(edgeVertex_color, outterVertext_i_color,
                        innerVertexs[i], outterVertexs[i], edgeVertexs[i].Item1);

                    DrawStairsTriangle_Right(edgeVertex_color, outterVertext_j_color,
                        innerVertexs[j], outterVertexs[j], edgeVertexs[i].Item2);

                }
                else {
                    // ���ֳ�����

                    // �뵥���ھӵı�Ե�ı���
                    // Item1 - edgePoint_Low; Item2 - edgePoint_High
                    //AddQuad(edgeVertexs[i].Item1, edgeVertexs[i].Item2, innerVertexs[j], innerVertexs[i]);
                    //AddQuadColor(
                    //    edgeVertex_color,
                    //    edgeVertex_color,
                    //     GridColor, GridColor
                    //);

                    DrawSubdivisionQuad(
                        edgeVertexs[i].Item1, edgeVertexs[i].Item2, 
                        innerVertexs[j], innerVertexs[i],
                        edgeVertex_color,
                        edgeVertex_color,
                         GridColor, GridColor,
                         3
                    );

                    // �������ھӵı�Ե�����Σ���ʼ�����������Σ�
                    if (ShouldDivideStair(highGap_i)) {
                        // �������Ҫ�������ô���������ҲҪ�γ�����
                        DrawStairFlatTriangle_Left(edgeVertex_color, outterVertext_i_color,
                            innerVertexs[i], outterVertexs[i], edgeVertexs[i].Item1);

                    } else {
                        // ������ ֱ�����ɼ���
                        AddTriangle(innerVertexs[i], outterVertexs[i], edgeVertexs[i].Item1);
                        AddTriangleColor(
                            GridColor,
                            outterVertext_i_color,
                            edgeVertex_color
                        );
                    }

                    if (ShouldDivideStair(highGap_k)) {

                        DrawStairFlatTriangle_Right(edgeVertex_color, outterVertext_j_color, 
                            innerVertexs[j], outterVertexs[j], edgeVertexs[i].Item2);

                    } else {
                        AddTriangle(innerVertexs[j], edgeVertexs[i].Item2, outterVertexs[j]);
                        AddTriangleColor(
                            GridColor,
                            edgeVertex_color,
                            outterVertext_j_color
                        );
                    }
                }

            }

            hexMesh.vertices = vertices.ToArray();
            hexMesh.triangles = triangles.ToArray();
            //hexMesh.colors = colors.ToArray();

            // ��ɾ�˶�
            //int fistNeighborID = GetNeighbor(0) == null ? -1 : (int)GetNeighbor(0).HexID;
            //Debug.Log(HexID + ", Hex:" + hexMesh.vertices[1] + transform.position + ", first neighbor is:" + fistNeighborID);

            hexMesh.RecalculateNormals();
            
            // ʹ����ɫ��������/ʹ�������������� 
            //hexMesh.SetColors(colors);

            hexMesh.SetUVs(1, terrainTypes);
        }


        private Point[] GetDividePoints(Point startPoint, Point endPoint, int lerpNum) {
            // �����м���
            Point space = (endPoint - startPoint) / 3;

            Point[] points = new Point[lerpNum + 1];
            points[0] = startPoint;
            points[lerpNum] = endPoint;
            for (int i = 1; i < lerpNum; i ++) {
                // ��ֵ���� �����м� �Ķ���
                points[i] = startPoint + space * i;
            }
            return points;
        }

        private Color[] GetDivideColors(Color startColor, Color endColor, int lerpNum) {
            Color[] ans = new Color[lerpNum + 1];
            ans[0] = startColor;
            ans[lerpNum] = endColor;

            float lerp = 1 / lerpNum;
            float step = lerp;
            for(int i = 1; i < lerpNum; i++) {
                // ������ɫ��ֵ
                ans[i] = Color.Lerp(startColor, endColor, lerp * i);
            }
            return ans;
        }

        /// <summary>
        /// ϸ�� ָ����������, A ��ϸ�ֳ�������, ע�� A B C �����˳��
        /// </summary>
        private void DrawSubdivisionTriangle(Vector3 A, Point B, Point C,
            Color a_color, Color b_color, Color c_color, int lerpNum) {
            Point[] edgePoints = GetDividePoints(B, C, lerpNum);
            Color[] color_BC = GetDivideColors(b_color, c_color, lerpNum);
            for (int i = 1; i < edgePoints.Length; i++) {
                AddTriangle(A, edgePoints[i], edgePoints[i - 1]);
                AddTriangleColor(a_color, color_BC[i], color_BC[i - 1]);

                //Vector3 types;
                //types.x = types.y = types.z = 1;
                //AddTriangleTerrainTypes(types);
                //AddTriangleTerrainTypes(types);
                //AddTriangleTerrainTypes(types);
                //AddTriangleTerrainTypes(types);
            }
        }

        /// <summary>
        /// ϸ��ָ�����ı��Σ�ϸ�ַ���ΪA -> B
        /// </summary>
        private void DrawSubdivisionQuad(Point A, Point B, Point C, Point D, 
            Color a_color, Color b_color, Color c_color, Color d_color, int lerpNum) {
            Point[] slerp_AB = GetDividePoints(A, B, lerpNum);
            Point[] slerp_DC = GetDividePoints(D, C, lerpNum);
            Color[] color_AB = GetDivideColors(a_color, b_color, lerpNum);
            Color[] color_DC = GetDivideColors(d_color, c_color, lerpNum);

            for (int i = 1; i < slerp_AB.Length; i++) {
                AddQuad(slerp_DC[i - 1], slerp_AB[i - 1], slerp_AB[i], slerp_DC[i]);
                AddQuadColor( color_DC[i - 1], color_AB[i - 1], color_AB[i], color_DC[i]);
            }
        }


        // NOTICE: 0.4f, 3 �ǹ��õĲ�ֵ����
        /// <summary>
        /// ������β�ֵ���ֺ� �������м��
        /// </summary>
        /// <param name="startPoint">���</param>
        /// <param name="endPoint">�յ�</param>
        private Point[] GetDivideStairPoints(Point startPoint, Point endPoint, float divideRatio, int lerpNum) {
            // �ֳ�������εĲ�ֵ�����ǲ�һ���ģ�
            // int lerpNum = 3;
            // 0.4f, 0.4f, 0.2f
            Point[] points = new Point[lerpNum + 1];

            double mid_z_i = (startPoint.z + endPoint.z) * 0.5f;

            // ��� divideRatio = 0.4f
            Point space_i = (endPoint - startPoint) * divideRatio;

            points[0] = startPoint;
            points[1] = startPoint + space_i;
            points[1].z = mid_z_i;
            points[2] = startPoint + space_i * 2;
            points[2].z = mid_z_i;
            points[3] = endPoint;

            return points;
        }

        /// <summary>
        /// ��������ṹ�ı�Ե - �����ĸ�����Ķ��㣨ע��˳��
        /// </summary>
        /// <param name="color_Neighbor">�ھ��������ɫ�����ڲ�ֵ</param>
        /// <param name="edgeVertex_color">��ߵ���ɫ�����ڲ�ֵ</param>
        /// <param name="edgeVertex_Low">�����ߵĶ���</param>
        /// <param name="edgeVertex_High">����ұߵĶ���</param>
        /// <param name="innerVertex_Low">�ڱ���ߵĶ���</param>
        /// <param name="innerVertex_High">�ڱ��ұߵĶ���</param>
        private void DrawStairsQuad(Color color_Neighbor, Color edgeVertex_color, 
            Point edgeVertex_Low, Point edgeVertex_High, Point innerVertex_Low, Point innerVertex_High) {

            // 0.4f   3.0f
            List<Point> slerpPoints_inner_Low = GetDivideStairPoints(innerVertex_Low, edgeVertex_Low, 0.4f, 3).ToList();
            List<Point> slerpPoints_inner_High = GetDivideStairPoints(innerVertex_High, edgeVertex_High, 0.4f, 3).ToList();

            // TODO: Ҳ����Բ��ѭ����
            //float step = 0.2f;
            //float curRatio = -0.2f;
            float step1 = 0.2f;
            float step2 = 0;
            for (int i = 1; i < slerpPoints_inner_Low.Count; i++) {

                //AddQuad(slerpPoints_inner_Low[i], slerpPoints_inner_High[i], slerpPoints_inner_High[i - 1], slerpPoints_inner_Low[i - 1]);
                //// ERROR: ��ɫ�����⣡������������ֵ����
                //// TODO: �ο������ԭ������ɫ��ֵ�����߼� ����д�£�ע���ֵ
                //AddQuadColor(
                //    GridColor * (1 - step1) + color_Neighbor * step1,
                //    GridColor * (1 - step1) + color_Neighbor * step1,
                //    GridColor * (1 - step2) + color_Neighbor * step2,
                //    GridColor * (1 - step2) + color_Neighbor * step2
                //);

                DrawSubdivisionQuad(
                    slerpPoints_inner_Low[i], slerpPoints_inner_High[i], 
                    slerpPoints_inner_High[i - 1], slerpPoints_inner_Low[i - 1],
                    GridColor * (1 - step1) + color_Neighbor * step1,
                    GridColor * (1 - step1) + color_Neighbor * step1,
                    GridColor * (1 - step2) + color_Neighbor * step2,
                    GridColor * (1 - step2) + color_Neighbor * step2,
                    3
                );

                // ���ܳ�����ֵ��Χ
                step1 += 0.2f;
                step1 = Mathf.Min(step1, 1.0f);
                step1 = Mathf.Max(step1, 0);
                step2 += 0.2f;
                step2 = Mathf.Min(step2, 1.0f);
                step2 = Mathf.Max(step2, 0);
            }


            // �Ȳ�Ҫɾ������
            // ��������ṹ
            // j : �ǵ�ǰ���� �ڶ��� i , j �����ŵ� ����������
            /*AddQuad(slerpPoints_inner_Low[0], slerpPoints_inner_High[0], innerVertex_High, innerVertex_Low);
            AddQuadColor(
                GridColor * 4 / 5 + color_Neighbor * 1 / 5,
                GridColor * 4 / 5 + color_Neighbor * 1 / 5,
                GridColor * 5 / 5 + color_Neighbor * 0,
                GridColor * 5 / 5 + color_Neighbor * 0
            );

            AddQuad(slerpPoints_inner_Low[1], slerpPoints_inner_High[1], slerpPoints_inner_High[0], slerpPoints_inner_Low[0]);
            AddQuadColor(
                GridColor * 3 / 5 + color_Neighbor * 2 / 5,
                GridColor * 3 / 5 + color_Neighbor * 2 / 5,
                GridColor * 4 / 5 + color_Neighbor * 1 / 5,
                GridColor * 4 / 5 + color_Neighbor * 1 / 5
            );

            // edgeVertex_color = (GridColor + color_Neighbor) / 2
            AddQuad(edgeVertex_Low, edgeVertex_High, slerpPoints_inner_High[1], slerpPoints_inner_Low[1]);
            AddQuadColor(
                GridColor * 0 + edgeVertex_color * 5 / 5,
                GridColor * 0 + edgeVertex_color * 5 / 5,
                GridColor * 3 / 5 + color_Neighbor * 2 / 5,
                GridColor * 3 / 5 + color_Neighbor * 2 / 5
            );*/
        }

        // TODO: �������ɴ˴��Ĵ��룬����̫����
        /// <summary>
        /// ���������Աߵģ���ߣ�С�����Σ�Ҳ������ṹ�� - ������������
        /// </summary>
        private void DrawStairsTriangle_Left(Color edgeVertex_color, Color outterVertext_color,
           Point innerVertex, Point outterVertex, Point edgeVertex) {

            /*Point[] slerpPoints_inner_Edge = GetDivideStairPoints(innerVertex, edgeVertex, 0.4f, 3);
            // �������νṹ��������
            float step = 0.4f;
            float curRatio = 0;
            float nextRatio = 0;
            for (int i = 1; i < slerpPoints_inner_Edge.Length; i++) {
                curRatio += step;
                curRatio = Mathf.Min(1.0f, curRatio);
                nextRatio += step;
                nextRatio = Mathf.Min(1.0f, nextRatio);
                // ���� ������
                AddTriangle(slerpPoints_inner_Edge[i-1], outterVertex, slerpPoints_inner_Edge[i]);
                AddTriangleColor(
                    edgeVertex_color * curRatio + GridColor * (1 - curRatio),
                    outterVertext_color,
                    edgeVertex_color * nextRatio + GridColor * (1 - nextRatio)
                );
            }*/

            List<Point> slerpPoints_inner_outter = GetDivideStairPoints(innerVertex, outterVertex, 0.4f, 3).ToList();
            List<Point> slerpPoints_inner_Edge = GetDivideStairPoints(innerVertex, edgeVertex, 0.4f, 3).ToList();

            // ���νṹ��������
            AddTriangle(slerpPoints_inner_outter[1], slerpPoints_inner_Edge[1], innerVertex);
            AddTriangleColor(
                outterVertext_color * 2 / 5 + GridColor * 3 / 5,
                edgeVertex_color * 2 / 5 + GridColor * 3 / 5,
                GridColor
            );

            AddQuad(slerpPoints_inner_outter[2], slerpPoints_inner_Edge[2], slerpPoints_inner_Edge[1], slerpPoints_inner_outter[1]);
            AddQuadColor(
                outterVertext_color * 4 / 5 + GridColor * 1 / 5,
                edgeVertex_color * 4 / 5 + GridColor * 1 / 5,
                edgeVertex_color * 2 / 5 + GridColor * 3 / 5,
                outterVertext_color * 2 / 5 + GridColor * 3 / 5
            );
            AddQuad(outterVertex, edgeVertex, slerpPoints_inner_Edge[2], slerpPoints_inner_outter[2]);
            AddQuadColor(
                outterVertext_color,
                edgeVertex_color,
                edgeVertex_color * 4 / 5 + GridColor * 1 / 5,
                outterVertext_color * 4 / 5 + GridColor * 1 / 5
            );
        }

        /// <summary>
        /// ���������Աߵģ��ұߣ�С�����Σ�Ҳ������ṹ�������߷�����������Ҫע��
        /// </summary>
        private void DrawStairsTriangle_Right(Color edgeVertex_color, Color outterVertext_color,
            Point innerVertex, Point outterVertex, Point edgeVertex) {

            // , 0.4f, 3
            //Point[] slerpPoints_inner_Edge = GetDivideStairPoints(innerVertex, edgeVertex, 0.4f, 3);
            /*// �������νṹ��������
            float step = 0.4f;
            float curRatio = 0;
            float nextRatio = 0;
            for (int i = 1; i < slerpPoints_inner_Edge.Length; i++) {
                curRatio += step;
                curRatio = Mathf.Min(1.0f, curRatio);
                nextRatio += step;
                nextRatio = Mathf.Min(1.0f, nextRatio);
                // ���� ���� ��ߵ�������
                AddTriangle(slerpPoints_inner_Edge[i - 1], slerpPoints_inner_Edge[i], outterVertex);
                AddTriangleColor(
                    edgeVertex_color * curRatio + GridColor * (1 - curRatio),
                    edgeVertex_color * nextRatio + GridColor * (1 - nextRatio),
                    outterVertext_color
                );
            }*/

            List<Point> slerpPoints_inner_outter = GetDivideStairPoints(innerVertex, outterVertex, 0.4f, 3).ToList();
            List<Point> slerpPoints_inner_Edge = GetDivideStairPoints(innerVertex, edgeVertex, 0.4f, 3).ToList();

            // ��С������ 
            AddTriangle(slerpPoints_inner_Edge[1], slerpPoints_inner_outter[1], innerVertex);
            AddTriangleColor(
                edgeVertex_color * 2 / 5 + GridColor * 3 / 5,
                outterVertext_color * 2 / 5 + GridColor * 3 / 5,
                GridColor
            );

            AddQuad(slerpPoints_inner_Edge[2], slerpPoints_inner_outter[2], slerpPoints_inner_outter[1], slerpPoints_inner_Edge[1]);
            AddQuadColor(
                edgeVertex_color * 4 / 5 + GridColor * 1 / 5,
                outterVertext_color * 4 / 5 + GridColor * 1 / 5,
                outterVertext_color * 2 / 5 + GridColor * 3 / 5,
                edgeVertex_color * 2 / 5 + GridColor * 3 / 5
            );

            AddQuad(edgeVertex, outterVertex, slerpPoints_inner_outter[2], slerpPoints_inner_Edge[2]);
            AddQuadColor(
                edgeVertex_color,
                outterVertext_color,
                outterVertext_color * 4 / 5 + GridColor * 1 / 5,
                edgeVertex_color * 4 / 5 + GridColor * 1 / 5
            );
        }

        /// <summary>
        /// ���� һ�������� һ����б�µ����Ӵ�
        /// </summary>
        private void DrawStairFlatTriangle_Left(Color edgeVertex_color, Color outterVertext_color,
            Point innerVertex, Point outterVertex, Point edgeVertex) {

            List<Point> slerpPoints_inner_outter = GetDivideStairPoints(innerVertex, outterVertex, 0.4f, 3).ToList();

            float curRatio = 0;
            //for (int i = 1; i < slerpPoints_inner_outter.Count; i++) {
            //    // ���� �ұߵ�С������
            //    curRatio = Mathf.Max(curRatio, 1);
            //    AddTriangle(slerpPoints_inner_outter[i - 1], slerpPoints_inner_outter[i], edgeVertex);
            //    AddTriangleColor(
            //        outterVertext_color * curRatio + GridColor * (1 - curRatio),
            //        outterVertext_color * (curRatio + 0.4f) + GridColor * (1 - curRatio - 0.4f),
            //        edgeVertex_color
            //    );
            //    curRatio += 0.4f;
            //}

            AddTriangle(innerVertex, slerpPoints_inner_outter[1], edgeVertex);
            AddTriangleColor(
                GridColor,
                outterVertext_color * 2 / 5 + GridColor * 3 / 5,
                edgeVertex_color
            );
            AddTriangle(slerpPoints_inner_outter[1], slerpPoints_inner_outter[2], edgeVertex);
            AddTriangleColor(
                outterVertext_color * 2 / 5 + GridColor * 3 / 5,
                outterVertext_color * 4 / 5 + GridColor * 1 / 5,
                edgeVertex_color
            );
            AddTriangle(slerpPoints_inner_outter[2], outterVertex, edgeVertex);
            AddTriangleColor(
                outterVertext_color * 4 / 5 + GridColor * 1 / 5,
                outterVertext_color,
                edgeVertex_color
            );
        }

        private void DrawStairFlatTriangle_Right(Color edgeVertex_color, Color outterVertext_color,
            Point innerVertex, Point outterVertex, Point edgeVertex) {
            // , 0.4f, 3
            List<Point> slerpPoints_inner_outter = GetDivideStairPoints(innerVertex, outterVertex, 0.4f, 3).ToList();

            float curRatio = 0;
            //for(int i = 1; i < slerpPoints_inner_outter.Count; i++) {
            //    // ���� �ұߵ�С������
            //    curRatio = Mathf.Max(curRatio, 1);
            //    AddTriangle(slerpPoints_inner_outter[i - 1], edgeVertex, slerpPoints_inner_outter[i]);
            //    AddTriangleColor(
            //        outterVertext_color * curRatio + GridColor * (1 - curRatio),
            //        edgeVertex_color,
            //        outterVertext_color * (curRatio + 0.4f) + GridColor * (1 - curRatio - 0.4f)
            //    );
            //    curRatio += 0.4f;
            //}

            // ���� �ұߵ�С������
            AddTriangle(innerVertex, edgeVertex, slerpPoints_inner_outter[1]);
            AddTriangleColor(
                GridColor,
                edgeVertex_color,
                outterVertext_color * 2 / 5 + GridColor * 3 / 5
            );
            AddTriangle(slerpPoints_inner_outter[1], edgeVertex, slerpPoints_inner_outter[2]);
            AddTriangleColor(
                outterVertext_color * 2 / 5 + GridColor * 3 / 5,
                edgeVertex_color,
                outterVertext_color * 4 / 5 + GridColor * 1 / 5
            );
            AddTriangle(slerpPoints_inner_outter[2], edgeVertex, outterVertex);
            AddTriangleColor(
                outterVertext_color * 4 / 5 + GridColor * 1 / 5,
                edgeVertex_color,
                outterVertext_color
            );
        }



        /// <summary>
        /// �����������������
        /// </summary>
        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            int vertexIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            // ����ܹ֣����������ֶ���ķ�����
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }

        private void AddTriangleColor(Color c1, Color c2, Color c3) {
            // Ҫ�� AddTriangle ֮�����̵���
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c3);

            Vector3 types;
            types.x = types.y = types.z = 5;
            AddTriangleTerrainTypes(types);
        }

        public void AddTriangleTerrainTypes(Vector3 types) {
            terrainTypes.Add(types);
            terrainTypes.Add(types);
            terrainTypes.Add(types);
        }

        /// <summary>
        /// ������������ı���
        /// </summary>
        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            int vertexIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);
            // Ҫ������ȷ�ķ����� �����˳��Ϊ: v1 v3 v2, v1 v4 v3
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        private void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
            // Ҫ�� AddQuad ֮�����̵���
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c3);
            colors.Add(c4);

            Vector3 types;
            types.x = types.y = types.z = 5;
            AddQuadTerrainTypes(types);
        }

        public void AddQuadTerrainTypes(Vector3 types) {
            terrainTypes.Add(types);
            terrainTypes.Add(types);
            terrainTypes.Add(types);
            terrainTypes.Add(types);
        }

        // TODO: Ŀǰ������õ�һ���������ɫ��Ҫ��
        UnityEngine.Color[] gridColors = new UnityEngine.Color[5]{
            UnityEngine.Color.yellow,
            UnityEngine.Color.blue,
            new UnityEngine.Color(1,0.7f,0),
            UnityEngine.Color.green,
            UnityEngine.Color.green,
        };

        private UnityEngine.Color GetGridColor_Random() {
            int idx = UnityEngine.Random.Range(0, gridColors.Length);
            return gridColors[idx];
        }

        private float GetGridHigh_Random() {
            // ���ݵ�ͼ������ɫ ��� ��Ӧ������߶�
            if (GridColor == Color.green) {
                return 0;
            } else if (GridColor == Color.yellow) {
                return 2.0f;
            } else if (GridColor == Color.blue) {
                return -2.0f;
            } else if (GridColor == new UnityEngine.Color(1, 0.7f, 0)) {
                return 4.0f;
            } else {
                return 0;
            }

            //int idx = UnityEngine.Random.Range(0, gridHighs.Length);
            //return gridHighs[idx];
        }

        #endregion

        #region ����Mesh ���ƺ���

        private void DrawRiverMesh() {

        }


        #endregion
    }
}