using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Application.TimeTask;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// 招募事务
    /// </summary>
    public class RecruitTask : BaseTask {
        //public uint costDay;                    //招募所需的时间
        //public uint lastDay;                    //剩余需要的时间

        public uint armyNum = 1000;             //招募的士兵数目
        public ArmyUnitData armyUnitData;       //正在招募的士兵种类

        public RecruitTask(uint costHour, ArmyUnitData armyUnitData) : base(TaskType.Day, costHour) {
            //this.costDay = costHour + 10;       // 基础招募时间: 10 (后面最好改掉)
            //this.lastDay = costHour + 10;
            this.armyNum = 1000;
            this.armyUnitData = armyUnitData;

            IsOver = false;
        }

    }
}