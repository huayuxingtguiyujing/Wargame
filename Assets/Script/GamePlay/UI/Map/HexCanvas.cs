using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// 网格的ui显示管理脚本
    /// </summary>
    public class HexCanvas : MonoBehaviour {
        [SerializeField] Canvas canvas;

        [Header("省份中央文本")]
        [SerializeField] TMP_Text CenterText;
        [Header("占领进度条")]
        [SerializeField] SliderBar occupySliderBar;
        [Header("招兵进度条")]
        [SerializeField] CircleProcess recruitProcess;
        [Header("建筑进度条")]
        [SerializeField] CircleProcess buildProcess;

        /// <summary>
        /// 初始化/设置省份 的canvas，显示省份位置
        /// </summary>
        public void InitHexCanvas(float xSize, float ySize, Vector3 coordinate, Vector3 position) {
            string centerText = new Vector3Int((int)coordinate.x, (int)coordinate.y, (int)coordinate.z).ToString();
            InitHexCanvas(xSize, ySize, centerText, position);
        }

        /// <summary>
        /// 初始化/设置省份 的canvas，显示省份ID
        /// </summary>
        public void InitHexCanvas(float xSize, float ySize, uint ID, Vector3 position) {
            InitHexCanvas(xSize, ySize, ID.ToString(), position);
        }

        /// <summary>
        /// 初始化/设置省份 的canvas
        /// </summary>
        /// <param name="xSize">canvas的宽度</param>
        /// <param name="ySize">canvas的长度</param>
        /// <param name="centerText">canvas中心显示的文本</param>
        /// <param name="position">canvas的位置，设置为省份位置</param>
        private void InitHexCanvas(float xSize, float ySize, string centerText, Vector3 position) {
            //坐标文字,让网格显示自己的位置
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(xSize, ySize);
            CenterText.text = centerText;
            canvas.transform.position = new Vector3(
                canvas.transform.position.x, canvas.transform.position.y, -1
            );

            this.name = CenterText.text;

            canvas.worldCamera = Camera.main;

            transform.position = position;
        }

        public void SetCenterText(string centerText) {
            CenterText.text = centerText;
        }


        #region 省份事务的ui（如围城、建筑、造兵等）

        public void ShowProvinceName(string name) {
            CenterText.text = name;
        }

        public void ShowOccupyProcess(int maxDay, int currentProcess) {
            if (!occupySliderBar.isActiveAndEnabled) {
                occupySliderBar.gameObject.SetActive(true);
            }
            occupySliderBar.UpdateSliderBar(maxDay, currentProcess, true);
        }

        public void HideOccupyProcess() {
            occupySliderBar.gameObject.SetActive(false);
        }

        public void ShowRecruitProcess(int maxHour, int curProcess, string curTaskNum) {
            if (!recruitProcess.isActiveAndEnabled) {
                recruitProcess.gameObject.SetActive(true);
            }
            recruitProcess.UpdateProcess(maxHour, curProcess, "x" + curTaskNum);
        }

        public void HideRecruitProcess() {
            recruitProcess.gameObject.SetActive(false);
        }

        public void ShowBuildProcess(int maxHour, int curProcess, string curTaskNum) {
            if (!buildProcess.isActiveAndEnabled) {
                buildProcess.gameObject.SetActive(true);
            }
            buildProcess.UpdateProcess(maxHour, curProcess, "x" + curTaskNum);
        }

        public void HideBuildProcess() {
            buildProcess.gameObject.SetActive(false);
        }

        #endregion

    }
}