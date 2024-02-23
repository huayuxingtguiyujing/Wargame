using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.Application {

    /// <summary>
    /// 计时器工具类
    /// </summary>
    public class Timer : MonoBehaviour{
        public delegate void CompleteEvent();
        public delegate void UpdateEvent(float time);

        UpdateEvent OnUpdate;
        CompleteEvent OnCompleted;

        // 计时时间
        private float timeTarget;

        // 开始计时时间
        private float timeStart;

        // 计时偏差
        private float offsetTime;

        // 是否正在计时
        private bool isTimer;

        // 计时结束后是否销毁
        private bool isDestory = true;

        // 计时是否结束
        private bool isEnd;
        public bool IsEnd { get => isEnd; }

        // 是否忽略时间速率
        private bool isIgnoreTimeScale = true;

        //是否重复
        private bool isRepeate;

        //当前时间 正计时
        private float now;

        //倒计时
        private float downNow;

        //是否是倒计时
        private bool isDownNow = false;

        // 是否使用游戏的真实时间 不依赖游戏的时间速度
        // Time.realtimeSinceStartup
        float TimeNow {
            get { return isIgnoreTimeScale ? Time.realtimeSinceStartup : Time.time; }
        }

        /// <summary>
        /// 创建计时器:名字  根据名字可以创建多个计时器对象
        /// </summary>
        public static Timer CreateTimer(string gobjName = "Timer") {
            GameObject gameObject = new GameObject(gobjName);
            Timer timer = gameObject.AddComponent<Timer>();
            return timer;
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="time_">目标时间</param>
        /// <param name="isDownNow">是否是倒计时</param>
        /// <param name="onCompleted_">完成回调函数</param>
        /// <param name="update">计时器进程回调函数</param>
        /// <param name="isIgnoreTimeScale_">是否忽略时间倍数</param>
        /// <param name="isRepeate_">是否重复</param>
        /// <param name="isDestory_">完成后是否销毁</param>
        public void StartTiming(float timeTarget, bool isDestory = true, bool isDownNow = false,
            CompleteEvent onCompleted_ = null, UpdateEvent update = null,
            bool isIgnoreTimeScale = true, bool isRepeate = false, 
            float offsetTime = 0, bool isEnd = false, bool isTimer = true) {

            this.timeTarget = timeTarget;
            this.isIgnoreTimeScale = isIgnoreTimeScale;
            this.isRepeate = isRepeate;
            this.isDestory = isDestory;
            this.offsetTime = offsetTime;
            this.isEnd = isEnd;
            this.isTimer = isTimer;
            this.isDownNow = isDownNow;
            timeStart = TimeNow;

            if (onCompleted_ != null)
                OnCompleted = onCompleted_;
            if (update != null)
                OnUpdate = update;

        }

        private void Update() {
            if (isTimer) {
                //当前程序运行时间 减去起始时间 获得当前时间
                now = TimeNow - offsetTime - timeStart;

                //获得剩余倒计时
                downNow = timeTarget - now; ;

                //Debug.Log("剩余倒计时" + downNow);

                //执行 OnUpdate 事件
                if (OnUpdate != null) {
                    if (isDownNow) {
                        OnUpdate(downNow);
                    } else {
                        OnUpdate(now);
                    }
                }

                if (downNow < 0) {
                    //执行 OnCompleted 事件
                    if (OnCompleted != null) {
                        OnCompleted();
                    }

                    //判断是否重复计时
                    if (!isRepeate) {
                        Destory();
                    } else {
                        RestartTimer();
                    }
                        
                }
            }
        }

        /// <summary>
        /// 获取剩余时间
        /// </summary>
        /// <returns></returns>
        public float GetLastTimeNow() {
            return Mathf.Clamp(timeTarget - now, 0, timeTarget);
        }

        /// <summary>
        /// 计时结束
        /// </summary>
        public void Destory() {
            isTimer = false;
            isEnd = true;
            if (isDestory) {
                GameObject.Destroy(gameObject);
            }
        }

        #region 控制计时器的操作
        float _pauseTime;

        /// <summary>
        /// 暂停计时
        /// </summary>
        public void PauseTimer() {
            if (isEnd) {
                Debug.Log("计时已经结束！");
                return;
            } else {
                if (isTimer) {
                    isTimer = false;
                    _pauseTime = TimeNow;
                }
            }
        }

        /// <summary>
        /// 继续计时
        /// </summary>
        public void ConnitueTimer() {
            if (isEnd) {
                Debug.LogWarning("计时已经结束！请重新计时！");
                return;
            } else {
                if (!isTimer) {
                    offsetTime += (TimeNow - _pauseTime);
                    isTimer = true;
                }
            }
        }

        /// <summary>
        /// 重新计时
        /// </summary>
        public void RestartTimer() {
            timeStart = TimeNow;
            offsetTime = 0;
        }

        /// <summary>
        /// 更改目标时间
        /// </summary>
        public void ChangeTargetTime(float time_) {
            timeTarget = time_;
            timeStart = TimeNow;
        }

        #endregion
    }
}