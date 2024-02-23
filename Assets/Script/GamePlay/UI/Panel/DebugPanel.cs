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

        [Header("�����")]
        [SerializeField] TMP_InputField commandInput;

        [Header("��������")]
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

            // ����ѡ�� command input ��Ϊ����
            if (!commandInput.isFocused) {
                commandInput.ActivateInputField();
            }

            if (Input.GetKeyDown(KeyCode.Return)) {
                // ���յ�enter���룬����ִ������
                //Debug.Log("enter return!");
                ExcuteLog();
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                Debug.Log("enter escape!");
                HandlePanel();
            }

        }

        /// <summary>
        /// ִ�� ����� command 
        /// </summary>
        public void ExcuteLog() {
            string command = commandInput.text;
            if (!string.IsNullOrEmpty(command)) {
                // ִ�и��������ȡִ�н��
                DebugExcuter.ExcuteCommand(command);
                string exceteError = DebugExcuter.GetErrorLog();
                ShowLog(command, exceteError);
                commandInput.text = "";
            }
        }

        /// <summary>
        /// ����Ļ ��ӡִ�н��
        /// </summary>
        public void ShowLog(string inputText, string errorLog) {
            TMP_Text logText = Instantiate(DebugLogTextPrefab, logParent).GetComponentInChildren<TMP_Text>();
            logText.text = inputText;
            if (!string.IsNullOrEmpty(errorLog)) {
                logText.text = errorLog;
                logText.color = Color.red;
            }

            // �Զ�����λ�õ��ײ�
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