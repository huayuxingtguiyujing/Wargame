using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.Infrastructure.UIOrganizedMess;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.ArmyPart {
    
    [System.Serializable]
    public class UnitModify{

        // ���ʿ��-ʿ���ָ�
        public float maxMoraleModify = 0;
        public float recoverMoraleModify = 0;

        // ѵ����
        public float discipineModify = 0;

        // �ƶ��ٶ�
        public float speedModify = 0;

        // ����-����
        public float armyAttackModify = 0;
        public float armyDefendModify = 0;

        // ������Χ
        public uint armyAttackScopeModify = 0;

        // �Ƽ�-����
        public float armyArmorCladkModify = 0;
        public float armyArmorPenetrateModify = 0;

        // ʿ������-����
        public float armyMoraleAttackModify = 0;
        public float armyMoraleDefendModify = 0;
        public float armyMoraleModifyInRear = 0;
        public float armyMoraleModifyToRear = 0;

        // ����������Я����
        public float supplyCostModiy = 0;
        public float supplyMaxModify = 0;

        // �ʽ𻨷�-��ļʱ��
        public float costModify = 0;
        public float recruitCostModify = 0;

        // 2023.12.31: ��������صļӳ�
        public float moveLossModify = 0;        // �ƶ�ʱ�������
        public float dayLossModify = 0;         // ÿ�վ������

        // 2023.12.31: ���Ӳ�Ա�ٶ�
        public float recoverManpowerModify = 0;

        // ��ǰ�������ļӳ�����
        public DetailMessage ModifyMessage;

        public UnitModify(float modify = 0) {
            if (ModifyMessage == null) {
                ModifyMessage = new DetailMessage();
            }
            ResetModify(modify);
        }

        public void ResetModify(float reset = 0) {
            maxMoraleModify = reset;
            recoverMoraleModify = reset;

            // ѵ����
            discipineModify = reset;

            // �ƶ��ٶ�
            speedModify = reset;

            // ����-����
            armyAttackModify = reset;
            armyDefendModify = reset;

            // ������Χ
            armyAttackScopeModify = 0;

            // �Ƽ�-����
            armyArmorCladkModify = reset;
            armyArmorPenetrateModify = reset;

            // ʿ������-����
            armyMoraleAttackModify = reset;
            armyMoraleDefendModify = reset;
            armyMoraleModifyInRear = reset;
            armyMoraleModifyToRear = reset;

            // �ʽ𻨷�-��ļʱ��
            costModify = 0;
            recruitCostModify = 0;

            // ����������Я����
            supplyCostModiy = reset;
            supplyMaxModify = reset;

            // 12.31new: ���
            moveLossModify = reset;
            dayLossModify = reset;

            // 12.31new: ʿ��/�����ָ�
            recoverManpowerModify = reset;

            // ���¼ӳ���Ŀ����
            UpdateMessageItemList();
        }

        /// <summary>
        /// ����һ��UnitModify���ӷ�
        /// </summary>
        public void AddUnitModify(UnitModify modify) {
            maxMoraleModify += modify.maxMoraleModify;
            recoverMoraleModify += modify.recoverMoraleModify;

            // ѵ����
            discipineModify += modify.discipineModify;

            // �ƶ��ٶ�
            speedModify += modify.speedModify;

            // ����-����
            armyAttackModify += modify.armyAttackModify;
            armyDefendModify += modify.armyDefendModify;

            // ������Χ
            armyAttackScopeModify += modify.armyAttackScopeModify;

            // �Ƽ�-����
            armyArmorCladkModify += modify.armyArmorCladkModify;
            armyArmorPenetrateModify += modify.armyArmorPenetrateModify;

            // ʿ������-����
            armyMoraleAttackModify += modify.armyMoraleAttackModify;
            armyMoraleDefendModify += modify.armyMoraleDefendModify;
            armyMoraleModifyInRear += modify.armyMoraleModifyInRear;
            armyMoraleModifyToRear += modify.armyMoraleModifyToRear;

            // �ʽ𻨷�-��ļʱ��
            costModify += modify.costModify;
            recruitCostModify += modify.recruitCostModify;

            supplyCostModiy += modify.supplyCostModiy;
            supplyMaxModify += modify.supplyMaxModify;

            // 12.31new: ���
            moveLossModify += modify.moveLossModify;
            dayLossModify += modify.dayLossModify;

            // 12.31new: �����ָ�
            recoverManpowerModify += modify.recoverManpowerModify;

            UpdateMessageItemList();
        }

        public void ReduceUnitModify(UnitModify modify) {
            maxMoraleModify -= modify.maxMoraleModify;
            recoverMoraleModify -= modify.recoverMoraleModify;

            // ѵ����
            discipineModify -= modify.discipineModify;

            // �ƶ��ٶ�
            speedModify -= modify.speedModify;

            // ����-����
            armyAttackModify -= modify.armyAttackModify;
            armyDefendModify -= modify.armyDefendModify;

            // ������Χ
            armyAttackScopeModify -= modify.armyAttackScopeModify;

            // �Ƽ�-����
            armyArmorCladkModify -= modify.armyArmorCladkModify;
            armyArmorPenetrateModify -= modify.armyArmorPenetrateModify;

            // ʿ������-����
            armyMoraleAttackModify -= modify.armyMoraleAttackModify;
            armyMoraleDefendModify -= modify.armyMoraleDefendModify;
            armyMoraleModifyInRear -= modify.armyMoraleModifyInRear;
            armyMoraleModifyToRear -= modify.armyMoraleModifyToRear;

            // �ʽ𻨷�-��ļʱ��
            costModify -= modify.costModify;
            recruitCostModify -= modify.recruitCostModify;

            supplyCostModiy -= modify.supplyCostModiy;
            supplyMaxModify -= modify.supplyMaxModify;

            // 12.31new: ���
            moveLossModify -= modify.moveLossModify;
            dayLossModify -= modify.dayLossModify;

            // 12.31new: �����ָ�
            recoverManpowerModify -= modify.recoverManpowerModify;

            UpdateMessageItemList();
        }

        #region �ӳ�����
        public static string maxMoraleModifyDescrip = "ʿ���ӳ�";
        public static string recoverMoraleModifyDescrip = "ʿ���ָ��ӳ�";

        public static string discipineModifyDescrip = "ѵ���ȼӳ�";

        public static string speedModifyDescrip = "���ټӳ�";

        public static string armyAttackModifyDescrip = "�����ӳ�";
        public static string armyDefendModifyDescrip = "�����ӳ�";

        public static string armyAttackScopeModifyDescrip = "�������";

        public static string armyArmorPenetrateModifyDescrip = "�Ƽ׼ӳ�";
        public static string armyArmorCladkModifyDescrip = "���׼ӳ�";

        public static string armyMoraleAttackModifyDescrip = "ʿ������";
        public static string armyMoraleDefendModifyDescrip = "ʿ������";
        public static string armyMoraleModifyInRearDescrip = "�Ժ���ʿ������";
        public static string armyMoraleModifyToRearDescrip = "�Ժ���ʿ������";

        public static string supplyCostModiyDescrip = "��������";
        public static string supplyMaxModifyDescrip = "����Я��";

        public static string moveLossModifyDescrip = "�ƶ����";
        public static string dayLossModifyDescrip = "ÿ�����";

        public static string recoverManpowerModifyDescrip = "�����ָ��ٶ�";
        #endregion

        /// <summary>
        /// ���µ�ǰ�ĸ���ӳ�����
        /// </summary>
        /// <param name="ModifyMessage"></param>
        public void UpdateMessageItemList() {
            if (ModifyMessage == null) {
                ModifyMessage = new DetailMessage();
            }

            ModifyMessage.AddMessage(maxMoraleModifyDescrip, maxMoraleModify);
            ModifyMessage.AddMessage(recoverMoraleModifyDescrip, recoverMoraleModify);

            ModifyMessage.AddMessage(discipineModifyDescrip, discipineModify);

            ModifyMessage.AddMessage(speedModifyDescrip, speedModify);

            ModifyMessage.AddMessage(armyAttackModifyDescrip, armyAttackModify);
            ModifyMessage.AddMessage(armyDefendModifyDescrip, armyDefendModify);

            ModifyMessage.AddMessage(armyAttackScopeModifyDescrip, armyAttackScopeModify);

            ModifyMessage.AddMessage(armyArmorPenetrateModifyDescrip, armyArmorPenetrateModify);
            ModifyMessage.AddMessage(armyArmorCladkModifyDescrip, armyArmorCladkModify);

            ModifyMessage.AddMessage(armyMoraleAttackModifyDescrip, armyMoraleAttackModify);
            ModifyMessage.AddMessage(armyMoraleDefendModifyDescrip, armyMoraleDefendModify);
            ModifyMessage.AddMessage(armyMoraleModifyInRearDescrip, armyMoraleModifyInRear);
            ModifyMessage.AddMessage(armyMoraleModifyToRearDescrip, armyMoraleModifyToRear);

            ModifyMessage.AddMessage(supplyCostModiyDescrip, supplyCostModiy);
            ModifyMessage.AddMessage(supplyMaxModifyDescrip, supplyMaxModify);

            // 12.31new: 
            ModifyMessage.AddMessage(moveLossModifyDescrip, moveLossModify);
            ModifyMessage.AddMessage(dayLossModifyDescrip, dayLossModify);

            ModifyMessage.AddMessage(recoverManpowerModifyDescrip, recoverManpowerModify);

        }

/*
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref maxMoraleModify);
            serializer.SerializeValue(ref recoverMoraleModify);

            serializer.SerializeValue(ref discipineModify);
            serializer.SerializeValue(ref speedModify);

            serializer.SerializeValue(ref armyAttackModify);
            serializer.SerializeValue(ref armyDefendModify);

            serializer.SerializeValue(ref armyAttackScopeModify);

            serializer.SerializeValue(ref armyArmorCladkModify);
            serializer.SerializeValue(ref armyArmorPenetrateModify);

            serializer.SerializeValue(ref armyMoraleAttackModify);
            serializer.SerializeValue(ref armyMoraleDefendModify);
            serializer.SerializeValue(ref armyMoraleModifyInRear);
            serializer.SerializeValue(ref armyMoraleModifyToRear);

            serializer.SerializeValue(ref supplyCostModiy);
            serializer.SerializeValue(ref supplyMaxModify);

            serializer.SerializeValue(ref costModify);
            serializer.SerializeValue(ref recruitCostModify);

            serializer.SerializeValue(ref moveLossModify);
            serializer.SerializeValue(ref dayLossModify);

            serializer.SerializeValue(ref recoverManpowerModify);
            serializer.SerializeValue(ref ModifyMessage);
            //serializer.SerializeValue(ref );

        }
*/
        /*/// <summary>
        /// ��ȡ�ӳ�����-����������
        /// </summary>
        /// <returns></returns>
        public MessageItemList GetModifyMessage() {
            return ModifyMessage.GetValidMessageList();
        }*/

    }

}