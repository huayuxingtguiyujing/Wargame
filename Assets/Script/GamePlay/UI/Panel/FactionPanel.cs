using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    public class FactionPanel : MonoBehaviour {

        private Faction PlayerFaction;

        [SerializeField] Image factionLeaderProfile;

        [Header("Faction 资源项")]
        [SerializeField] StatusItem moneyText;
        [SerializeField] StatusItem grainText;
        [SerializeField] StatusItem manpowerText;
        [SerializeField] StatusItem prestigeText;
        [SerializeField] StatusItem anthorityText;

        [Header("面板按钮")]
        [SerializeField] Button dipButton;
        [SerializeField] Button economyButton;
        [SerializeField] Button supplyButton;
        [SerializeField] Button militaryButton;

        [Header("ui面板")]
        [SerializeField] EconomyPanel economyPanel;
        [SerializeField] DiplomaticPanel diplimacyPanel;
        [SerializeField] GrainPanel grainPanel;
        [SerializeField] MilitaryPanel militaryPanel;


        public async void InitFactionPanel(Faction faction) {
            PlayerFaction = faction;

            // 设置领导的头像
            factionLeaderProfile.sprite = await GeneralProfileHelper.GetLeaderProfileAsync(faction.FactionInfo.FactionTag);

            // 更新界面（资源部分）
            UpdateFaction(faction);

            if (!economyPanel.gameObject.activeSelf) {
                economyPanel.gameObject.SetActive(true);
            }
            if (!diplimacyPanel.gameObject.activeSelf) {
                diplimacyPanel.gameObject.SetActive(true);
            }

            // 绑定按钮事件
            economyButton.onClick.AddListener(EconomyPanelEvent);
            economyPanel.Hide();
            dipButton.onClick.AddListener(DiplomaPanelEvent);
            diplimacyPanel.Hide();
            supplyButton.onClick.AddListener(SupplyPanelEvent);
            grainPanel.Hide();
            militaryButton.onClick.AddListener(MilitaryPanelEvent);
            militaryPanel.Hide();

            TimeSimulator.Instance.SubscribeDayCall(UpdateFaction_Day);
        }

        private void UpdateFaction_Day() {
            UpdateFaction(PlayerFaction);

            // 更新经济面板
            if (economyPanel.Visible) {
                economyPanel.UpdateEconomyPanel(PlayerFaction);
            }

            if (grainPanel.Visible) {
                grainPanel.UpdateGrainPanel(PlayerFaction);
            }
        }

        /// <summary>
        /// 更新factionpanel上的数值
        /// </summary>
        private void UpdateFaction(Faction faction) {
            moneyText.SetStatusItem(faction.Resource.Money.ToString());
            grainText.SetStatusItem(faction.Resource.GrainDeposits.ToString());
            manpowerText.SetStatusItem(faction.Resource.TotalManpower.ToString());
            prestigeText.SetStatusItem(faction.Resource.Prestige.ToString());
            anthorityText.SetStatusItem(faction.Resource.Authority.ToString());
        }

        // 按钮 事件

        private void EconomyPanelEvent() {
            SetAllPanelUnvisible();

            if (economyPanel.Visible) {
                economyPanel.Hide();
            } else {
                economyPanel.Show();
                economyPanel.UpdateEconomyPanel(PlayerFaction);
            }
        }

        private void DiplomaPanelEvent() {
            SetAllPanelUnvisible();

            if (diplimacyPanel.Visible) {
                diplimacyPanel.Hide();
            } else {
                diplimacyPanel.Show();
                diplimacyPanel.UpdateDipPanel(PlayerFaction);
            }
        }

        private void SupplyPanelEvent() {
            SetAllPanelUnvisible();

            if (grainPanel.Visible) {
                grainPanel.Hide();
            } else {
                grainPanel.Show();
                grainPanel.UpdateGrainPanel(PlayerFaction);
            }
        }
        
        private void MilitaryPanelEvent() {
            SetAllPanelUnvisible();

            if (militaryPanel.Visible) {
                militaryPanel.Hide();
            } else {
                militaryPanel.Show();
                militaryPanel.InitMilitaryPanel(PlayerFaction);
            }
        }

        private void SetAllPanelUnvisible() {
            economyPanel.Hide();
            diplimacyPanel.Hide();
            grainPanel.Hide();
        }

    }
}