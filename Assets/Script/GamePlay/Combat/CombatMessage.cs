using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    /// <summary>
    /// 封装战役结果数据，结算战役对双方势力的影响
    /// </summary>
    public class CombatMessage {

        // 是否是进攻者胜利（初始设定为false）
        private bool attackerWin = false;
        public bool AttackerWin { get => attackerWin; private set => attackerWin = value; }

        // 初始时双方的部队
        public uint attackersTotalNum = 0;
        public uint attackersInfantryNum = 0;
        public uint attackersCavalryNum = 0;

        public uint defendersTotalNum = 0;
        public uint defendersInfantryNum = 0;
        public uint defendersCavalryNum = 0;

        // 结束时双方的军队
        public uint attackersTotalNum_End = 0;
        public uint attackersInfantryNum_End = 0;
        public uint attackersCavalryNum_End = 0;

        public uint defendersTotalNum_End = 0;
        public uint defendersInfantryNum_End = 0;
        public uint defendersCavalryNum_End = 0;

        

        // 对国家势力的影响
        public CombatMessage(uint attackersTotalNum, uint attackersInfantryNum, uint attackersCavalryNum, uint defendersTotalNum, uint defendersInfantryNum, uint defendersCavalryNum) {
            this.attackersTotalNum = attackersTotalNum;
            this.attackersInfantryNum = attackersInfantryNum;
            this.attackersCavalryNum = attackersCavalryNum;
            this.defendersTotalNum = defendersTotalNum;
            this.defendersInfantryNum = defendersInfantryNum;
            this.defendersCavalryNum = defendersCavalryNum;
        }

        public void SetEndCombatMessage(bool IsAttackerWin, uint attackersTotalNum, uint attackersInfantryNum, uint attackersCavalryNum, uint defendersTotalNum, uint defendersInfantryNum, uint defendersCavalryNum) {
            AttackerWin = IsAttackerWin;
            this.attackersTotalNum_End = attackersTotalNum;
            this.attackersInfantryNum_End = attackersInfantryNum;
            this.attackersCavalryNum_End = attackersCavalryNum;
            this.defendersTotalNum_End = defendersTotalNum;
            this.defendersInfantryNum_End = defendersInfantryNum;
            this.defendersCavalryNum_End = defendersCavalryNum;
        }
    }
}