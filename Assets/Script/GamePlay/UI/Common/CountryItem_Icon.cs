using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    public class CountryItem_Icon : CountryItem {

        private ChoosePanel choosePanel;

        [SerializeField] Button ItemButton;
        [SerializeField] Button BottomItemButton;

        [Header("用于展示faction信息的组件")]
        [SerializeField] Image factionLeaderProfile;
        [SerializeField] Image chooseFlag;
        [SerializeField] TMP_Text factionLeaderName;
        [SerializeField] TMP_Text factionLeaderOffice;

        private bool hasInitItem = false;

        public override async void InitCountryItem(FactionInfo factionInfo) {
            this.ItemFaction = factionInfo;

            // 加载领袖头像 等信息
            factionLeaderProfile.sprite = await FactionInfo.GetFactionLeaderProfile(factionInfo.FactionTag);
            factionLeaderName.text = factionInfo.FactionName;
            factionLeaderOffice.text = factionInfo.FactionPosition;

            if (!hasInitItem) {
                hasInitItem = true;
                ItemButton.onClick.AddListener(SetCurChooseFaction);
                BottomItemButton.onClick.AddListener(SetCurChooseFaction);
            }
        }

        /// <summary>
        /// 选中Icon事件，选择对应的countryItem
        /// </summary>
        private void SetCurChooseFaction() {
            chooseCountryItemCall.Invoke(this);
        }

        public override Sprite GetCountryLeaderSprite() {
            return factionLeaderProfile.sprite;
        }
    }
}