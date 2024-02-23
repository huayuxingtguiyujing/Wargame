using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.Recorder {
    /// <summary>
    /// 实现一个简单的计时器功能
    /// </summary>
    public class BaseRecorder {

        // 初始化、结束 事件
        private bool isenter = true;
        private Action enterEvent;
        private bool isOver = false;
        private Action exitEvent;
        public void RegisterEnterEvent(Action callback) {
            enterEvent = callback;
        }
        public void RegisterExitEvent(Action callback) {
            exitEvent = callback;
        }


        // 调用多少次后会结束该事件
        int count = 1;
        int initCount = 1;
        public int LastCountTime {  get { return count; } }

        public BaseRecorder(int count) {
            enterEvent = VoidEvent;
            exitEvent = VoidEvent;
            this.count = count;
            initCount = 1;
        }

        public virtual void CountRecorder(Action callback = null) {
            if(count <= 0) {
                isOver = true;
                exitEvent();
                return;
            }

            if (isenter) {
                isenter = false;
                enterEvent();
            }

            count--;

            // 调用实际的逻辑,可以不传入
            if(callback != null) {
                callback.Invoke();
            }
            
        }

        public virtual void RestartRecorder(int newCount = -1) {

            // 重新设定时间
            if(newCount > 0) {
                count = newCount;
            } else {
                count = initCount;
            }
            
            isOver = false;
        }

        public virtual bool IsOver() {
            return isOver;
        } 

        private void VoidEvent() { }

    }
}