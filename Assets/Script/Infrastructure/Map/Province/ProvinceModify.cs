using OfficeOpenXml.FormulaParsing.Excel.Operators;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.Map.Provinces {
    /// <summary>
    /// 封装了对省份的加成，包括了对大部分省份数据字段的修正
    /// </summary>
    [System.Serializable]
    public class ProvinceModify {

        // TODO: 完成！

        #region 地理信息
        public float MoveCostModify;

        public float ArmyLossModify;

        #endregion

        #region 经济信息

        // TODO: 人口


        // TODO: 人力

        // TODO: 繁荣度、荒废度etc 尚未实装
        
        // 税收
        public float TaxModify;

        // 粮食
        public float GrainModify;

        // 维护费
        public float MaintenModify;

        #endregion

        #region 政治信息

        // 治安
        public float PublicSafetyModify;

        // 行政效率
        public float AdminEfficiencyModify;

        // 支持度
        public float ApprovalRatingModify;

        #endregion

        #region 其他信息

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