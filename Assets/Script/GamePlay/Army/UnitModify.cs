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

        // 最大士气-士气恢复
        public float maxMoraleModify = 0;
        public float recoverMoraleModify = 0;

        // 训练度
        public float discipineModify = 0;

        // 移动速度
        public float speedModify = 0;

        // 攻击-防御
        public float armyAttackModify = 0;
        public float armyDefendModify = 0;

        // 攻击范围
        public uint armyAttackScopeModify = 0;

        // 破甲-披甲
        public float armyArmorCladkModify = 0;
        public float armyArmorPenetrateModify = 0;

        // 士气攻击-防御
        public float armyMoraleAttackModify = 0;
        public float armyMoraleDefendModify = 0;
        public float armyMoraleModifyInRear = 0;
        public float armyMoraleModifyToRear = 0;

        // 补给消耗与携带量
        public float supplyCostModiy = 0;
        public float supplyMaxModify = 0;

        // 资金花费-招募时间
        public float costModify = 0;
        public float recruitCostModify = 0;

        // 2023.12.31: 添加损耗相关的加成
        public float moveLossModify = 0;        // 移动时军队损耗
        public float dayLossModify = 0;         // 每日军队损耗

        // 2023.12.31: 军队补员速度
        public float recoverManpowerModify = 0;

        // 当前该修正的加成描述
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

            // 训练度
            discipineModify = reset;

            // 移动速度
            speedModify = reset;

            // 攻击-防御
            armyAttackModify = reset;
            armyDefendModify = reset;

            // 攻击范围
            armyAttackScopeModify = 0;

            // 破甲-披甲
            armyArmorCladkModify = reset;
            armyArmorPenetrateModify = reset;

            // 士气攻击-防御
            armyMoraleAttackModify = reset;
            armyMoraleDefendModify = reset;
            armyMoraleModifyInRear = reset;
            armyMoraleModifyToRear = reset;

            // 资金花费-招募时间
            costModify = 0;
            recruitCostModify = 0;

            // 补给消耗与携带量
            supplyCostModiy = reset;
            supplyMaxModify = reset;

            // 12.31new: 损耗
            moveLossModify = reset;
            dayLossModify = reset;

            // 12.31new: 士气/人力恢复
            recoverManpowerModify = reset;

            // 更新加成条目数据
            UpdateMessageItemList();
        }

        /// <summary>
        /// 对另一个UnitModify做加法
        /// </summary>
        public void AddUnitModify(UnitModify modify) {
            maxMoraleModify += modify.maxMoraleModify;
            recoverMoraleModify += modify.recoverMoraleModify;

            // 训练度
            discipineModify += modify.discipineModify;

            // 移动速度
            speedModify += modify.speedModify;

            // 攻击-防御
            armyAttackModify += modify.armyAttackModify;
            armyDefendModify += modify.armyDefendModify;

            // 攻击范围
            armyAttackScopeModify += modify.armyAttackScopeModify;

            // 破甲-披甲
            armyArmorCladkModify += modify.armyArmorCladkModify;
            armyArmorPenetrateModify += modify.armyArmorPenetrateModify;

            // 士气攻击-防御
            armyMoraleAttackModify += modify.armyMoraleAttackModify;
            armyMoraleDefendModify += modify.armyMoraleDefendModify;
            armyMoraleModifyInRear += modify.armyMoraleModifyInRear;
            armyMoraleModifyToRear += modify.armyMoraleModifyToRear;

            // 资金花费-招募时间
            costModify += modify.costModify;
            recruitCostModify += modify.recruitCostModify;

            supplyCostModiy += modify.supplyCostModiy;
            supplyMaxModify += modify.supplyMaxModify;

            // 12.31new: 损耗
            moveLossModify += modify.moveLossModify;
            dayLossModify += modify.dayLossModify;

            // 12.31new: 人力恢复
            recoverManpowerModify += modify.recoverManpowerModify;

            UpdateMessageItemList();
        }

        public void ReduceUnitModify(UnitModify modify) {
            maxMoraleModify -= modify.maxMoraleModify;
            recoverMoraleModify -= modify.recoverMoraleModify;

            // 训练度
            discipineModify -= modify.discipineModify;

            // 移动速度
            speedModify -= modify.speedModify;

            // 攻击-防御
            armyAttackModify -= modify.armyAttackModify;
            armyDefendModify -= modify.armyDefendModify;

            // 攻击范围
            armyAttackScopeModify -= modify.armyAttackScopeModify;

            // 破甲-披甲
            armyArmorCladkModify -= modify.armyArmorCladkModify;
            armyArmorPenetrateModify -= modify.armyArmorPenetrateModify;

            // 士气攻击-防御
            armyMoraleAttackModify -= modify.armyMoraleAttackModify;
            armyMoraleDefendModify -= modify.armyMoraleDefendModify;
            armyMoraleModifyInRear -= modify.armyMoraleModifyInRear;
            armyMoraleModifyToRear -= modify.armyMoraleModifyToRear;

            // 资金花费-招募时间
            costModify -= modify.costModify;
            recruitCostModify -= modify.recruitCostModify;

            supplyCostModiy -= modify.supplyCostModiy;
            supplyMaxModify -= modify.supplyMaxModify;

            // 12.31new: 损耗
            moveLossModify -= modify.moveLossModify;
            dayLossModify -= modify.dayLossModify;

            // 12.31new: 人力恢复
            recoverManpowerModify -= modify.recoverManpowerModify;

            UpdateMessageItemList();
        }

        #region 加成描述
        public static string maxMoraleModifyDescrip = "士气加成";
        public static string recoverMoraleModifyDescrip = "士气恢复加成";

        public static string discipineModifyDescrip = "训练度加成";

        public static string speedModifyDescrip = "移速加成";

        public static string armyAttackModifyDescrip = "攻击加成";
        public static string armyDefendModifyDescrip = "防御加成";

        public static string armyAttackScopeModifyDescrip = "攻击宽度";

        public static string armyArmorPenetrateModifyDescrip = "破甲加成";
        public static string armyArmorCladkModifyDescrip = "披甲加成";

        public static string armyMoraleAttackModifyDescrip = "士气攻击";
        public static string armyMoraleDefendModifyDescrip = "士气防御";
        public static string armyMoraleModifyInRearDescrip = "对后排士气攻击";
        public static string armyMoraleModifyToRearDescrip = "对后排士气防御";

        public static string supplyCostModiyDescrip = "补给消耗";
        public static string supplyMaxModifyDescrip = "补给携带";

        public static string moveLossModifyDescrip = "移动损耗";
        public static string dayLossModifyDescrip = "每日损耗";

        public static string recoverManpowerModifyDescrip = "人力恢复速度";
        #endregion

        /// <summary>
        /// 更新当前的各项加成描述
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
        /// 获取加成描述-交由外界调用
        /// </summary>
        /// <returns></returns>
        public MessageItemList GetModifyMessage() {
            return ModifyMessage.GetValidMessageList();
        }*/

    }

}