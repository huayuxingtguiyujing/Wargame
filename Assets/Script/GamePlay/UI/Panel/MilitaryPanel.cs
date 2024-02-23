using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    public class MilitaryPanel : BasePopUI {
        
        [SerializeField] GameObject troopsPanel;
        [SerializeField] GeneralPanel generalPanel;

        public void InitMilitaryPanel(Faction PlayerFaction) {
            // 初始化将领面板
            generalPanel.InitGeneralPanel(PlayerFaction.FactionGenerals);

            // 初始化军队面板
            //troopsPanel.
        }

    }
}