using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WarGame_True.GamePlay.UI {
    public class SliderBar_Type2 : MonoBehaviour {

        [SerializeField] SliderBar sliderBar;
        [SerializeField] TMP_Text sliderText;

        public void InitSliderBar(float maxStatu, float currentStatu) {

            // 保留两位小数
            sliderText.text = currentStatu.ToString("0.00") + "/" + maxStatu.ToString("0.00");
            sliderBar.UpdateSliderBar(maxStatu, currentStatu);
        }

    }
}