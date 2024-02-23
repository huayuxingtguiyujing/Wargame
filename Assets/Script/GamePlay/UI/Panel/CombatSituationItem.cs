using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// ��CombatPanel����ʾ��ǰ˫�����ӵļӳɣ�֧�ָ�����ʾ��Ϣ
    /// </summary>
    public class CombatSituationItem : MonoBehaviour {

        // ���ڸ�combatPanelʶ���situationGuid
        public string SituationGuid;
        public bool isActive => gameObject.activeSelf;

        [SerializeField] TMP_Text itemText;
        [SerializeField] Image SituationImage;
        [SerializeField] DetailPopController detailPopController;

        public void InitCombatSituationItem(DetailTextPop detailTextPop, DetailMessage detailMessage, string situationGuid, string itemText, Sprite situationSprite) {
            SetCombatSituationItem(situationGuid, itemText);
            SituationImage.sprite = situationSprite;
            detailPopController.InitPopController(detailTextPop, detailMessage);
        }

        public void SetCombatSituationItem(string situationGuid, string itemText) {
            SituationGuid = situationGuid;
            this.itemText.text = itemText;
        }
    }
}