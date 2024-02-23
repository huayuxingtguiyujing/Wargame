using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// ��װһЩ�Ծ��ӵĲ�������
    /// </summary>
    public class ArmyHelper {

        #region ����ģʽ
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
                //�ж��Ƿ�ͬ��һ��ʡ��
                ans = ans && provinceId.Equals(army.CurrentProvince.provincePos);

                //�ж��Ƿ���ͬһ��������ͬʱѡ�� ��Ȼ��ͬһ����,Ӧ�ðɣ�

                //�жϾ��ӱ����Ƿ�����ϲ�
                ans = ans && army.AbleToMergeArmy();

                if (!ans) return false;
            }
            return true;
        }


        #region ����ս��

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
    /// ��װһЩ�Ծ��ӵ�λ�Ĳ�������
    /// </summary>
    public class ArmyUnitHelper {

        public static async Task<ArmyUnitData> GetArmyUnitDataAsync(string unitName) {
            // ���δָ������Ĭ�Ϸ��ز���
            if (string.IsNullOrEmpty(unitName)) unitName = "Infantry";
            string path = "Assets/Prefab/Unit/" + unitName + ".asset";
            ArmyUnitData armyUnit = await Addressables.LoadAssetAsync<ArmyUnitData>(path).Task;
            return armyUnit;
        }

    }

}