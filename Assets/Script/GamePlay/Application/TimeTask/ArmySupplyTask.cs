using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.Application.TimeTask {
    [System.Serializable]
    public class ArmySupplyTask : BaseTask {

        // 补给线的目的地 军队
        public Army SupplyTarget { get; private set; }

        // 补给线开始的省份 (一般是首都)
        public Province SupplyLineStart { get; private set; }

        public List<Province> SupplyLine {  get; private set; }

        // 当前是否可以对军队进行补给
        private bool canGetSupplyFlag = false;

        // 补给线记录
        private uint supplyStartRec;
        private uint supplyEndRec;

        // 获取补给的时间
        private int SupplyTime;
        private int SupplyTimeRec;

        public ArmySupplyTask(Army army, Province Capital, List<Province> SupplyLine) : base(TaskType.Day, 999) {
            ResetSupplyTask(army, Capital, SupplyLine);
        }

        /// <summary>
        /// 重设补给线
        /// </summary>
        public void ResetSupplyTask(Army army, Province Capital, List<Province> SupplyLine) {
            SupplyTarget = army;
            SupplyLineStart = Capital;
            this.SupplyLine = SupplyLine;

            supplyStartRec = SupplyLineStart.provinceID;
            supplyEndRec = SupplyTarget.CurrentProvince.provinceID;

            // SupplyTime 应当根据补给线长度、运输难度而改变
            SupplyTime = 1;
            SupplyTimeRec = SupplyTime;
        }

        public override bool CountTask() {
            base.CountTask();
            UpdateSupplyLine();
            return IsOver;
        }

        public void UpdateSupplyLine() {
            // 判断是否需要重构补给路线
            ShouldRebuildSupplyLine();

            // 每隔一段时间(SupplyTime)，会为军队输送补给，输送补给时长会因补给线的变化而延长
            SupplyTimeRec--;
            if (SupplyTimeRec <= 0) {
                SupplyTimeRec = SupplyTime;
                canGetSupplyFlag = true;
                // MODIFIED: 应当在 FactionResource 中轮询每只军队，判断是否能进行补给
                // 到达补给时间，执行输送补给逻辑
                //PoliticLoader.Instance.GetFactionByTag(SupplyTarget.ArmyData.ArmyTag).PaySupply(SupplyTarget);
                Debug.Log("成功获取到了补给: " + SupplyTarget.ArmyData.armyName + ", 所在省份" + SupplyTarget.CurrentProvince.provinceID);
            }
        }

        public bool CanGetSupply() {
            if (canGetSupplyFlag) {
                canGetSupplyFlag = false;
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// 检查补给线是否发生了变化，若变化了则重构路线
        /// </summary>
        public void ShouldRebuildSupplyLine() {
            // 检查补给线是否发生了变化 (起点与终点ID发生了改变)
            if (SupplyLineStart.provinceID != supplyStartRec
                || SupplyTarget.CurrentProvince.provinceID != supplyEndRec) {
                // 发生了变更，补给供给时长增加
                SupplyTimeRec++;

                // 如果当前地图模式为supplymode，则更新补给线上的省份
                if (MapController.CurMapMode == MapMode.SupplyMap) {
                    ArmyController.Instance.HideArmySupplyLine(SupplyTarget);
                }

                // 发生变化，重构补给线路
                Province capital = PoliticLoader.Instance.GetFactionByTag(SupplyTarget.ArmyData.ArmyTag).GetCapital();
                SupplyLine = MapController.Instance.GetSupplyPath(capital, SupplyTarget.CurrentProvince);

                if (MapController.CurMapMode == MapMode.SupplyMap) {
                    ArmyController.Instance.ShowArmySupplyLine(SupplyTarget);
                }

                // 更新补给线 两端的位置
                supplyStartRec = SupplyLineStart.provinceID;
                supplyEndRec = SupplyTarget.CurrentProvince.provinceID;
            }
        }

        public override void ForceToComplete() {
            base.ForceToComplete();
        }

    }
}