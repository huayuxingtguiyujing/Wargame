using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WarGame_True.GamePlay.UI {
    public class RecuitButton : MonoBehaviour {

        [SerializeField] Button button;

        [SerializeField] TMP_Text unitText;
        [SerializeField] TMP_Text costMoneyText;
        [SerializeField] TMP_Text costManpowerText;
        [SerializeField] TMP_Text costDayText;

        public void InitRecuitButton(UnityAction buttonCall, string unitName, uint costMoney, uint costMan, uint costDay) {
            button.onClick.AddListener(buttonCall);
            unitText.text = unitName;
            costMoneyText.text = costMoney.ToString();
            costManpowerText.text = costMan.ToString();
            costDayText.text = costDay.ToString();
        }
        

    }
}