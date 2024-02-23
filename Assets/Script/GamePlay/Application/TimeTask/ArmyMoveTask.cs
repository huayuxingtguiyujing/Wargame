using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.Application.TimeTask {
    public class ArmyMoveTask : BaseTask {

        Army army;
        Province destination;

        //路径栈
        Stack<Province> movePathStack;
        public Province PeekProvince { get {
            if (movePath.Count >= 1) {
                return movePathStack.Peek();
            } else {
                return null;
            }
        }}

        //路径列表,不建议更改
        private List<Province> movePath;
        public List<Province> MovePath { get => movePath; private set => movePath = value; }

        private uint currentCost;

        public ArmyMoveTask(uint costTime, Army army, List<Province> movePath) : base(TaskType.Day, costTime) {
            this.army = army;
            this.destination = movePath[movePath.Count - 1];

            currentCost = movePath[0].provinceData.MoveCost;

            MovePath = movePath;
            movePathStack = new Stack<Province>();
            for(int i = movePath.Count - 1; i >= 0; i--) {
                movePathStack.Push(movePath[i]);
            }

            //NOTICE：出于使用的算法，传入的路径会包含起始的省份
            //军队所在的起始省份，出栈
            MovePath.Remove(movePathStack.Peek());
            movePathStack.Pop();

            //无效的移动路径
            if (movePathStack.Count <= 0) {
                IsOver = true;
            }

            /* 获得路径总花费时间
             * string hourcost = "the total cost: ";
            foreach (Province pro in MovePath)
            {
                hourcost += pro.provinceData.provinceGeo.moveCost.ToString() + "_";
            }
            Debug.Log(hourcost);*/

        }

        public override bool CountTask() {
            base.CountTask();
            UpdateMoveTask();
            return IsOver;
        }

        private void UpdateMoveTask() {
            if (IsOver) return;

            //路径已经走完(只剩下目的地),则结束本task
            if(movePathStack.Count <= 0) {
                IsOver = true;
                return;
            }

            //Debug.Log(currentCost);
            currentCost--;

            if (currentCost <= 0) {
                //设置省份不在路径上
                movePathStack.Peek().SetProvinceCloseMovePath();

                //达到走出当前省份的时间，移动军队到下一个省份，从路径中移除省份
                army.MoveArmy(movePathStack.Peek());
                if (MovePath.Contains(movePathStack.Peek())) {
                    MovePath.Remove(movePathStack.Peek());
                }
                movePathStack.Pop();

                if (movePathStack.Count <= 0) {
                    IsOver = true;
                    return;
                }

                //更新行走时间，让军队呈行走状态
                currentCost = movePathStack.Peek().provinceData.MoveCost;

                //更新路径
                if (army.ArmyActionState == ArmyActionState.Withdrawing) {
                    army.SetArmyWithdraw(movePathStack.Peek());
                } else {
                    army.SetArmyMoving(movePathStack.Peek());
                }
                

                //army.HideMovePath();
                //army.ShowMovePath();
            }
        }

        
    }
}