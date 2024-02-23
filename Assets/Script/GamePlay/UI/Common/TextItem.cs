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
                // ������ɫ
                textColor = Color.white;
            } else if (colorType == 1) {
                // ��ɫ ��ֵ
                textColor = Color.green;
            } else if (colorType == 2) {
                // ��ɫ ��ֵ
                textColor = Color.red;
            } else if (colorType == 3){
                // ��ɫ 
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