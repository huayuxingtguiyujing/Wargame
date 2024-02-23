using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.Infrastructure.NetworkPackage;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// ʿ����λ ���࣬�����Զ���
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "ArmyUnitData", menuName = "WarGame/ArmyUnitData")]
    public class ArmyUnitData : GuidScriptableObject, INetworkSerializable {

        public string armyUnitName;
        public string armyUnitDescription;
        public ArmyUnitType armyUnitType = ArmyUnitType.Infantry;
        

        [Header("��������")]
        // ��λ���� - ������λ���� = ��ɵ��˺�ֵ
        public uint armyBaseAttack;
        public uint armyBaseDefend;
        // ������Χ
        public uint armyAttackScope;

        // �������Ƽ�ֵ - ����������ֵ < 0 ʱ: ��������ɵ��˺����ٸ�ֵ
        public uint armorCladRate;
        public uint armorPenetration;

        // ��λʿ������ - ��λʿ������ = ÿ����ɵ�ʿ���˺�����ֵ������ֵΪ0��
        public float armyBaseMoraleAttack = 0.1f;
        public float armyBaseMoraleAttack_Rear = 0.01f;
        public float armyBaseMoraleDefend = 0f;
        public float armyBaseMoraleDefend_Rear = 0f;
        public float armyBaseSpeed = 1;
        
        [Header("ʿ��ѵ����")]
        public float armyBaseMorale = 10.0f;
        public float armyBaseMaxMorale = 10.0f;
        
        // ѵ���ȣ��ù�������
        public float armyBaseDiscipine = 1.0f;

        [Header("ʿ���ָ�������")]
        [Tooltip("ʿ���ָ��ٶ�,ÿ�ջָ�")]
        public float recoverMoraleSpeed = 0.2f;
        [Tooltip("��󲹸�Я���� �� ��ǰ������")]
        public float currentSupply = 10.0f;
        public float supplyBaseMax = 10.0f;
        public float supplyDayCost = 1.0f;

        [Header("½������")]
        [Tooltip("��ļ����")]
        public uint armyCostMoney = 1000;
        public uint armyCostBaseDay = 30;
        public uint armyCostManpower = 1000;
        public uint armyCurrentMappower = 1000;
        [Tooltip("����ά������")]
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