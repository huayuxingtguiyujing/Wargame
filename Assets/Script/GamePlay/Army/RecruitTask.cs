using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Application.TimeTask;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// ��ļ����
    /// </summary>
    public class RecruitTask : BaseTask {
        //public uint costDay;                    //��ļ�����ʱ��
        //public uint lastDay;                    //ʣ����Ҫ��ʱ��

        public uint armyNum = 1000;             //��ļ��ʿ����Ŀ
        public ArmyUnitData armyUnitData;       //������ļ��ʿ������

        public RecruitTask(uint costHour, ArmyUnitData armyUnitData) : base(TaskType.Day, costHour) {
            //this.costDay = costHour + 10;       // ������ļʱ��: 10 (������øĵ�)
            //this.lastDay = costHour + 10;
            this.armyNum = 1000;
            this.armyUnitData = armyUnitData;

            IsOver = false;
        }

    }
}