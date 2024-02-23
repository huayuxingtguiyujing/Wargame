using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WarGame_True.GamePlay.UI {
    public class StatusItem : MonoBehaviour {
        [SerializeField] TMP_Text NumText;
        [SerializeField] Image BGImage;

        [Header("不同样式的图片")]
        [SerializeField] Sprite type1Image;
        [SerializeField] Sprite type2Image;
        [SerializeField] Sprite type3Image;
        [SerializeField] Sprite type4Image;
        [SerializeField] Sprite type5Image;
        [SerializeField] Sprite type6Image;

        public void SetStatusItem(string text, StatusType statusType = StatusType.Type1) {
            //Debug.Log(BGImage == null);
            //Debug.Log(type1Image == null);
            switch (statusType) {
                case StatusType.Type1:
                    // type1 不设置背景
                    //BGImage.sprite = type1Image;
                    break;
                case StatusType.Type2:
                    BGImage.sprite = type2Image;
                    break;
                case StatusType.Type3:
                    BGImage.sprite = type3Image;
                    break;
                case StatusType.Type4:
                    BGImage.sprite = type4Image;
                    break;
                case StatusType.Type5:
                    BGImage.sprite = type5Image;
                    break;
                case StatusType.Type6:
                    BGImage.sprite = type6Image;
                    break;
            }
            NumText.text = text;
        }
    }

    public enum StatusType {
        Type1, Type2, Type3, Type4, Type5, Type6
    }
}