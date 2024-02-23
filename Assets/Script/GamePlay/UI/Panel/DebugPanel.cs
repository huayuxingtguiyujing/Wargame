using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.Infrastructure.DebugPack;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class DebugPanel : BasePopUI {
        
        public static DebugPanel Instance;

        [Header("输入框")]
        [SerializeField] TMP_InputField commandInput;

        [Header("文字物体")]
        [SerializeField] GameObject DebugLogTextPrefab;
        [SerializeField] Transform logParent;

        private DebugExcuter DebugExcuter;

        private void Awake() {
            Instance = this;
            DebugExcuter = new DebugExcuter();
            Hide();
        }

        private void Update() {

            if (!Visible) {
                return;
            }

            // 主动选中 command input 作为焦点
            if (!commandInput.isFocused) {
                commandInput.ActivateInputField();
            }

            if (Input.GetKeyDown(KeyCode.Return)) {
                // 接收到enter输入，则尝试执行命令
                //Debug.Log("enter return!");
                ExcuteLog();
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                Debug.Log("enter escape!");
                HandlePanel();
            }

        }

        /// <summary>
        /// 执行 输入的 command 
        /// </summary>
        public void ExcuteLog() {
            string command = commandInput.text;
            if (!string.IsNullOrEmpty(command)) {
                // 执行该条命令，获取执行结果
                DebugExcuter.ExcuteCommand(command);
                string exceteError = DebugExcuter.GetErrorLog();
                ShowLog(command, exceteError);
                commandInput.text = "";
            }
        }

        /// <summary>
        /// 在屏幕 打印执行结果
        /// </summary>
        public void ShowLog(string inputText, string errorLog) {
            TMP_Text logText = Instantiate(DebugLogTextPrefab, logParent).GetComponentInChildren<TMP_Text>();
            logText.text = inputText;
            if (!string.IsNullOrEmpty(errorLog)) {
                logText.text = errorLog;
                logText.color = Color.red;
            }

            // 自动滚动位置到底部
            logParent.GetComponentInParent<ScrollRect>().normalizedPosition = new Vector2(0, 0);
        }

        public void ClearLog() {
            logParent.ClearObjChildren();
        }

        public override void HandlePanel() {
            if (Visible) {
                ClearLog();
                Hide();
            } else {
                Show();
            }
        }

    }
}