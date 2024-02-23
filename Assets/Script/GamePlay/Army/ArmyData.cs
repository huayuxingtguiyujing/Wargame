using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// 一支军团的数据类，军团能容纳多只军队
    /// </summary>
    [System.Serializable]
    public class ArmyData {

        [Tooltip("军队名称，可以重复")]
        public string armyName;

        [Tooltip("隶属于哪个势力")]
        public string ArmyTag { get; set; }
        public bool IsArmyTag(string tag) { return ArmyTag == tag;}

        [Tooltip("军队总数目")]
        public uint ArmyNum;
        public uint ArmyInfantryNum;
        public uint ArmyCavalryNum;
        public uint ArmyBaggageNum;
        
        // 训练度 是所有单位的平均值
        public float Discipine;

        // 士气 是所有单位的平均值
        public float CurMorale;
        public float MaxMorale;

        // 补给
        public float CurSupply;
        public float SupplyCost;
        public float MaxSupply;

        // 补给的来源
        public DetailMessage SupplyDetail;
        public float LocalSupplySupport;
        //public float ;
        public bool NeedSupplySupport;              // 需要建立补给运输线 以供给这只军队
        public float SupplyFromTrans = 0;           // 补给运输线提供的补给

        /// <summary>
        /// 为军队输送补给
        /// </summary>
        public void MakeArmyGetSupply(float SupplyAdd) {
            // TODO: 补给线 提供的补给值 怎么搞？？？！！！
            ChangeArmyCurSupply(SupplyAdd);
            SupplyDetail.AddMessage("后勤运输", SupplyAdd);
        }

        public void ChangeArmyCurSupply(float changeValue) {
            // 军队的当前补给 不会低于0 大于最大携带量
            CurSupply = Mathf.Clamp(CurSupply + changeValue, 0, MaxSupply);
        }

        // 当前该军团旗下 所有军队单位， 改变该值时请谨慎
        private List<ArmyUnit> armyUnitDatas;
        public List<ArmyUnit> ArmyUnitDatas { get => armyUnitDatas; set => armyUnitDatas = value; }

        /// <summary>
        /// 重新计算所有麾下单位的总人力数
        /// </summary>
        public void RecaculateArmyUnitManNums() {
            uint rec = 0;
            foreach (var armyUnit in ArmyUnitDatas)
            {
                rec += armyUnit.ArmyCurrentManpower;
            }
            ArmyNum = rec;
        }

        // 当前战役阶段的加成
        private UnitModify lastStageModify = new UnitModify();

        // 组织度: 组织度是军队行动、战斗时的必要品
        private int currentOrganization;
        public int CurrentOrganization { get => currentOrganization; private set => currentOrganization = value; }


        #region 将领相关

        // 当前将领: 将领可以影响单位属性
        private General currentGeneral;
        public General CurrentGeneral {
            get {
                if (currentGeneral == null) {
                    currentGeneral = General.GetNoLeader(ArmyTag);
                }
                return currentGeneral;
            }
            private set => currentGeneral = value;
        }
        public void AssignGeneral(General general) {
            // 移除上一个将领
            RemoveGeneral();

            // TODO: 更改将领当前信息，将领只能被任命到一个军队单位上

            CurrentGeneral = general;
            // 将将领对单位的修正值 附加到单位上
            AddGeneralModify(CurrentGeneral.NormalModify);
        }
        public void RemoveGeneral() {
            if (CurrentGeneral != null) {
                RemoveGeneralModify(CurrentGeneral.NormalModify);
                //TODO : 当前有将领时，需要更改将领当前信息
            }

            CurrentGeneral = General.GetNoLeader(ArmyTag);
            // 将将领对单位的修正值 附加到单位上
            AddGeneralModify(CurrentGeneral.NormalModify);
        }

        // TODO：写完
        /// <summary>
        /// 将将领对军队属性的修正值 附加到每个单位上
        /// </summary>
        private void AddGeneralModify(UnitModify unitModify) {
            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                armyUnit.unitModify.AddUnitModify(unitModify);
            }
        }

        private void RemoveGeneralModify(UnitModify unitModify) {
            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                armyUnit.unitModify.ReduceUnitModify(unitModify);
            }
        }

        #endregion

        public ArmyData() {
            armyName = "army01";
            ArmyTag = "NoOwner";

            ArmyNum = 0;
            ArmyInfantryNum = 0;
            ArmyCavalryNum = 0;
            ArmyBaggageNum = 0;

            Discipine = 0;
            MaxMorale = 1;
            CurMorale = 0;

            ArmyUnitDatas = new List<ArmyUnit>();
        }

        public ArmyData(ArmyData armyData) {
            armyName = armyData.armyName + "1";
            ArmyTag = armyData.ArmyTag;

            ArmyNum = 0;
            ArmyInfantryNum = 0;
            ArmyCavalryNum = 0;
            ArmyBaggageNum = 0;

            Discipine = armyData.Discipine;
            MaxMorale = armyData.MaxMorale;
            CurMorale = armyData.CurMorale;

            CurSupply = armyData.CurSupply;

            SupplyDetail = new DetailMessage();

            // TODO：这个构造函数没有半点意义，要改
            ArmyUnitDatas = new List<ArmyUnit>();
        }

        public ArmyData(ArmyUnit armyUnitData) {
            armyName = "army01";
            ArmyTag = "NoOwner";
            ArmyNum = armyUnitData.ArmyCostManpower;

            Discipine = armyUnitData.ArmyBaseDiscipine;
            MaxMorale = armyUnitData.ArmyBaseMaxMorale;
            CurMorale = armyUnitData.ArmyBaseMorale;

            // 创建时补给默认补满
            CurSupply = armyUnitData.SupplyBaseMax;

            SupplyDetail = new DetailMessage();

            ArmyUnitDatas = new List<ArmyUnit> {
                armyUnitData
            };

            // 初始时 设置无将领
            RemoveGeneral();

            ReCaculateStatu();
        }

        #region 拆解 合并军队
        /// <summary>
        /// 往该支军队里添加单位，各项数值需要重新计算
        /// </summary>
        public void AddUnit(ArmyUnit armyUnit) {
            if (ArmyUnitDatas == null) {
                ArmyUnitDatas = new List<ArmyUnit>();
            }

            //NOTICE：多线程中可能会生成同样的Guid！不能在这里初始化！
            // 初始化guid
            //armyUnitData.InitThisUnitGuid();

            // 加入到armyunitdatas
            ArmyUnitDatas.Add(armyUnit);
            ReCaculateStatu();
        }

        public void AddArmyData(ArmyData armyData) {
            // 单位增加
            foreach (var armyUnitData in armyData.ArmyUnitDatas) {
                AddUnit(armyUnitData);
            }
            ReCaculateStatu();
            // 补给增加
            ChangeArmyCurSupply(armyData.CurSupply);
        }

        public void SettleArmyData() {
            int count = ArmyUnitDatas.Count;
            for (int i = count - 1; i >= 0; i--) {
                if (!ArmyUnitDatas[i].IsAlive) {
                    RemoveUnit(ArmyUnitDatas[i]);
                }
            }
        }

        public void RemoveUnit(ArmyUnit armyUnit) {
            if (ArmyUnitDatas.Contains(armyUnit))
            {
                ArmyUnitDatas.Remove(armyUnit);
            }
            
            ReCaculateStatu();
        }

        /// <summary>
        /// 将当前的军队拆分成两只 军队单位平均分配 返回新拆分的ArmyData
        /// </summary>
        public ArmyData SplitArmyData() {
            int totalCount = ArmyUnitDatas.Count / 2;

            // 补给对半分
            ChangeArmyCurSupply(-CurSupply / 2);
            ArmyData armyData = new ArmyData(this);

            for (int i = 0; i < totalCount; i++) {
                //从当前军队中移除单位 并加入到新的armydata中
                armyData.AddUnit(ArmyUnitDatas[i]);
                ArmyUnitDatas.Remove(ArmyUnitDatas[i]);
            }

            // 新拆分的军队 设定为无将领
            armyData.RemoveGeneral();

            ReCaculateStatu();

            return armyData;
        }
        #endregion

        #region 军队状态更新: 补给、磨损、士气恢复、维护费用、人力恢复
        /// <summary>
        /// 随时间结算军队状态: 恢复补给、士气、计算损耗，需注册到日结事件中
        /// </summary>
        public void CountArmyStatu(float localGrainSupport) {
            foreach (ArmyUnit armyUnit in ArmyUnitDatas)
            {
                // 计算失去士气
                armyUnit.LossMorale();
                armyUnit.LossManpower();

                // 计算恢复
                armyUnit.RecoverMorale();
                armyUnit.RecoverManpower();
                // NOTICE: 补给不应该按单位计算，应该按军队计算

            }

            // TODO: 
            if(localGrainSupport < SupplyCost) {
                // 本地提供的粮食 不够消耗 则申请建立粮道
                NeedSupplySupport = true;
                //RequestGrainSupport();
            } else {
                NeedSupplySupport = false;
            }
            LocalSupplySupport = localGrainSupport;
            SupplyDetail.AddMessage("本地补给", localGrainSupport);

            // 更新补给时，计算 SupplyFromTrans
            CountArmySupply(localGrainSupport + SupplyFromTrans);

            ReCaculateStatu();
        }

        /// <summary>
        /// 计算军队的补给变化
        /// </summary>
        public void CountArmySupply(float supplySupport) {
            // 每一次计算后，都会把 SupplyFromTrans 置为0
            SupplyFromTrans = 0;
            ChangeArmyCurSupply(-SupplyCost + supplySupport);
        }

        public void RecoverArmy_Immediately(float Morale, float Supply, uint Manpower) {
            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                // 直接恢复军队的状态
                armyUnit.RecoverMorale(Morale);
                armyUnit.RecoverManpower(Manpower);
            }
            Debug.Log("army recover status!");
            ReCaculateStatu();
        }

        /// <summary>
        /// 获取维护这只军队的正常开销
        /// </summary>
        public float GetArmyMaintenanceCost() {
            uint maintenanceCost = 0;
            foreach (var unit in ArmyUnitDatas) {
                maintenanceCost += unit.ArmyReinforcementCost;
            }
            return maintenanceCost;
        }

        /// <summary>
        /// 这只军队已经拿到工资啦（晚唐的骄兵悍将，你懂的）
        /// </summary>
        public void MakeArmyPaied() {

        }

        public void MakeArmyUnpaied() {
            // TODO: 欠钱，军队各项数值下降，甚至可以叛变
        }
        
        #endregion

        #region 计算军队加成、数值
        // 当前
        public MoraleStage curArmyMoraleStage;

        public SupplyStage curArmySupplyStage;

        // 当前 士气/补给 阶段 的加成
        UnitModify moraleModify = new UnitModify();

        UnitModify supplyModify = new UnitModify();

        /// <summary>
        /// 根据现有的军队单位 重新计算军队的所有属性值
        /// </summary>
        public void ReCaculateStatu() {
            // TODO: 写完不同的补给、士气对军队战斗力的影响
            CaculateMoraleSupplyStage();

            float newMaxMorale = 0;
            float newMorale = 0;
            float newDiscipine = 0;

            float totalSupplyCost = 0;
            float unitMaxSupply = 0;

            uint newArmyNum = 0;
            uint newInfantryNum = 0;
            uint newCavalryNum = 0;
            uint newBaggageNum = 0;

            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                newMaxMorale += armyUnit.ArmyBaseMaxMorale;
                newMorale += armyUnit.ArmyBaseMorale;
                newDiscipine += armyUnit.ArmyBaseDiscipine;
                newArmyNum += armyUnit.ArmyCurrentManpower;

                totalSupplyCost += armyUnit.SupplyDayCost;
                unitMaxSupply += armyUnit.SupplyBaseMax;

                switch (armyUnit.ArmyUnitType) {
                    case ArmyUnitType.Infantry:
                        newInfantryNum += armyUnit.ArmyCurrentManpower;
                        break;
                    case ArmyUnitType.Cavalry:
                        newCavalryNum += armyUnit.ArmyCurrentManpower;
                        break;
                    case ArmyUnitType.Baggage:
                        newBaggageNum += armyUnit.ArmyCurrentManpower;
                        break;
                }
            }
            MaxMorale = newMaxMorale / ArmyUnitDatas.Count;
            CurMorale = newMorale / ArmyUnitDatas.Count;
            Discipine = newDiscipine / ArmyUnitDatas.Count;

            SupplyCost = totalSupplyCost;
            // 一只军队的补给携带上限,是所有单位的补给携带最大量的和
            MaxSupply = unitMaxSupply;

            ArmyNum = newArmyNum;
            ArmyInfantryNum = newInfantryNum;
            ArmyCavalryNum = newCavalryNum;
            ArmyBaggageNum = newBaggageNum;
        }

        /// <summary>
        /// 应用将领在不同战役阶段的修正，会同时移除在上一个阶段的修正
        /// </summary>
        /// <param name="combatStage">指定的战役阶段</param>
        /// <returns></returns>
        public UnitModify CaculateGeneralModifyInCombat(CombatStageEnum combatStage) {
            // 移除之前的军队加成
            if (lastStageModify != null) {
                RemoveGeneralModify(lastStageModify);
            }

            if (combatStage == CombatStageEnum.StandOff) {
                AddGeneralModify(CurrentGeneral.StandOffModify);
                return CurrentGeneral.StandOffModify;
            } else if (combatStage == CombatStageEnum.Siege) {
                AddGeneralModify(CurrentGeneral.SiegeModify);
                return CurrentGeneral.SiegeModify;
            } else if (combatStage == CombatStageEnum.Engagement) {
                AddGeneralModify(CurrentGeneral.EngagementModify);
                return CurrentGeneral.EngagementModify;
            } else if (combatStage == CombatStageEnum.OpenBattle) {
                AddGeneralModify(CurrentGeneral.OpenBattleModify);
                return CurrentGeneral.OpenBattleModify;
            } else if (combatStage == CombatStageEnum.WithdrawAndPursuit) {
                AddGeneralModify(CurrentGeneral.WithdrawModify);
                return CurrentGeneral.WithdrawModify;
            } else {
                return CurrentGeneral.StandOffModify;
            }
        }

        /// <summary>
        /// 计算不同 士气阶段 与 补给状况 对军队单位的影响
        /// </summary>
        private void CaculateMoraleSupplyStage() {

            // 士气 = 血量;
            float moraleRatio = CurMorale / MaxMorale;
            moraleModify.ResetModify();
            if (moraleRatio >= 1.25f) {
                // 士气 > 125% 时 所有单位攻击 +20%
                moraleModify.armyAttackModify = 0.2f;
                curArmyMoraleStage = MoraleStage.VeryHigh;
            } else if (moraleRatio < 1.25f && moraleRatio >= 1.0f) {
                // 士气 > 100% 时 所有单位攻击 +10%
                moraleModify.armyAttackModify = 0.1f;
                curArmyMoraleStage = MoraleStage.High;
            } else if (moraleRatio <= 0.5f && moraleRatio > 0.25f) {
                // 士气 < 50% 时 所有单位防御 -10%
                moraleModify.armyDefendModify = -0.1f;
                curArmyMoraleStage = MoraleStage.Low;
            } else if (moraleRatio <= 0.25f) {
                // 士气 < 25% 时 所有单位防御 -20%，军队移动损耗 +5% 
                moraleModify.armyDefendModify = -0.2f;
                moraleModify.moveLossModify = 0.05f;
                curArmyMoraleStage = MoraleStage.VeryLow;
            } else {
                curArmyMoraleStage = MoraleStage.Normal;
            }
            // 军队移动损耗: 每次军队移动将会损失的士气和人力比例（按最大值计算）
            // Q: 为什么士气低落的时候，移动会有损耗呢？
            // A: 因为士兵都跑光了

            // 当前补给程度
            float supplyRatio = CurSupply / MaxSupply;
            supplyModify.ResetModify();
            if (supplyRatio <= 0.5f && supplyRatio > 0.25f) {
                // 补给度 < 50% 时, 军队移动损耗 +1%, 军队补员速度 -10%
                moraleModify.moveLossModify = 0.01f;
                moraleModify.recoverManpowerModify = -0.10f;
                curArmySupplyStage = SupplyStage.Middle;
            } else if (supplyRatio <= 0.25 && supplyRatio > 0.1f) {
                // 补给度 < 25% 时，军队移动损耗 +3%，军队补员速度 -25%
                moraleModify.moveLossModify = 0.03f;
                moraleModify.recoverManpowerModify = -0.25f;
                curArmySupplyStage = SupplyStage.Low;
            } else if (supplyRatio <= 0.1f && supplyRatio > 0) {
                // 补给度 < 10% 时，军队移动损耗 +5%，军队补员速度 -50%
                moraleModify.moveLossModify = 0.05f;
                moraleModify.recoverManpowerModify = -0.50f;
                curArmySupplyStage = SupplyStage.VeryLow;
            } else if (supplyRatio == 0) {
                // 补给度 = 0   时, 军队移动损耗 +8%，军队补员速度 -75%，所有单位防御 -20%
                moraleModify.moveLossModify = 0.08f;
                moraleModify.recoverManpowerModify = -0.75f;
                moraleModify.armyDefendModify = -0.2f;
                curArmySupplyStage = SupplyStage.EatMan;
            } else {
                curArmySupplyStage = SupplyStage.Normal;
            }
            // Q: 这样的设计是说，粮食为0时，不移动就不会死吗？
            // A: 对的，大家都知道饿的时候就不要乱动

            // 更新到每个单位中
            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                armyUnit.CaculateMoraleSupplyStage(moraleModify, supplyModify);
            }

        }

        #endregion

    }

    
    public enum MoraleStage {
        VeryLow,        // 士气 < 25% 时 所有单位防御 -20%，军队移动损耗 +5% 
        Low,            // 士气 < 50% 时 所有单位防御 -10%
        Normal,         // 士气 > 50%，<100%
        High,           // 士气 > 100% 时 所有单位攻击 +10%
        VeryHigh,       // 士气 > 125% 时 所有单位攻击 +20%
    }

    public enum SupplyStage {
        Normal,         // 补给度 > 50% 时
        Middle,         // 补给度 < 50% 时, 军队移动损耗 +1%, 军队补员速度 -10%
        Low,            // 补给度 < 25% 时，军队移动损耗 +3%，军队补员速度 -25%
        VeryLow,        // 补给度 < 10% 时，军队移动损耗 +5%，军队补员速度 -50%
        EatMan          // 补给度 = 0   时, 军队移动损耗 +8%，军队补员速度 -75%，所有单位防御 -20%
    }

}