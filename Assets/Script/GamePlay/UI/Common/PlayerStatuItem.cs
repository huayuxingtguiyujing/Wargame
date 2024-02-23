using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WarGame_True.GamePlay.UI {
    public class PlayerStatuItem : MonoBehaviour {

        [SerializeField] TMP_Text playerNameText;
        [SerializeField] TMP_Text playerChooseText;

        public void SetPlayerStatuItem(string playerName, string playerChoose, Color playerColor) {
            playerNameText.text = playerName;
            playerNameText.color = playerColor;
            playerChooseText.text = "Ñ¡Ôñ:" + playerChoose;
        }

    }
}