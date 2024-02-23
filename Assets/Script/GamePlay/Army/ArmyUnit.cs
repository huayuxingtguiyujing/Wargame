using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.Infrastructure.NetworkPackage;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// 单个军队单位 的数据类
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


        #region 攻击-防御 
        [Header("基础属性")]
        private uint armyBaseAttack;
        private uint armyBaseDefend;
        public uint ArmyBaseAttack { get => (uint)(armyBaseAttack * (1 + unitModify.armyAttackModify)); private set => armyBaseAttack = value; }
        public uint ArmyBaseDefend { get => (uint)(armyBaseDefend * (1 + unitModify.armyDefendModify)); private set => armyBaseDefend = value; }

        // 攻击范围
        private uint armyAttackScope;
        public uint ArmyAttackScope { get => armyAttackScope + unitModify.armyAttackScopeModify; private set => armyAttackScope = value; }


        // 攻击方破甲值 - 防御方披甲值 < 0 时: 攻击方造成的伤害减少该值
        private uint armorCladRate;
        private uint armorPenetration;
        public uint ArmorCladRate { get => (uint)(armorCladRate * (1 + unitModify.armyArmorCladkModify)); private set => armorCladRate = value; }
        public uint ArmorPenetration { get => (uint)(armorPenetration * (1 + unitModify.armyArmorPenetrateModify)); private set => armorPenetration = value; }

        #endregion

        #region 士气 与 训练度
        // 单位士气攻击 - 单位士气防御 = 每轮造成的士气伤害修正值（基础值为0）
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

        [Header("士气训练度")]
        private float armyBaseMorale = 3.0f;
        private float armyBaseMaxMorale = 3.0f;
        public float ArmyBaseMorale { get => armyBaseMorale; private set => armyBaseMorale = value; }
        public float ArmyBaseMaxMorale { get => armyBaseMaxMorale * (1 + unitModify.maxMoraleModify); private set => armyBaseMaxMorale = value; }

        // 训练度：让攻防翻倍
        private float armyBaseDiscipine = 1.0f;
        public float ArmyBaseDiscipine { get => armyBaseDiscipine * (1 + unitModify.discipineModify); private set => armyBaseDiscipine = value; }

        [Tooltip("士气恢复速度")]
        private float recoverMoraleSpeed = 0.2f;
        public float RecoverMoraleSpeed { get => recoverMoraleSpeed * (1 + unitModify.recoverMoraleModify); private set => recoverMoraleSpeed = value; }

        [Tooltip("人力恢复速度")]
        private float recoverManpowerSpeed = 100;
        public float RecoverManpowerSpeed { get => recoverManpowerSpeed * (1 + unitModify.recoverManpowerModify); private set => recoverManpowerSpeed = value; }
        #endregion


        #region 补给 陆军花费
        [Header("补给")]
        [Tooltip("最大补给携带量/当前补给量/每日消耗的补给量")]
        private float supplyBaseMax = 10.0f;
        public float SupplyBaseMax { get => supplyBaseMax * (1 + unitModify.supplyMaxModify); private set => supplyBaseMax = value; }
        private float supplyDayCost;        // base cost: 1.0f
        public float SupplyDayCost { get => supplyDayCost * (1 + unitModify.supplyCostModiy); private set => supplyDayCost = value; }

        [Header("陆军花费")]
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

        [Header("修正值")]
        public UnitModify unitModify = new UnitModify();

        public ArmyUnit() {
            InitThisUnitGuid();
        }

        public ArmyUnit(ArmyUnitData armyUnitData, NetworkGuid networkGuid) {

            // 每个单位生成时都有一个NetworkGuid,用于网络同步
            InitThisUnitGuid(networkGuid);

            //继承从 ArmyUnitData 中获取到的数据
            ArmyUnitName = armyUnitData.armyUnitName;
            ArmyUnitDescription = armyUnitData.armyUnitDescription;
            ArmyUnitType = armyUnitData.armyUnitType;

            // 基础 攻击-防御
            ArmyBaseAttack = armyUnitData.armyBaseAttack;
            ArmyBaseDefend = armyUnitData.armyBaseDefend;

            // 攻击范围
            ArmyAttackScope = armyUnitData.armyAttackScope;

            // 披甲率-破甲值
            ArmorCladRate = armyUnitData.armorCladRate;
            ArmorPenetration = armyUnitData.armorPenetration;

            // 士气攻击-防御
            ArmyBaseMoraleAttack = armyUnitData.armyBaseMoraleAttack;
            ArmyBaseMoraleDefend = armyUnitData.armyBaseMoraleDefend;
            ArmyBaseSpeed = armyUnitData.armyBaseSpeed;

            // 士气训练度
            ArmyBaseMorale = armyUnitData.armyBaseMorale;
            ArmyBaseMaxMorale = armyUnitData.armyBaseMaxMorale;
            ArmyBaseMoraleAttack_Rear = armyUnitData.armyBaseMoraleAttack_Rear;
            ArmyBaseMoraleDefend_Rear = armyUnitData.armyBaseMoraleDefend_Rear;

            ArmyBaseDiscipine = armyUnitData.armyBaseDiscipine;
            RecoverMoraleSpeed = armyUnitData.recoverMoraleSpeed;
            // RecoverManpowerSpeed = 100; //人力恢复速度是固定数值，不应该因为兵种的改变而改变

            // 补给相关
            SupplyBaseMax = armyUnitData.supplyBaseMax;
            SupplyDayCost = armyUnitData.supplyDayCost;

            // 花费
            ArmyCostMoney = armyUnitData.armyCostMoney;
            ArmyCostBaseDay = armyUnitData.armyCostBaseDay;
            ArmyCostManpower = armyUnitData.armyCostManpower;
            ArmyCurrentManpower = armyUnitData.armyCurrentMappower;
            ArmyReinforcementCost = armyUnitData.armyReinforcementCost;
        }

        #region 更改军队的数值
        // 该军队单位最近一次受到的伤害
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
            // 直接设置当前的人力
            ArmyCurrentManpower = (uint)Mathf.Clamp(manpower, 0, ArmyCostManpower);
        }

        public void TakeMoraleDamage(float damage, bool IsInRear = false) {
            if (IsInRear) {
                // 该单位在后排，需要与后排士气防御修正相乘
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
            // 直接设置当前的士气
            ArmyBaseMorale = Mathf.Clamp(morale, 0, ArmyBaseMaxMorale);
        }

        /// <summary>
        /// 注册到日结事件中，每日恢复一定值的士气，恢复后值不会超过最大士气
        /// </summary>
        public void RecoverMorale() {
            if (ArmyBaseMorale > ArmyBaseMaxMorale)
            {
                return;
            }
            ArmyBaseMorale = Mathf.Min(ArmyBaseMaxMorale, ArmyBaseMorale + RecoverMoraleSpeed);
        }

        public void RecoverMorale(float recoverMorale) {
            // 士气可以超过最大士气
            ArmyBaseMorale += recoverMorale;
        }

        /// <summary>
        /// 根据损耗率，让军队损失士气，注册到日结事件中
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
        /// 根据损耗率，让军队损失人力，注册到日结事件中
        /// </summary>
        public void LossManpower() {
            ArmyCurrentManpower = (uint)Mathf.Max(0, ArmyCurrentManpower - ArmyCostManpower * unitModify.dayLossModify);
        }

        public void LossManpower(uint manpower) {
            ArmyCurrentManpower = (uint)Mathf.Max(0, ArmyCurrentManpower - manpower);
        }

        /// <summary>
        /// 执行移动损耗，每次移动时调用
        /// </summary>
        public void TakeMoveLoss() {
            ArmyBaseMorale = Mathf.Max(0, ArmyBaseMorale - ArmyBaseMaxMorale * unitModify.moveLossModify);
            ArmyCurrentManpower = (uint)Mathf.Max(0, ArmyCurrentManpower - unitModify.moveLossModify);
        }

        /// <summary>
        /// 更新调整军队单位的各项数值
        /// </summary>
        public void RecaculateUnitStatus() {

            // 清空所有的修正值
            unitModify.ResetModify();

        }
        #endregion

        #region 军队的维护、士气恢复、人力恢复

        // 当前士气 状况 带来的加成
        // 士气 > 125% 时 所有单位攻击 +20%
        // 士气 > 100% 时 所有单位攻击 +10%
        // 士气 < 50% 时 所有单位防御 -10%
        // 士气 < 25% 时 所有单位防御 -20%，军队移动损耗 +5% 
        private UnitModify curMoraleStageModify = new UnitModify();

        // 当前补给 状况 带来的加成
        // 补给度 < 50% 时, 军队移动损耗 +1%, 军队补员速度 -10%
        // 补给度 < 25% 时，军队移动损耗 +3%，军队补员速度 -25%
        // 补给度 < 10% 时，军队移动损耗 +5%，军队补员速度 -50%
        // 补给度 = 0   时, 军队移动损耗 +8%，军队补员速度 -75%，所有单位防御 -20%
        private UnitModify curSupplyStageModify = new UnitModify();

        /// <summary>
        /// 根据补给状况、士气状况、维护费用，更改军队单位的修正
        /// </summary>
        public void CaculateMoraleSupplyStage(UnitModify moraleModify, UnitModify supplyModify) {
            // 移除上一个修正
            unitModify.ReduceUnitModify(curMoraleStageModify);
            unitModify.ReduceUnitModify(curSupplyStageModify);

            curMoraleStageModify = moraleModify;
            curSupplyStageModify = supplyModify;

            // 添加modify
            unitModify.AddUnitModify(curMoraleStageModify);
            unitModify.AddUnitModify(curSupplyStageModify);
        }
        #endregion

        //TODO : 优化判断撤退的条件
        /// <summary>
        /// 判断是否应该撤退，士气小于最大士气50%时，撤退概率开始变大
        /// </summary>
        public bool ShouldWithdraw() {

            // 暂时：记得改
            return ArmyBaseMorale < 0.5f || ArmyCurrentManpower < 200;

            //当 randomIndex < 100 * [1 - (ratio + 0.5 * ArmyBaseDiscipine)^2]时 会撤退

            float ratioModify = 100 * Mathf.Pow(ArmyBaseMorale / ArmyBaseMaxMorale + 0.5f * ArmyBaseDiscipine, 2);
            int randomIndex = UnityEngine.Random.Range(1, 101);
            Debug.Log("the random int : " + randomIndex.ToString() + ", the modify:" + ratioModify);

            return randomIndex < ratioModify;
        }
        

    }

}