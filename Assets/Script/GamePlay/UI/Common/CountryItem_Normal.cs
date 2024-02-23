using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    public class CountryItem_Normal : CountryItem {

        [SerializeField] Image FactionLeaderProfile;
        [SerializeField] TMP_Text FactionLeaderName;
        [SerializeField] TMP_Text FactionLeaderPosition;

        [SerializeField] protected Button chooseButton;

        [Header("选中标记")]
        [SerializeField] Image chooseMark;
        [SerializeField] Image PlayerColor;
        [SerializeField] TMP_Text PlayerName;

        public override async void InitCountryItem(FactionInfo info) {
            ItemFaction = info;

            //this.chooseCountryItemCall = chooseCountryItemCall;

            FactionLeaderProfile.sprite = await FactionInfo.GetFactionLeaderProfile(info.FactionTag);
            FactionLeaderName.text = info.FactionName;
            FactionLeaderPosition.text = info.FactionPosition;

            chooseButton.onClick.AddListener(delegate {
                chooseCountryItemCall.Invoke(this);
            });
        }


        public override void SetChoosen() {
            chooseMark.gameObject.SetActive(true);
        }

        /// <summary>
        /// 专门用于联机时，设置当前选择的玩家的 名称、颜色 方法
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="color"></param>
        public override void SetChoosePlayer(string playerName, Color color) {
            PlayerColor.gameObject.SetActive(true);
            PlayerName.gameObject.SetActive(true);
            PlayerName.text = playerName;
            PlayerColor.color = color;
        }

        public override void SetUnchoosen() {
            chooseMark.gameObject.SetActive(false);
            PlayerColor.gameObject.SetActive(false);
            PlayerName.gameObject.SetActive(false);
        }

        public override Sprite GetCountryLeaderSprite() {
            return FactionLeaderProfile.sprite;
        }
    }

}