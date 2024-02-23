using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WarGame_True.GamePlay.UI {
    public class TextItem : MonoBehaviour {

        [SerializeField] TMP_Text frontText;
        [SerializeField] TMP_Text rearText;

        Color textColor = Color.white;

        public void InitTextItem(string front, string rear, int colorType = 0, bool onlyRear = true) {

            if(front != null) {
                frontText.text = front;
            }
            rearText.text = rear;

            if (colorType == 0) {
                // 正常颜色
                textColor = Color.white;
            } else if (colorType == 1) {
                // 绿色 加值
                textColor = Color.green;
            } else if (colorType == 2) {
                // 红色 减值
                textColor = Color.red;
            } else if (colorType == 3){
                // 黄色 
                textColor = Color.yellow;
            }else if (colorType == 4) {
                // 
                textColor= Color.black;
            }
            SetTextColor(textColor, onlyRear);

        }

        private void SetTextColor(Color targetColor, bool onlyRear) {
            if (!onlyRear) {
                frontText.color = targetColor;
            }
            
            rearText.color = targetColor;
        }

    }

}