using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.Infrastructure.NetworkPackage;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// 士兵单位 种类，可以自定义
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "ArmyUnitData", menuName = "WarGame/ArmyUnitData")]
    public class ArmyUnitData : GuidScriptableObject, INetworkSerializable {

        public string armyUnitName;
        public string armyUnitDescription;
        public ArmyUnitType armyUnitType = ArmyUnitType.Infantry;
        

        [Header("基础属性")]
        // 单位攻击 - 被击单位防御 = 造成的伤害值
        public uint armyBaseAttack;
        public uint armyBaseDefend;
        // 攻击范围
        public uint armyAttackScope;

        // 攻击方破甲值 - 防御方披甲值 < 0 时: 攻击方造成的伤害减少该值
        public uint armorCladRate;
        public uint armorPenetration;

        // 单位士气攻击 - 单位士气防御 = 每轮造成的士气伤害修正值（基础值为0）
        public float armyBaseMoraleAttack = 0.1f;
        public float armyBaseMoraleAttack_Rear = 0.01f;
        public float armyBaseMoraleDefend = 0f;
        public float armyBaseMoraleDefend_Rear = 0f;
        public float armyBaseSpeed = 1;
        
        [Header("士气训练度")]
        public float armyBaseMorale = 10.0f;
        public float armyBaseMaxMorale = 10.0f;
        
        // 训练度：让攻防翻倍
        public float armyBaseDiscipine = 1.0f;

        [Header("士气恢复、补给")]
        [Tooltip("士气恢复速度,每日恢复")]
        public float recoverMoraleSpeed = 0.2f;
        [Tooltip("最大补给携带量 与 当前补给量")]
        public float currentSupply = 10.0f;
        public float supplyBaseMax = 10.0f;
        public float supplyDayCost = 1.0f;

        [Header("陆军花费")]
        [Tooltip("招募费用")]
        public uint armyCostMoney = 1000;
        public uint armyCostBaseDay = 30;
        public uint armyCostManpower = 1000;
        public uint armyCurrentMappower = 1000;
        [Tooltip("军队维护费用")]
        public uint armyReinforcementCost = 100;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref armyUnitName);
            serializer.SerializeValue(ref armyUnitDescription);
            serializer.SerializeValue(ref armyUnitType);

            serializer.SerializeValue(ref armyBaseAttack);
            serializer.SerializeValue(ref armyBaseDefend);
            serializer.SerializeValue(ref armyAttackScope);

            serializer.SerializeValue(ref armorCladRate);
            serializer.SerializeValue(ref armorPenetration);

            serializer.SerializeValue(ref armyBaseMoraleAttack);
            serializer.SerializeValue(ref armyBaseMoraleAttack_Rear);
            serializer.SerializeValue(ref armyBaseMoraleDefend);
            serializer.SerializeValue(ref armyBaseMoraleDefend_Rear);
            serializer.SerializeValue(ref armyBaseSpeed);

            serializer.SerializeValue(ref armyBaseMorale);
            serializer.SerializeValue(ref armyBaseMaxMorale);

            serializer.SerializeValue(ref armyBaseDiscipine);
            serializer.SerializeValue(ref recoverMoraleSpeed);
            serializer.SerializeValue(ref currentSupply);
            serializer.SerializeValue(ref supplyBaseMax);

            serializer.SerializeValue(ref armyCostMoney);
            serializer.SerializeValue(ref armyCostBaseDay);
            serializer.SerializeValue(ref armyCostManpower);
            serializer.SerializeValue(ref armyCurrentMappower);
            serializer.SerializeValue(ref armyReinforcementCost);

        }
    }

    public enum ArmyUnitType {
        Infantry,
        Cavalry,
        Baggage
    }
}