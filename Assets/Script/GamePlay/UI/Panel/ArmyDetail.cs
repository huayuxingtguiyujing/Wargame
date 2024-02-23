using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.States;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class ArmyDetail : BasePopUI {

        private Army army;
        private ArmyPanel armyPanel;

        [Header("UI���")]
        [SerializeField] GameObject unitContent;
        [SerializeField] ArmyItem headerInfo;

        [Header("prefab")]
        [SerializeField] GameObject unitPrefab;

        [Header("�����")]
        [SerializeField] GeneralHolder generalHolder;

        [Header("���Ľ�����")]
        [SerializeField] GameObject generalItemPrefab; 
        [SerializeField] Button closeChooseGeneral;
        [SerializeField] Transform changeGenralPanel;
        [SerializeField] Transform generalItemParent;
        

        public void UpdateArmyDetail(Army army, ArmyPanel armyPanel) {
            if(this.army != army) {

                // ��ʼ�����ӽ����ui��������Ľ����¼�
                generalHolder.InitGeneralHolder(army.ArmyData.CurrentGeneral, ChangeGeneralEvent);
                closeChooseGeneral.onClick.AddListener(delegate {
                    changeGenralPanel.gameObject.SetActive(false);
                });

                this.army = army;
            }

            this.armyPanel = armyPanel;

            headerInfo.DisableButtons();
            unitContent.ClearObjChildren();

            headerInfo.InitArmyItem(army, armyPanel);

            foreach (ArmyUnit armyUnitData in army.ArmyData.ArmyUnitDatas)
            {
                GameObject unitObject = Instantiate(unitPrefab, unitContent.transform);
                ArmyUnitItem armyUnitItem = unitObject.GetComponent<ArmyUnitItem>();
                armyUnitItem.UpdateUnitItem(armyUnitData);
            }

            UpdateGeneralChoosePanel();
        }

        private void UpdateGeneralChoosePanel() {
            
            // ��ȡ�����������н���
            General[] generals = GamePlayState.GetFaction(army.ArmyData.ArmyTag).FactionGenerals.ToArray();
            // ��ʼ����ѡ����򣬲�����֮
            generalItemParent.ClearObjChildren();
            foreach (General general in generals) {
                GeneralHolder generalHolder = Instantiate(generalItemPrefab, generalItemParent).GetComponent<GeneralHolder>();
                generalHolder.InitGeneralHolder(general, null, ConfirmChangeGeneral);
            }

        }



        public override void Hide() {
            base.Hide();
        }

        public override void Show() {
            base.Show();
        }


        /// <summary>
        /// ��������쵼�ߴ��������Ľ����¼������Ѹ��Ľ������
        /// </summary>
        private void ChangeGeneralEvent() {
            // ���Ѹ��Ľ������
            changeGenralPanel.gameObject.SetActive(true);
            //Debug.Log("change general start!");
        }

        private void ConfirmChangeGeneral(General general) {

            // ȷ���޸Ľ��죬���ý������Ϣ���뵽������
            army.ChangeGeneral(general);
            // ���뵽ui��
            generalHolder.InitGeneralHolder(army.ArmyData.CurrentGeneral, ChangeGeneralEvent);

            changeGenralPanel.gameObject.SetActive(false);
        }
        

    }
}