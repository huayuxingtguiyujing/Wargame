using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    public class OpenBattleStage : CombatStage {
        public OpenBattleStage() : base(CombatStageEnum.OpenBattle, 1.0f, 1.0f, 0) {
            NextStage = new WithdrawAndPursuitStage();
        }
    }
}