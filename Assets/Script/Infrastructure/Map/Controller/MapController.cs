using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Infrastructure.HexagonGrid.Controller;
using WarGame_True.Infrastructure.HexagonGrid.DataStruct;
using WarGame_True.Infrastructure.HexagonGrid.MapObject;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.Infrastructure.Map.Controller {
    /// <summary>
    /// ��ͼ�����࣬��¼������ʡ�ݵ���Ϣ�����о�����Ϣ
    /// </summary>
    public class MapController : MonoBehaviour {

        public static MapController Instance { get; private set; }

        [SerializeField] SpriteRenderer MapSprite;

        [Header("ʡ�����ݹ�����")]
        [SerializeField] ProvinceInject provinceInject;

        [Header("����������������")]
        [SerializeField] HexConstructor hexConstructor;

        #region ʡ���������
        [Header("����λ�� - ʡ�� ��ӳ��")]
        private Dictionary<Vector3, Province> posProvinceDic = new Dictionary<Vector3, Province>();
        public Dictionary<Vector3, Province> PosProvinceDic {
            get => posProvinceDic;
            private set => posProvinceDic = value;
        }

        [Header("ID - ʡ�� ��ӳ��")]
        private Dictionary<uint, Province> idProvinceDic = new Dictionary<uint, Province>();
        public Dictionary<uint, Province> IDProvinceDic {
            get => idProvinceDic;
            private set => idProvinceDic = value;
        }

        [Header("��ǰʹ�õ�ʡ������")]
        [SerializeField] TextAsset provinceDataFile;

        public Province GetProvinceByID(uint provinceID) {
            if (idProvinceDic.ContainsKey(provinceID)) {
                return idProvinceDic[provinceID];
            }
            return null;
        }

        #endregion

        public void InitMap() {
            Instance = this;

            //���²���ȡ��ǰ��ͼӳ��
            hexConstructor.UpdateCurrentHexProvince();
            Dictionary<uint, HexGrid> hexagonDic = hexConstructor.CurrentIDHex;
            //Debug.Log("-----------");
            foreach (KeyValuePair<uint, HexGrid> keyValuePair in hexagonDic)
            {
                GameObject valueObject = keyValuePair.Value.gameObject;
                Province province;

                province = keyValuePair.Value.GetComponent<Province>();
                if (province == null) {
                    province = valueObject.AddComponent<Province>();
                }

                // ��ӵ� ID-ʡ�� ӳ����
                if (!IDProvinceDic.ContainsKey(keyValuePair.Key)) {
                    IDProvinceDic.Add(keyValuePair.Key, province);
                } else {
                    IDProvinceDic[keyValuePair.Key] = province;
                }

                // ��ӵ� position-ʡ��ӳ����
                Vector3 postion = keyValuePair.Value.hexPosition;
                if (!PosProvinceDic.ContainsKey(postion)) {
                    PosProvinceDic.Add(postion, province);
                } else {
                    PosProvinceDic[postion] = province;
                }
                //HexGirdProvince.Add(keyValuePair.Key, province);

                //�ҵ���Ӧ��ʡ�� ������Ϣ

                //��ʼ��ʡ����Ϣ
                province.InitProvince(keyValuePair.Key, keyValuePair.Value.hexPosition);
                //Debug.Log(keyValuePair.Value.hexCanvas == null);
            }

            //Debug.Log("�ѳ�ʼ�� ��ͼ!");
        }

        public void SetProvinceData() {
            // �Ӵ洢�ж�ȡʡ������ ������
            //await provinceInject.ImportCSVToCurrentMap();
            provinceInject.ImportCSVToCurrentMap(provinceDataFile);
        }
    
        /// <summary>
        /// ��ʡ�ݷ��䵽 ��Ӧ�Ĺ�����,����λ�÷���
        /// </summary>
        public void InitTerritory_ByPos(List<Faction> BookMarkFactions) {
            foreach (Faction faction in BookMarkFactions)
            {
                string[] territory = faction.TerritoryInitString.ToArray();
                //��string�������ַ���תΪprovince ���뵽faction��
                foreach(string territoryStr in territory) {
                    try {
                        string[] terPos = territoryStr.Split("_");

                        //�������������Ƿ���ȷ
                        if (terPos.Length != 3) continue;

                        Vector3 position = new Vector3(
                            int.Parse(terPos[0]), 
                            int.Parse(terPos[1]), 
                            int.Parse(terPos[2])
                        );
                        if (PosProvinceDic.ContainsKey(position)) {
                            //�õ�ͼ���� תΪprovince,���뵽��ǰ���������У�ͬʱ����ʡ�ݱ�����
                            faction.Territory.Add(PosProvinceDic[position]);
                            PosProvinceDic[position].SetProvinceControlStatu(
                                faction.FactionInfo.FactionTag, 
                                faction.FactionInfo.FactionTag, 
                                faction.FactionInfo.FactionColor
                            );
                        }

                    } catch {
                        Debug.LogError("��ȡ����ʱ���ִ���");
                    }
                }
            }
        }

        public void InitTerritory_ByID(List<Faction> BookMarkFactions) {
            foreach (Faction faction in BookMarkFactions) {
                string[] territory = faction.TerritoryInitString.ToArray();
                //��string�������ַ���תΪprovince ���뵽faction��
                foreach (string territoryStr in territory) {
                    try {
                        uint provinceID = uint.Parse(territoryStr);
                        if (IDProvinceDic.ContainsKey(provinceID)) {
                            //�õ�ͼ���� תΪprovince,���뵽��ǰ���������У�ͬʱ����ʡ�ݱ�����
                            faction.Territory.Add(IDProvinceDic[provinceID]);
                            IDProvinceDic[provinceID].SetProvinceControlStatu(
                                faction.FactionInfo.FactionTag,
                                faction.FactionInfo.FactionTag,
                                faction.FactionInfo.FactionColor
                            );
                        }

                    } catch {
                        Debug.LogError("��ȡ����ʱ���ִ���");
                    }
                }
            }
        }

        #region �ƶ� ��λ ��ؽӿ�
        /// <summary>
        /// ��ô�һ��ʡ�ݵ���һ��ʡ�ݵ�·��������BFS
        /// </summary>
        public List<Province> GetMovePath(Province start, Province destination) {
            Queue<Province> queue = new Queue<Province>();
            // ��¼·��
            Dictionary<Province, Province> cameFrom = new Dictionary<Province, Province>();
            // ��start����Ӧ��Ļ���
            Dictionary<Province, int> costSoFar = new Dictionary<Province, int>();

            ////��ʼ�� ����·��
            //foreach (Province province in HexGirdProvince.Values)
            //{
            //    costSoFar.Add(province, 0);
            //}

            queue.Enqueue(start);
            cameFrom[start] = null;
            costSoFar[start] = 0;

            while (queue.Count > 0) {
                Province current = queue.Dequeue();

                // �ж��Ƿ񵽴�Ŀ�ĵ�
                if (current == destination) {
                    return ReconstructPath(cameFrom, current);
                }

                // ��ȡ��ǰ���������ڵ������Σ�����ʵ��ȡ������������������ṹ
                List<Province> neighbors = GetNeighbors(current);

                //Debug.Log("�ھ�������" + neighbors.Count);

                foreach (Province neighbor in neighbors) {
                    int newCost = costSoFar[current] + (int)neighbor.provinceData.MoveCost;

                    // �ж��Ƿ�����·���У������Ƿ�ȼ�¼·������
                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]) {
                        costSoFar[neighbor] = newCost;
                        queue.Enqueue(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }

            // ����޷��ҵ�·�������ؿ��б�
            return new List<Province>();
        }

        private List<Province> ReconstructPath(Dictionary<Province, Province> cameFrom, Province current) {
            List<Province> path = new List<Province>();
            while (current != null) {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// ��ȡ��ǰprovince������province
        /// </summary>
        private List<Province> GetNeighbors(Province province) {
            List<Province> neighbors = new List<Province>();
            foreach (Vector3 direction in province.GetNeighborProvinces()) {
                if (PosProvinceDic.ContainsKey(direction)) {
                    neighbors.Add(PosProvinceDic[direction]);
                } else {
                    //Debug.Log("�޷��ҵ���Ӧ���ھ�ʡ�ݣ�");
                }
            }

            return neighbors;
        }
        
        /// <summary>
        /// ��ȡ���еĳ���Ŀ��
        /// </summary>
        public Province GetWithdrawTarget(Province withdrawStart, string tag) {

            
            int curStep = 1;

            Province curProvince = withdrawStart;
            Queue<Vector3> waitVisitProvince = new Queue<Vector3>();
            Dictionary<Vector3, bool> hasVisitedProvince = new Dictionary<Vector3, bool>();

            waitVisitProvince.Enqueue(curProvince.provincePosition);

            while (curStep < 7) {
                // �����ڽ�ʡ��
                int curCount = waitVisitProvince.Count;

                while (curCount > 0) {
                    // ��ȡ���ж���Ԫ��
                    Vector3 curProvincePos = waitVisitProvince.Peek();
                    waitVisitProvince.Dequeue();

                    // ��Ǹ�ʡ��Ϊ�Ѿ�����
                    if (hasVisitedProvince.ContainsKey(curProvincePos)) {
                        hasVisitedProvince[curProvincePos] = true;
                        continue;
                    } else {
                        hasVisitedProvince.Add(curProvincePos, true);
                    }

                    
                    curProvince = PosProvinceDic[curProvincePos];
                    // �����ھӽڵ㣬���뵽 queue ��
                    foreach (Vector3 direction in curProvince.GetNeighborProvinces()) {
                        // �ж�ʡ���Ƿ����
                        if (PosProvinceDic.ContainsKey(direction)) {
                            // �Ѿ����ʹ� ������
                            if (hasVisitedProvince.ContainsKey(direction)) {
                                continue;
                            }

                            // ��������withdrawStart������,�ұ��������Ƶ�ʡ�ݣ���Զ����6����
                            if (curStep >= 3 && PosProvinceDic[direction].UnderTagControl(tag)) {
                                Debug.Log($"�ҵ��˺��ʵĳ���ʡ��,λ��Ϊ:{direction},ʡ��IDΪ:{PosProvinceDic[direction].provinceData.provinceID}");
                                return PosProvinceDic[direction];
                            }

                            waitVisitProvince.Enqueue(direction);
                        }
                    }
                    
                    curCount--;
                }

                curStep++;
            }

            // ���û�з���Ҫ���ʡ�����������
            Debug.Log("û���ҵ����ʵ�ʡ��!");
            return null;
        }

        #endregion

        #region ����ϵͳ ��ؽӿ�
        
        // TODO: ���֮
        public List<Province> GetSupplyPath(Province start, Province des) {
            return GetMovePath(start, des);
        }

        #endregion

        #region �˿�Ǩ��ϵͳ

        #endregion

        #region ��ͬ�ĵ�ͼģʽ

        public static MapMode CurMapMode = MapMode.Normal;

        /// <summary>
        /// �ı��ͼģʽ �ӿ�
        /// </summary>
        /// <param name="TargetMode">Ŀ���ͼģʽ</param>
        public void ChangeMapMode(MapMode TargetMode) {

            // �ȸ��ݵ�ǰ��MapMode��ִ���˳��߼�
            switch (CurMapMode) {
                case MapMode.Normal:

                    break;
                case MapMode.SupplyMap:
                    ExitSupplyMapMode();
                    break;
            }

            // ����Ŀ�ĵ�ͼģʽ��ִ�н����߼�
            switch (TargetMode) {
                case MapMode.Normal:
                    EnterNormalMapMode();
                    break;
                case MapMode.SupplyMap:
                    EnterSupplyMapMode();
                    break;
            }

            CurMapMode = TargetMode;
        }

        private void EnterNormalMapMode() {
            
            MapSprite.color = Color.white;

        }

        /// <summary>
        /// ����·�ߵ�ͼ���򿪸õ�ͼ��������о��ӵĲ�����
        /// </summary>
        private void EnterSupplyMapMode() {
            // TODO: 1.��ͼ���ȵ���
            MapSprite.color = new Color(0.6f, 0.6f, 0.6f);

            // TODO: 2.��ȡ�����еľ��ӵĲ�������Ϣ
            ArmyController.Instance.ShowArmySupplyLine();

            // TODO: 3.�Ѵ��ڲ������ϵ�ʡ�����ø���
        }

        private void ExitSupplyMapMode() {
            MapSprite.color = Color.white;

            ArmyController.Instance.HideArmySupplyLine();

        }


        #endregion

    }

    public enum MapMode {
        Normal,
        SupplyMap
    }
}