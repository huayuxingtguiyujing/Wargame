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
    /// ս������
    /// </summary>
    /// <remarks>
    /// ��Ҫע�⣺1.ս���еĵ�λ�ĸ��»Ḳ���������ӵĸ��£���ս�ۿ�ʼ�������ط�������ȫ��ʧЧ
    /// </remarks>
    public class CombatTask : BaseTask {

        #region ��ǰս�۵�˫��

        public List<Army> Attackers { get; private set; }
        public List<Army> Defenders { get; private set; }

        public void GetArmySupply(bool IsAttacker) {
            if (IsAttacker) {

            } else {

            }
        }
        #endregion

        #region ǰ��-��-���� ����
        // ˫���󷽲���
        Dictionary<BattlePlace, ArmyUnit> rearAttackerUnits;
        Dictionary<BattlePlace, ArmyUnit> rearDefenderUnits;

        // ˫��ǰ�߲���
        // ǰ�����ɴ��ڲ�����ս����Ⱦ���
        // ��˫����ս������ǲ���ȫ��ͬ�ģ���������߼���δд��
        private int attackerWidth = 5;
        private int defenderWidth = 5;
        Dictionary<BattlePlace, ArmyUnit> frontAttackerUnits;
        Dictionary<BattlePlace, ArmyUnit> frontDefenderUnits;

        // ˫�����˲���
        Dictionary<BattlePlace, ArmyUnit> withdrawAttackerUnits;
        Dictionary<BattlePlace, ArmyUnit> withdrawDefenderUnits;

        private ArmyUnit ContainsInCombat(Guid guid, bool IsAttacker) {
            if (IsAttacker) {
                //��ѯ ������
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
        /// ���ǰ�ߵ�λ����������
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
        /// ǰ���Ƿ���ڵ�λ
        /// </summary>
        private bool IsFrontExistUnit(bool IsAttacker) {
            return GetFrontUnitCount(IsAttacker) > 0;
        }

        /// <summary>
        /// ��ú󷽵�λ����������
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
        /// ��ó��˵�λ����������
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
        /// ��ȡ��ǰ�����͵�λ�����;�������
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
        /// ��ѯ���ӵ�ǰʿ�������ʿ��
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
        /// ��ȡ�ڶ���� �ڹ�����Χ�ڵ� ��λ
        /// </summary>
        private List<ArmyUnit> GetInFrontUnit(BattlePlace battlePlace, ArmyUnit armyUnit, bool IsAttacker) {
            List<ArmyUnit> units = new List<ArmyUnit>();

            if (IsAttacker) {
                foreach (BattlePlace bp in frontAttackerUnits.Keys) {
                    //����Ƿ��ڹ�����Χ��
                    if (bp.InAttackScope(battlePlace, armyUnit.ArmyAttackScope)) {

                    }
                }
            } else {

            }

            return units;
        }

        // ��ȡǰ�ߡ����ˡ��󷽵�λ BattlePlaceNetwork �б�
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


        // ս��������Ϣ
        public CombatMessage CombatCountMessage;

        #region ս��Ҫ�ر䶯
        // TODO: ��������ͬ����

        // ս�۽׶�
        public CombatStage CombatStage;
        
        private BaseRecorder CombatRecorder;

        // ÿ������ʱ���ͻ�䶯�� ս������ӳ�
        private BaseRecorder RandomAddRecorder;

        public uint AttackRandomAdd = 1;

        public uint DefendRandomAdd = 1;

        // ÿ������ʱ�� ����䶯 ˫��ս��
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

        // ˫���Ľ���
        private General attackerLeader;
        private General defenderLeader;
        public General AttackerLeader { get => attackerLeader; set => attackerLeader = value; }
        public General DefenderLeader { get => defenderLeader; set => defenderLeader = value; }

        // TODO����ǰ˫����buff
        // TODO: ��������ͬ����
        private List<string> currentAttackBuffs;
        private List<string> currentDefendBuffs; 
        #endregion

        public CombatTask(uint costTime, List<Army> attackers, List<Army> defenders) : base(TaskType.Hour, costTime) {
            this.Attackers = attackers;
            this.Defenders = defenders;

            // ˫���󷽲���
            rearAttackerUnits = new Dictionary<BattlePlace, ArmyUnit>();
            rearDefenderUnits = new Dictionary<BattlePlace, ArmyUnit>();

            // ��ʼʱ�������в��Ӽ��뵽�󷽲�����
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

            // ˫��ǰ�߲���
            frontAttackerUnits = new Dictionary<BattlePlace, ArmyUnit>();
            frontDefenderUnits = new Dictionary<BattlePlace, ArmyUnit>();

            // ˫�����˲���
            withdrawAttackerUnits = new Dictionary<BattlePlace, ArmyUnit>();
            withdrawDefenderUnits = new Dictionary<BattlePlace, ArmyUnit>();

            // ��ʼ����ս����
            GetTotalArmyNum(true, out uint totalInfantryNum_A, out uint totalCavalryNum_A, out uint totalNum_A);
            GetTotalArmyNum(true, out uint totalInfantryNum_D, out uint totalCavalryNum_D, out uint totalNum_D);
            CombatCountMessage = new CombatMessage(
                totalNum_A, totalInfantryNum_A, totalCavalryNum_A,
                totalNum_D, totalInfantryNum_D, totalCavalryNum_D
            );

            Debug.Log("start init!" + "D rear:" + rearDefenderUnits.Count + ",A rear:" + rearAttackerUnits.Count);

            // TODO: ������һ������Ҫ��������ͬ��
            // ����˫���Ľ��� �� ��ʼ�׶ν���ӳ�(TODO : ��Ҫ��������)
            AttackerLeader = attackers[0].ArmyData.CurrentGeneral;
            DefenderLeader = defenders[0].ArmyData.CurrentGeneral;
            CurAttackerGeneralModify = new UnitModify();
            CurDefenderGeneralModify = new UnitModify();

            // ���ó�ʼʱ��ս�۽׶�
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

        #region ս������
        uint attackerDamage = 0;
        float attackerMoraleDamage = 0;
        uint defenderDamage = 0;
        float defenderMoraleDamage = 0;

        /// <summary>
        /// ����ս�����ӣ�ע�ᵽСʱ���¼���
        /// </summary>
        private void RenewCombatUnits() {
            InitFrontUnits();

            // ����ǰ�߲��ӵ�����״��
            TakeDamageToFront();

            // ���µ���Ӧ�ľ��ӵ���
            RenewUnitToArmy();

            try {
                WithdrawAndReinforce();
            } catch {
                Debug.Log("����Աδ�ܼ�ʱ���ǰ�ߣ�");
            }

            // �ж��Ƿ����㳷������
            JudgeEnterWithdraw();

            // �ж��Ƿ�����ǿ�ƽ�������

        }

        /// <summary>
        /// ��ʼ��ǰ�߲��ӣ�����ǰ�߲���Ϊ��ʱ��ִ���߼�
        /// </summary>
        private void InitFrontUnits() {

            // ǰ��û�в���
            if (frontAttackerUnits.Count <= 0 && rearAttackerUnits.Count > 0) {
                // �������һ����������
                for (int i = 0; i < attackerWidth; i++) {
                    if (rearAttackerUnits.Count > 0) {
                        KeyValuePair<BattlePlace, ArmyUnit> rearPair = rearAttackerUnits.GetRandomPair();
                        // �½�һ��ս��λ��
                        BattlePlace battlePlace = new BattlePlace(rearPair.Key, i);
                        frontAttackerUnits.Add(battlePlace, rearPair.Value);

                        // �Ӻ󷽲������Ƴ�
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
                        // �½�һ��ս��λ��
                        BattlePlace battlePlace = new BattlePlace(rearPair.Key, i);
                        frontDefenderUnits.Add(battlePlace, rearPair.Value);

                        //�Ӻ󷽲������Ƴ�
                        rearDefenderUnits.Remove(rearPair.Key);
                    } else {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// ����˫��ǰ�߲��ӵ���ֵ�������˫�����ӵ��˺�
        /// </summary>
        private void TakeDamageToFront() {
            // ǰ���в���ʱ
            // 1.����ǰ�ߵ�˫������״������ԶԷ����˺�
            if (frontAttackerUnits.Count >= 0) {

                /* �ɵ���ս�߼���ֱ�Ӹ���˫��ǰ�߲��� ���㹥��ƽ��ֵ��ʩ�����˺�
                 * // (1).���ø��ֵ���������
                attackerDamage = 0;
                attackerMoraleDamage = 0;
                
                CaculateDamange(frontAttackerUnits.Values.ToArray(), frontDefenderUnits.Values.ToArray(), ref attackerDamage, ref attackerMoraleDamage);
                // (2).���µ�ǰ�ߺͺ󷽲�����
                foreach (ArmyUnit armyUnitData in frontDefenderUnits.Values) {
                    armyUnitData.TakeDamage(attackerDamage);
                    armyUnitData.TakeMoraleDamage(attackerMoraleDamage);
                }*/

                /*int currentAttackWidth = frontAttackerUnits.Count;
                int attackOrder = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> attackPair in frontAttackerUnits)
                {
                    // (1).��ù����ߵ���������ǰ�ߵĴ���
                    int currentDefendWidth = frontDefenderUnits.Count;
                    int defendOrder = 0;
                    foreach (KeyValuePair<BattlePlace, ArmyUnit> defendPair in frontDefenderUnits)
                    {
                        // (2).���ݷ������빥���ߵ���Ŀ ����ս������ֵ
                        int offset = Mathf.Abs((currentDefendWidth - currentAttackWidth) / 2);
                        if (currentAttackWidth > currentDefendWidth) {
                        } else {
                            // ����ֵ�ȹ����߶�ʱ
                            offset = -offset;
                        }

                        if (attackOrder + (attackPair.Value.ArmyAttackScope - 1) / 2 >= defendOrder + offset
                            && attackOrder - (attackPair.Value.ArmyAttackScope - 1) / 2 <= defendOrder + offset) {
                            // (3).�ڹ�����Χ��,��Ըõ�λִ�й���
                            uint manDamage = CaculateDamage(attackPair.Value, defendPair.Value);
                            float moraleDamage = CaculateMoraleDamage(attackPair.Value, defendPair.Value);
                            attackerDamage += manDamage;
                            attackerMoraleDamage += moraleDamage;
                            defendPair.Value.TakeDamage(manDamage);
                            defendPair.Value.TakeMoraleDamage(moraleDamage);

                        }
                        defendOrder++;
                    }

                    // (4).�󷽲���Ҳ���ܵ�����ʿ���˺�
                    if(attackPair.Value.ArmyBaseMoraleAttack_Rear > 0) {
                        foreach (ArmyUnit armyUnitData in rearDefenderUnits.Values) {
                            float moraleDamage = CaculateMoraleDamage_Rear(attackPair.Value, armyUnitData);
                            armyUnitData.TakeMoraleDamage(moraleDamage);
                        }
                    }
                    

                    attackOrder++;
                }*/

                // TODO������� CombatStage.stageIntensity �ֶ����ֽ�,���ս���׶ε�����
                uint outManDamage = 0;
                float outMoraleDamage = 0;
                if (IsFrontExistUnit(IsAttacker: false)) {
                    // �Է�ǰ�ߴ��ڵ�λ�����ԶԶԷ���ǰ������˺�
                    TakeDamageToFront(ref frontAttackerUnits, ref frontDefenderUnits,
                    AttackRandomAdd, CombatStage.stageIntensity, CombatStage.stageMoraleIntensity, CombatStage.attackAdd, CombatStage.AttackTactic, CombatStage.DefendTactic,
                    out outManDamage, out outMoraleDamage);
                } else{
                    // TODO: ��Ҫ����
                    // �Է�ǰ��û�е�λ�����ҷ���ǰ�ߵ�λ���ݹ�����Χ�������ѡ�з�����һ����λ����˺�
                    TakeDamageWhenNoFrontUnit(ref frontAttackerUnits, ref withdrawDefenderUnits, 
                    AttackRandomAdd, CombatStage.stageIntensity, CombatStage.stageMoraleIntensity, CombatStage.attackAdd, CombatStage.AttackTactic, CombatStage.DefendTactic,
                    out outManDamage, out outMoraleDamage);
                }
                
                // ��¼ÿ����ɵ��˺�ֵ
                attackerDamage = outManDamage;
                // NOTICE��ʿ���˺�Ҫ���Ե�ǰ�ܾ�������
                GetTotalArmyNum(false, out uint totalInfantryNum, out uint totalCavalryNum, out uint totalNum);
                attackerMoraleDamage = outMoraleDamage / GetFrontUnitCount(true);

                Debug.Log("A damage:" + attackerDamage + ",A moraleDam:" + attackerMoraleDamage);
            }

            if (frontDefenderUnits.Count >= 0) {
                /* �ɵ���ս�߼���ֱ�Ӹ���˫��ǰ�߲��� ���㹥��ƽ��ֵ��ʩ�����˺�
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
                // (1).��� ������ ������ ����������ǰ�ߵĴ���
                int currentDefendWidth = frontDefenderUnits.Count;
                int defendOrder = 0;
                foreach (KeyValuePair<BattlePlace, ArmyUnit> defendPair in frontDefenderUnits) {
                    // (2).���ݷ������빥���ߵ���Ŀ ����ս������ֵ
                    int offset = Mathf.Abs((currentDefendWidth - currentAttackWidth) / 2);
                    if (currentAttackWidth > currentDefendWidth) {
                    } else {
                        // ����ֵ�ȹ����߶�ʱ
                        offset = -offset;
                    }

                    if (attackOrder + (attackPair.Value.ArmyAttackScope - 1) / 2 >= defendOrder + offset
                        && attackOrder - (attackPair.Value.ArmyAttackScope - 1) / 2 <= defendOrder + offset) {
                        // (3).�ڹ�����Χ��,��Ըõ�λִ�й���
                        uint manDamage = CaculateDamage(attackPair.Value, defendPair.Value, RandomAdd, AttackTactic, DefendTactic);
                        manDamage = (uint)(manDamage * intensity * (1 + stageAdd));
                        float moraleDamage = CaculateMoraleDamage(attackPair.Value, defendPair.Value, AttackTactic, DefendTactic);
                        moraleDamage = moraleDamage * moraleIntensity * (1 + stageAdd);

                        // (4).��¼�˺�
                        OutManDamage += manDamage;
                        OutMoraleDamage += moraleDamage;

                        // (5).ִ���˺�
                        defendPair.Value.TakeDamage(manDamage);
                        defendPair.Value.TakeMoraleDamage(moraleDamage);

                    }
                    defendOrder++;
                }

                // (4).�󷽲���Ҳ���ܵ�����ʿ���˺�
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
        /// ���㵱һ��û��ǰ�ߵ�λʱ���Է�������ɵ��˺�
        /// </summary>
        /// <param name="frontAttackerUnits">�����ߵ�ǰ�ߵ�λ</param>
        /// <param name="withdrawDefenderUnits">�����ߵ����г��˵�λ</param>
        /// <param name="RandomAdd">������ӳ�</param>
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

            // TODO: �˺��߼���
            // ÿ�ν����˺�ʱ��������˷���ǰ��û�е�λ��
            // ��׷������ǰ�ߵ�λ�����ѡ���˷�(������Χ����λ)����˺�
            OutManDamage = 0;
            OutMoraleDamage = 0;

            List<BattlePlace> guidList = withdrawDefenderUnits.Keys.ToList();
            int GuidsCount = guidList.Count;
            foreach (KeyValuePair<BattlePlace, ArmyUnit> attackPair in frontAttackerUnits) {
                // (1).����ÿһ�������ߵ�ǰ�ߵ�λ������乥����Χ
                int attackScope = (int)attackPair.Value.ArmyAttackScope;
                for(int i = 0; i < attackScope; i++) {
                    // (2).ÿһ��ǰ�ߵ�λ��"���"��(������Χ)�����˵�λ����˺�
                    int attackTargetIndex = UnityEngine.Random.Range(0, GuidsCount);
                    BattlePlace attackTargetGuid = guidList[attackTargetIndex];

                    // (3).�ڹ�����Χ��,��Ըõ�λִ�й���
                    uint manDamage = CaculateDamage(attackPair.Value, withdrawDefenderUnits[attackTargetGuid], RandomAdd, AttackTactic, DefendTactic);
                    manDamage = (uint)(manDamage * intensity * (1 + stageAdd));
                    float moraleDamage = CaculateMoraleDamage(attackPair.Value, withdrawDefenderUnits[attackTargetGuid], AttackTactic, DefendTactic);
                    moraleDamage = moraleDamage * moraleIntensity * (1 + stageAdd);

                    // (4).��¼�˺�
                    OutManDamage += manDamage;
                    OutMoraleDamage += moraleDamage;

                    // (5).�Ըõ�λִ���˺�
                    withdrawDefenderUnits[attackTargetGuid].TakeDamage(manDamage);
                    withdrawDefenderUnits[attackTargetGuid].TakeMoraleDamage(moraleDamage);
                }
            }
        }

        /// <summary>
        /// ���ǰ���Ƿ������˵ĵ�λ�����򽫺󷽲������
        /// </summary>
        private void WithdrawAndReinforce() {
            // ������ˣ�����ӵ�ǰ��
            if (frontAttackerUnits.Count > 0) {

                // (1).��������Ƿ��в���������������������Ӧ�ÿ��Ǹ��ʣ�
                ArmyUnit[] attackersList = frontAttackerUnits.Values.ToArray();
                BattlePlace[] attackerGuids = frontAttackerUnits.Keys.ToArray();

                for (int i = 0; i < attackerGuids.Length; i++) {
                    if (frontAttackerUnits[attackerGuids[i]].ShouldWithdraw()) {

                        // (2).���������������Ĳ��ӵ�λ�����뵽���˶�����
                        withdrawAttackerUnits.Add(attackerGuids[i], frontAttackerUnits[attackerGuids[i]]);

                        // (3).��ǰ�߲������Ƴ�
                        int orderInFront = attackerGuids[i].CountInFront;
                        frontAttackerUnits.Remove(attackerGuids[i]);
                        
                        // (4).�������һ�����õĺ󷽲��ӣ����뵽ǰ���еĶ�Ӧλ��
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
                        // (2).��ǰ�߲������Ƴ�
                        frontDefenderUnits.Remove(defenderGuids[i]);
                        // (3).���������������Ĳ��ӵ�λ�����뵽���˶�����
                        int orderInFront = defenderGuids[i].CountInFront;
                        withdrawDefenderUnits.Add(defenderGuids[i], defendersList[i]);
                        // (4).�������һ�����õĺ󷽲��ӣ����뵽ǰ����
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
        /// �����е�ǰ���ϲ��ӵ�״̬���µ���Ӧ�ľ�����
        /// </summary>
        private void RenewUnitToArmy() {

            // ˢ�¾���ʿ�����������ȴ������ӵ�λ��Ӱ��


            // ��ս���е�λ��״̬ͬ����������
            int attackerTotalCount = GetTotalUnitCount(true);
            foreach (Army attacker in Attackers)
            {
                // ��ÿ��ArmyUnit���µ�Army��
                for (int i = 0; i < attacker.ArmyData.ArmyUnitDatas.Count; i++) {
                    Guid guid = attacker.ArmyData.ArmyUnitDatas[i].Guid;
                    // �ҵ���ս����Ӧ��ArmyUnit
                    ArmyUnit armyUnit = ContainsInCombat(guid, true);
                    if (armyUnit != null) {
                        // NOTICE: �������¿��ܻḲ�ǵ�Army�������ط��ĸ���
                        // �����ҵ���Ӧ �����
                        attacker.ArmyData.ArmyUnitDatas[i] = armyUnit;
                    }
                }

                // ˢ��ui��ʾ,���������˺�����������ʿ���˺�ƽ��ֵ
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
        /// �ж��Ƿ����㳷��������������˫��
        /// </summary>
        private void JudgeEnterWithdraw() {
            // ����ڳ��˽׶� ���ж��Ƿ�������볷�˽׶ε�����
            if (CombatStage is WithdrawAndPursuitStage) {
                return;
            }

            if ((rearAttackerUnits.Count <= 0 && frontAttackerUnits.Count <= 0)) {
                // ��������ʤ����
                //Debug.Log("the defenders win!");

                // TODO: ����һ���������ݣ�ͬʱ�ṩ�ӿڸ��������ֻ�ȡ��������
                GetCombatCompleteMessage(false);

                // TODO: ��Ϊת�볷�˽׶Σ����˽׶�֮���ٽ�������
                EnterWithdrawStage(IsAttackerWithdraw: true);
                //ForceToComplete();
            }else if ((rearDefenderUnits.Count <= 0 && frontDefenderUnits.Count <= 0)) {
                // ��������ʤ����
                //Debug.Log("the attackers win!");

                GetCombatCompleteMessage(true);

                // TODO: ��Ϊת�볷�˽׶Σ����˽׶�֮���ٽ�������
                EnterWithdrawStage(IsAttackerWithdraw: false);
                //ForceToComplete();
            }
        }

        /// <summary>
        /// ���볷�˽׶�
        /// </summary>
        public void EnterWithdrawStage(bool IsAttackerWithdraw) {
            CombatStage = new WithdrawAndPursuitStage();
            CombatStage.WithdrawStage(IsAttackerWithdraw);
            CombatRecorder.RestartRecorder(UnityEngine.Random.Range(5, 15));
            //Debug.Log("�����˳��˽׶�:" + CombatRecorder.LastCountTime);
            // ˢ��˫�����ӵĽ���ӳ�
            UpdateGeneralStageModify(CombatStage);
        }

        /// <summary>
        /// �ж��Ƿ�����ǿ�ƽ�������
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
        /// ������ս����˫�������˳�ս��
        /// </summary>
        public override void ForceToComplete() {
            base.ForceToComplete();

            ////������ս����ս��״̬
            //ArmyHelper.GetInstance().ExitCombat(Attackers);
            ArmyNetworkCtrl.Instance.ExitCombatEvent(Attackers);
            //ArmyHelper.GetInstance().ExitCombat(Defenders);
            ArmyNetworkCtrl.Instance.ExitCombatEvent(Defenders);

            // ������ӵ�λ,û�������ᱻ����
            foreach (Army army in Attackers) {
                army.SettlementArmyData();
            }
            foreach (Army army in Defenders) {
                army.SettlementArmyData();
            }

            if (CombatCountMessage.AttackerWin) {
                if (Defenders.Count >= 1) {
                    // ���������е�λ ����г���
                    Province withdrawStart = Defenders[0].CurrentProvince;
                    Province withdrawTarget = ArmyController.Instance.GetWithdrawTarget(Defenders[0].ArmyData.ArmyTag, withdrawStart);
                    //ArmyController.Instance.SetArmyDestination(withdrawTarget, Defenders, true);
                    ArmyNetworkCtrl.Instance.MoveArmyEvent(Defenders, withdrawTarget.provinceData.provinceID, true);
                }

                if (Attackers.Count >= 1) {
                    // սʤ�����ڱ�ʡ��
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

        #region ս���׶α仯��ս��Ҫ�ر䶯

        public UnitModify CurAttackerGeneralModify { get; private set; }
        public UnitModify CurDefenderGeneralModify { get; private set; }

        /// <summary>
        /// ����ս����ǰ���ƣ�����ս�۽׶Ρ����õ�ս�������ʸı䣩������ս��Ҫ�أ�ע�ᵽСʱ���¼���
        /// </summary>
        private void RenewCombatFactors() {
            RenewCombatStage();
            RenewCombatTactic();
            RenewBuff();
        }

        /// <summary>
        /// ����ս�۽׶Σ������ڲ�ͬս�۽׶ε�buff
        /// </summary>
        private void RenewCombatStage() { 
            if(CombatRecorder == null) {
                // ��ȡһ�����������Ϊ��ս�۽׶εĵ���ʱ
                int stageCount = UnityEngine.Random.Range(3, 30);
                CombatRecorder = new BaseRecorder(stageCount);
            }

            CombatRecorder.CountRecorder();

            if (CombatRecorder.IsOver()) {
                Debug.Log("Recorder , ��ǰ�׶ε���: " + CombatStage.stageName);
                // ���˽׶ε��ڣ���������
                if (CombatStage is WithdrawAndPursuitStage) {
                    ForceToComplete();
                    return;
                }

                // ��ս�׶β�������ֱ��һ������Ϊֹ
                if (CombatStage is OpenBattleStage) {
                    return;
                }

                // ս�۽׶ν��� �л�����һ���׶Σ�ˢ��Recorder��
                CombatStage = CombatStage.ExitStage();
                CombatRecorder.RestartRecorder(UnityEngine.Random.Range(3, 10));

                // TODO : ������
                // ˢ��˫�����ӵĽ���ӳ�
                UpdateGeneralStageModify(CombatStage);

                Debug.Log("��ǰս�۽׶ν�����������һ��ս�۽׶�:" + CombatStage.stageName);
                
            }
        }

        /// <summary>
        /// ���ؽ����ڲ�ͬս�۽׶εļӳ�
        /// </summary>
        /// <remarks>
        /// ����ʱ���ã��л�ս�۽׶�ʱ����
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
                // ���ü�ʱ ����˫���л�ս��
                AbleToChangeTactic_Attack = true;
                AbleToChangeTactic_Defend = true;
                TacticRecorder.RestartRecorder();
            }
        }

        private void RenewBuff() {
            if(RandomAddRecorder == null) {
                // ĿǰΪ4Сʱ ����һ������ӳ�
                RandomAddRecorder = new BaseRecorder(4);
            }

            RandomAddRecorder.CountRecorder();

            if (RandomAddRecorder.IsOver()) {
                // ��ʱ���� ��������ӳ�
                AttackRandomAdd = (uint)UnityEngine.Random.Range(0, 30);
                DefendRandomAdd = (uint)UnityEngine.Random.Range(0, 30);
                RandomAddRecorder.RestartRecorder();
            }
        }
        
        #endregion

        #region �����սʱ�ĸ�����ֵ

        private uint CaculateDamage(ArmyUnit attacker, ArmyUnit defender, uint RandomAdd, Tactic AttackTactic, Tactic DefendTactic) {
            // ���� ��������
            uint attack = (uint)(attacker.ArmyBaseAttack * (1 + AttackTactic.armyAttackModify));
            uint defend = (uint)(defender.ArmyBaseDefend * (1 + DefendTactic.armyDefendModify));

            uint damage_a_d = (uint)Mathf.Max(0, attack - defend);

            // ���� �Ƽ�����
            uint armorPenetrate = (uint)(attacker.ArmorPenetration * (1 + AttackTactic.armyArmorPenetrateModify));
            uint armorClad = defender.ArmorCladRate;

            uint damage_c_p = armorPenetrate > armorClad ? (armorPenetrate - armorClad) / 2 : armorClad - armorPenetrate;

            // ���� ��������

            // ���㹫ʽ����Աɱ�� = ���ֵ + (�������� - ��������) - (������������ - �������Ƽ�ֵ) + �������������� 
            uint manDamage = (uint)UnityEngine.Random.Range(0, 5) * RandomAdd + damage_a_d + damage_c_p;

            
            // ���ݵ�ǰ��ս���Ҷ� ս��bufff ����ӳ�

            return manDamage;
        }

        private float CaculateMoraleDamage(ArmyUnit attacker, ArmyUnit defender, Tactic AttackTactic, Tactic DefendTactic) {
            float attackMorale = attacker.ArmyBaseMoraleAttack * (1 + AttackTactic.armyMoraleAttackModify);
            float defendMorale = defender.ArmyBaseMoraleDefend * (1 + DefendTactic.armyMoraleDefendModify);

            float damage_a_d = Mathf.Max(0, attackMorale - defendMorale);

            // ���㹫ʽ��ʿ��ɱ�� = ���ֵ + �������� - �������� + ��������������
            float moraleDamage = UnityEngine.Random.Range(0, 0.1f) + damage_a_d;

            return moraleDamage;
        }

        private float CaculateMoraleDamage_Rear(ArmyUnit attacker, ArmyUnit defender) {
            float attackMorale = attacker.ArmyBaseMoraleAttack_Rear;
            float defendMorale = defender.ArmyBaseMoraleDefend_Rear;

            float damage_a_d = Mathf.Max(0, attackMorale - defendMorale);

            // ���㹫ʽ��ʿ��ɱ�� = ���ֵ + �������� - �������� + ��������������
            float moraleDamage = UnityEngine.Random.Range(0, 0.01f) + damage_a_d / 10;

            return moraleDamage;
        }

        private void CaculateDamange(ArmyUnit[] attackSide, ArmyUnit[] defendSide, ref uint manDamage, ref float moraleDamage) {
            
            // ����������Ľ���ֵ
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
            

            // ����������ķ���ֵ
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

            // ���㹫ʽ����Աɱ�� = ���ֵ + ��������ƽ��ֵ - ��������ƽ��ֵ + ��������������
            // TODO : ������������
            manDamage = (uint)(UnityEngine.Random.Range(0, 200) + (uint)Mathf.Max(0, attack - defend));

            // ���㹫ʽ��ʿ��ɱ�� = ���ֵ + ��������ƽ��ֵ - ��������ƽ��ֵ + ��������������
            // TODO : ������������
            moraleDamage = UnityEngine.Random.Range(0, 0.1f) + Mathf.Max(0, moraleAttack - moraleDefend);

            return;
        }

        // Deprecated
        private float CaculateMoraleDamage(ArmyUnit[] attackSide, ArmyUnit[] defendSide) {

            // �����������ʿ������ֵ
            float totalAttack = 0;
            uint count1 = 0;
            foreach (ArmyUnit unitData in attackSide) {
                totalAttack += unitData.ArmyBaseMoraleAttack;
                count1++;
            }
            float attack = totalAttack / count1;

            // �����������ʿ������ֵ
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

        #region ս������ ����
        /// <summary>
        /// ��ȡ����ս�۵Ľ�������
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

        #region ս������ ����ͬ��

        /// <summary>
        /// �����ⲿ�����ս�����ݣ�����ս������
        /// </summary>
        public void UpdateCombatUnits(BattlePlaceNetwork[] frontAtUnits, BattlePlaceNetwork[] frontDeUnits, BattlePlaceNetwork[] rearAtUnits, BattlePlaceNetwork[] rearDeUnits,
            BattlePlaceNetwork[] withdrawAtUnits, BattlePlaceNetwork[] withdrawDeUnits) {

            // ˫��ǰ�߲���
            // ǰ�����ɴ��ڲ�����ս����Ⱦ���
            // ��˫����ս������ǲ���ȫ��ͬ�ģ���������߼���δд��
            //int attackerWidth = 5;
            //int defenderWidth = 5;

            // ʹ�ô���� BattlePlaceNetwork ���飬����ǰ�ߡ��󷽡�����
            foreach (var placeNet in frontAtUnits)
            {
                DispathAndRenewUnit(placeNet, ref rearAttackerUnits, ref withdrawAttackerUnits, ref frontAttackerUnits);
                
            }
            foreach (var placeNet in frontDeUnits) {

                DispathAndRenewUnit(placeNet, ref rearDefenderUnits, ref withdrawDefenderUnits, ref frontDefenderUnits);
                
            }
            
            // ˫�����˲���
            foreach (var placeNet in withdrawAtUnits) {

                DispathAndRenewUnit(placeNet, ref frontAttackerUnits, ref rearAttackerUnits, ref withdrawAttackerUnits);
                
            }
            foreach (var placeNet in withdrawDeUnits) {

                DispathAndRenewUnit(placeNet, ref frontDefenderUnits, ref rearDefenderUnits, ref withdrawDefenderUnits);
                
            }
            
            // ˫���󷽲���
            foreach (var placeNet in rearAtUnits) {

                DispathAndRenewUnit(placeNet, ref frontAttackerUnits, ref withdrawAttackerUnits, ref rearAttackerUnits);
                
            }
            foreach (var placeNet in rearDeUnits) {

                DispathAndRenewUnit(placeNet, ref frontDefenderUnits, ref withdrawDefenderUnits, ref rearDefenderUnits);
                
            }

            RenewUnitToArmy();

            Debug.Log("ս�۸����¼�,dispath over!");
            
        }

        /// <summary>
        /// ������� BattlePlaceNetwork ��ָ������λ������£�������ȵ�ָ����λ
        /// </summary>
        /// <param name="placeNet">���ӵ�λ���ڵ���λ��ǰ�ߡ��󷽡����˶��У�</param>
        private void DispathAndRenewUnit(BattlePlaceNetwork placeNet, ref Dictionary<BattlePlace, ArmyUnit> dispatchOriginDic1, ref Dictionary<BattlePlace, ArmyUnit> dispatchOriginDic2, ref Dictionary<BattlePlace, ArmyUnit> dispatchToDic) {

            if (dispatchToDic.ContainsKey(placeNet.BatPlace)) {
                // ����Ҫ���ȣ�����֮
                ArmyUnit placeUnit = dispatchToDic[placeNet.BatPlace];
                placeUnit.SetCurManpower(placeNet.curManpower);
                placeUnit.SetCurMorale(placeNet.curMorale);
                dispatchToDic[placeNet.BatPlace] = placeUnit;
            } else if (dispatchOriginDic1.ContainsKey(placeNet.BatPlace)) {
                // ��Ҫ���ȵ����
                ArmyUnit placeUnit = dispatchOriginDic1[placeNet.BatPlace];
                // ��ս������ͬ�����Ըõ�λ����˺�
                placeUnit.SetCurManpower(placeNet.curManpower);
                placeUnit.SetCurMorale(placeNet.curMorale);
                // �ӳ��˴��Ƴ� 
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

        // TODO: ����ͬ��ս��Ҫ�أ����졢�ӳɡ�buff��ս�۽׶Ρ�����ӳɵȣ�
        public void UpdateCombatFactors() {

        }

        #endregion
    }

    /// <summary>
    /// ����ǰ��һ��λ�õ�һ֧����
    /// </summary>
    public class BattlePlace : INetworkSerializable {
        private NetworkGuid guid;              //��armyUnit��guid
        private int countInFront;       //��armyUnit��λ��

        //// һ�ν����¼� �˺�
        //public uint damage;
        //// һ�ν����¼� ʿ���˺�
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
            // NOTICE: ǰ�ߵ�λfront��ֵ������ֵ����ʹguid��ȣ�����ʵ��Ҳ��һ�������ǲ��Ե�
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
            // ������guid�Ĺ�ϣ��
            return Guid.GetHashCode();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref guid);
            serializer.SerializeValue(ref countInFront);
        }

    }

    /// <summary>
    /// ����������ʱ���� ÿ����λ������
    /// </summary>
    public class BattlePlaceNetwork : INetworkSerializable {

        public BattlePlace BatPlace;

        // ����ǰ�õ�λ���ڵ�ս��λ��
        public bool IsInFront = false;

        public bool IsInWithDraw = false;

        // �˺�
        public uint curManpower;
        // ʿ���˺�
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