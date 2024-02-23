using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.Politic {
    public enum FactionType {
        //腐败型，喜欢外交侮辱，内政极其腐败，外交没有逻辑
        Corruption,
        //守土型，不会发起主动战争，倾向于建设守卫
        Defender,
        //投机者，倾向于联弱抗强，使用外交策略，反复横跳
        Speculators,
        //进取型，热衷于扩张，让别人成为自己的附庸，往往在乎短期利益
        Enterprise,
        //野心型，最强大的一种军阀，期望吞并全部版图
        Ambitious
    }
}