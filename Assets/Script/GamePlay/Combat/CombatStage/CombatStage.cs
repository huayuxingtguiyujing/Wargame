using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    public class CombatStage {

        // 阶段名称
        public CombatStageEnum stageName {  get; private set; }

        // 该战役阶段的烈度
        public float stageIntensity;
        public float stageMoraleIntensity;

        // 该战役阶段 攻击方/防御方 获得的加成
        // NOTICE: 计算公式为: manDamage = (uint)(manDamage * intensity * (1 + stageAdd));
        public float defendAdd = 0;
        public float attackAdd = 0;

        // 阶段可以采取的战术
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

        // 下一个阶段
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
        /// 强制转入撤退阶段
        /// </summary>
        public virtual void WithdrawStage(bool IsAttackerWithdraw) {
            ExitStage();
        }

        public static string GetStageChineseName(CombatStageEnum combatStageEnum) {
            switch(combatStageEnum) {
                case CombatStageEnum.StandOff:
                    return "对峙阶段";
                case CombatStageEnum.Siege:
                    return "围城阶段";
                case CombatStageEnum.OpenBattle:
                    return "合战阶段";
                case CombatStageEnum.Engagement:
                    return "接战阶段";
                case CombatStageEnum.WithdrawAndPursuit:
                    return "撤退追击阶段";
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