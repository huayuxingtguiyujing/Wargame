using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    public class CountryListItem : MonoBehaviour {
        [Header("UI×é¼þ")]
        [SerializeField] private Image ProfileImage;
        [SerializeField] private TMP_Text FactionNameText;
        [SerializeField] private TMP_Text FactionDesText;

        public void InitPoliticPanelItem(FactionInfo factionInfo) {
            FactionNameText.text = factionInfo.FactionName;
            FactionDesText.text = factionInfo.FactionDes;
        }
    }
}