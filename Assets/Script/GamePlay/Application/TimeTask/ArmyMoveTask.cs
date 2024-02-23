using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.Application.TimeTask {
    public class ArmyMoveTask : BaseTask {

        Army army;
        Province destination;

        //·��ջ
        Stack<Province> movePathStack;
        public Province PeekProvince { get {
            if (movePath.Count >= 1) {
                return movePathStack.Peek();
            } else {
                return null;
            }
        }}

        //·���б�,���������
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

            //NOTICE������ʹ�õ��㷨�������·���������ʼ��ʡ��
            //�������ڵ���ʼʡ�ݣ���ջ
            MovePath.Remove(movePathStack.Peek());
            movePathStack.Pop();

            //��Ч���ƶ�·��
            if (movePathStack.Count <= 0) {
                IsOver = true;
            }

            /* ���·���ܻ���ʱ��
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

            //·���Ѿ�����(ֻʣ��Ŀ�ĵ�),�������task
            if(movePathStack.Count <= 0) {
                IsOver = true;
                return;
            }

            //Debug.Log(currentCost);
            currentCost--;

            if (currentCost <= 0) {
                //����ʡ�ݲ���·����
                movePathStack.Peek().SetProvinceCloseMovePath();

                //�ﵽ�߳���ǰʡ�ݵ�ʱ�䣬�ƶ����ӵ���һ��ʡ�ݣ���·�����Ƴ�ʡ��
                army.MoveArmy(movePathStack.Peek());
                if (MovePath.Contains(movePathStack.Peek())) {
                    MovePath.Remove(movePathStack.Peek());
                }
                movePathStack.Pop();

                if (movePathStack.Count <= 0) {
                    IsOver = true;
                    return;
                }

                //��������ʱ�䣬�þ��ӳ�����״̬
                currentCost = movePathStack.Peek().provinceData.MoveCost;

                //����·��
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