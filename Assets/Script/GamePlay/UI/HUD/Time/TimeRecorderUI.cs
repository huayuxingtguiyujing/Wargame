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

        [Header("当前速度")]
        [SerializeField] SliderBar speedSliderBar;
        [SerializeField] Button addSpeedButton;
        [SerializeField] Button reduceSpeedButton;

        [Header("当前时间")]
        [SerializeField] TMP_Text yearText;
        [SerializeField] TMP_Text monthText;
        [SerializeField] TMP_Text dayText;
        [SerializeField] TMP_Text hourText;

        [Header("暂停时间")]
        [SerializeField] Button stopSpeedButton;
        [SerializeField] Button continueSpeedButton;

        public void InitTimeRecoder(TimeSimulator timeSimulator) {
            this.timeSimulator = timeSimulator;

            addSpeedButton.onClick.AddListener(AddTimeSpeed);
            reduceSpeedButton.onClick.AddListener(ReduceTimeSpeed);

            stopSpeedButton.onClick.AddListener(StopTime);
            continueSpeedButton.onClick.AddListener(ContinueTime);

            // 设置当前的速度
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

        #region 按钮事件
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
        /// 关闭ui上控制时间的所有功能: 开始、暂停、调整时间速度
        /// </summary>
        public void DisableCtrlTime() {
            stopSpeedButton.enabled = false;
            continueSpeedButton.enabled = false;
            addSpeedButton.enabled = false;
            reduceSpeedButton.enabled = false;
        }

        /// <summary>
        /// 为ui按钮挂载额外事件，目前用于为服务器端的时间控制ui增加控制客户端时间同步功能
        /// </summary>
        public void AddEventToTimeButtons(UnityAction addSpeedServerEvent, UnityAction reduceSpeedServerEvent) {
            addSpeedButton.onClick.AddListener(addSpeedServerEvent);
            reduceSpeedButton.onClick.AddListener(reduceSpeedServerEvent);
        }

    }
}