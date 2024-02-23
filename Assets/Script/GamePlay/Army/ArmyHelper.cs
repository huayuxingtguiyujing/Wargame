using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// 封装一些对军队的操作方法
    /// </summary>
    public class ArmyHelper {

        #region 单例模式
        private static ArmyHelper instance;
        public static ArmyHelper GetInstance() {
            if (instance == null) {
                instance = new ArmyHelper();
            }
            return instance;
        }
        public ArmyHelper() { }
        #endregion

        public void SetArmyStayIn(List<Army> armies) {
            foreach (Army army in armies) {
                army.SetArmyStayIn();
            }
        }

        public bool AbleToMergeArmy(List<Army> mergeArmies) {

            if (mergeArmies.Count <= 1) {
                return false;
            }

            bool ans = true;
            string provinceId = mergeArmies[0].CurrentProvince.provincePos;
            foreach (Army army in mergeArmies) {
                //判断是否同在一个省份
                ans = ans && provinceId.Equals(army.CurrentProvince.provincePos);

                //判断是否是同一势力（能同时选中 自然是同一势力,应该吧）

                //判断军队本身是否允许合并
                ans = ans && army.AbleToMergeArmy();

                if (!ans) return false;
            }
            return true;
        }


        #region 军队战斗

        private void EnterCombat(List<Army> armies) {
            foreach (Army army in armies) {
                army.EnterCombat();
            }
        }

        public void ExitCombat(List<Army> armies) {
            foreach (Army army in armies)
            {
                army.ExitCombat();
            }
        }
        #endregion



    }

    /// <summary>
    /// 封装一些对军队单位的操作方法
    /// </summary>
    public class ArmyUnitHelper {

        public static async Task<ArmyUnitData> GetArmyUnitDataAsync(string unitName) {
            // 如果未指定，则默认返回步兵
            if (string.IsNullOrEmpty(unitName)) unitName = "Infantry";
            string path = "Assets/Prefab/Unit/" + unitName + ".asset";
            ArmyUnitData armyUnit = await Addressables.LoadAssetAsync<ArmyUnitData>(path).Task;
            return armyUnit;
        }

    }

}