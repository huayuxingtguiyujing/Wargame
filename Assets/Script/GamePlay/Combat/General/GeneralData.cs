//using OfficeOpenXml;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;

namespace WarGame_True.GamePlay.CombatPart {
    /// <summary>
    /// �����ScriptableObject��������ΪԤ����Ľ�������
    /// </summary>
    [CreateAssetMenu(fileName ="GeneralData", menuName = "WarGame/GeneralData")]
    public class GeneralData : ScriptableObject {

        public string generalName;

        public string generalChineseName;

        public string generalDescription;

        public string generalTag;


        [Header("�Ծ��ӵ�����")]
        public float organizationModify;
        public uint maxCommandUnits;
        public int visibleGrid;

        //public float supplyReduceCostModify;
        //public float supplyMaxModify;


        [Header("��ͬս���׶��¶Ծ��ӵ�λ������")]
        public UnitModify normalModify;

        public UnitModify standOffModify;

        public UnitModify siegeModify;

        public UnitModify engagementModify;

        public UnitModify openBattleModify;

        public UnitModify withdrawModify;

        // TODO��
        // �������� - ���ݶԾ��ӵ�λ���������
        [Header("��������")]
        public List<GeneralTrait> generalTraits = new List<GeneralTrait>();

        [Header("����ɲ��õ�ս��")]
        public List<Tactic> generalTactics = new List<Tactic>();

        // NOTICE: ��Ҫɾ!!!
        // TODO: excel�������ڷ�editor�ļ����µ���
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

            // ѵ����
            normalModify.discipineModify = Convert.ToSingle(rowdata[row, 11].Text);

            // �ƶ��ٶ�
            normalModify.speedModify = Convert.ToSingle(rowdata[row, 12].Text);

            // ����-����
            normalModify.armyAttackModify = Convert.ToSingle(rowdata[row, 13].Text);
            normalModify.armyDefendModify = Convert.ToSingle(rowdata[row, 14].Text);

            // ������Χ
            normalModify.armyAttackScopeModify = 0;

            // �Ƽ�-����
            normalModify.armyArmorCladkModify = Convert.ToSingle(rowdata[row, 15].Text);
            normalModify.armyArmorPenetrateModify = Convert.ToSingle(rowdata[row, 16].Text);

            // ʿ������-����
            normalModify.armyMoraleAttackModify = Convert.ToSingle(rowdata[row, 17].Text);
            normalModify.armyMoraleDefendModify = Convert.ToSingle(rowdata[row, 18].Text);
            normalModify.armyMoraleModifyInRear = Convert.ToSingle(rowdata[row, 19].Text);
            normalModify.armyMoraleModifyToRear = Convert.ToSingle(rowdata[row, 20].Text);

            // �ʽ𻨷�-��ļʱ��
            normalModify.costModify = 0;
            normalModify.recruitCostModify = 0;

            // ����������Я����
            normalModify.supplyCostModiy = Convert.ToSingle(rowdata[row, 21].Text);
            normalModify.supplyMaxModify = Convert.ToSingle(rowdata[row, 22].Text);

        }
*/
    }
}