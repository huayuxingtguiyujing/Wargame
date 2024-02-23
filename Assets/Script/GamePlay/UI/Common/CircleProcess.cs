using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// 圆形进度条
    /// </summary>
    public class CircleProcess : MonoBehaviour {

        [SerializeField] TMP_Text centerText;
        [SerializeField] Image processImage;

        public void InitCircleProcess() {

        }

        public void UpdateProcess(float maxProcess, float curProcess, string text = "") {
            centerText.text = text;
            processImage.fillAmount = curProcess/maxProcess;
        }

    }
}