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
    /// 地图控制类，记录了所有省份的信息、所有军队信息
    /// </summary>
    public class MapController : MonoBehaviour {

        public static MapController Instance { get; private set; }

        [SerializeField] SpriteRenderer MapSprite;

        [Header("省份数据管理器")]
        [SerializeField] ProvinceInject provinceInject;

        [Header("六边形网格生成器")]
        [SerializeField] HexConstructor hexConstructor;

        #region 省份数据相关
        [Header("地理位置 - 省份 的映射")]
        private Dictionary<Vector3, Province> posProvinceDic = new Dictionary<Vector3, Province>();
        public Dictionary<Vector3, Province> PosProvinceDic {
            get => posProvinceDic;
            private set => posProvinceDic = value;
        }

        [Header("ID - 省份 的映射")]
        private Dictionary<uint, Province> idProvinceDic = new Dictionary<uint, Province>();
        public Dictionary<uint, Province> IDProvinceDic {
            get => idProvinceDic;
            private set => idProvinceDic = value;
        }

        [Header("当前使用的省份数据")]
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

            //更新并获取当前地图映射
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

                // 添加到 ID-省份 映射中
                if (!IDProvinceDic.ContainsKey(keyValuePair.Key)) {
                    IDProvinceDic.Add(keyValuePair.Key, province);
                } else {
                    IDProvinceDic[keyValuePair.Key] = province;
                }

                // 添加到 position-省份映射中
                Vector3 postion = keyValuePair.Value.hexPosition;
                if (!PosProvinceDic.ContainsKey(postion)) {
                    PosProvinceDic.Add(postion, province);
                } else {
                    PosProvinceDic[postion] = province;
                }
                //HexGirdProvince.Add(keyValuePair.Key, province);

                //找到对应的省份 配置信息

                //初始化省份信息
                province.InitProvince(keyValuePair.Key, keyValuePair.Value.hexPosition);
                //Debug.Log(keyValuePair.Value.hexCanvas == null);
            }

            //Debug.Log("已初始化 地图!");
        }

        public void SetProvinceData() {
            // 从存储中读取省份数据 并加载
            //await provinceInject.ImportCSVToCurrentMap();
            provinceInject.ImportCSVToCurrentMap(provinceDataFile);
        }
    
        /// <summary>
        /// 将省份分配到 对应的国家中,基于位置分配
        /// </summary>
        public void InitTerritory_ByPos(List<Faction> BookMarkFactions) {
            foreach (Faction faction in BookMarkFactions)
            {
                string[] territory = faction.TerritoryInitString.ToArray();
                //将string的坐标字符串转为province 加入到faction中
                foreach(string territoryStr in territory) {
                    try {
                        string[] terPos = territoryStr.Split("_");

                        //检查输入的坐标是否正确
                        if (terPos.Length != 3) continue;

                        Vector3 position = new Vector3(
                            int.Parse(terPos[0]), 
                            int.Parse(terPos[1]), 
                            int.Parse(terPos[2])
                        );
                        if (PosProvinceDic.ContainsKey(position)) {
                            //该地图存在 转为province,加入到当前控制领土中，同时设置省份被控制
                            faction.Territory.Add(PosProvinceDic[position]);
                            PosProvinceDic[position].SetProvinceControlStatu(
                                faction.FactionInfo.FactionTag, 
                                faction.FactionInfo.FactionTag, 
                                faction.FactionInfo.FactionColor
                            );
                        }

                    } catch {
                        Debug.LogError("获取领土时出现错误");
                    }
                }
            }
        }

        public void InitTerritory_ByID(List<Faction> BookMarkFactions) {
            foreach (Faction faction in BookMarkFactions) {
                string[] territory = faction.TerritoryInitString.ToArray();
                //将string的坐标字符串转为province 加入到faction中
                foreach (string territoryStr in territory) {
                    try {
                        uint provinceID = uint.Parse(territoryStr);
                        if (IDProvinceDic.ContainsKey(provinceID)) {
                            //该地图存在 转为province,加入到当前控制领土中，同时设置省份被控制
                            faction.Territory.Add(IDProvinceDic[provinceID]);
                            IDProvinceDic[provinceID].SetProvinceControlStatu(
                                faction.FactionInfo.FactionTag,
                                faction.FactionInfo.FactionTag,
                                faction.FactionInfo.FactionColor
                            );
                        }

                    } catch {
                        Debug.LogError("获取领土时出现错误");
                    }
                }
            }
        }

        #region 移动 单位 相关接口
        /// <summary>
        /// 获得从一个省份到另一个省份的路径，基于BFS
        /// </summary>
        public List<Province> GetMovePath(Province start, Province destination) {
            Queue<Province> queue = new Queue<Province>();
            // 记录路径
            Dictionary<Province, Province> cameFrom = new Dictionary<Province, Province>();
            // 从start到相应点的花费
            Dictionary<Province, int> costSoFar = new Dictionary<Province, int>();

            ////初始化 花费路径
            //foreach (Province province in HexGirdProvince.Values)
            //{
            //    costSoFar.Add(province, 0);
            //}

            queue.Enqueue(start);
            cameFrom[start] = null;
            costSoFar[start] = 0;

            while (queue.Count > 0) {
                Province current = queue.Dequeue();

                // 判断是否到达目的地
                if (current == destination) {
                    return ReconstructPath(cameFrom, current);
                }

                // 获取当前六边形相邻的六边形，具体实现取决于您的六边形网格结构
                List<Province> neighbors = GetNeighbors(current);

                //Debug.Log("邻居数量：" + neighbors.Count);

                foreach (Province neighbor in neighbors) {
                    int newCost = costSoFar[current] + (int)neighbor.provinceData.MoveCost;

                    // 判断是否已在路径中，花费是否比记录路径更少
                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]) {
                        costSoFar[neighbor] = newCost;
                        queue.Enqueue(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }

            // 如果无法找到路径，返回空列表
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
        /// 获取当前province的相邻province
        /// </summary>
        private List<Province> GetNeighbors(Province province) {
            List<Province> neighbors = new List<Province>();
            foreach (Vector3 direction in province.GetNeighborProvinces()) {
                if (PosProvinceDic.ContainsKey(direction)) {
                    neighbors.Add(PosProvinceDic[direction]);
                } else {
                    //Debug.Log("无法找到对应的邻居省份！");
                }
            }

            return neighbors;
        }
        
        /// <summary>
        /// 获取可行的撤退目标
        /// </summary>
        public Province GetWithdrawTarget(Province withdrawStart, string tag) {

            
            int curStep = 1;

            Province curProvince = withdrawStart;
            Queue<Vector3> waitVisitProvince = new Queue<Vector3>();
            Dictionary<Vector3, bool> hasVisitedProvince = new Dictionary<Vector3, bool>();

            waitVisitProvince.Enqueue(curProvince.provincePosition);

            while (curStep < 7) {
                // 搜索邻近省份
                int curCount = waitVisitProvince.Count;

                while (curCount > 0) {
                    // 获取队列顶部元素
                    Vector3 curProvincePos = waitVisitProvince.Peek();
                    waitVisitProvince.Dequeue();

                    // 标记该省份为已经访问
                    if (hasVisitedProvince.ContainsKey(curProvincePos)) {
                        hasVisitedProvince[curProvincePos] = true;
                        continue;
                    } else {
                        hasVisitedProvince.Add(curProvincePos, true);
                    }

                    
                    curProvince = PosProvinceDic[curProvincePos];
                    // 遍历邻居节点，加入到 queue 中
                    foreach (Vector3 direction in curProvince.GetNeighborProvinces()) {
                        // 判断省份是否存在
                        if (PosProvinceDic.ContainsKey(direction)) {
                            // 已经访问过 则跳过
                            if (hasVisitedProvince.ContainsKey(direction)) {
                                continue;
                            }

                            // 搜索距离withdrawStart三格外,且被己方控制的省份（最远搜索6步）
                            if (curStep >= 3 && PosProvinceDic[direction].UnderTagControl(tag)) {
                                Debug.Log($"找到了合适的撤退省份,位置为:{direction},省份ID为:{PosProvinceDic[direction].provinceData.provinceID}");
                                return PosProvinceDic[direction];
                            }

                            waitVisitProvince.Enqueue(direction);
                        }
                    }
                    
                    curCount--;
                }

                curStep++;
            }

            // 如果没有符合要求的省份则随机撤退
            Debug.Log("没有找到合适的省份!");
            return null;
        }

        #endregion

        #region 补给系统 相关接口
        
        // TODO: 完成之
        public List<Province> GetSupplyPath(Province start, Province des) {
            return GetMovePath(start, des);
        }

        #endregion

        #region 人口迁移系统

        #endregion

        #region 不同的地图模式

        public static MapMode CurMapMode = MapMode.Normal;

        /// <summary>
        /// 改变地图模式 接口
        /// </summary>
        /// <param name="TargetMode">目标地图模式</param>
        public void ChangeMapMode(MapMode TargetMode) {

            // 先根据当前的MapMode，执行退出逻辑
            switch (CurMapMode) {
                case MapMode.Normal:

                    break;
                case MapMode.SupplyMap:
                    ExitSupplyMapMode();
                    break;
            }

            // 根据目的地图模式，执行进入逻辑
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
        /// 补给路线地图，打开该地图会出现所有军队的补给线
        /// </summary>
        private void EnterSupplyMapMode() {
            // TODO: 1.地图亮度调暗
            MapSprite.color = new Color(0.6f, 0.6f, 0.6f);

            // TODO: 2.获取到所有的军队的补给线信息
            ArmyController.Instance.ShowArmySupplyLine();

            // TODO: 3.把处于补给线上的省份设置高亮
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