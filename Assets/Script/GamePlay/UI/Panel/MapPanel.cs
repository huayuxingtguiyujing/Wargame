using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Controller;

namespace WarGame_True.GamePlay.UI {
    public class MapPanel : BasePopUI {

        [SerializeField] Button NormalMapMode;
        [SerializeField] Button TerrainMapMode;
        [SerializeField] Button SupplyMapMode;
        [SerializeField] Button AIWeightMapMode;

        public void InitMapPanel() {

            NormalMapMode.onClick.AddListener(delegate {
                MapController.Instance.ChangeMapMode(MapMode.Normal);
            });
            TerrainMapMode.onClick.AddListener(delegate {
                MapController.Instance.ChangeMapMode(MapMode.Terrain);
            });
            SupplyMapMode.onClick.AddListener(delegate {
                MapController.Instance.ChangeMapMode(MapMode.SupplyMap);
            });
            AIWeightMapMode.onClick.AddListener(delegate {
                MapController.Instance.ChangeMapMode(MapMode.AIWeight);
            });
        }


    }
}