using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.Infrastructure.DebugPack {
    public class ArmyCommand : Command {

        public ulong handleArmyID;      // ����ʱ���ӵ� NetworkObjectID

        public int handleNum;         // �������ӵ���Ŀ

        public int handleProvinceID;    // ����ָ����ʡ��

        public ArmyCommandType ArmyCmdType;

        public string ArmyUnitName;


        //army create 10 in 1001 			// ��ָ��id��ʡ�ݴ���������Ŀ
        //army create 10 in 1001 infantry	// ָ�����ӵ����ͣ�Ĭ�ϸþ�������ʡ����������

        //army recruit 10 in 1001			// ��ļ����
        //army recruit 10 in 1001 infantry	// ָ�����ӵ�����

        //army remove -id 1				    // �Ƴ�ָ��id�ľ���
        //army remove -province 1010	    // �Ƴ�ָ��ʡ���ϵ����о���
        //army remove -tag tag			    // �Ƴ�ָ��tag�����о���
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
                // removeProvince Ϊ����ʱ����Ϊ��Чid����ʱӦ�ò�������ID
                // Ϊʲô���� removeArmyID ���жϣ���Ϊ����ulong����
                Army army = ArmyNetworkCtrl.Instance.GetArmyByID(removeArmyID);
                List<Army> armies = new List<Army>() { army };
                ArmyNetworkCtrl.Instance.RemoveArmyEvent(armies);
            } else {
                // ���� ProvinceID
                Province targetProvince = MapController.Instance.GetProvinceByID((uint)removeProvince);
                ArmyNetworkCtrl.Instance.RemoveArmyEvent(targetProvince.ArmiesInProvince);
            }
        }

        private async void RecruitArmy(int num, int provinceID, string unitName) {
            ArmyUnitData armyUnitData = await ArmyUnitHelper.GetArmyUnitDataAsync(unitName);
            Province targetProvince = MapController.Instance.GetProvinceByID((uint)provinceID);
            for (int i = 0; i < num; i++) {
                // NOTICE: ��ΪMapNetworkCtrl ��ļ����ʱ��֧������״̬������Ҫ����ʡ�ݵķ���
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