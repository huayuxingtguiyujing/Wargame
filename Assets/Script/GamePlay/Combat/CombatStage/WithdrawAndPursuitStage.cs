using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    public class WithdrawAndPursuitStage : CombatStage {
        public WithdrawAndPursuitStage() : base(CombatStageEnum.WithdrawAndPursuit, 1.0f, 0.5f, 0) {
            
        }


        public override void WithdrawStage(bool IsAttackerWithdraw) {
            base.WithdrawStage(IsAttackerWithdraw);
            // 战役里的攻击方是撤退者，则更改双方的战役加成
            // 撤退的一方 攻击有50%的减成，追击的一方 攻击100%加成
            if(IsAttackerWithdraw) {
                attackAdd = -0.5f;
                defendAdd = 1;
            } else {
                attackAdd = 1;
                defendAdd = -0.5f;
            }
        }
    }
}