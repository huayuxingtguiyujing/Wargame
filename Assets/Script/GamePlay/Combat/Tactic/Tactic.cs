using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    /// <summary>
    /// TODO: 完善之；战术，在每个不同的阶段使用
    /// </summary>
    [System.Serializable]
    public class Tactic {

        // 需要消耗的 组织度
        public int OrganizationCost = 20;

        // 基础攻防 修正
        public float armyAttackModify = 0;
        public float armyDefendModify = 0;

        // 攻击范围修正
        public int armyAttackScopeModify = 0;

        // 破甲修正
        public float armyArmorPenetrateModify = 0;

        // 士气攻击防御修正
        public float armyMoraleAttackModify = 0;
        public float armyMoraleDefendModify = 0;
        public float armyMoraleModifyInRear = 0;
        public float armyMoraleModifyToRear = 0;

        // 对敌方补给攻击
        public int supplyAttack = 0;
        public int supplyRecover = 0;
    }
}