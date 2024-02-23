using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.Recorder {
    /// <summary>
    /// ʵ��һ���򵥵ļ�ʱ������
    /// </summary>
    public class BaseRecorder {

        // ��ʼ�������� �¼�
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


        // ���ö��ٴκ��������¼�
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

            // ����ʵ�ʵ��߼�,���Բ�����
            if(callback != null) {
                callback.Invoke();
            }
            
        }

        public virtual void RestartRecorder(int newCount = -1) {

            // �����趨ʱ��
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