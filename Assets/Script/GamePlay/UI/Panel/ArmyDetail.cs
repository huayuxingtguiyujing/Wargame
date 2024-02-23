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

        [Header("UI组件")]
        [SerializeField] GameObject unitContent;
        [SerializeField] ArmyItem headerInfo;

        [Header("prefab")]
        [SerializeField] GameObject unitPrefab;

        [Header("将领框")]
        [SerializeField] GeneralHolder generalHolder;

        [Header("更改将领栏")]
        [SerializeField] GameObject generalItemPrefab; 
        [SerializeField] Button closeChooseGeneral;
        [SerializeField] Transform changeGenralPanel;
        [SerializeField] Transform generalItemParent;
        

        public void UpdateArmyDetail(Army army, ArmyPanel armyPanel) {
            if(this.army != army) {

                // 初始化军队将领框ui，传入更改将领事件
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
            
            // 获取到势力的所有将领
            General[] generals = GamePlayState.GetFaction(army.ArmyData.ArmyTag).FactionGenerals.ToArray();
            // 初始化可选将领框，并隐藏之
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
        /// 点击军队领导者触发，更改将领事件，唤醒更改将领面板
        /// </summary>
        private void ChangeGeneralEvent() {
            // 唤醒更改将领面板
            changeGenralPanel.gameObject.SetActive(true);
            //Debug.Log("change general start!");
        }

        private void ConfirmChangeGeneral(General general) {

            // 确认修改将领，将该将领的信息载入到军队上
            army.ChangeGeneral(general);
            // 载入到ui上
            generalHolder.InitGeneralHolder(army.ArmyData.CurrentGeneral, ChangeGeneralEvent);

            changeGenralPanel.gameObject.SetActive(false);
        }
        

    }
}