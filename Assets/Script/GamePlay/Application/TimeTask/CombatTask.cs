using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Infrastructure.NetworkPackage;
using WarGame_True.Infrastructure.Recorder;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.Application.TimeTask {
    /// <summary>
    /// 战役事务
    /// </summary>
    /// <remarks>
    /// 需要注意：1.战役中的单位的更新会覆盖他处军队的更新，即战役开始后，其他地方的修正全部失效
    /// </remarks>
    public class CombatTask : BaseTask {

        #region 当前战役的双方

        public List<Army> Attackers { get; private set; }
        public List<Army> Defenders { get; private set; }

        public void GetArmySupply(bool IsAttacker) {
            if (IsAttacker) {

            } else {

            }
        }
        #endregion

        #region 前线-后方-溃退 部队
        // 双方后方部队
        Dictionary<BattlePlace, ArmyUnit> rearAttackerUnits;
        Dictionary<BattlePlace, ArmyUnit> rearDefenderUnits;

        // 双方前线部队
        // 前线最大可存在部队由战场宽度决定
        // （双方的战场宽度是不完全相同的，具体计算逻辑还未写）
        private int attackerWidth = 5;
        private int defenderWidth = 5;
        Dictionary<BattlePlace, ArmyUnit> frontAttackerUnits;
        Dictionary<BattlePlace, ArmyUnit> frontDefenderUnits;

        // 双方溃退部队
        Dictionary<BattlePlace, ArmyUnit> withdrawAttackerUnits;
        Dictionary<BattlePlace, ArmyUnit> withdrawDefenderUnits;

        private ArmyUnit ContainsInCombat(Guid guid, bool IsAttacker) {
            if (IsAttacker) {
                //查询 进攻方
                if (rearAttackerUnits.ContainsKey(guid)) {
                    return rearAttackerUnits[guid];
                } else if (withdrawAttackerUnits.ContainsKey(guid)) {
                    return withdrawAttackerUnits[guid];
                } else {
                    foreach (BattlePlace battlePlace in frontAttackerUnits.Keys) {
                        if (battlePlace.Guid == guid) return frontAttackerUnits[battlePlace];
                    }
                    return null;
                }
            } else {
                if (rearDefenderUnits.ContainsKey(guid)) {
                    return rearDefenderUnits[guid];
                } else if (withdrawDefenderUnits.ContainsKey(guid)) {
                    return withdrawDefenderUnits[guid];
                } else {
                    foreach (BattlePlace battlePlace in frontDefenderUnits.Keys) {
                        if (battlePlace.Guid == guid) return frontDefenderUnits[battlePlace];
                    }
                    return null;
                }
            }
        }

        private int GetTotalUnitCount(bool IsAttacker) {
            if (IsAttacker) {
                return rearAttackerUnits.Count + frontAttackerUnits.Count + withdrawAttackerUnits.Count;
            } else {
                return rearDefenderUnits.Count + frontDefenderUnits.Count + withdrawDefenderUnits.Count;
            }
        }
        /// <summary>
        /// 获得前线单位总数、总量
        /// </summary>
        public int GetFrontUnitCount(bool IsAttacker) {
            if (IsAttacker) {
                return frontAttackerUnits.Count;
            } else {
                return frontDefenderUnits.Count;
            }
        }
        public int GetFrontArmyNum(bool IsAttacker) {
            if (IsAttacker) {
                return GetArmyNum(frontAttackerUnits.Values.ToArray());
            } else {
                return GetArmyNum(frontDefenderUnits.Values.ToArray());
            }
        }
        /// <summary>
        /// 前线是否存在单位
        /// </summary>
        private bool IsFrontExistUnit(bool IsAttacker) {
            return GetFrontUnitCount(IsAttacker) > 0;
        }

        /// <summary>
        /// 获得后方单位总数、总量
        /// </summary>
        public int GetRearUnitCount(bool IsAttacker) {
            if (IsAttacker) {
                return rearAttackerUnits.Count;
            } else {
                return rearDefenderUnits.Count;
            }
        }
        public int GetRearArmyNum(bool IsAttacker) {
            if (IsAttacker) {
                return GetArmyNum(rearAttackerUnits.Values.ToArray());
            } else {
                return GetArmyNum(rearDefenderUnits.Values.ToArray());
            }
        }
        /// <summary>
        /// 获得撤退单位总数、总量
        /// </summary>
        public int GetWithdrawUnitCount(bool IsAttacker) {
            if (IsAttacker) {
                return withdrawAttackerUnits.Count;
            } else {
                return withdrawDefenderUnits.Count;
            }
        }
        public int GetWithdrawArmyNum(bool IsAttacker) {
            if (IsAttacker) {
                return GetArmyNum(withdrawAttackerUnits.Values.ToArray());
            } else {
                return GetArmyNum(withdrawDefenderUnits.Values.ToArray());
            }
        }

        private int GetArmyNum(ArmyUnit[] armyUnits) {
            uint num = 0;
            foreach (ArmyUnit unit in armyUnits) {
                num += unit.ArmyCurrentManpower;
            }
            return (int)num;
        }
        /// <summary>
        /// 获取当前各类型单位总量和军队总量
        /// </summary>
        public void GetTotalArmyNum(bool IsAttacker, out uint totalInfantryNum, out uint totalCavalryNum, out uint totalNum) {
            totalInfantryNum = 0;
            totalCavalryNum = 0;
            totalNum = 0;
            if (IsAttacker) {
                foreach (ArmyUnit unit in rearAttackerUnits.Values) {
                    totalNum += unit.ArmyCurrentManpower;
                    if (unit.ArmyUnitType == ArmyUnitType.Infantry) {
                        totalInfantryNum += unit.ArmyCurrentManpower;
                    } else if (unit.ArmyUnitType == ArmyUnitType.Cavalry) {
                        totalCavalryNum += unit.ArmyCurrentManpower;
                    }
                }
                foreach (ArmyUnit unit in frontAttackerUnits.Values) {
                    totalNum += unit.ArmyCurrentManpower;
                    if (unit.ArmyUnitType == ArmyUnitType.Infantry) {
                        totalInfantryNum += unit.ArmyCurrentManpower;
                    } else if (unit.ArmyUnitType == ArmyUnitType.Cavalry) {
                        totalCavalryNum += unit.ArmyCurrentManpower;
                    }
                }
                foreach (ArmyUnit unit in withdrawAttackerUnits.Values) {
                    totalNum += unit.ArmyCurrentManpower;
                    if (unit.ArmyUnitType == ArmyUnitType.Infantry) {
                        totalInfantryNum += unit.ArmyCurrentManpower;
                    } else if (unit.ArmyUnitType == ArmyUnitType.Cavalry) {
                        totalCavalryNum += unit.ArmyCurrentManpower;
                    }
                }

            } else {
                foreach (ArmyUnit unit in rearDefenderUnits.Values) {
                    totalNum += unit.ArmyCurrentManpower;
                    if (unit.ArmyUnitType == ArmyUnitType.Infantry) {
                        totalInfantryNum += unit.ArmyCurrentManpower;
                    } else if (unit.ArmyUnitType == ArmyUnitType.Cavalry) {
                        totalCavalryNum += unit.ArmyCurrentManpower;
                    }
                }
                foreach (ArmyUnit unit in frontDefenderUnits.Values) {
                    totalNum += unit.ArmyCurrentManpower;
                    if (unit.ArmyUnitType == ArmyUnitType.Infantry) {
                        totalInfantryNum += unit.ArmyCurrentManpower;
                    } else if (unit.ArmyUnitType == ArmyUnitType.Cavalry) {
                        totalCavalryNum += unit.ArmyCurrentManpower;
                    }
                }
                foreach (ArmyUnit unit in withdrawDefenderUnits.Values) {
                    totalNum += unit.ArmyCurrentManpower;
                    if (unit.ArmyUnitType == ArmyUnitType.Infantry) {
                        totalInfantryNum += unit.ArmyCurrentManpower;
                    } else if (unit.ArmyUnitType == ArmyUnitType.Cavalry) {
                        totalCavalryNum += unit.ArmyCurrentManpower;
                    }
                }
            }
            return;
        }
        public int GetTotalArmyNum(bool IsAttacker) {
            return GetWithdrawArmyNum(IsAttacker) + GetRearArmyNum(IsAttacker) + GetFrontArmyNum(IsAttacker);
        }
        /// <summary>
        /// 查询军队当前士气与最大士气
        /// </summary>
        public void GetArmyMorale(bool IsAttacker, out float maxMorale, out float morale) {
            maxMorale = 0;
            morale = 0;
            if (IsAttacker) {
                foreach (ArmyUnit unit in rearAttackerUnits.Values) {
                    morale += unit.ArmyBaseMorale;
                    maxMorale += unit.ArmyBaseMaxMorale;
                }
                foreach (ArmyUnit unit in frontAttackerUnits.Values) {
                    morale += unit.ArmyBaseMorale;
                    maxMorale += unit.ArmyBaseMaxMorale;
                }
                foreach (ArmyUnit unit in withdrawAttackerUnits.Values) {
                    morale += unit.ArmyBaseMorale;
                    maxMorale += unit.ArmyBaseMaxMorale;
                }
                morale /= GetTotalUnitCount(true);
                maxMorale /= GetTotalUnitCount(true);
            } else {
                foreach (ArmyUnit unit in rearDefenderUnits.Values) {
                    morale += unit.ArmyBaseMorale;
                    maxMorale += unit.ArmyBaseMaxMorale;
                }
                foreach (ArmyUnit unit in frontDefenderUnits.Values) {
                    morale += unit.ArmyBaseMorale;
                    maxMorale += unit.ArmyBaseMaxMorale;
                }
                foreach (ArmyUnit unit in withdrawDefenderUnits.Values) {
                    morale += unit.ArmyBaseMorale;
                    maxMorale += unit.ArmyBaseMaxMorale;
                }
                morale /= GetTotalUnitCount(false);
                maxMorale /= GetTotalUnitCount(false);
            }
        }
        /// <summary>
        /// 获取在对面的 在攻击范围内的 单位
        /// </summary>
        private List<ArmyUnit> GetInFrontUnit(BattlePlace battlePlace, ArmyUnit armyUnit, bool IsAttacker) {
            List<ArmyUnit> units = new List<ArmyUnit>();

            if (IsAttacker) {
                foreach (BattlePlace bp in frontAttackerUnits.Keys) {
                    //检测是否在攻击范围中
                    if (bp.InAttackScope(battlePlace, armyUnit.ArmyAttackScope)) {

                    }
                }
            } else {

            }

            return units;
        }

        // 获取前线、撤退、后方单位 BattlePlaceNetwork 列表
        public BattlePlaceNetwork[] GetFrontUnitBPList(bool IsAttacker) {
            if (IsAttacker) {
                BattlePlaceNetwork[] rec = new BattlePlaceNetwork[GetFrontUnitCount(true)];

                int i = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> pair in frontAttackerUnits) {
                    rec[i] = new BattlePlaceNetwork(
                        true, pair.Key, pair.Value.ArmyCurrentManpower, pair.Value.ArmyBaseMorale
                    );
                    i++;
                }
                return rec;

            } else {
                BattlePlaceNetwork[] rec = new BattlePlaceNetwork[GetFrontUnitCount(false)];

                int i = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> pair in frontDefenderUnits) {
                    rec[i] = new BattlePlaceNetwork(
                        true, pair.Key, pair.Value.ArmyCurrentManpower, pair.Value.ArmyBaseMorale
                    );
                    i++;
                }
                return rec;
            }
        }

        public BattlePlaceNetwork[] GetRearUnitBPList(bool IsAttacker) {
            if (IsAttacker) {
                BattlePlaceNetwork[] rec = new BattlePlaceNetwork[GetRearUnitCount(true)];

                int i = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> pair in rearAttackerUnits) {
                    rec[i] = new BattlePlaceNetwork(
                        true, pair.Key, pair.Value.ArmyCurrentManpower, pair.Value.ArmyBaseMorale
                    );
                    i++;
                }
                return rec;

            } else {
                BattlePlaceNetwork[] rec = new BattlePlaceNetwork[GetRearUnitCount(false)];

                int i = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> pair in rearDefenderUnits) {
                    rec[i] = new BattlePlaceNetwork(
                        true, pair.Key, pair.Value.ArmyCurrentManpower, pair.Value.ArmyBaseMorale
                    );
                    i++;
                }
                return rec;
            }
        }

        public BattlePlaceNetwork[] GetWithdrawUnitBPList(bool IsAttacker) {
            if (IsAttacker) {
                BattlePlaceNetwork[] rec = new BattlePlaceNetwork[GetWithdrawUnitCount(true)];

                int i = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> pair in withdrawAttackerUnits) {
                    rec[i] = new BattlePlaceNetwork(
                        true, pair.Key, pair.Value.ArmyCurrentManpower, pair.Value.ArmyBaseMorale
                    );
                    i++;
                }
                return rec;

            } else {
                BattlePlaceNetwork[] rec = new BattlePlaceNetwork[GetWithdrawUnitCount(false)];

                int i = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> pair in withdrawDefenderUnits) {
                    rec[i] = new BattlePlaceNetwork(
                        true, pair.Key, pair.Value.ArmyCurrentManpower, pair.Value.ArmyBaseMorale
                    );
                    i++;
                }
                return rec;
            }
        }

        #endregion


        // 战役描述信息
        public CombatMessage CombatCountMessage;

        #region 战场要素变动
        // TODO: 加入网络同步中

        // 战役阶段
        public CombatStage CombatStage;
        
        private BaseRecorder CombatRecorder;

        // 每隔两个时辰就会变动的 战斗随机加成
        private BaseRecorder RandomAddRecorder;

        public uint AttackRandomAdd = 1;

        public uint DefendRandomAdd = 1;

        // 每隔两个时辰 允许变动 双方战术
        private BaseRecorder TacticRecorder;
        private bool AbleToChangeTactic_Attack = true;
        private bool AbleToChangeTactic_Defend = true;
        public void SetAttackTactic(Tactic tactic) {
            if (AbleToChangeTactic_Attack) {
                AbleToChangeTactic_Attack = false;
                CombatStage.SetAttackTactic(tactic);
            }
        }
        public void SetDefendTactic(Tactic tactic) {
            if (AbleToChangeTactic_Defend) {
                AbleToChangeTactic_Defend = false;
                CombatStage.SetDefendTactic(tactic);
            }
        }

        // 双方的将领
        private General attackerLeader;
        private General defenderLeader;
        public General AttackerLeader { get => attackerLeader; set => attackerLeader = value; }
        public General DefenderLeader { get => defenderLeader; set => defenderLeader = value; }

        // TODO：当前双方的buff
        // TODO: 加入网络同步中
        private List<string> currentAttackBuffs;
        private List<string> currentDefendBuffs; 
        #endregion

        public CombatTask(uint costTime, List<Army> attackers, List<Army> defenders) : base(TaskType.Hour, costTime) {
            this.Attackers = attackers;
            this.Defenders = defenders;

            // 双方后方部队
            rearAttackerUnits = new Dictionary<BattlePlace, ArmyUnit>();
            rearDefenderUnits = new Dictionary<BattlePlace, ArmyUnit>();

            // 开始时，将所有部队加入到后方部队中
            foreach (Army army in attackers){
                foreach (ArmyUnit unit in army.ArmyData.ArmyUnitDatas){
                    try {
                        rearAttackerUnits.Add(unit.Guid, unit);
                    } catch {
                        Debug.Log("attacker warning!GUID problem: " + unit.Guid + "," + unit);
                    }
                }
            }

            foreach (Army army in defenders) {
                foreach (ArmyUnit unit in army.ArmyData.ArmyUnitDatas) {
                    try {
                        rearDefenderUnits.Add(unit.Guid, unit);
                    } catch {
                        Debug.Log("defender warning!GUID problem: " + unit.Guid + "," + unit);
                    }
                }
            }

            // 双方前线部队
            frontAttackerUnits = new Dictionary<BattlePlace, ArmyUnit>();
            frontDefenderUnits = new Dictionary<BattlePlace, ArmyUnit>();

            // 双方溃退部队
            withdrawAttackerUnits = new Dictionary<BattlePlace, ArmyUnit>();
            withdrawDefenderUnits = new Dictionary<BattlePlace, ArmyUnit>();

            // 初始化作战数据
            GetTotalArmyNum(true, out uint totalInfantryNum_A, out uint totalCavalryNum_A, out uint totalNum_A);
            GetTotalArmyNum(true, out uint totalInfantryNum_D, out uint totalCavalryNum_D, out uint totalNum_D);
            CombatCountMessage = new CombatMessage(
                totalNum_A, totalInfantryNum_A, totalCavalryNum_A,
                totalNum_D, totalInfantryNum_D, totalCavalryNum_D
            );

            Debug.Log("start init!" + "D rear:" + rearDefenderUnits.Count + ",A rear:" + rearAttackerUnits.Count);

            // TODO: 下面这一部分需要考虑网络同步
            // 设置双方的将领 和 初始阶段将领加成(TODO : 需要继续完善)
            AttackerLeader = attackers[0].ArmyData.CurrentGeneral;
            DefenderLeader = defenders[0].ArmyData.CurrentGeneral;
            CurAttackerGeneralModify = new UnitModify();
            CurDefenderGeneralModify = new UnitModify();

            // 设置初始时的战役阶段
            CombatStage = new StandOffStage();
            CombatStage.EnterStage();
            UpdateGeneralStageModify(CombatStage);

            //RenewCombatUnits();

            //Debug.Log("inited!" + "attacker front:" + frontAttackerUnits.Count + ",defender front:" + frontDefenderUnits.Count);
        }

        public override bool CountTask() {
            RenewCombatUnits();
            RenewCombatFactors();
            return IsOver;
        }

        #region 战斗进程
        uint attackerDamage = 0;
        float attackerMoraleDamage = 0;
        uint defenderDamage = 0;
        float defenderMoraleDamage = 0;

        /// <summary>
        /// 更新战场部队，注册到小时结事件中
        /// </summary>
        private void RenewCombatUnits() {
            InitFrontUnits();

            // 计算前线部队的伤亡状况
            TakeDamageToFront();

            // 更新到对应的军队当中
            RenewUnitToArmy();

            try {
                WithdrawAndReinforce();
            } catch {
                Debug.Log("后方人员未能及时填补到前线！");
            }

            // 判断是否满足撤退条件
            JudgeEnterWithdraw();

            // 判断是否满足强制结束条件

        }

        /// <summary>
        /// 初始化前线部队，仅当前线部队为空时会执行逻辑
        /// </summary>
        private void InitFrontUnits() {

            // 前线没有部队
            if (frontAttackerUnits.Count <= 0 && rearAttackerUnits.Count > 0) {
                // 随机调度一批部队上阵
                for (int i = 0; i < attackerWidth; i++) {
                    if (rearAttackerUnits.Count > 0) {
                        KeyValuePair<BattlePlace, ArmyUnit> rearPair = rearAttackerUnits.GetRandomPair();
                        // 新建一个战场位置
                        BattlePlace battlePlace = new BattlePlace(rearPair.Key, i);
                        frontAttackerUnits.Add(battlePlace, rearPair.Value);

                        // 从后方部队中移除
                        rearAttackerUnits.Remove(rearPair.Key);
                    } else {
                        break;
                    }
                }
            }

            if (frontDefenderUnits.Count <= 0 && rearDefenderUnits.Count > 0) {
                for (int i = 0; i < defenderWidth; i++) {
                    if (rearDefenderUnits.Count > 0) {
                        KeyValuePair<BattlePlace, ArmyUnit> rearPair = rearDefenderUnits.GetRandomPair();
                        // 新建一个战场位置
                        BattlePlace battlePlace = new BattlePlace(rearPair.Key, i);
                        frontDefenderUnits.Add(battlePlace, rearPair.Value);

                        //从后方部队中移除
                        rearDefenderUnits.Remove(rearPair.Key);
                    } else {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 根据双方前线部队的数值，计算对双方部队的伤害
        /// </summary>
        private void TakeDamageToFront() {
            // 前线有部队时
            // 1.根据前线的双方部队状况计算对对方的伤害
            if (frontAttackerUnits.Count >= 0) {

                /* 旧的作战逻辑，直接根据双方前线部队 计算攻防平均值，施加以伤害
                 * // (1).重置该轮的伤亡计算
                attackerDamage = 0;
                attackerMoraleDamage = 0;
                
                CaculateDamange(frontAttackerUnits.Values.ToArray(), frontDefenderUnits.Values.ToArray(), ref attackerDamage, ref attackerMoraleDamage);
                // (2).更新到前线和后方部队中
                foreach (ArmyUnit armyUnitData in frontDefenderUnits.Values) {
                    armyUnitData.TakeDamage(attackerDamage);
                    armyUnitData.TakeMoraleDamage(attackerMoraleDamage);
                }*/

                /*int currentAttackWidth = frontAttackerUnits.Count;
                int attackOrder = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> attackPair in frontAttackerUnits)
                {
                    // (1).获得攻击者的总数、在前线的次序
                    int currentDefendWidth = frontDefenderUnits.Count;
                    int defendOrder = 0;
                    foreach (KeyValuePair<BattlePlace, ArmyUnit> defendPair in frontDefenderUnits)
                    {
                        // (2).根据防御者与攻击者的数目 计算战线修正值
                        int offset = Mathf.Abs((currentDefendWidth - currentAttackWidth) / 2);
                        if (currentAttackWidth > currentDefendWidth) {
                        } else {
                            // 防御值比攻击者多时
                            offset = -offset;
                        }

                        if (attackOrder + (attackPair.Value.ArmyAttackScope - 1) / 2 >= defendOrder + offset
                            && attackOrder - (attackPair.Value.ArmyAttackScope - 1) / 2 <= defendOrder + offset) {
                            // (3).在攻击范围内,则对该单位执行攻击
                            uint manDamage = CaculateDamage(attackPair.Value, defendPair.Value);
                            float moraleDamage = CaculateMoraleDamage(attackPair.Value, defendPair.Value);
                            attackerDamage += manDamage;
                            attackerMoraleDamage += moraleDamage;
                            defendPair.Value.TakeDamage(manDamage);
                            defendPair.Value.TakeMoraleDamage(moraleDamage);

                        }
                        defendOrder++;
                    }

                    // (4).后方部队也会受到部分士气伤害
                    if(attackPair.Value.ArmyBaseMoraleAttack_Rear > 0) {
                        foreach (ArmyUnit armyUnitData in rearDefenderUnits.Values) {
                            float moraleDamage = CaculateMoraleDamage_Rear(attackPair.Value, armyUnitData);
                            armyUnitData.TakeMoraleDamage(moraleDamage);
                        }
                    }
                    

                    attackOrder++;
                }*/

                // TODO：建议从 CombatStage.stageIntensity 字段做手脚,解决战斗阶段的问题
                uint outManDamage = 0;
                float outMoraleDamage = 0;
                if (IsFrontExistUnit(IsAttacker: false)) {
                    // 对方前线存在单位，所以对对方的前线造成伤害
                    TakeDamageToFront(ref frontAttackerUnits, ref frontDefenderUnits,
                    AttackRandomAdd, CombatStage.stageIntensity, CombatStage.stageMoraleIntensity, CombatStage.attackAdd, CombatStage.AttackTactic, CombatStage.DefendTactic,
                    out outManDamage, out outMoraleDamage);
                } else{
                    // TODO: 需要测试
                    // 对方前线没有单位，则我方的前线单位根据攻击范围，随机挑选敌方任意一个单位造成伤害
                    TakeDamageWhenNoFrontUnit(ref frontAttackerUnits, ref withdrawDefenderUnits, 
                    AttackRandomAdd, CombatStage.stageIntensity, CombatStage.stageMoraleIntensity, CombatStage.attackAdd, CombatStage.AttackTactic, CombatStage.DefendTactic,
                    out outManDamage, out outMoraleDamage);
                }
                
                // 记录每轮造成的伤害值
                attackerDamage = outManDamage;
                // NOTICE：士气伤害要除以当前总军队数量
                GetTotalArmyNum(false, out uint totalInfantryNum, out uint totalCavalryNum, out uint totalNum);
                attackerMoraleDamage = outMoraleDamage / GetFrontUnitCount(true);

                Debug.Log("A damage:" + attackerDamage + ",A moraleDam:" + attackerMoraleDamage);
            }

            if (frontDefenderUnits.Count >= 0) {
                /* 旧的作战逻辑，直接根据双方前线部队 计算攻防平均值，施加以伤害
                 * defenderDamage = 0;
                defenderMoraleDamage = 0;
                CaculateDamange(frontDefenderUnits.Values.ToArray(), frontAttackerUnits.Values.ToArray(), ref defenderDamage, ref defenderMoraleDamage);

                foreach (ArmyUnit armyUnitData in frontAttackerUnits.Values) {
                    armyUnitData.TakeDamage(defenderDamage);
                    armyUnitData.TakeMoraleDamage(defenderMoraleDamage);
                }

                foreach (ArmyUnit armyUnitData in rearAttackerUnits.Values) {
                    armyUnitData.TakeMoraleDamage(defenderMoraleDamage * 0.2f);
                }

                Debug.Log("D damage:" + defenderDamage + ",D moraleDam:" + defenderMoraleDamage);*/
                uint outManDamage = 0;
                float outMoraleDamage = 0;
                if (IsFrontExistUnit(IsAttacker: true)) {
                    TakeDamageToFront(ref frontDefenderUnits, ref frontAttackerUnits,
                    DefendRandomAdd, CombatStage.stageIntensity, CombatStage.stageMoraleIntensity, CombatStage.defendAdd, CombatStage.DefendTactic, CombatStage.AttackTactic,
                    out outManDamage, out outMoraleDamage);
                } else {
                    TakeDamageWhenNoFrontUnit(ref frontDefenderUnits, ref withdrawAttackerUnits,
                    DefendRandomAdd, CombatStage.stageIntensity, CombatStage.stageMoraleIntensity, CombatStage.defendAdd, CombatStage.DefendTactic, CombatStage.AttackTactic,
                    out outManDamage, out outMoraleDamage);
                }
                    

                defenderDamage = outManDamage;
                GetTotalArmyNum(true, out uint totalInfantryNum, out uint totalCavalryNum, out uint totalNum);
                defenderMoraleDamage = outMoraleDamage / GetFrontUnitCount(false);

                Debug.Log("D damage:" + defenderDamage + ",D moraleDam:" + defenderMoraleDamage);
            }

        }

        private void TakeDamageToFront(ref Dictionary<BattlePlace, ArmyUnit> frontAttackerUnits, ref Dictionary<BattlePlace, ArmyUnit> frontDefenderUnits, 
            uint RandomAdd, float intensity, float moraleIntensity, float stageAdd, Tactic AttackTactic, Tactic DefendTactic,
            out uint OutManDamage, out float OutMoraleDamage) {
            OutManDamage = 0;
            OutMoraleDamage = 0;

            int currentAttackWidth = frontAttackerUnits.Count;
            int attackOrder = 0;
            foreach (KeyValuePair<BattlePlace, ArmyUnit> attackPair in frontAttackerUnits) {
                // (1).获得 攻击者 防御者 的总数、在前线的次序
                int currentDefendWidth = frontDefenderUnits.Count;
                int defendOrder = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> defendPair in frontDefenderUnits) {
                    // (2).根据防御者与攻击者的数目 计算战线修正值
                    int offset = Mathf.Abs((currentDefendWidth - currentAttackWidth) / 2);
                    if (currentAttackWidth > currentDefendWidth) {
                    } else {
                        // 防御值比攻击者多时
                        offset = -offset;
                    }

                    if (attackOrder + (attackPair.Value.ArmyAttackScope - 1) / 2 >= defendOrder + offset
                        && attackOrder - (attackPair.Value.ArmyAttackScope - 1) / 2 <= defendOrder + offset) {
                        // (3).在攻击范围内,则对该单位执行攻击
                        uint manDamage = CaculateDamage(attackPair.Value, defendPair.Value, RandomAdd, AttackTactic, DefendTactic);
                        manDamage = (uint)(manDamage * intensity * (1 + stageAdd));
                        float moraleDamage = CaculateMoraleDamage(attackPair.Value, defendPair.Value, AttackTactic, DefendTactic);
                        moraleDamage = moraleDamage * moraleIntensity * (1 + stageAdd);

                        // (4).记录伤害
                        OutManDamage += manDamage;
                        OutMoraleDamage += moraleDamage;

                        // (5).执行伤害
                        defendPair.Value.TakeDamage(manDamage);
                        defendPair.Value.TakeMoraleDamage(moraleDamage);

                    }
                    defendOrder++;
                }

                // (4).后方部队也会受到部分士气伤害
                if (attackPair.Value.ArmyBaseMoraleAttack_Rear > 0) {
                    foreach (ArmyUnit armyUnitData in rearDefenderUnits.Values) {
                        float moraleDamageRear = CaculateMoraleDamage_Rear(attackPair.Value, armyUnitData);
                        OutMoraleDamage += moraleDamageRear;
                        armyUnitData.TakeMoraleDamage(moraleDamageRear);
                    }
                }

                attackOrder++;
            }
        }

        /// <summary>
        /// 计算当一方没有前线单位时，对方对其造成的伤害
        /// </summary>
        /// <param name="frontAttackerUnits">攻击者的前线单位</param>
        /// <param name="withdrawDefenderUnits">防御者的所有撤退单位</param>
        /// <param name="RandomAdd">随机数加成</param>
        /// <param name="intensity"></param>
        /// <param name="moraleIntensity"></param>
        /// <param name="stageAdd"></param>
        /// <param name="AttackTactic"></param>
        /// <param name="DefendTactic"></param>
        /// <param name="OutManDamage"></param>
        /// <param name="OutMoraleDamage"></param>
        private void TakeDamageWhenNoFrontUnit(ref Dictionary<BattlePlace, ArmyUnit> frontAttackerUnits, ref Dictionary<BattlePlace, ArmyUnit> withdrawDefenderUnits,
            uint RandomAdd, float intensity, float moraleIntensity, float stageAdd, Tactic AttackTactic, Tactic DefendTactic,
            out uint OutManDamage, out float OutMoraleDamage) {

            // TODO: 伤害逻辑：
            // 每次结算伤害时，如果撤退方的前线没有单位，
            // 则追击方的前线单位随机挑选撤退方(攻击范围个单位)造成伤害
            OutManDamage = 0;
            OutMoraleDamage = 0;

            List<BattlePlace> guidList = withdrawDefenderUnits.Keys.ToList();
            int GuidsCount = guidList.Count;
            foreach (KeyValuePair<BattlePlace, ArmyUnit> attackPair in frontAttackerUnits) {
                // (1).遍历每一个攻击者的前线单位，获得其攻击范围
                int attackScope = (int)attackPair.Value.ArmyAttackScope;
                for(int i = 0; i < attackScope; i++) {
                    // (2).每一个前线单位，"随机"对(攻击范围)个撤退单位造成伤害
                    int attackTargetIndex = UnityEngine.Random.Range(0, GuidsCount);
                    BattlePlace attackTargetGuid = guidList[attackTargetIndex];

                    // (3).在攻击范围内,则对该单位执行攻击
                    uint manDamage = CaculateDamage(attackPair.Value, withdrawDefenderUnits[attackTargetGuid], RandomAdd, AttackTactic, DefendTactic);
                    manDamage = (uint)(manDamage * intensity * (1 + stageAdd));
                    float moraleDamage = CaculateMoraleDamage(attackPair.Value, withdrawDefenderUnits[attackTargetGuid], AttackTactic, DefendTactic);
                    moraleDamage = moraleDamage * moraleIntensity * (1 + stageAdd);

                    // (4).记录伤害
                    OutManDamage += manDamage;
                    OutMoraleDamage += moraleDamage;

                    // (5).对该单位执行伤害
                    withdrawDefenderUnits[attackTargetGuid].TakeDamage(manDamage);
                    withdrawDefenderUnits[attackTargetGuid].TakeMoraleDamage(moraleDamage);
                }
            }
        }

        /// <summary>
        /// 检测前线是否有溃退的单位，有则将后方部队填补上
        /// </summary>
        private void WithdrawAndReinforce() {
            // 检测溃退，填补部队到前线
            if (frontAttackerUnits.Count > 0) {

                // (1).遍历检测是否有部队满足溃退条件（溃退应该考虑概率）
                ArmyUnit[] attackersList = frontAttackerUnits.Values.ToArray();
                BattlePlace[] attackerGuids = frontAttackerUnits.Keys.ToArray();

                for (int i = 0; i < attackerGuids.Length; i++) {
                    if (frontAttackerUnits[attackerGuids[i]].ShouldWithdraw()) {

                        // (2).将满足溃退条件的部队单位，加入到溃退队列中
                        withdrawAttackerUnits.Add(attackerGuids[i], frontAttackerUnits[attackerGuids[i]]);

                        // (3).从前线部队中移除
                        int orderInFront = attackerGuids[i].CountInFront;
                        frontAttackerUnits.Remove(attackerGuids[i]);
                        
                        // (4).随机调度一个可用的后方部队，加入到前线中的对应位置
                        if (rearAttackerUnits.Count >= 1) {
                            KeyValuePair<BattlePlace, ArmyUnit> availableUnit = rearAttackerUnits.GetRandomPair();

                            BattlePlace battlePlace = new BattlePlace(availableUnit.Key, orderInFront);
                            frontAttackerUnits.Add(battlePlace, availableUnit.Value);
                            rearAttackerUnits.Remove(availableUnit.Key);
                        }

                    }
                }
            }

            if (frontDefenderUnits.Count > 0) {
                ArmyUnit[] defendersList = frontDefenderUnits.Values.ToArray();
                BattlePlace[] defenderGuids = frontDefenderUnits.Keys.ToArray();

                for (int i = 0; i < defendersList.Length; i++) {
                    if (defendersList[i].ShouldWithdraw()) {
                        // (2).从前线部队中移除
                        frontDefenderUnits.Remove(defenderGuids[i]);
                        // (3).将满足溃退条件的部队单位，加入到溃退队列中
                        int orderInFront = defenderGuids[i].CountInFront;
                        withdrawDefenderUnits.Add(defenderGuids[i], defendersList[i]);
                        // (4).随机调度一个可用的后方部队，加入到前线中
                        if (rearDefenderUnits.Count >= 1) {
                            KeyValuePair<BattlePlace, ArmyUnit> availableUnit = rearDefenderUnits.GetRandomPair();

                            BattlePlace battlePlace = new BattlePlace(availableUnit.Key, orderInFront);
                            frontDefenderUnits.Add(battlePlace, availableUnit.Value);
                            rearDefenderUnits.Remove(availableUnit.Key);
                        }

                    }
                }
            }

            Debug.Log("Hour result: " + "D rear:" + rearDefenderUnits.Count + ",A rear:" + rearAttackerUnits.Count + ",D front:" + frontDefenderUnits.Count + ",A front:" + frontAttackerUnits.Count);
        }

        /// <summary>
        /// 将所有当前场上部队的状态更新到对应的军队中
        /// </summary>
        private void RenewUnitToArmy() {

            // 刷新军队士气、补给、等带给军队单位的影响


            // 将战役中单位的状态同步到军队里
            int attackerTotalCount = GetTotalUnitCount(true);
            foreach (Army attacker in Attackers)
            {
                // 将每个ArmyUnit更新到Army中
                for (int i = 0; i < attacker.ArmyData.ArmyUnitDatas.Count; i++) {
                    Guid guid = attacker.ArmyData.ArmyUnitDatas[i].Guid;
                    // 找到在战场对应的ArmyUnit
                    ArmyUnit armyUnit = ContainsInCombat(guid, true);
                    if (armyUnit != null) {
                        // NOTICE: 这样更新可能会覆盖掉Army在其他地方的更新
                        // 若能找到对应 则更新
                        attacker.ArmyData.ArmyUnitDatas[i] = armyUnit;
                    }
                }

                // 刷新ui显示,计算人数伤害总量，计算士气伤害平均值
                //uint totalDefenderDamage = (uint)defenderDamage;
                //float averageMoraleDamage = defenderMoraleDamage;
                attacker.UpdateArmyUI();
                //attacker.UpdateArmyUI(totalDefenderDamage, averageMoraleDamage);
            }


            int defenderTotalCount = GetTotalUnitCount(false);
            foreach (Army defender in Defenders) 
            {
                for (int i = 0; i < defender.ArmyData.ArmyUnitDatas.Count; i++) {
                    Guid guid = defender.ArmyData.ArmyUnitDatas[i].Guid;
                    ArmyUnit armyUnit = ContainsInCombat(guid, false);
                    if (armyUnit != null) {
                        defender.ArmyData.ArmyUnitDatas[i] = armyUnit;
                    }
                }

                //uint totalAttackerDamage = (uint)attackerDamage;
                //float averageMoraleDamage = attackerMoraleDamage;
                defender.UpdateArmyUI();
                //defender.UpdateArmyUI(totalAttackerDamage, averageMoraleDamage);
            }
        }

        /// <summary>
        /// 判断是否满足撤退条件，并结算双方
        /// </summary>
        private void JudgeEnterWithdraw() {
            // 如果在撤退阶段 则不判断是否满足进入撤退阶段的条件
            if (CombatStage is WithdrawAndPursuitStage) {
                return;
            }

            if ((rearAttackerUnits.Count <= 0 && frontAttackerUnits.Count <= 0)) {
                // 防御方是胜利者
                //Debug.Log("the defenders win!");

                // TODO: 生成一个结算数据，同时提供接口给其他部分获取结算数据
                GetCombatCompleteMessage(false);

                // TODO: 改为转入撤退阶段，撤退阶段之后，再结束事务
                EnterWithdrawStage(IsAttackerWithdraw: true);
                //ForceToComplete();
            }else if ((rearDefenderUnits.Count <= 0 && frontDefenderUnits.Count <= 0)) {
                // 进攻方是胜利者
                //Debug.Log("the attackers win!");

                GetCombatCompleteMessage(true);

                // TODO: 改为转入撤退阶段，撤退阶段之后，再结束事务
                EnterWithdrawStage(IsAttackerWithdraw: false);
                //ForceToComplete();
            }
        }

        /// <summary>
        /// 进入撤退阶段
        /// </summary>
        public void EnterWithdrawStage(bool IsAttackerWithdraw) {
            CombatStage = new WithdrawAndPursuitStage();
            CombatStage.WithdrawStage(IsAttackerWithdraw);
            CombatRecorder.RestartRecorder(UnityEngine.Random.Range(5, 15));
            //Debug.Log("设置了撤退阶段:" + CombatRecorder.LastCountTime);
            // 刷新双方军队的将领加成
            UpdateGeneralStageModify(CombatStage);
        }

        /// <summary>
        /// 判断是否满足强制结束条件
        /// </summary>
        private void JudgeEnterOver() {
            GetTotalArmyNum(true, out uint At_totalInfantryNum, out uint At_totalCavalryNum, out uint At_totalNum);
            if (At_totalNum <= 0) {
                ForceToComplete();
            }
            GetTotalArmyNum(false, out uint De_totalInfantryNum, out uint De_totalCavalryNum, out uint De_totalNum);
            if (De_totalNum <= 0) {
                ForceToComplete();
            }
        }

        /// <summary>
        /// 结束对战，让双方军队退出战斗
        /// </summary>
        public override void ForceToComplete() {
            base.ForceToComplete();

            ////结束对战方的战斗状态
            //ArmyHelper.GetInstance().ExitCombat(Attackers);
            ArmyNetworkCtrl.Instance.ExitCombatEvent(Attackers);
            //ArmyHelper.GetInstance().ExitCombat(Defenders);
            ArmyNetworkCtrl.Instance.ExitCombatEvent(Defenders);

            // 清算军队单位,没有人力会被销毁
            foreach (Army army in Attackers) {
                army.SettlementArmyData();
            }
            foreach (Army army in Defenders) {
                army.SettlementArmyData();
            }

            if (CombatCountMessage.AttackerWin) {
                if (Defenders.Count >= 1) {
                    // 防御方还有单位 则进行撤退
                    Province withdrawStart = Defenders[0].CurrentProvince;
                    Province withdrawTarget = ArmyController.Instance.GetWithdrawTarget(Defenders[0].ArmyData.ArmyTag, withdrawStart);
                    //ArmyController.Instance.SetArmyDestination(withdrawTarget, Defenders, true);
                    ArmyNetworkCtrl.Instance.MoveArmyEvent(Defenders, withdrawTarget.provinceData.provinceID, true);
                }

                if (Attackers.Count >= 1) {
                    // 战胜方留在本省份
                    //ArmyHelper.GetInstance().SetArmyStayIn(Attackers);
                    ArmyNetworkCtrl.Instance.SetArmyStayInEvent(Attackers);
                }

            } 
            else {
                if (Attackers.Count >= 1) {
                    Province withdrawStart = Attackers[0].CurrentProvince;
                    Province withdrawTarget = ArmyController.Instance.GetWithdrawTarget(Attackers[0].ArmyData.ArmyTag, withdrawStart);
                    //ArmyController.Instance.SetArmyDestination(withdrawTarget, Attackers, true);
                    ArmyNetworkCtrl.Instance.MoveArmyEvent(Attackers, withdrawTarget.provinceData.provinceID, true);
                }

                if (Defenders.Count >= 1) {
                    //ArmyHelper.GetInstance().SetArmyStayIn(Defenders);
                    ArmyNetworkCtrl.Instance.SetArmyStayInEvent(Defenders);
                }
            }

        }

        #endregion

        #region 战场阶段变化、战场要素变动

        public UnitModify CurAttackerGeneralModify { get; private set; }
        public UnitModify CurDefenderGeneralModify { get; private set; }

        /// <summary>
        /// 更新战场当前局势，包括战役阶段、采用的战术（概率改变）、其他战场要素，注册到小时结事件中
        /// </summary>
        private void RenewCombatFactors() {
            RenewCombatStage();
            RenewCombatTactic();
            RenewBuff();
        }

        /// <summary>
        /// 更新战役阶段，结算在不同战役阶段的buff
        /// </summary>
        private void RenewCombatStage() { 
            if(CombatRecorder == null) {
                // 获取一个随机数，作为该战役阶段的倒计时
                int stageCount = UnityEngine.Random.Range(3, 30);
                CombatRecorder = new BaseRecorder(stageCount);
            }

            CombatRecorder.CountRecorder();

            if (CombatRecorder.IsOver()) {
                Debug.Log("Recorder , 当前阶段到期: " + CombatStage.stageName);
                // 撤退阶段到期，结束事务
                if (CombatStage is WithdrawAndPursuitStage) {
                    ForceToComplete();
                    return;
                }

                // 合战阶段不结束，直到一方撤退为止
                if (CombatStage is OpenBattleStage) {
                    return;
                }

                // 战役阶段结束 切换到下一个阶段，刷新Recorder，
                CombatStage = CombatStage.ExitStage();
                CombatRecorder.RestartRecorder(UnityEngine.Random.Range(3, 10));

                // TODO : 测试下
                // 刷新双方军队的将领加成
                UpdateGeneralStageModify(CombatStage);

                Debug.Log("当前战役阶段结束，进入下一个战役阶段:" + CombatStage.stageName);
                
            }
        }

        /// <summary>
        /// 加载将领在不同战役阶段的加成
        /// </summary>
        /// <remarks>
        /// 开局时调用，切换战役阶段时调用
        /// </remarks>
        private void UpdateGeneralStageModify(CombatStage combatStage) {
            foreach (Army army in Attackers) {
                CurAttackerGeneralModify = army.ArmyData.CaculateGeneralModifyInCombat(combatStage.stageName);
            }
            //Debug.Log("CurAttackerGeneralModify: " + (CurAttackerGeneralModify == null) + ", stage is : " + CombatStage.stageName);

            foreach (Army army in Defenders) {
                CurDefenderGeneralModify = army.ArmyData.CaculateGeneralModifyInCombat(combatStage.stageName);
            }

        }

        private void RenewCombatTactic() {
            if(TacticRecorder == null) {
                TacticRecorder = new BaseRecorder(2);
            }

            TacticRecorder.CountRecorder();

            if (TacticRecorder.IsOver()) {
                // 重置计时 允许双方切换战术
                AbleToChangeTactic_Attack = true;
                AbleToChangeTactic_Defend = true;
                TacticRecorder.RestartRecorder();
            }
        }

        private void RenewBuff() {
            if(RandomAddRecorder == null) {
                // 目前为4小时 更换一次随机加成
                RandomAddRecorder = new BaseRecorder(4);
            }

            RandomAddRecorder.CountRecorder();

            if (RandomAddRecorder.IsOver()) {
                // 计时结束 重置随机加成
                AttackRandomAdd = (uint)UnityEngine.Random.Range(0, 30);
                DefendRandomAdd = (uint)UnityEngine.Random.Range(0, 30);
                RandomAddRecorder.RestartRecorder();
            }
        }
        
        #endregion

        #region 计算对战时的各项数值

        private uint CaculateDamage(ArmyUnit attacker, ArmyUnit defender, uint RandomAdd, Tactic AttackTactic, Tactic DefendTactic) {
            // 计算 基础攻防
            uint attack = (uint)(attacker.ArmyBaseAttack * (1 + AttackTactic.armyAttackModify));
            uint defend = (uint)(defender.ArmyBaseDefend * (1 + DefendTactic.armyDefendModify));

            uint damage_a_d = (uint)Mathf.Max(0, attack - defend);

            // 计算 破甲披甲
            uint armorPenetrate = (uint)(attacker.ArmorPenetration * (1 + AttackTactic.armyArmorPenetrateModify));
            uint armorClad = defender.ArmorCladRate;

            uint damage_c_p = armorPenetrate > armorClad ? (armorPenetrate - armorClad) / 2 : armorClad - armorPenetrate;

            // 计算 其他修正

            // 计算公式：人员杀伤 = 随机值 + (攻方攻击 - 防方防御) - (防御方披甲率 - 攻击方破甲值) + 其他修正（！） 
            uint manDamage = (uint)UnityEngine.Random.Range(0, 5) * RandomAdd + damage_a_d + damage_c_p;

            
            // 根据当前的战役烈度 战场bufff 计算加成

            return manDamage;
        }

        private float CaculateMoraleDamage(ArmyUnit attacker, ArmyUnit defender, Tactic AttackTactic, Tactic DefendTactic) {
            float attackMorale = attacker.ArmyBaseMoraleAttack * (1 + AttackTactic.armyMoraleAttackModify);
            float defendMorale = defender.ArmyBaseMoraleDefend * (1 + DefendTactic.armyMoraleDefendModify);

            float damage_a_d = Mathf.Max(0, attackMorale - defendMorale);

            // 计算公式：士气杀伤 = 随机值 + 攻方攻击 - 防方防御 + 其他修正（！）
            float moraleDamage = UnityEngine.Random.Range(0, 0.1f) + damage_a_d;

            return moraleDamage;
        }

        private float CaculateMoraleDamage_Rear(ArmyUnit attacker, ArmyUnit defender) {
            float attackMorale = attacker.ArmyBaseMoraleAttack_Rear;
            float defendMorale = defender.ArmyBaseMoraleDefend_Rear;

            float damage_a_d = Mathf.Max(0, attackMorale - defendMorale);

            // 计算公式：士气杀伤 = 随机值 + 攻方攻击 - 防方防御 + 其他修正（！）
            float moraleDamage = UnityEngine.Random.Range(0, 0.01f) + damage_a_d / 10;

            return moraleDamage;
        }

        private void CaculateDamange(ArmyUnit[] attackSide, ArmyUnit[] defendSide, ref uint manDamage, ref float moraleDamage) {
            
            // 计算进攻方的进攻值
            uint totalAttack = 0;
            float totalMoraleAttack = 0;
            uint count1 = 0;
            foreach (ArmyUnit unitData in attackSide)
            {
                totalAttack += unitData.ArmyBaseAttack;
                totalMoraleAttack += unitData.ArmyBaseMoraleAttack;
                count1++;
            }
            uint attack = totalAttack;
            float moraleAttack = totalMoraleAttack / count1;
            

            // 计算防御方的防御值
            uint totalDefend = 0;
            float totalMoraleDefend = 0;
            uint count2 = 0;
            foreach (ArmyUnit unitData in defendSide) {
                totalDefend += unitData.ArmyBaseDefend;
                totalMoraleDefend += unitData.ArmyBaseMoraleDefend;
                count2++;
            }
            uint defend = totalAttack / count2;
            float moraleDefend = totalMoraleAttack / count2;

            // 计算公式：人员杀伤 = 随机值 + 攻方攻击平均值 - 防方防御平均值 + 其他修正（！）
            // TODO : 引入其他修正
            manDamage = (uint)(UnityEngine.Random.Range(0, 200) + (uint)Mathf.Max(0, attack - defend));

            // 计算公式：士气杀伤 = 随机值 + 攻方攻击平均值 - 防方防御平均值 + 其他修正（！）
            // TODO : 引入其他修正
            moraleDamage = UnityEngine.Random.Range(0, 0.1f) + Mathf.Max(0, moraleAttack - moraleDefend);

            return;
        }

        // Deprecated
        private float CaculateMoraleDamage(ArmyUnit[] attackSide, ArmyUnit[] defendSide) {

            // 计算进攻方的士气进攻值
            float totalAttack = 0;
            uint count1 = 0;
            foreach (ArmyUnit unitData in attackSide) {
                totalAttack += unitData.ArmyBaseMoraleAttack;
                count1++;
            }
            float attack = totalAttack / count1;

            // 计算防御方的士气防御值
            float totalDefend = 0;
            uint count = 0;
            foreach (ArmyUnit unitData in defendSide) {
                totalDefend += unitData.ArmyBaseMoraleDefend;
                count++;
            }
            float defend = totalDefend / count;

            
            float damage = UnityEngine.Random.Range(0, 0.1f) +  Mathf.Max(0, attack - defend);

            return damage;
        }
        #endregion

        #region 战斗数据 生成
        /// <summary>
        /// 获取本场战役的结算数据
        /// </summary>
        public void GetCombatCompleteMessage(bool attackWin) {
            
            GetTotalArmyNum(true, out uint totalInfantryNum_A, out uint totalCavalryNum_A, out uint totalNum_A);
            GetTotalArmyNum(true, out uint totalInfantryNum_D, out uint totalCavalryNum_D, out uint totalNum_D);
            CombatCountMessage.SetEndCombatMessage(attackWin,
                totalNum_A, totalInfantryNum_A, totalCavalryNum_A,
                totalNum_D, totalInfantryNum_D, totalCavalryNum_D
            );
        }
        #endregion

        #region 战斗进程 网络同步

        /// <summary>
        /// 接收外部传入的战役数据，更新战场部队
        /// </summary>
        public void UpdateCombatUnits(BattlePlaceNetwork[] frontAtUnits, BattlePlaceNetwork[] frontDeUnits, BattlePlaceNetwork[] rearAtUnits, BattlePlaceNetwork[] rearDeUnits,
            BattlePlaceNetwork[] withdrawAtUnits, BattlePlaceNetwork[] withdrawDeUnits) {

            // 双方前线部队
            // 前线最大可存在部队由战场宽度决定
            // （双方的战场宽度是不完全相同的，具体计算逻辑还未写）
            //int attackerWidth = 5;
            //int defenderWidth = 5;

            // 使用传入的 BattlePlaceNetwork 数组，构建前线、后方、撤退
            foreach (var placeNet in frontAtUnits)
            {
                DispathAndRenewUnit(placeNet, ref rearAttackerUnits, ref withdrawAttackerUnits, ref frontAttackerUnits);
                
            }
            foreach (var placeNet in frontDeUnits) {

                DispathAndRenewUnit(placeNet, ref rearDefenderUnits, ref withdrawDefenderUnits, ref frontDefenderUnits);
                
            }
            
            // 双方溃退部队
            foreach (var placeNet in withdrawAtUnits) {

                DispathAndRenewUnit(placeNet, ref frontAttackerUnits, ref rearAttackerUnits, ref withdrawAttackerUnits);
                
            }
            foreach (var placeNet in withdrawDeUnits) {

                DispathAndRenewUnit(placeNet, ref frontDefenderUnits, ref rearDefenderUnits, ref withdrawDefenderUnits);
                
            }
            
            // 双方后方部队
            foreach (var placeNet in rearAtUnits) {

                DispathAndRenewUnit(placeNet, ref frontAttackerUnits, ref withdrawAttackerUnits, ref rearAttackerUnits);
                
            }
            foreach (var placeNet in rearDeUnits) {

                DispathAndRenewUnit(placeNet, ref frontDefenderUnits, ref withdrawDefenderUnits, ref rearDefenderUnits);
                
            }

            RenewUnitToArmy();

            Debug.Log("战役更新事件,dispath over!");
            
        }

        /// <summary>
        /// 若传入的 BattlePlaceNetwork 在指定的区位，则更新，否则调度到指定区位
        /// </summary>
        /// <param name="placeNet">军队单位所在的区位（前线、后方、撤退队列）</param>
        private void DispathAndRenewUnit(BattlePlaceNetwork placeNet, ref Dictionary<BattlePlace, ArmyUnit> dispatchOriginDic1, ref Dictionary<BattlePlace, ArmyUnit> dispatchOriginDic2, ref Dictionary<BattlePlace, ArmyUnit> dispatchToDic) {

            if (dispatchToDic.ContainsKey(placeNet.BatPlace)) {
                // 不需要调度，更新之
                ArmyUnit placeUnit = dispatchToDic[placeNet.BatPlace];
                placeUnit.SetCurManpower(placeNet.curManpower);
                placeUnit.SetCurMorale(placeNet.curMorale);
                dispatchToDic[placeNet.BatPlace] = placeUnit;
            } else if (dispatchOriginDic1.ContainsKey(placeNet.BatPlace)) {
                // 需要调度的情况
                ArmyUnit placeUnit = dispatchOriginDic1[placeNet.BatPlace];
                // （战役数据同步）对该单位造成伤害
                placeUnit.SetCurManpower(placeNet.curManpower);
                placeUnit.SetCurMorale(placeNet.curMorale);
                // 从撤退处移除 
                dispatchToDic.Add(placeNet.BatPlace, placeUnit);
                dispatchOriginDic1.Remove(placeNet.BatPlace);
            } else if (dispatchOriginDic2.ContainsKey(placeNet.BatPlace)) {
                ArmyUnit placeUnit = dispatchOriginDic2[placeNet.BatPlace];
                placeUnit.SetCurManpower(placeNet.curManpower);
                placeUnit.SetCurMorale(placeNet.curMorale);
                dispatchToDic.Add(placeNet.BatPlace, placeUnit);
                dispatchOriginDic2.Remove(placeNet.BatPlace);
            }

        }

        // TODO: 网络同步战场要素（将领、加成、buff、战役阶段、随机加成等）
        public void UpdateCombatFactors() {

        }

        #endregion
    }

    /// <summary>
    /// 代表前线一个位置的一支部队
    /// </summary>
    public class BattlePlace : INetworkSerializable {
        private NetworkGuid guid;              //该armyUnit的guid
        private int countInFront;       //该armyUnit的位序

        //// 一次结算事件 伤害
        //public uint damage;
        //// 一次结算事件 士气伤害
        //public float moraleDamage;


        public Guid Guid { get => guid.ToGuid(); private set => guid = value.ToNetworkGuid(); }
        public int CountInFront { get => countInFront; private set => countInFront = value; }

        public BattlePlace(Guid guid, int countInFront) {
            this.Guid = guid;
            this.CountInFront = countInFront;
            //damage = 0;
            //moraleDamage = 0;
        }

        public BattlePlace() {
            this.CountInFront = -1;
        }

        public bool InAttackScope(BattlePlace battlePlace, uint scope) {
            if (battlePlace.CountInFront - (scope - 1) / 2 <= this.CountInFront
                 && battlePlace.CountInFront + (scope - 1) / 2 >= this.CountInFront) {
                return true;
            } else {
                return false;
            }
        }

        
        public static implicit operator Guid(BattlePlace battlePlace) {
            return battlePlace.Guid;
        }

        public static implicit operator int(BattlePlace battlePlace) {
            return battlePlace.CountInFront;
        }

        public static implicit operator BattlePlace(Guid guid) {
            return new BattlePlace(guid, -1);
        }

        public static bool operator ==(BattlePlace battlePlace1, BattlePlace battlePlace2) {
            // NOTICE: 前线单位front有值，后方无值，即使guid相等，两个实例也不一样，这是不对的
            return
                //battlePlace1.CountInFront == battlePlace2.CountInFront || 
                battlePlace1.Guid == battlePlace2.Guid;
        }

        public static bool operator !=(BattlePlace battlePlace1, BattlePlace battlePlace2) {
            return !(battlePlace1 == battlePlace2);
        }

        public override bool Equals(object obj) {
            BattlePlace other = (BattlePlace)obj;
            return Guid == other.Guid;
        }

        public override int GetHashCode() {
            // 仅返回guid的哈希码
            return Guid.GetHashCode();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref guid);
            serializer.SerializeValue(ref countInFront);
        }

    }

    /// <summary>
    /// 用于在联网时传递 每个单位的数据
    /// </summary>
    public class BattlePlaceNetwork : INetworkSerializable {

        public BattlePlace BatPlace;

        // 代表当前该单位存在的战线位置
        public bool IsInFront = false;

        public bool IsInWithDraw = false;

        // 伤害
        public uint curManpower;
        // 士气伤害
        public float curMorale;

        public Guid UnitGuid { get => BatPlace.Guid; }

        public BattlePlaceNetwork() {
            IsInFront = false;
            this.BatPlace = new BattlePlace();
            this.curManpower = 0;
            this.curMorale = 0;
        }

        public BattlePlaceNetwork(bool isInFront, BattlePlace battlePlace, uint damage, float moraleDamage) {
            IsInFront = isInFront;
            this.BatPlace = battlePlace;
            this.curManpower = damage;
            this.curMorale = moraleDamage;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref IsInFront);
            serializer.SerializeValue(ref IsInWithDraw);
            serializer.SerializeValue(ref curManpower);
            serializer.SerializeValue(ref curMorale);
            BatPlace.NetworkSerialize(serializer);
        }
    }

}