using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.Infrastructure.NetworkPackage;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// �������ӵ�λ ��������
    /// </summary>
    public class ArmyUnit : GuidObject {
        private string armyUnitName;
        public string ArmyUnitName { get => armyUnitName; private set => armyUnitName = value; }

        private string armyUnitDescription;
        public string ArmyUnitDescription { get => armyUnitDescription; private set => armyUnitDescription = value; }


        private ArmyUnitType armyUnitType = ArmyUnitType.Infantry;
        public ArmyUnitType ArmyUnitType { get => armyUnitType; private set => armyUnitType = value; }

        private bool unitAlive = true;
        public bool IsAlive { get => unitAlive; private set => unitAlive = value; }


        #region ����-���� 
        [Header("��������")]
        private uint armyBaseAttack;
        private uint armyBaseDefend;
        public uint ArmyBaseAttack { get => (uint)(armyBaseAttack * (1 + unitModify.armyAttackModify)); private set => armyBaseAttack = value; }
        public uint ArmyBaseDefend { get => (uint)(armyBaseDefend * (1 + unitModify.armyDefendModify)); private set => armyBaseDefend = value; }

        // ������Χ
        private uint armyAttackScope;
        public uint ArmyAttackScope { get => armyAttackScope + unitModify.armyAttackScopeModify; private set => armyAttackScope = value; }


        // �������Ƽ�ֵ - ����������ֵ < 0 ʱ: ��������ɵ��˺����ٸ�ֵ
        private uint armorCladRate;
        private uint armorPenetration;
        public uint ArmorCladRate { get => (uint)(armorCladRate * (1 + unitModify.armyArmorCladkModify)); private set => armorCladRate = value; }
        public uint ArmorPenetration { get => (uint)(armorPenetration * (1 + unitModify.armyArmorPenetrateModify)); private set => armorPenetration = value; }

        #endregion

        #region ʿ�� �� ѵ����
        // ��λʿ������ - ��λʿ������ = ÿ����ɵ�ʿ���˺�����ֵ������ֵΪ0��
        private float armyBaseMoraleAttack = 1;
        private float armyBaseMoraleDefend = 1;
        private float armyBaseMoraleAttack_Rear = 0f;
        private float armyBaseMoraleDefend_Rear = 0f;
        private float armyBaseSpeed = 1;
        public float ArmyBaseMoraleAttack { get => armyBaseMoraleAttack * (1 + unitModify.armyMoraleAttackModify); private set => armyBaseMoraleAttack = value; }
        public float ArmyBaseMoraleDefend { get => armyBaseMoraleDefend * (1 + unitModify.armyMoraleDefendModify); private set => armyBaseMoraleDefend = value; }
        public float ArmyBaseMoraleAttack_Rear { get => armyBaseMoraleAttack_Rear * (1 + unitModify.armyMoraleModifyToRear); set => armyBaseMoraleAttack_Rear = value; }
        public float ArmyBaseMoraleDefend_Rear { get => armyBaseMoraleDefend_Rear * (1 + unitModify.armyMoraleModifyInRear); set => armyBaseMoraleDefend_Rear = value; }
        public float ArmyBaseSpeed { get => armyBaseSpeed * (1 + unitModify.speedModify); private set => armyBaseSpeed = value; }

        [Header("ʿ��ѵ����")]
        private float armyBaseMorale = 3.0f;
        private float armyBaseMaxMorale = 3.0f;
        public float ArmyBaseMorale { get => armyBaseMorale; private set => armyBaseMorale = value; }
        public float ArmyBaseMaxMorale { get => armyBaseMaxMorale * (1 + unitModify.maxMoraleModify); private set => armyBaseMaxMorale = value; }

        // ѵ���ȣ��ù�������
        private float armyBaseDiscipine = 1.0f;
        public float ArmyBaseDiscipine { get => armyBaseDiscipine * (1 + unitModify.discipineModify); private set => armyBaseDiscipine = value; }

        [Tooltip("ʿ���ָ��ٶ�")]
        private float recoverMoraleSpeed = 0.2f;
        public float RecoverMoraleSpeed { get => recoverMoraleSpeed * (1 + unitModify.recoverMoraleModify); private set => recoverMoraleSpeed = value; }

        [Tooltip("�����ָ��ٶ�")]
        private float recoverManpowerSpeed = 100;
        public float RecoverManpowerSpeed { get => recoverManpowerSpeed * (1 + unitModify.recoverManpowerModify); private set => recoverManpowerSpeed = value; }
        #endregion


        #region ���� ½������
        [Header("����")]
        [Tooltip("��󲹸�Я����/��ǰ������/ÿ�����ĵĲ�����")]
        private float supplyBaseMax = 10.0f;
        public float SupplyBaseMax { get => supplyBaseMax * (1 + unitModify.supplyMaxModify); private set => supplyBaseMax = value; }
        private float supplyDayCost;        // base cost: 1.0f
        public float SupplyDayCost { get => supplyDayCost * (1 + unitModify.supplyCostModiy); private set => supplyDayCost = value; }

        [Header("½������")]
        private uint armyCostMoney = 10;
        private uint armyCostBaseDay = 30;
        private uint armyCostManpower = 1000;
        private uint armyCurrentMappower = 1000;
        private uint armyReinforcementCost = 1;
        public uint ArmyCostMoney { get => (uint)(armyCostMoney * (1 + unitModify.costModify)); private set => armyCostMoney = value; }
        public uint ArmyCostBaseDay { get => (uint)(armyCostBaseDay * (1 + unitModify.recruitCostModify)); private set => armyCostBaseDay = value; }
        public uint ArmyCostManpower { get => armyCostManpower; private set => armyCostManpower = value; }
        public uint ArmyCurrentManpower { get => armyCurrentMappower; private set => armyCurrentMappower = value; }
        public uint ArmyReinforcementCost { get => armyReinforcementCost; set => armyReinforcementCost = value; }

        #endregion

        [Header("����ֵ")]
        public UnitModify unitModify = new UnitModify();

        public ArmyUnit() {
            InitThisUnitGuid();
        }

        public ArmyUnit(ArmyUnitData armyUnitData, NetworkGuid networkGuid) {

            // ÿ����λ����ʱ����һ��NetworkGuid,��������ͬ��
            InitThisUnitGuid(networkGuid);

            //�̳д� ArmyUnitData �л�ȡ��������
            ArmyUnitName = armyUnitData.armyUnitName;
            ArmyUnitDescription = armyUnitData.armyUnitDescription;
            ArmyUnitType = armyUnitData.armyUnitType;

            // ���� ����-����
            ArmyBaseAttack = armyUnitData.armyBaseAttack;
            ArmyBaseDefend = armyUnitData.armyBaseDefend;

            // ������Χ
            ArmyAttackScope = armyUnitData.armyAttackScope;

            // ������-�Ƽ�ֵ
            ArmorCladRate = armyUnitData.armorCladRate;
            ArmorPenetration = armyUnitData.armorPenetration;

            // ʿ������-����
            ArmyBaseMoraleAttack = armyUnitData.armyBaseMoraleAttack;
            ArmyBaseMoraleDefend = armyUnitData.armyBaseMoraleDefend;
            ArmyBaseSpeed = armyUnitData.armyBaseSpeed;

            // ʿ��ѵ����
            ArmyBaseMorale = armyUnitData.armyBaseMorale;
            ArmyBaseMaxMorale = armyUnitData.armyBaseMaxMorale;
            ArmyBaseMoraleAttack_Rear = armyUnitData.armyBaseMoraleAttack_Rear;
            ArmyBaseMoraleDefend_Rear = armyUnitData.armyBaseMoraleDefend_Rear;

            ArmyBaseDiscipine = armyUnitData.armyBaseDiscipine;
            RecoverMoraleSpeed = armyUnitData.recoverMoraleSpeed;
            // RecoverManpowerSpeed = 100; //�����ָ��ٶ��ǹ̶���ֵ����Ӧ����Ϊ���ֵĸı���ı�

            // �������
            SupplyBaseMax = armyUnitData.supplyBaseMax;
            SupplyDayCost = armyUnitData.supplyDayCost;

            // ����
            ArmyCostMoney = armyUnitData.armyCostMoney;
            ArmyCostBaseDay = armyUnitData.armyCostBaseDay;
            ArmyCostManpower = armyUnitData.armyCostManpower;
            ArmyCurrentManpower = armyUnitData.armyCurrentMappower;
            ArmyReinforcementCost = armyUnitData.armyReinforcementCost;
        }

        #region ���ľ��ӵ���ֵ
        // �þ��ӵ�λ���һ���ܵ����˺�
        public uint damageInRound = 0;
        public float moraleDamageInRound = 0;

        public void TakeDamage(uint damage) {
            ArmyCurrentManpower = (uint)Mathf.Max(0, ArmyCurrentManpower - damage);

            damageInRound = damage;
            if (ArmyCurrentManpower <= 0) {
                ArmyCurrentManpower = 0;
                IsAlive = false;
                //Debug.Log("this army has died!");
                return;
            }
        }

        public void SetCurManpower(uint manpower) {
            // ֱ�����õ�ǰ������
            ArmyCurrentManpower = (uint)Mathf.Clamp(manpower, 0, ArmyCostManpower);
        }

        public void TakeMoraleDamage(float damage, bool IsInRear = false) {
            if (IsInRear) {
                // �õ�λ�ں��ţ���Ҫ�����ʿ�������������
                damage *= (1.0f + unitModify.armyMoraleModifyInRear);
            }

            moraleDamageInRound = damage;
            if (ArmyBaseMorale <= 0) {
                ArmyBaseMorale = 0;
                //Debug.Log("this army has 0 morale!");
                return;
            }

            ArmyBaseMorale = Mathf.Max(0, ArmyBaseMorale - damage);
        }

        public void SetCurMorale(float morale) {
            // ֱ�����õ�ǰ��ʿ��
            ArmyBaseMorale = Mathf.Clamp(morale, 0, ArmyBaseMaxMorale);
        }

        /// <summary>
        /// ע�ᵽ�ս��¼��У�ÿ�ջָ�һ��ֵ��ʿ�����ָ���ֵ���ᳬ�����ʿ��
        /// </summary>
        public void RecoverMorale() {
            if (ArmyBaseMorale > ArmyBaseMaxMorale)
            {
                return;
            }
            ArmyBaseMorale = Mathf.Min(ArmyBaseMaxMorale, ArmyBaseMorale + RecoverMoraleSpeed);
        }

        public void RecoverMorale(float recoverMorale) {
            // ʿ�����Գ������ʿ��
            ArmyBaseMorale += recoverMorale;
        }

        /// <summary>
        /// ��������ʣ��þ�����ʧʿ����ע�ᵽ�ս��¼���
        /// </summary>
        public void LossMorale() {
            ArmyBaseMorale = Mathf.Max(0, ArmyBaseMorale - ArmyBaseMaxMorale * unitModify.dayLossModify);
        }

        public void LossMorale(float costMorale) {
            ArmyBaseMorale = Mathf.Max(0, ArmyBaseMorale - costMorale);
        }

        public void RecoverManpower(uint ImmediatelyManpower = 0) {
            ArmyCurrentManpower = (uint)Mathf.Min(ArmyCostManpower, ArmyCurrentManpower + RecoverManpowerSpeed + ImmediatelyManpower);
        }

        /// <summary>
        /// ��������ʣ��þ�����ʧ������ע�ᵽ�ս��¼���
        /// </summary>
        public void LossManpower() {
            ArmyCurrentManpower = (uint)Mathf.Max(0, ArmyCurrentManpower - ArmyCostManpower * unitModify.dayLossModify);
        }

        public void LossManpower(uint manpower) {
            ArmyCurrentManpower = (uint)Mathf.Max(0, ArmyCurrentManpower - manpower);
        }

        /// <summary>
        /// ִ���ƶ���ģ�ÿ���ƶ�ʱ����
        /// </summary>
        public void TakeMoveLoss() {
            ArmyBaseMorale = Mathf.Max(0, ArmyBaseMorale - ArmyBaseMaxMorale * unitModify.moveLossModify);
            ArmyCurrentManpower = (uint)Mathf.Max(0, ArmyCurrentManpower - unitModify.moveLossModify);
        }

        /// <summary>
        /// ���µ������ӵ�λ�ĸ�����ֵ
        /// </summary>
        public void RecaculateUnitStatus() {

            // ������е�����ֵ
            unitModify.ResetModify();

        }
        #endregion

        #region ���ӵ�ά����ʿ���ָ��������ָ�

        // ��ǰʿ�� ״�� �����ļӳ�
        // ʿ�� > 125% ʱ ���е�λ���� +20%
        // ʿ�� > 100% ʱ ���е�λ���� +10%
        // ʿ�� < 50% ʱ ���е�λ���� -10%
        // ʿ�� < 25% ʱ ���е�λ���� -20%�������ƶ���� +5% 
        private UnitModify curMoraleStageModify = new UnitModify();

        // ��ǰ���� ״�� �����ļӳ�
        // ������ < 50% ʱ, �����ƶ���� +1%, ���Ӳ�Ա�ٶ� -10%
        // ������ < 25% ʱ�������ƶ���� +3%�����Ӳ�Ա�ٶ� -25%
        // ������ < 10% ʱ�������ƶ���� +5%�����Ӳ�Ա�ٶ� -50%
        // ������ = 0   ʱ, �����ƶ���� +8%�����Ӳ�Ա�ٶ� -75%�����е�λ���� -20%
        private UnitModify curSupplyStageModify = new UnitModify();

        /// <summary>
        /// ���ݲ���״����ʿ��״����ά�����ã����ľ��ӵ�λ������
        /// </summary>
        public void CaculateMoraleSupplyStage(UnitModify moraleModify, UnitModify supplyModify) {
            // �Ƴ���һ������
            unitModify.ReduceUnitModify(curMoraleStageModify);
            unitModify.ReduceUnitModify(curSupplyStageModify);

            curMoraleStageModify = moraleModify;
            curSupplyStageModify = supplyModify;

            // ���modify
            unitModify.AddUnitModify(curMoraleStageModify);
            unitModify.AddUnitModify(curSupplyStageModify);
        }
        #endregion

        //TODO : �Ż��жϳ��˵�����
        /// <summary>
        /// �ж��Ƿ�Ӧ�ó��ˣ�ʿ��С�����ʿ��50%ʱ�����˸��ʿ�ʼ���
        /// </summary>
        public bool ShouldWithdraw() {

            // ��ʱ���ǵø�
            return ArmyBaseMorale < 0.5f || ArmyCurrentManpower < 200;

            //�� randomIndex < 100 * [1 - (ratio + 0.5 * ArmyBaseDiscipine)^2]ʱ �᳷��

            float ratioModify = 100 * Mathf.Pow(ArmyBaseMorale / ArmyBaseMaxMorale + 0.5f * ArmyBaseDiscipine, 2);
            int randomIndex = UnityEngine.Random.Range(1, 101);
            Debug.Log("the random int : " + randomIndex.ToString() + ", the modify:" + ratioModify);

            return randomIndex < ratioModify;
        }
        

    }

}