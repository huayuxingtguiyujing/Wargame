using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.Infrastructure.Map.Provinces {
    /// <summary>
    /// ���� ScriptableObject
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Building", menuName = "WarGame/Building")]
    public class Building : ScriptableObject {

        public string BuildingName;

        public string BuildingChineseName;

        public string BuildingDesc;

        [Header("��ʡ�ݵļӳ�")]
        public ProvinceModify ProvinceModify;

        [Header("�޽��ɱ�")]
        public int CostMoney = 10000;

        public int CostTime = 10;   // ��λ Day

        public int MaintenCost = 1000;

        [Header("������")]
        public bool IsSupplyCenter = false;

        /// <summary>
        /// ��ȡ��ǰ���п��õĽ���
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Building>> GetAllBuilding() {
            var list = new List<Building>();

            string buildingDir = "Assets/Prefab/Building/";
            string suffix = ".asset";
            // Assets/Prefab/Building/��������.asset
            // Assets/Prefab/Building/����.asset
            // Assets/Prefab/Building/�ٵ�.asset

            string path1 = buildingDir + "��������" + suffix;
            string path2 = buildingDir + "����" + suffix;
            string path3 = buildingDir + "�ٵ�" + suffix;

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
    /// ����������
    /// </summary>
    public class BuildingData {



    }

}