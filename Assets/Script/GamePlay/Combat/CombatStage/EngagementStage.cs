using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    public class EngagementStage : CombatStage {

        public EngagementStage() : base(CombatStageEnum.Engagement, 0.25f, 0.5f, 0) {
            NextStage = new OpenBattleStage();
        }
    }
}