using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    public class MilitaryPanel : BasePopUI {
        
        [SerializeField] GameObject troopsPanel;
        [SerializeField] GeneralPanel generalPanel;

        public void InitMilitaryPanel(Faction PlayerFaction) {
            // ��ʼ���������
            generalPanel.InitGeneralPanel(PlayerFaction.FactionGenerals);

            // ��ʼ���������
            //troopsPanel.
        }

    }
}