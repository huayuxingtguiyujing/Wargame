using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WarGame_True.GamePlay.Application;
using static WarGame_True.GamePlay.UI.TimeRecorderUI;
using Debug = UnityEngine.Debug;
using Timer = WarGame_True.GamePlay.Application.Timer;

namespace WarGame_True.GamePlay.UI {
    public class TimeRecorderUI : TimeRecorder {

        private TimeSimulator timeSimulator;

        [Header("��ǰ�ٶ�")]
        [SerializeField] SliderBar speedSliderBar;
        [SerializeField] Button addSpeedButton;
        [SerializeField] Button reduceSpeedButton;

        [Header("��ǰʱ��")]
        [SerializeField] TMP_Text yearText;
        [SerializeField] TMP_Text monthText;
        [SerializeField] TMP_Text dayText;
        [SerializeField] TMP_Text hourText;

        [Header("��ͣʱ��")]
        [SerializeField] Button stopSpeedButton;
        [SerializeField] Button continueSpeedButton;

        public void InitTimeRecoder(TimeSimulator timeSimulator) {
            this.timeSimulator = timeSimulator;

            addSpeedButton.onClick.AddListener(AddTimeSpeed);
            reduceSpeedButton.onClick.AddListener(ReduceTimeSpeed);

            stopSpeedButton.onClick.AddListener(StopTime);
            continueSpeedButton.onClick.AddListener(ContinueTime);

            // ���õ�ǰ���ٶ�
            UpdateCurTimeSpeed(TimeSimulator.CurrentSpeedLevel);
        }

        public void UpdateTimeRecoder(SerializedGameTime gameTime) {
            hourText.text = gameTime.Hour.ToString();
            dayText.text = gameTime.Day.ToString();
            monthText.text = TimeEnum.MonthChineseDic[gameTime.Month];
            yearText.text = gameTime.Year.ToString();
        }

        public void UpdateCurTimeSpeed(TimeSpeedLevel speedLevel) {
            speedSliderBar.UpdateSliderBar(5, (int)speedLevel + 1);
        }

        #region ��ť�¼�
        public void AddTimeSpeed() {
            timeSimulator.ModifyTimeLevel(1);
            UpdateCurTimeSpeed(TimeSimulator.CurrentSpeedLevel);
        }

        public void ReduceTimeSpeed() {
            timeSimulator.ModifyTimeLevel(-1);
            UpdateCurTimeSpeed(TimeSimulator.CurrentSpeedLevel);
        }

        public void StopTime() {
            timeSimulator.StopTime();
        }

        public void ContinueTime() {
            timeSimulator.ContinueTime();
        }
        #endregion

        /// <summary>
        /// �ر�ui�Ͽ���ʱ������й���: ��ʼ����ͣ������ʱ���ٶ�
        /// </summary>
        public void DisableCtrlTime() {
            stopSpeedButton.enabled = false;
            continueSpeedButton.enabled = false;
            addSpeedButton.enabled = false;
            reduceSpeedButton.enabled = false;
        }

        /// <summary>
        /// Ϊui��ť���ض����¼���Ŀǰ����Ϊ�������˵�ʱ�����ui���ӿ��ƿͻ���ʱ��ͬ������
        /// </summary>
        public void AddEventToTimeButtons(UnityAction addSpeedServerEvent, UnityAction reduceSpeedServerEvent) {
            addSpeedButton.onClick.AddListener(addSpeedServerEvent);
            reduceSpeedButton.onClick.AddListener(reduceSpeedServerEvent);
        }

    }
}