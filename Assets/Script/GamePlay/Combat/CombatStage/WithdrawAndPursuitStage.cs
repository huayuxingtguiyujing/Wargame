using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    public class WithdrawAndPursuitStage : CombatStage {
        public WithdrawAndPursuitStage() : base(CombatStageEnum.WithdrawAndPursuit, 1.0f, 0.5f, 0) {
            
        }


        public override void WithdrawStage(bool IsAttackerWithdraw) {
            base.WithdrawStage(IsAttackerWithdraw);
            // ս����Ĺ������ǳ����ߣ������˫����ս�ۼӳ�
            // ���˵�һ�� ������50%�ļ��ɣ�׷����һ�� ����100%�ӳ�
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