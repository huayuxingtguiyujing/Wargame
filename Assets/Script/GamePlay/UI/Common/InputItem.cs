using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WarGame_True.GamePlay.UI {
    public class InputItem : MonoBehaviour {

        [Header("输入框头部")]
        [SerializeField] TMP_Text MesText;
        [SerializeField] Image IconImage;

        [Header("输入框")]
        [SerializeField] TMP_InputField InputText;

        [Header("尾部按钮")]
        [SerializeField] Button ClearButton;
        [SerializeField] Button HideInputButton;

        private bool hasSetButtonEvent = false;

        public void InitInputItem(bool UseMesText, bool UseClearButton, bool UseHideButton) {
            if (UseMesText) {
                MesText.gameObject.SetActive(true);
                IconImage.gameObject.SetActive(false);
            } else {
                MesText.gameObject.SetActive(false);
                IconImage.gameObject.SetActive(true);
            }

            if (!UseClearButton) {
                ClearButton.gameObject.SetActive(false);
            }

            if (!UseHideButton) {
                HideInputButton.gameObject.SetActive(false);
            }

            if (!hasSetButtonEvent) {
                hasSetButtonEvent = true;
                ClearButton.onClick.AddListener(ClearInputText);
                HideInputButton.onClick.AddListener(SetInputTextUnvisible);
            }
        }

        public void SetOnValueChangeCall(UnityAction<string> inputValueChangeCall) {
            InputText.onValueChanged.AddListener(inputValueChangeCall);
        }

        public string GetInputText() {
            return InputText.text;
        }

        public void SetInputText(string text) {
            InputText.text = text;
        }

        public void ClearInputText() {
            InputText.text = string.Empty;
        }

        /// <summary>
        /// 设置输入框文本为不可见的密码
        /// </summary>
        public void SetInputTextUnvisible() {
            if(InputText.contentType == TMP_InputField.ContentType.Standard) {
                InputText.contentType = TMP_InputField.ContentType.Password;
            } else {
                InputText.contentType = TMP_InputField.ContentType.Standard;
            }
        }

    }
}