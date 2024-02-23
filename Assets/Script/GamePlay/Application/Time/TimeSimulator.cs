using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using WarGame_True.GamePlay.UI;
using static WarGame_True.GamePlay.UI.TimeRecorderUI;
using Debug = UnityEngine.Debug;

namespace WarGame_True.GamePlay.Application {
    /// <summary>
    /// ģ����Ϸ��ʱ������
    /// </summary>
    public class TimeSimulator : MonoBehaviour, TimeInterface {

        // ����ģʽ
        public static TimeSimulator Instance;

        #region ע���¼�(������-������)
        public delegate void TimeCallback();
        
        private List<TimeCallback> hourCallback = new List<TimeCallback>();
        public void SubscribeHourCall(TimeCallback timeCallback) {
            if (!hourCallback.Contains(timeCallback)) {
                hourCallback.Add(timeCallback);
            }
        }
        public void UnsubscribeHourCall(TimeCallback timeCallback) {
            if (hourCallback.Contains(timeCallback)) {
                hourCallback.Remove(timeCallback);
            }
        }
        
        private List<TimeCallback> dayCallback = new List<TimeCallback>();
        public void SubscribeDayCall(TimeCallback timeCallback) {
            if (!dayCallback.Contains(timeCallback)) {
                dayCallback.Add(timeCallback);
            }
        }
        public void UnsubscribeDayCall(TimeCallback timeCallback) {
            if (dayCallback.Contains(timeCallback)) {
                dayCallback.Remove(timeCallback);
            }
            
        }
        
        private List<TimeCallback> monthCallback = new List<TimeCallback>();
        public void SubscribeMonthCall(TimeCallback timeCallback) {
            if (!monthCallback.Contains(timeCallback)) {
                monthCallback.Add(timeCallback);
            }
        }
        public void UnsubscribeMonthCall(TimeCallback timeCallback) {
            if (monthCallback.Contains(timeCallback)) {
                monthCallback.Remove(timeCallback);
            }
        }
        
        private List<TimeCallback> yearCallback = new List<TimeCallback>();
        public void SubscribeYearCall(TimeCallback timeCallback) {
            if (!yearCallback.Contains(timeCallback)) {
                yearCallback.Add(timeCallback);
            }
        }
        public void UnsubscribeYearCall(TimeCallback timeCallback) {
            if (yearCallback.Contains(timeCallback)) {
                yearCallback.Remove(timeCallback);
            }
        }
        
        #endregion

        // ʱ�������ٶ�
        public static TimeSpeedLevel CurrentSpeedLevel = TimeSpeedLevel.Level5;
        public static float TimeSpeed {
            get => TimeEnum.LevelSpeedDic[CurrentSpeedLevel];
            private set => TimeEnum.LevelSpeedDic[CurrentSpeedLevel] = value;
        }

        // ʱ��ģ������UI
        [SerializeField] 
        private TimeRecorderUI timeRecorderUI;

        // ��ʱ��
        Timer timer;

        #region ʵ�ֹ��� �ı������ص�
        // ��ϷĿǰʱ��
        GameTime gameTime = GameTime.GameStartTime;

        public void SetGameTime(SerializedGameTime time) {
            gameTime.SetGameTime(time);
        }

        public GameTime CurGameTime { get => gameTime; }


        public delegate void TimePassCompleteDelegate();

        public TimePassCompleteDelegate TimePassComplete;
        
        #endregion

        private void Awake() {
            if(Instance == null) {
                Instance = this;
            }

            InitTimer();
            timeRecorderUI.InitTimeRecoder(this);

            //װ���¼�
            gameTime.RegisterCallback(
                HourGone, DayGone, MonthGone, YearGone
            );
            
        }

        private void Update() {
            //����recoder��ģ��
            UpdateTime();
            timeRecorderUI.UpdateTimeRecoder(gameTime.Time);
        }

        #region ģ�� ʱ������ ����
        private void InitTimer() {
            //������ʱ�� ��ʼ��ʱ
            timer = Timer.CreateTimer("GameTime");
            //Debug.Log("Start!:" + TimeSimulator.TimeSpeed.ToString());
            timer.StartTiming(
                TimeSimulator.TimeSpeed, false, true, OnTimerComplete
            );
            timer.PauseTimer();
        }

        private void OnTimerComplete() {
            
            //ʱ������
            gameTime.TimePass();

            if(TimePassComplete != null) {
                TimePassComplete();
            }
            //Debug.Log("һ���ȥ�ˣ���ʱ��" + TimeSimulator.TimeSpeed.ToString());
        }

        private bool UpdateTime() {
            if (timer.IsEnd) {
                timer.StartTiming(
                    TimeSimulator.TimeSpeed, false, true, OnTimerComplete
                );

                //��ʱ���
                return true;
            }

            //��δ��ʱ���
            return false;
        }

        /// <summary>
        /// ��ͣʱ�����
        /// </summary>
        public void StopTime() {
            timer.PauseTimer();
        }

        public void ContinueTime() {
            timer.ConnitueTimer();
        }

        /// <summary>
        /// �޸ĵ�ǰ�� ʱ�������ٶ�
        /// </summary>
        /// <param name="modify">ֻ����������</param>
        public void ModifyTimeLevel(int modify) {
            int currentTimeLevel = (int)CurrentSpeedLevel;
            if(modify > 0) {
                if (currentTimeLevel >= 4) {
                    return;
                }
                currentTimeLevel++;
                
            } else if(modify < 0) {
                if(currentTimeLevel <= 0) {
                    return;
                }
                currentTimeLevel--;

            }
            
            CurrentSpeedLevel = (TimeSpeedLevel)currentTimeLevel;

            Debug.Log("current time speed: " + CurrentSpeedLevel);
        }
        
        /// <summary>
        /// ִ��ʱ�����ţ������¼�
        /// </summary>
        public void CompleteTime() {
            gameTime.TimePass();
        }

        #endregion

        #region Time�¼��ӿ�
        // ���� ���� - ������ ģʽ�������ⲿע���¼�
        public void HourGone() {
            for (int i = 0; i < hourCallback.Count; i++) {
                //try {
                    hourCallback[i]();
                //} catch {
                //    throw new Exception("ע���Сʱ�ắ������");
                //}
            }
        }

        public void DayGone() {
            for (int i = 0; i < dayCallback.Count; i++)
            {
                //try {
                    dayCallback[i]();
                //} catch {
                //    throw new Exception("ע����սắ������");
                //}
            }
        }

        public void MonthGone() {
            for (int i = 0; i < monthCallback.Count; i++) {
                try {
                    monthCallback[i]();
                } catch {
                    throw new Exception("ע����½ắ������");
                }
            }
        }

        public void YearGone() {
            for (int i = 0; i < yearCallback.Count; i++) {
                try {
                    yearCallback[i]();
                } catch {
                    throw new Exception("ע�����ắ������");
                }
            }
        } 
        #endregion

    }

    public interface TimeInterface {
        public void HourGone();
        public void DayGone();
        public void MonthGone();
        public void YearGone();

    }
}