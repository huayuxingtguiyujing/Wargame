using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.Application.TimeTask {
    public class OccupyTask : BaseTask {

        public Province occupyTarget {  get; protected set; }

        // 正在占领省份的军队，军队必须处于静止状态
        public List<Army> occupyArmys { get; protected set; }
        public void UpdateOccupyingArmy(List<Army> armies) {
            occupyArmys = armies;
        }
        /// <summary>
        /// 新添加参加占领的军队
        /// </summary>
        public void AddOccupyingArmy(Army army) {
            if (!occupyArmys.Contains(army)) {
                occupyArmys.Add(army);
            }
        }
        
        /// <summary>
        /// 移除掉不在本省份参与占领的军队，NOTICE: 逻辑已经在province类中完成，不需要调用了
        /// </summary>
        public void RemoveOccupyingArmy(Army army) {
            if (occupyArmys.Contains(army)) {
                occupyArmys.Remove(army);
            }
        }
        public void RemoveLeftArmy() {
            int count = occupyArmys.Count;
            for (int i = count - 1; i >= 0; i--) {
                if (occupyArmys[i].ArmyActionState == ArmyActionState.IsMoving
                    || occupyArmys[i].CurrentProvince != occupyTarget) {
                    occupyArmys.Remove(occupyArmys[i]);
                }
            }
        }

        // 正在占领省份的军队的所属Tag
        public string occupyingTag { get; protected set; }

        // 占领进度、消耗
        public int totalOccupyCost { get => (int)costTime;}
        public int currentOccupyCost { get => (int)(costTime > lastTime ? costTime - lastTime : 0); }

        public OccupyTask(Province occupyTarget, List<Army> occupyArmys, uint costTime) : base(TaskType.Day, costTime) {
            this.occupyTarget  = occupyTarget;
            this.occupyArmys = occupyArmys;
            occupyingTag = occupyArmys[0].ArmyData.ArmyTag;

            Debug.Log("占领开始!" + "参与占领的军队人数:" + occupyArmys.Count + ",占领者为:" + occupyingTag);

        }

        public override bool CountTask() {

            if (occupyArmys == null) {
                // 没有在占领的敌人，强制结束
                ForceToComplete();
            }

            // 判断是否有占领军队离开或者加入，更新之

            // 

            return base.CountTask();
        }

    }
}