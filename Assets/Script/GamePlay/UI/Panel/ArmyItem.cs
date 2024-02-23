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

        [Header("������Ϣ")]
        [SerializeField] TMP_Text armyName;
        [SerializeField] TMP_Text armyCurrentArea;

        [SerializeField] StatusItem infantryNum;
        [SerializeField] StatusItem cavalryNum;
        [SerializeField] StatusItem baggageNum;
        [SerializeField] StatusItem totalNum;

        [Header("��������")]
        [SerializeField] StatusItem maxMorale;
        [SerializeField] StatusItem morale;
        [SerializeField] StatusItem loss;

        [Header("��ť")]
        [SerializeField] Button removeButton;
        [SerializeField] Button unchooseButton;
        [SerializeField] Button chooseButton;

        [Header("������")]
        [SerializeField] SliderBar moraleBar;
        [SerializeField] SliderBar supplyBar;
        [SerializeField] DetailPopController moralePopCtrl;
        [SerializeField] DetailPopController supplyPopCtrl;
        [SerializeField] DetailTextPop BarPop;

        [Header("ͼ��")]
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
        /// ˢ��ArmyItem
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
            
            // ʿ���벹���ĵ���
            DetailMessage moraleMes = new DetailMessage();
            moraleMes.AddMessage("��ǰʿ��", itemArmy.ArmyData.CurMorale, false, false);
            moraleMes.AddMessage("���ʿ��", itemArmy.ArmyData.MaxMorale, false, false);
            moralePopCtrl.InitPopController(BarPop, moraleMes);
            //Debug.Log(moraleMes.Count);

            DetailMessage supplyMes = new DetailMessage();
            supplyMes.AddMessage("��ǰ����", itemArmy.ArmyData.CurSupply, false, false);
            supplyMes.AddMessage("��󲹸�", itemArmy.ArmyData.MaxSupply, false, false);
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

            //TODO������armyitem��UI
            Destroy(gameObject);
        }

        private void CancelChoose() {
            //����armyitem��UI
            Destroy(gameObject);
        }

        private void ShowArmyDetail() {
            //TODO��չʾ����ϸ��

            armyPanel.ShowArmyDetail(itemArmy);
        }

    }
}