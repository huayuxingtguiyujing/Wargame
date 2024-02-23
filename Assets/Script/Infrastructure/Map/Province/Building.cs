using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.Infrastructure.Map.Provinces {
    /// <summary>
    /// 建筑 ScriptableObject
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Building", menuName = "WarGame/Building")]
    public class Building : ScriptableObject {

        public string BuildingName;

        public string BuildingChineseName;

        public string BuildingDesc;

        [Header("对省份的加成")]
        public ProvinceModify ProvinceModify;

        [Header("修建成本")]
        public int CostMoney = 10000;

        public int CostTime = 10;   // 单位 Day

        public int MaintenCost = 1000;

        [Header("特殊标记")]
        public bool IsSupplyCenter = false;

        /// <summary>
        /// 获取当前所有可用的建筑
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Building>> GetAllBuilding() {
            var list = new List<Building>();

            string buildingDir = "Assets/Prefab/Building/";
            string suffix = ".asset";
            // Assets/Prefab/Building/补给中心.asset
            // Assets/Prefab/Building/粮仓.asset
            // Assets/Prefab/Building/官道.asset

            string path1 = buildingDir + "补给中心" + suffix;
            string path2 = buildingDir + "粮仓" + suffix;
            string path3 = buildingDir + "官道" + suffix;

            Building building1 = await Addressables.LoadAssetAsync<Building>(path1).Task;
            Building building2 = await Addressables.LoadAssetAsync<Building>(path2).Task;
            Building building3 = await Addressables.LoadAssetAsync<Building>(path3).Task;

            list.Add(building1);
            list.Add(building2);
            list.Add(building3);

            return list;
        }

    }

    /// <summary>
    /// 建筑数据类
    /// </summary>
    public class BuildingData {



    }

}