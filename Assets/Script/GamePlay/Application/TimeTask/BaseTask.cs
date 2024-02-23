using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;

namespace WarGame_True.GamePlay.Application.TimeTask {
    public class BaseTask {
        public readonly TaskType taskType;

        public readonly uint costTime;                    //Task所需的时间
        public uint lastTime;                    //剩余需要的时间


        private bool isOver;
        public bool IsOver { get => isOver; protected set => isOver = value; }

        public bool IsSuccess { get; protected set; }

        public BaseTask(TaskType taskType, uint costTime) {
            this.taskType = taskType;

            this.costTime = costTime;
            lastTime = costTime;

            IsOver = false;
            // 默认情况下，事务成功结束
            IsSuccess = true;
        }

        public virtual bool CountTask() {
            if (IsOver) return true;
            lastTime--; 
            if (lastTime <= 0) IsOver = true;
            return lastTime <= 0;
        }

        public virtual void ForceToComplete() {
            IsOver = true;
            IsSuccess = false;
        }
    }

    /// <summary>
    /// 事务种类，日结、月结、年结
    /// </summary>
    public enum TaskType {
        Hour,
        Day,
        Month,
        Year
    }

}