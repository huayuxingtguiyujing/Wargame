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
    /// 模拟游戏内时间流逝
    /// </summary>
    public class TimeSimulator : MonoBehaviour, TimeInterface {

        // 单例模式
        public static TimeSimulator Instance;

        #region 注册事件(发布者-订阅者)
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

        // 时间流逝速度
        public static TimeSpeedLevel CurrentSpeedLevel = TimeSpeedLevel.Level5;
        public static float TimeSpeed {
            get => TimeEnum.LevelSpeedDic[CurrentSpeedLevel];
            private set => TimeEnum.LevelSpeedDic[CurrentSpeedLevel] = value;
        }

        // 时间模拟器的UI
        [SerializeField] 
        private TimeRecorderUI timeRecorderUI;

        // 计时器
        Timer timer;

        #region 实现功能 的变量、回调
        // 游戏目前时间
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

            //装配事件
            gameTime.RegisterCallback(
                HourGone, DayGone, MonthGone, YearGone
            );
            
        }

        private void Update() {
            //根据recoder的模拟
            UpdateTime();
            timeRecorderUI.UpdateTimeRecoder(gameTime.Time);
        }

        #region 模拟 时间流逝 功能
        private void InitTimer() {
            //创建计时器 开始计时
            timer = Timer.CreateTimer("GameTime");
            //Debug.Log("Start!:" + TimeSimulator.TimeSpeed.ToString());
            timer.StartTiming(
                TimeSimulator.TimeSpeed, false, true, OnTimerComplete
            );
            timer.PauseTimer();
        }

        private void OnTimerComplete() {
            
            //时间流逝
            gameTime.TimePass();

            if(TimePassComplete != null) {
                TimePassComplete();
            }
            //Debug.Log("一天过去了！耗时：" + TimeSimulator.TimeSpeed.ToString());
        }

        private bool UpdateTime() {
            if (timer.IsEnd) {
                timer.StartTiming(
                    TimeSimulator.TimeSpeed, false, true, OnTimerComplete
                );

                //计时完毕
                return true;
            }

            //还未计时完毕
            return false;
        }

        /// <summary>
        /// 暂停时间结算
        /// </summary>
        public void StopTime() {
            timer.PauseTimer();
        }

        public void ContinueTime() {
            timer.ConnitueTimer();
        }

        /// <summary>
        /// 修改当前的 时间流逝速度
        /// </summary>
        /// <param name="modify">只区分正负号</param>
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
        /// 执行时间流逝，结算事件
        /// </summary>
        public void CompleteTime() {
            gameTime.TimePass();
        }

        #endregion

        #region Time事件接口
        // 采用 发布 - 订阅者 模式，允许外部注册事件
        public void HourGone() {
            for (int i = 0; i < hourCallback.Count; i++) {
                //try {
                    hourCallback[i]();
                //} catch {
                //    throw new Exception("注册的小时结函数出错！");
                //}
            }
        }

        public void DayGone() {
            for (int i = 0; i < dayCallback.Count; i++)
            {
                //try {
                    dayCallback[i]();
                //} catch {
                //    throw new Exception("注册的日结函数出错！");
                //}
            }
        }

        public void MonthGone() {
            for (int i = 0; i < monthCallback.Count; i++) {
                try {
                    monthCallback[i]();
                } catch {
                    throw new Exception("注册的月结函数出错！");
                }
            }
        }

        public void YearGone() {
            for (int i = 0; i < yearCallback.Count; i++) {
                try {
                    yearCallback[i]();
                } catch {
                    throw new Exception("注册的年结函数出错！");
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