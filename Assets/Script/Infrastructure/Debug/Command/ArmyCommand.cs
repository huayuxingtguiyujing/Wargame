using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.Infrastructure.DebugPack {
    public class ArmyCommand : Command {

        public ulong handleArmyID;      // 联网时军队的 NetworkObjectID

        public int handleNum;         // 操作军队的数目

        public int handleProvinceID;    // 操作指定的省份

        public ArmyCommandType ArmyCmdType;

        public string ArmyUnitName;


        //army create 10 in 1001 			// 在指定id的省份创建军队数目
        //army create 10 in 1001 infantry	// 指定军队的类型，默认该军队属于省份所属国家

        //army recruit 10 in 1001			// 招募军队
        //army recruit 10 in 1001 infantry	// 指定军队的类型

        //army remove -id 1				    // 移除指定id的军队
        //army remove -province 1010	    // 移除指定省份上的所有军队
        //army remove -tag tag			    // 移除指定tag的所有军队
        public ArmyCommand(ulong handleArmyID, int handleNum, int handleProvinceID, ArmyCommandType armyCmdType, string armyUnitName) {
            this.handleArmyID = handleArmyID;
            this.handleNum = handleNum;
            this.handleProvinceID = handleProvinceID;
            ArmyCmdType = armyCmdType;
            ArmyUnitName = armyUnitName;
        }

        public override void Excute() {
            switch (ArmyCmdType) {
                case ArmyCommandType.Create:
                    CreateArmy(handleNum, handleProvinceID, ArmyUnitName);
                    break;
                case ArmyCommandType.Remove:
                    RemoveArmy(handleProvinceID, handleArmyID);
                    break;
                case ArmyCommandType.Recruit:
                    RecruitArmy(handleNum, handleProvinceID, ArmyUnitName);
                    break;
            }
        }


        private async void CreateArmy(int createNum, int createProvinceID, string createUnitName) {
            ArmyUnitData armyUnitData = await ArmyUnitHelper.GetArmyUnitDataAsync(createUnitName);
            for (int i = 0; i < createNum; i++) {
                ArmyNetworkCtrl.Instance.CreateArmyEvent(armyUnitData, (uint)createProvinceID);
            }
        }

        private void RemoveArmy(int removeProvince, ulong removeArmyID) {
            if (removeProvince < 0) {
                // removeProvince 为负数时，即为无效id，此时应该操作军队ID
                // 为什么不用 removeArmyID 作判断，因为它是ulong类型
                Army army = ArmyNetworkCtrl.Instance.GetArmyByID(removeArmyID);
                List<Army> armies = new List<Army>() { army };
                ArmyNetworkCtrl.Instance.RemoveArmyEvent(armies);
            } else {
                // 操作 ProvinceID
                Province targetProvince = MapController.Instance.GetProvinceByID((uint)removeProvince);
                ArmyNetworkCtrl.Instance.RemoveArmyEvent(targetProvince.ArmiesInProvince);
            }
        }

        private async void RecruitArmy(int num, int provinceID, string unitName) {
            ArmyUnitData armyUnitData = await ArmyUnitHelper.GetArmyUnitDataAsync(unitName);
            Province targetProvince = MapController.Instance.GetProvinceByID((uint)provinceID);
            for (int i = 0; i < num; i++) {
                // NOTICE: 因为MapNetworkCtrl 招募军队时仅支持联网状态，所以要调用省份的方法
                targetProvince.RecruitArmyEvent(armyUnitData);
            }
        }
    }


    public enum ArmyCommandType {
        Create,
        Remove,
        Recruit
    }

}