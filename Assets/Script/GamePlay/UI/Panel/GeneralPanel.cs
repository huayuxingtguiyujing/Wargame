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

        [Header("����ui���")]
        [SerializeField] GameObject generalItemPrefab;
        [SerializeField] Transform generalItemParent;

        [Header("������ϸ��Ϣ")]
        [SerializeField] GeneralDetail generalDetail;

        public void InitGeneralPanel(List<General> generals) {
            generalDetail.Hide();

            // ���ݴ����һ�����ҵĽ��� ����ui
            generalItemParent.ClearObjChildren();
            foreach (General general in generals)
            {
                GameObject generalObject = Instantiate(generalItemPrefab, generalItemParent);
                GeneralHolder generalHolder = generalObject.GetComponent<GeneralHolder>();
                generalHolder.InitGeneralHolder(general, null, LocateToGeneral, RemoveGeneralFromArmy, ShowGeneralModify);
            }
        }

        /// <summary>
        /// ��λ�������λ��,ͬʱѡ�����ڵľ���
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
                // �Ӿ����Ƴ����ѽ����ٻ�
                faction.RemoveGeneralFromArmy(general);
            }
        }

        public void ShowGeneralModify(General general) {
            if (generalDetail.Visible) {
                generalDetail.Hide();
            } else {
                generalDetail.Show();
                // TODO: λ�ã�detail������ʾ��item�ĸ߶���
                generalDetail.InitGeneralDetail(general, transform.position.y);
            }
        }

    }
}