//using OfficeOpenXml;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;

namespace WarGame_True.GamePlay.CombatPart {
    /// <summary>
    /// 将领的ScriptableObject，可以作为预定义的将领数据
    /// </summary>
    [CreateAssetMenu(fileName ="GeneralData", menuName = "WarGame/GeneralData")]
    public class GeneralData : ScriptableObject {

        public string generalName;

        public string generalChineseName;

        public string generalDescription;

        public string generalTag;


        [Header("对军队的修正")]
        public float organizationModify;
        public uint maxCommandUnits;
        public int visibleGrid;

        //public float supplyReduceCostModify;
        //public float supplyMaxModify;


        [Header("不同战斗阶段下对军队单位的修正")]
        public UnitModify normalModify;

        public UnitModify standOffModify;

        public UnitModify siegeModify;

        public UnitModify engagementModify;

        public UnitModify openBattleModify;

        public UnitModify withdrawModify;

        // TODO：
        // 将领特质 - 根据对军队单位的修正获得
        [Header("将领特质")]
        public List<GeneralTrait> generalTraits = new List<GeneralTrait>();

        [Header("将领可采用的战术")]
        public List<Tactic> generalTactics = new List<Tactic>();

        // NOTICE: 不要删!!!
        // TODO: excel包不能在非editor文件夹下调用
        /*public void SetGeneralData(ExcelRange rowdata, int row) {
            generalName = rowdata[row, 1].Text;
            generalChineseName = rowdata[row, 2].Text;
            generalDescription = rowdata[row, 3].Text;
            generalTag = rowdata[row, 4].Text;

            organizationModify = Convert.ToSingle(rowdata[row, 6].Text);
            maxCommandUnits = (uint)int.Parse(rowdata[row, 7].Text);
            visibleGrid = int.Parse(rowdata[row, 8].Text);

            normalModify.maxMoraleModify = Convert.ToSingle(rowdata[row, 9].Text);
            normalModify.recoverMoraleModify = Convert.ToSingle(rowdata[row, 10].Text);

            // 训练度
            normalModify.discipineModify = Convert.ToSingle(rowdata[row, 11].Text);

            // 移动速度
            normalModify.speedModify = Convert.ToSingle(rowdata[row, 12].Text);

            // 攻击-防御
            normalModify.armyAttackModify = Convert.ToSingle(rowdata[row, 13].Text);
            normalModify.armyDefendModify = Convert.ToSingle(rowdata[row, 14].Text);

            // 攻击范围
            normalModify.armyAttackScopeModify = 0;

            // 破甲-披甲
            normalModify.armyArmorCladkModify = Convert.ToSingle(rowdata[row, 15].Text);
            normalModify.armyArmorPenetrateModify = Convert.ToSingle(rowdata[row, 16].Text);

            // 士气攻击-防御
            normalModify.armyMoraleAttackModify = Convert.ToSingle(rowdata[row, 17].Text);
            normalModify.armyMoraleDefendModify = Convert.ToSingle(rowdata[row, 18].Text);
            normalModify.armyMoraleModifyInRear = Convert.ToSingle(rowdata[row, 19].Text);
            normalModify.armyMoraleModifyToRear = Convert.ToSingle(rowdata[row, 20].Text);

            // 资金花费-招募时间
            normalModify.costModify = 0;
            normalModify.recruitCostModify = 0;

            // 补给消耗与携带量
            normalModify.supplyCostModiy = Convert.ToSingle(rowdata[row, 21].Text);
            normalModify.supplyMaxModify = Convert.ToSingle(rowdata[row, 22].Text);

        }
*/
    }
}