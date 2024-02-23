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

        // �����ߵ�Ŀ�ĵ� ����
        public Army SupplyTarget { get; private set; }

        // �����߿�ʼ��ʡ�� (һ�����׶�)
        public Province SupplyLineStart { get; private set; }

        public List<Province> SupplyLine {  get; private set; }

        // ��ǰ�Ƿ���ԶԾ��ӽ��в���
        private bool canGetSupplyFlag = false;

        // �����߼�¼
        private uint supplyStartRec;
        private uint supplyEndRec;

        // ��ȡ������ʱ��
        private int SupplyTime;
        private int SupplyTimeRec;

        public ArmySupplyTask(Army army, Province Capital, List<Province> SupplyLine) : base(TaskType.Day, 999) {
            ResetSupplyTask(army, Capital, SupplyLine);
        }

        /// <summary>
        /// ���貹����
        /// </summary>
        public void ResetSupplyTask(Army army, Province Capital, List<Province> SupplyLine) {
            SupplyTarget = army;
            SupplyLineStart = Capital;
            this.SupplyLine = SupplyLine;

            supplyStartRec = SupplyLineStart.provinceID;
            supplyEndRec = SupplyTarget.CurrentProvince.provinceID;

            // SupplyTime Ӧ�����ݲ����߳��ȡ������Ѷȶ��ı�
            SupplyTime = 1;
            SupplyTimeRec = SupplyTime;
        }

        public override bool CountTask() {
            base.CountTask();
            UpdateSupplyLine();
            return IsOver;
        }

        public void UpdateSupplyLine() {
            // �ж��Ƿ���Ҫ�ع�����·��
            ShouldRebuildSupplyLine();

            // ÿ��һ��ʱ��(SupplyTime)����Ϊ�������Ͳ��������Ͳ���ʱ�����򲹸��ߵı仯���ӳ�
            SupplyTimeRec--;
            if (SupplyTimeRec <= 0) {
                SupplyTimeRec = SupplyTime;
                canGetSupplyFlag = true;
                // MODIFIED: Ӧ���� FactionResource ����ѯÿֻ���ӣ��ж��Ƿ��ܽ��в���
                // ���ﲹ��ʱ�䣬ִ�����Ͳ����߼�
                //PoliticLoader.Instance.GetFactionByTag(SupplyTarget.ArmyData.ArmyTag).PaySupply(SupplyTarget);
                Debug.Log("�ɹ���ȡ���˲���: " + SupplyTarget.ArmyData.armyName + ", ����ʡ��" + SupplyTarget.CurrentProvince.provinceID);
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
        /// ��鲹�����Ƿ����˱仯�����仯�����ع�·��
        /// </summary>
        public void ShouldRebuildSupplyLine() {
            // ��鲹�����Ƿ����˱仯 (������յ�ID�����˸ı�)
            if (SupplyLineStart.provinceID != supplyStartRec
                || SupplyTarget.CurrentProvince.provinceID != supplyEndRec) {
                // �����˱������������ʱ������
                SupplyTimeRec++;

                // �����ǰ��ͼģʽΪsupplymode������²������ϵ�ʡ��
                if (MapController.CurMapMode == MapMode.SupplyMap) {
                    ArmyController.Instance.HideArmySupplyLine(SupplyTarget);
                }

                // �����仯���ع�������·
                Province capital = PoliticLoader.Instance.GetFactionByTag(SupplyTarget.ArmyData.ArmyTag).GetCapital();
                SupplyLine = MapController.Instance.GetSupplyPath(capital, SupplyTarget.CurrentProvince);

                if (MapController.CurMapMode == MapMode.SupplyMap) {
                    ArmyController.Instance.ShowArmySupplyLine(SupplyTarget);
                }

                // ���²����� ���˵�λ��
                supplyStartRec = SupplyLineStart.provinceID;
                supplyEndRec = SupplyTarget.CurrentProvince.provinceID;
            }
        }

        public override void ForceToComplete() {
            base.ForceToComplete();
        }

    }
}