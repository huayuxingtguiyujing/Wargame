using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// �����ui��ʾ����ű�
    /// </summary>
    public class HexCanvas : MonoBehaviour {
        [SerializeField] Canvas canvas;

        [Header("ʡ�������ı�")]
        [SerializeField] TMP_Text CenterText;
        [Header("ռ�������")]
        [SerializeField] SliderBar occupySliderBar;
        [Header("�б�������")]
        [SerializeField] CircleProcess recruitProcess;
        [Header("����������")]
        [SerializeField] CircleProcess buildProcess;

        /// <summary>
        /// ��ʼ��/����ʡ�� ��canvas����ʾʡ��λ��
        /// </summary>
        public void InitHexCanvas(float xSize, float ySize, Vector3 coordinate, Vector3 position) {
            string centerText = new Vector3Int((int)coordinate.x, (int)coordinate.y, (int)coordinate.z).ToString();
            InitHexCanvas(xSize, ySize, centerText, position);
        }

        /// <summary>
        /// ��ʼ��/����ʡ�� ��canvas����ʾʡ��ID
        /// </summary>
        public void InitHexCanvas(float xSize, float ySize, uint ID, Vector3 position) {
            InitHexCanvas(xSize, ySize, ID.ToString(), position);
        }

        /// <summary>
        /// ��ʼ��/����ʡ�� ��canvas
        /// </summary>
        /// <param name="xSize">canvas�Ŀ��</param>
        /// <param name="ySize">canvas�ĳ���</param>
        /// <param name="centerText">canvas������ʾ���ı�</param>
        /// <param name="position">canvas��λ�ã�����Ϊʡ��λ��</param>
        private void InitHexCanvas(float xSize, float ySize, string centerText, Vector3 position) {
            //��������,��������ʾ�Լ���λ��
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


        #region ʡ�������ui����Χ�ǡ�����������ȣ�

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