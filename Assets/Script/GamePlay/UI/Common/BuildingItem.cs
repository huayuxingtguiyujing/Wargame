using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.UI {
    public class BuildingItem : MonoBehaviour {

        [SerializeField] Button itemButton;
        [SerializeField] Image itemImage;
        [SerializeField] TMP_Text itemName;
        [SerializeField] TMP_Text itemDesc;

        public Building Building;

        private bool hasInit = false;

        public void InitBuildingItem(Building building, UnityAction<Building> action) {
            Building = building;

            itemName.text = building.BuildingChineseName;
            itemDesc.text = building.BuildingDesc;

            if (!hasInit) {
                // TODO: 绑定点击建筑组件回调
                itemButton.onClick.AddListener(
                    delegate {
                        action(Building);
                    });
                hasInit = true;
            }
        }

    }
}