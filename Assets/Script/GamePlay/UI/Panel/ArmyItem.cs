using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class ArmyItem : MonoBehaviour {
        Army itemArmy;
        ArmyPanel armyPanel;

        [Header("军队信息")]
        [SerializeField] TMP_Text armyName;
        [SerializeField] TMP_Text armyCurrentArea;

        [SerializeField] StatusItem infantryNum;
        [SerializeField] StatusItem cavalryNum;
        [SerializeField] StatusItem baggageNum;
        [SerializeField] StatusItem totalNum;

        [Header("军队属性")]
        [SerializeField] StatusItem maxMorale;
        [SerializeField] StatusItem morale;
        [SerializeField] StatusItem loss;

        [Header("按钮")]
        [SerializeField] Button removeButton;
        [SerializeField] Button unchooseButton;
        [SerializeField] Button chooseButton;

        [Header("滑动条")]
        [SerializeField] SliderBar moraleBar;
        [SerializeField] SliderBar supplyBar;
        [SerializeField] DetailPopController moralePopCtrl;
        [SerializeField] DetailPopController supplyPopCtrl;
        [SerializeField] DetailTextPop BarPop;

        [Header("图标")]
        [SerializeField] GameObject IsMovingIcon;
        [SerializeField] GameObject IsCombatingIcon;
        [SerializeField] GameObject MoraleStageIcon;
        [SerializeField] GameObject SupplyStageIcon;

        bool hasInited = false;

        public void InitArmyItem(Army army, ArmyPanel armyPanel) {
            itemArmy = army;
            this.armyPanel = armyPanel;

            UpdateArmyItem();

            if (!hasInited) {
                hasInited = true;
                removeButton.onClick.AddListener(RemoveThisArmy);
                unchooseButton.onClick.AddListener(CancelChoose);
                chooseButton.onClick.AddListener(ShowArmyDetail);

            }
        }

        /// <summary>
        /// 刷新ArmyItem
        /// </summary>
        public void UpdateArmyItem() {
            armyName.text = itemArmy.ArmyData.armyName;
            armyCurrentArea.text = itemArmy.CurrentProvince.provinceName;

            infantryNum.SetStatusItem(itemArmy.ArmyData.ArmyInfantryNum.ToString());
            cavalryNum.SetStatusItem(itemArmy.ArmyData.ArmyCavalryNum.ToString());
            baggageNum.SetStatusItem(itemArmy.ArmyData.ArmyBaggageNum.ToString());
            totalNum.SetStatusItem(itemArmy.ArmyData.ArmyNum.ToString());

            maxMorale.SetStatusItem(itemArmy.ArmyData.MaxMorale.ToString());
            morale.SetStatusItem(itemArmy.ArmyData.CurMorale.ToString());
            loss.SetStatusItem("0%");
            
            // 士气与补给的弹窗
            DetailMessage moraleMes = new DetailMessage();
            moraleMes.AddMessage("当前士气", itemArmy.ArmyData.CurMorale, false, false);
            moraleMes.AddMessage("最大士气", itemArmy.ArmyData.MaxMorale, false, false);
            moralePopCtrl.InitPopController(BarPop, moraleMes);
            //Debug.Log(moraleMes.Count);

            DetailMessage supplyMes = new DetailMessage();
            supplyMes.AddMessage("当前补给", itemArmy.ArmyData.CurSupply, false, false);
            supplyMes.AddMessage("最大补给", itemArmy.ArmyData.MaxSupply, false, false);
            supplyPopCtrl.InitPopController(BarPop, supplyMes);
            //Debug.Log(supplyMes.Count);
            BarPop.Hide();

            moraleBar.UpdateSliderBar(itemArmy.ArmyData.MaxMorale, itemArmy.ArmyData.CurMorale);
            supplyBar.UpdateSliderBar(itemArmy.ArmyData.MaxSupply, itemArmy.ArmyData.CurSupply);

            if (itemArmy.ArmyActionState == ArmyActionState.IsMoving) {
                IsMovingIcon.SetActive(true);
            } else {
                IsMovingIcon.SetActive(false);
            }

            if (itemArmy.ArmyActionState == ArmyActionState.IsInCombat) {
                IsCombatingIcon.SetActive(true);
            } else {
                IsCombatingIcon.SetActive(false);
            }

        }

        public void DisableButtons() {
            removeButton.gameObject.SetActive(false);
            unchooseButton.gameObject.SetActive(false);
            chooseButton.gameObject.SetActive(false);
        }
        
        private void RemoveThisArmy() {
            armyPanel.RemoveArmy_Single(itemArmy);

            //TODO：销毁armyitem的UI
            Destroy(gameObject);
        }

        private void CancelChoose() {
            //销毁armyitem的UI
            Destroy(gameObject);
        }

        private void ShowArmyDetail() {
            //TODO：展示军队细节

            armyPanel.ShowArmyDetail(itemArmy);
        }

    }
}