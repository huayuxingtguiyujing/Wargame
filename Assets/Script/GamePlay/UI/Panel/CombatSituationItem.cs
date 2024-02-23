using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// 在CombatPanel上显示当前双方军队的加成，支持浮窗显示信息
    /// </summary>
    public class CombatSituationItem : MonoBehaviour {

        // 用于给combatPanel识别的situationGuid
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