using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WarGame_True.GamePlay.UI {
    public class RateItem : MonoBehaviour {

        //[SerializeField] TMP_Text statuNameText;
        [SerializeField] TMP_Text statuNumText;
        //[SerializeField] Image BGImage;

        [Header("指针")]
        [SerializeField] Image indicatorImage;

        [Header("指针槽")]
        [SerializeField] Button slot1Button;
        [SerializeField] Button slot2Button;
        [SerializeField] Button slot3Button;
        [SerializeField] Button slot4Button;
        [SerializeField] Button slot5Button;

        UnityAction<int> rateButtonEvent;

        private bool hasInit = false;

        public void InitRateItem(int initialRate, UnityAction<int> rateButtonEvent) {
            SetLevelUI(initialRate);

            this.rateButtonEvent = rateButtonEvent;

            if (!hasInit) {
                slot1Button.onClick.AddListener(ChangeRate1);
                slot2Button.onClick.AddListener(ChangeRate2);
                slot3Button.onClick.AddListener(ChangeRate3);
                slot4Button.onClick.AddListener(ChangeRate4);
                slot5Button.onClick.AddListener(ChangeRate5);
                hasInit = true;
            }
        }
        

        /// <summary>
        /// 移动税率指针 （动画效果）
        /// </summary>
        private void ChangeRate1() {
            SetLevelUI(0);
            rateButtonEvent.Invoke(0);
        }
        private void ChangeRate2() {
            SetLevelUI(1);
            rateButtonEvent.Invoke(1);
        }
        private void ChangeRate3() {
            SetLevelUI(2);
            rateButtonEvent.Invoke(2);
        }
        private void ChangeRate4() {
            SetLevelUI(3);
            rateButtonEvent.Invoke(3);
        }
        private void ChangeRate5() {
            SetLevelUI(4);
            // 要 -1 ，原因是数组下标从0开始
            rateButtonEvent.Invoke(4);
        }


        private void SetLevelUI(int i) {
            if(i == 0) {
                indicatorImage.transform.position = new Vector3(
                    slot1Button.transform.position.x,
                    indicatorImage.transform.position.y
                );
                statuNumText.text = "Very Low";
            } else if (i == 1) {
                indicatorImage.transform.position = new Vector3(
                    slot2Button.transform.position.x,
                    indicatorImage.transform.position.y
                );
                statuNumText.text = "Low";
            } else if (i == 2) {
                indicatorImage.transform.position = new Vector3(
                    slot3Button.transform.position.x,
                    indicatorImage.transform.position.y
                );
                statuNumText.text = "Normal";
            } else if (i == 3) {
                indicatorImage.transform.position = new Vector3(
                    slot4Button.transform.position.x,
                    indicatorImage.transform.position.y
                );
                statuNumText.text = "High";
            } else if (i == 4) {
                indicatorImage.transform.position = new Vector3(
                    slot5Button.transform.position.x,
                    indicatorImage.transform.position.y
                );
                statuNumText.text = "Very High";
            }
        }
    }

}