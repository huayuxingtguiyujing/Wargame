using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    public class CombatStage {

        // �׶�����
        public CombatStageEnum stageName {  get; private set; }

        // ��ս�۽׶ε��Ҷ�
        public float stageIntensity;
        public float stageMoraleIntensity;

        // ��ս�۽׶� ������/������ ��õļӳ�
        // NOTICE: ���㹫ʽΪ: manDamage = (uint)(manDamage * intensity * (1 + stageAdd));
        public float defendAdd = 0;
        public float attackAdd = 0;

        // �׶ο��Բ�ȡ��ս��
        public List<Tactic> tactics;
        public Tactic AttackTactic = new BalanceTactic();
        public Tactic DefendTactic = new BalanceTactic();
        public void SetAttackTactic(Tactic tactic) {
            if (tactics.Contains(tactic)) {
                AttackTactic = tactic;
            }
        }
        public void SetDefendTactic(Tactic tactic) {
            if (tactics.Contains(tactic)) {
                DefendTactic = tactic;
            }
        }

        // ��һ���׶�
        public CombatStage NextStage;
        
        public CombatStage(CombatStageEnum stageName, float stageIntensity, float stageMoraleIntensity, float defendAdd) {
            this.stageName = stageName;
            this.stageIntensity = stageIntensity;
            this.stageMoraleIntensity = stageMoraleIntensity;
            this.defendAdd = defendAdd;
            tactics = new List<Tactic>();

        }

        public virtual void EnterStage() {

        }

        public virtual CombatStage ExitStage() {
            if (NextStage == null) {
                return null;
            }
            return NextStage;
        }

        /// <summary>
        /// ǿ��ת�볷�˽׶�
        /// </summary>
        public virtual void WithdrawStage(bool IsAttackerWithdraw) {
            ExitStage();
        }

        public static string GetStageChineseName(CombatStageEnum combatStageEnum) {
            switch(combatStageEnum) {
                case CombatStageEnum.StandOff:
                    return "���Ž׶�";
                case CombatStageEnum.Siege:
                    return "Χ�ǽ׶�";
                case CombatStageEnum.OpenBattle:
                    return "��ս�׶�";
                case CombatStageEnum.Engagement:
                    return "��ս�׶�";
                case CombatStageEnum.WithdrawAndPursuit:
                    return "����׷���׶�";
                default: 
                    return "";
            }
        }
    }

    public enum CombatStageEnum {
        StandOff,
        Siege,
        Engagement,
        OpenBattle,
        WithdrawAndPursuit
    }
}