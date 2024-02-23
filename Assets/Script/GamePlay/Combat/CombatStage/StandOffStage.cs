using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    public class StandOffStage : CombatStage {
        public StandOffStage() : base(CombatStageEnum.StandOff, 0.05f, 0.05f, 0) {
            NextStage = new EngagementStage();
        }
    }
}