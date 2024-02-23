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

        [Header("Faction��Ϣ")]
        [SerializeField] Image factionFlag;
        [SerializeField] Image factionImage;
        [SerializeField] TMP_Text leaderNameText;
        [SerializeField] TMP_Text factionDesText;

        [SerializeField] StatusItem provinceNumText;
        [SerializeField] StatusItem armyNumText;
        [SerializeField] StatusItem incomeNumText;

        public void SetFactionDetail(FactionInfo factionInfo) {
            // TODO: ��Ҫ��������������


            //factionImage.sprite = await FactionInfo.GetFactionLeaderProfile(factionInfo.FactionTag);
            leaderNameText.text = factionInfo.FactionName;
            factionDesText.text = factionInfo.FactionDes;

            provinceNumText.SetStatusItem(factionInfo.ProvincesInBeginning.Count.ToString());
            // TODO: ��Ҫ�ṩ��ȡ����������˰�������Ľӿ�
            armyNumText.SetStatusItem("0");
            incomeNumText.SetStatusItem("0");
        }

    }
}