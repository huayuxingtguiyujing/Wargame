using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore;
using WarGame_True.GamePlay.Politic;
using System.Threading.Tasks;

namespace WarGame_True.GamePlay.UI {
    public class FactionDetail : BasePopUI {

        [Header("Faction信息")]
        [SerializeField] Image factionFlag;
        [SerializeField] Image factionImage;
        [SerializeField] TMP_Text leaderNameText;
        [SerializeField] TMP_Text factionDesText;

        [SerializeField] StatusItem provinceNumText;
        [SerializeField] StatusItem armyNumText;
        [SerializeField] StatusItem incomeNumText;

        public void SetFactionDetail(FactionInfo factionInfo) {
            // TODO: 需要加载势力的旗帜


            //factionImage.sprite = await FactionInfo.GetFactionLeaderProfile(factionInfo.FactionTag);
            leaderNameText.text = factionInfo.FactionName;
            factionDesText.text = factionInfo.FactionDes;

            provinceNumText.SetStatusItem(factionInfo.ProvincesInBeginning.Count.ToString());
            // TODO: 需要提供获取军队人数、税收总数的接口
            armyNumText.SetStatusItem("0");
            incomeNumText.SetStatusItem("0");
        }

    }
}