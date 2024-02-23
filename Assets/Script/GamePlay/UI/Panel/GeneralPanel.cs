using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.WarCamera;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class GeneralPanel : BasePopUI {

        [Header("将领ui组件")]
        [SerializeField] GameObject generalItemPrefab;
        [SerializeField] Transform generalItemParent;

        [Header("将领详细信息")]
        [SerializeField] GeneralDetail generalDetail;

        public void InitGeneralPanel(List<General> generals) {
            generalDetail.Hide();

            // 根据传入的一个国家的将领 创建ui
            generalItemParent.ClearObjChildren();
            foreach (General general in generals)
            {
                GameObject generalObject = Instantiate(generalItemPrefab, generalItemParent);
                GeneralHolder generalHolder = generalObject.GetComponent<GeneralHolder>();
                generalHolder.InitGeneralHolder(general, null, LocateToGeneral, RemoveGeneralFromArmy, ShowGeneralModify);
            }
        }

        /// <summary>
        /// 定位到将领的位置,同时选中所在的军队
        /// </summary>
        public void LocateToGeneral(General general) {
            string curCountryTag = ApplicationController.PlayerFaction.FactionTag;
            Faction faction = PoliticLoader.Instance.GetFactionByTag(curCountryTag);
            if (faction != null && faction.IsInAssign(general)) {
                Army generalArmy = faction.GetGeneralArmy(general);
                Vector3 armyPos = generalArmy.transform.position;
                CameraController.Instance.SetCameraPos(armyPos);
                ArmyController.Instance.SetArmyChoosen_Single(generalArmy);
            }
        }

        public void RemoveGeneralFromArmy(General general) {
            string curCountryTag = ApplicationController.PlayerFaction.FactionTag;
            Faction faction = PoliticLoader.Instance.GetFactionByTag(curCountryTag);
            if (faction != null) {
                // 从军队移除，把将领召回
                faction.RemoveGeneralFromArmy(general);
            }
        }

        public void ShowGeneralModify(General general) {
            if (generalDetail.Visible) {
                generalDetail.Hide();
            } else {
                generalDetail.Show();
                // TODO: 位置，detail面板会显示在item的高度上
                generalDetail.InitGeneralDetail(general, transform.position.y);
            }
        }

    }
}