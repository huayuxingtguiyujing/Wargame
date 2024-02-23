using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WarGame_True.GamePlay.UI {
    public class SliderBar : MonoBehaviour {

        [SerializeField] Image BGImage;
        [SerializeField] Image StatuImage;

        [SerializeField] TMP_Text processText;

        public void UpdateSliderBar(float maxStatu, float currentStatu, bool showProcess = false) {
            float width = GetComponent<RectTransform>().sizeDelta.x;
            float height = GetComponent<RectTransform>().sizeDelta.y;

            float targetWidth = Mathf.Min(width * currentStatu / maxStatu, width) ;
            StatuImage.GetComponent<RectTransform>().sizeDelta = new Vector2(targetWidth, height);

            if (showProcess && processText != null) {
                processText.gameObject.SetActive(true);
                processText.text = (currentStatu / maxStatu * 100).ToString("0.0") + "%";
            } else if(processText != null) {
                processText.gameObject.SetActive(false);
            }
        }

    }
}