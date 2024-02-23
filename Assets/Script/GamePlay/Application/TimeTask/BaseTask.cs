using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;

namespace WarGame_True.GamePlay.Application.TimeTask {
    public class BaseTask {
        public readonly TaskType taskType;

        public readonly uint costTime;                    //Task�����ʱ��
        public uint lastTime;                    //ʣ����Ҫ��ʱ��


        private bool isOver;
        public bool IsOver { get => isOver; protected set => isOver = value; }

        public bool IsSuccess { get; protected set; }

        public BaseTask(TaskType taskType, uint costTime) {
            this.taskType = taskType;

            this.costTime = costTime;
            lastTime = costTime;

            IsOver = false;
            // Ĭ������£�����ɹ�����
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
    /// �������࣬�սᡢ�½ᡢ���
    /// </summary>
    public enum TaskType {
        Hour,
        Day,
        Month,
        Year
    }

}