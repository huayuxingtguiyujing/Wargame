using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.Application {

    /// <summary>
    /// ��ʱ��������
    /// </summary>
    public class Timer : MonoBehaviour{
        public delegate void CompleteEvent();
        public delegate void UpdateEvent(float time);

        UpdateEvent OnUpdate;
        CompleteEvent OnCompleted;

        // ��ʱʱ��
        private float timeTarget;

        // ��ʼ��ʱʱ��
        private float timeStart;

        // ��ʱƫ��
        private float offsetTime;

        // �Ƿ����ڼ�ʱ
        private bool isTimer;

        // ��ʱ�������Ƿ�����
        private bool isDestory = true;

        // ��ʱ�Ƿ����
        private bool isEnd;
        public bool IsEnd { get => isEnd; }

        // �Ƿ����ʱ������
        private bool isIgnoreTimeScale = true;

        //�Ƿ��ظ�
        private bool isRepeate;

        //��ǰʱ�� ����ʱ
        private float now;

        //����ʱ
        private float downNow;

        //�Ƿ��ǵ���ʱ
        private bool isDownNow = false;

        // �Ƿ�ʹ����Ϸ����ʵʱ�� ��������Ϸ��ʱ���ٶ�
        // Time.realtimeSinceStartup
        float TimeNow {
            get { return isIgnoreTimeScale ? Time.realtimeSinceStartup : Time.time; }
        }

        /// <summary>
        /// ������ʱ��:����  �������ֿ��Դ��������ʱ������
        /// </summary>
        public static Timer CreateTimer(string gobjName = "Timer") {
            GameObject gameObject = new GameObject(gobjName);
            Timer timer = gameObject.AddComponent<Timer>();
            return timer;
        }

        /// <summary>
        /// ��ʼ��ʱ
        /// </summary>
        /// <param name="time_">Ŀ��ʱ��</param>
        /// <param name="isDownNow">�Ƿ��ǵ���ʱ</param>
        /// <param name="onCompleted_">��ɻص�����</param>
        /// <param name="update">��ʱ�����̻ص�����</param>
        /// <param name="isIgnoreTimeScale_">�Ƿ����ʱ�䱶��</param>
        /// <param name="isRepeate_">�Ƿ��ظ�</param>
        /// <param name="isDestory_">��ɺ��Ƿ�����</param>
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
                //��ǰ��������ʱ�� ��ȥ��ʼʱ�� ��õ�ǰʱ��
                now = TimeNow - offsetTime - timeStart;

                //���ʣ�൹��ʱ
                downNow = timeTarget - now; ;

                //Debug.Log("ʣ�൹��ʱ" + downNow);

                //ִ�� OnUpdate �¼�
                if (OnUpdate != null) {
                    if (isDownNow) {
                        OnUpdate(downNow);
                    } else {
                        OnUpdate(now);
                    }
                }

                if (downNow < 0) {
                    //ִ�� OnCompleted �¼�
                    if (OnCompleted != null) {
                        OnCompleted();
                    }

                    //�ж��Ƿ��ظ���ʱ
                    if (!isRepeate) {
                        Destory();
                    } else {
                        RestartTimer();
                    }
                        
                }
            }
        }

        /// <summary>
        /// ��ȡʣ��ʱ��
        /// </summary>
        /// <returns></returns>
        public float GetLastTimeNow() {
            return Mathf.Clamp(timeTarget - now, 0, timeTarget);
        }

        /// <summary>
        /// ��ʱ����
        /// </summary>
        public void Destory() {
            isTimer = false;
            isEnd = true;
            if (isDestory) {
                GameObject.Destroy(gameObject);
            }
        }

        #region ���Ƽ�ʱ���Ĳ���
        float _pauseTime;

        /// <summary>
        /// ��ͣ��ʱ
        /// </summary>
        public void PauseTimer() {
            if (isEnd) {
                Debug.Log("��ʱ�Ѿ�������");
                return;
            } else {
                if (isTimer) {
                    isTimer = false;
                    _pauseTime = TimeNow;
                }
            }
        }

        /// <summary>
        /// ������ʱ
        /// </summary>
        public void ConnitueTimer() {
            if (isEnd) {
                Debug.LogWarning("��ʱ�Ѿ������������¼�ʱ��");
                return;
            } else {
                if (!isTimer) {
                    offsetTime += (TimeNow - _pauseTime);
                    isTimer = true;
                }
            }
        }

        /// <summary>
        /// ���¼�ʱ
        /// </summary>
        public void RestartTimer() {
            timeStart = TimeNow;
            offsetTime = 0;
        }

        /// <summary>
        /// ����Ŀ��ʱ��
        /// </summary>
        public void ChangeTargetTime(float time_) {
            timeTarget = time_;
            timeStart = TimeNow;
        }

        #endregion
    }
}