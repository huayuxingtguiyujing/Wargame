using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Controller;

namespace WarGame_True.GamePlay.UI {
    public class MapPanel : BasePopUI {

        [SerializeField] Button NormalMapMode;
        [SerializeField] Button SupplyMapMode;

        public void InitMapPanel() {

            NormalMapMode.onClick.AddListener(delegate {
                MapController.Instance.ChangeMapMode(MapMode.Normal);
            });
            SupplyMapMode.onClick.AddListener(delegate {
                MapController.Instance.ChangeMapMode(MapMode.SupplyMap);
            });
        }


    }
}