using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.Application.TimeTask {
    public class OccupyTask : BaseTask {

        public Province occupyTarget {  get; protected set; }

        // ����ռ��ʡ�ݵľ��ӣ����ӱ��봦�ھ�ֹ״̬
        public List<Army> occupyArmys { get; protected set; }
        public void UpdateOccupyingArmy(List<Army> armies) {
            occupyArmys = armies;
        }
        /// <summary>
        /// ����Ӳμ�ռ��ľ���
        /// </summary>
        public void AddOccupyingArmy(Army army) {
            if (!occupyArmys.Contains(army)) {
                occupyArmys.Add(army);
            }
        }
        
        /// <summary>
        /// �Ƴ������ڱ�ʡ�ݲ���ռ��ľ��ӣ�NOTICE: �߼��Ѿ���province������ɣ�����Ҫ������
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

        // ����ռ��ʡ�ݵľ��ӵ�����Tag
        public string occupyingTag { get; protected set; }

        // ռ����ȡ�����
        public int totalOccupyCost { get => (int)costTime;}
        public int currentOccupyCost { get => (int)(costTime > lastTime ? costTime - lastTime : 0); }

        public OccupyTask(Province occupyTarget, List<Army> occupyArmys, uint costTime) : base(TaskType.Day, costTime) {
            this.occupyTarget  = occupyTarget;
            this.occupyArmys = occupyArmys;
            occupyingTag = occupyArmys[0].ArmyData.ArmyTag;

            Debug.Log("ռ�쿪ʼ!" + "����ռ��ľ�������:" + occupyArmys.Count + ",ռ����Ϊ:" + occupyingTag);

        }

        public override bool CountTask() {

            if (occupyArmys == null) {
                // û����ռ��ĵ��ˣ�ǿ�ƽ���
                ForceToComplete();
            }

            // �ж��Ƿ���ռ������뿪���߼��룬����֮

            // 

            return base.CountTask();
        }

    }
}