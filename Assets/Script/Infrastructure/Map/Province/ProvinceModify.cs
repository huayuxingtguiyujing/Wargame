using OfficeOpenXml.FormulaParsing.Excel.Operators;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.Map.Provinces {
    /// <summary>
    /// ��װ�˶�ʡ�ݵļӳɣ������˶Դ󲿷�ʡ�������ֶε�����
    /// </summary>
    [System.Serializable]
    public class ProvinceModify {

        // TODO: ��ɣ�

        #region ������Ϣ
        public float MoveCostModify;

        public float ArmyLossModify;

        #endregion

        #region ������Ϣ

        // TODO: �˿�


        // TODO: ����

        // TODO: ���ٶȡ��ķ϶�etc ��δʵװ
        
        // ˰��
        public float TaxModify;

        // ��ʳ
        public float GrainModify;

        // ά����
        public float MaintenModify;

        #endregion

        #region ������Ϣ

        // �ΰ�
        public float PublicSafetyModify;

        // ����Ч��
        public float AdminEfficiencyModify;

        // ֧�ֶ�
        public float ApprovalRatingModify;

        #endregion

        #region ������Ϣ

        public float LocalSupplyAdd;

        #endregion

        public ProvinceModify() { }

        public ProvinceModify(float moveCostModify, float armyLossModify,
            float taxModify, float grainModify, float maintenModify, float publicSafetyModify,
            float adminEfficiencyModify, float approvalRatingModify, float localSupplyAdd) {
            MoveCostModify = moveCostModify;
            ArmyLossModify = armyLossModify;
            TaxModify = taxModify;
            GrainModify = grainModify;
            MaintenModify = maintenModify;
            PublicSafetyModify = publicSafetyModify;
            AdminEfficiencyModify = adminEfficiencyModify;
            ApprovalRatingModify = approvalRatingModify;
            LocalSupplyAdd = localSupplyAdd;
        }

        public static ProvinceModify operator +(ProvinceModify originMod, ProvinceModify modify) {
            originMod.MoveCostModify += modify.MoveCostModify;
            originMod.ArmyLossModify += modify.ArmyLossModify;

            originMod.TaxModify += modify.TaxModify;
            originMod.GrainModify += modify.GrainModify;
            originMod.MaintenModify += modify.MaintenModify;

            originMod.PublicSafetyModify += modify.PublicSafetyModify;
            originMod.AdminEfficiencyModify += modify.AdminEfficiencyModify;
            originMod.ApprovalRatingModify += modify.ApprovalRatingModify;
            originMod.LocalSupplyAdd += modify.LocalSupplyAdd;
            return originMod;
        }

        public static ProvinceModify operator -(ProvinceModify originMod, ProvinceModify modify) {
            originMod.MoveCostModify -= modify.MoveCostModify;
            originMod.ArmyLossModify -= modify.ArmyLossModify;

            originMod.TaxModify -= modify.TaxModify;
            originMod.GrainModify -= modify.GrainModify;
            originMod.MaintenModify -= modify.MaintenModify;

            originMod.PublicSafetyModify -= modify.PublicSafetyModify;
            originMod.AdminEfficiencyModify -= modify.AdminEfficiencyModify;
            originMod.ApprovalRatingModify -= modify.ApprovalRatingModify;
            originMod.LocalSupplyAdd -= modify.LocalSupplyAdd;
            return originMod;
        }

        public void ResetProvinceModify() {
            MoveCostModify = 0;
            ArmyLossModify = 0;
            TaxModify = 0;
            GrainModify = 0;
            MaintenModify = 0;
            PublicSafetyModify = 0;
            AdminEfficiencyModify = 0;
            ApprovalRatingModify = 0;
            LocalSupplyAdd = 0;
        }

    }
}