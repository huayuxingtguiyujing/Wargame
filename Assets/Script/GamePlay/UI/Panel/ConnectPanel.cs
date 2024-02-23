using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.Infrastructure.NetworkPackage;
using System;
using UnityEngine.Events;

namespace WarGame_True.GamePlay.UI {
    public class ConnectPanel : BasePopUI {

        public static ConnectPanel Instance;

        public const string defaultIP = "127.0.0.1";
        public const int defaultPortNum = 9998;

        [Header("输入框组件")]
        [SerializeField] InputItem PlayerNameInputItem;
        [SerializeField] InputItem IpInputItem;
        [SerializeField] InputItem PortInputItem;

        [Header("按钮组件")]
        [SerializeField] Button HostButton;
        [SerializeField] Button JoinButton;
        [SerializeField] Button UseLobbyButton;

        [Header("加载指示器")]
        public GameObject LoadingIndicator;

        [Header("提示")]
        [SerializeField] FadeText FadeText;

        public delegate void ConnectionDelegate();
        private ConnectionDelegate successCallback;
        private ConnectionDelegate failCallback;

        /// <summary>
        /// 初始化 ConnectPanel,放到场景管理器中执行
        /// </summary>
        public void InitConnectPanel(ConnectionDelegate successCallback, ConnectionDelegate failCallback, UnityAction showLobbyPanel) {
            Instance = this;

            LoadingIndicator.SetActive(false);

            PlayerNameInputItem.InitInputItem(true, true, false);
            IpInputItem.InitInputItem(true, true, false);
            PortInputItem.InitInputItem(true, true, false);

            // 设置valuechange回调
            IpInputItem.SetOnValueChangeCall(IPInputValueChange);
            PortInputItem.SetOnValueChangeCall(PortInputValueChange);

            HostButton.onClick.AddListener(HostWithIP);
            JoinButton.onClick.AddListener(JoinWithIP);
            UseLobbyButton.onClick.AddListener(showLobbyPanel);

            FadeText.InitText();

            this.successCallback = successCallback;
            this.failCallback = failCallback;
        }

        #region HostIP、JoinIP
        /// <summary>
        /// 以当前ip和port主持游戏
        /// </summary>
        public void HostWithIP() {
            string name = PlayerNameInputItem.GetInputText();
            string ip = IpInputItem.GetInputText();
            string port = PortInputItem.GetInputText();
            HostWithIP(name, ip, port);
        }
        private void HostWithIP(string name, string ip, string port) {
            Debug.Log("准备连接游戏:" + ip + "," + port);

            int.TryParse(port, out int portNum);
            if (portNum <= 0) {
                portNum = defaultPortNum;
            }

            if (string.IsNullOrEmpty(ip)) {
                ip = defaultIP;
            }

            ConnectionManager.Instance.StartHostIp(name, ip, portNum);

            HandleConnectingUI();
        }

        /// <summary>
        /// 加入指定的ip 进行游戏
        /// </summary>
        public void JoinWithIP() {
            string name = PlayerNameInputItem.GetInputText();
            string ip = IpInputItem.GetInputText();
            string port = PortInputItem.GetInputText();
            JoinWithIP(name, ip, port);
        }
        private void JoinWithIP(string name, string ip, string port) {
            Debug.Log("准备加入游戏:" + ip + "," + port);

            int.TryParse(port, out int portNum);
            if(portNum <= 0) {
                portNum = defaultPortNum;
            }

            ConnectionManager.Instance.StartClientIp(name, ip, portNum);

            HandleConnectingUI();
        }

        /// <summary>
        /// 展示加载界面
        /// </summary>
        private void HandleConnectingUI() {

            LoadingIndicator.SetActive(true);
            // 结束时的回调
            void OnTimeElapsed() {
                Hide();
                LoadingIndicator.SetActive(false);
            }
            void OnLoadSuccess() {
                OnTimeElapsed();
                // TODO: 跳转页面
                successCallback.Invoke();
            }
            void OnLoadFail() {
                OnTimeElapsed();
                // TODO: 显示加载失败信息
                failCallback.Invoke();
            }

            // 获取utp组件
            var utp = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            int maxConnectAttempts = utp.MaxConnectAttempts;
            int connectTimeoutMS = utp.ConnectTimeoutMS;

            StartCoroutine(DisplayUTPConnection(maxConnectAttempts, connectTimeoutMS, OnLoadSuccess, OnLoadFail));
        }

        private IEnumerator DisplayUTPConnection(int maxReconnectAttempts, int connectTimeoutMS, Action SuccessCall, Action FailCall) {
            float connectionDuration = maxReconnectAttempts * connectTimeoutMS / 1000f;

            int seconds = Mathf.CeilToInt(connectionDuration);

            bool hasConnected = false;
            while (seconds > 0 && !hasConnected) {
                hasConnected = NetworkManager.Singleton.IsClient 
                    || NetworkManager.Singleton.IsServer 
                    || NetworkManager.Singleton.IsHost;
                //Debug.Log("是否连接上:" + hasConnected);
                yield return new WaitForSeconds(1f);
                seconds--;
            }

            // 结束时进行回调
            if (hasConnected) {
                SuccessCall();
            } else {
                FailCall();
            }

        }

        #endregion


        #region 清理IP、Port的输入
        /// <summary>
        /// IP输入回调
        /// </summary>
        public void IPInputValueChange(string dirtyString) {
            dirtyString = SantilizeIP(dirtyString);
            IpInputItem.SetInputText(dirtyString);
        }

        /// <summary>
        /// 清洁IP中非法的字符
        /// </summary>
        public string SantilizeIP(string dirtyString) {
            // IP中不能含有除 数字 和 . 以外的字符
            // 使用正则表达式 匹配字符用空字符替代
            return Regex.Replace(dirtyString, "[^0-9.]", "");
        }

        public void PortInputValueChange(string dirtyString) {
            dirtyString = SantilizePort(dirtyString);
            PortInputItem.SetInputText(dirtyString);
        }

        public string SantilizePort(string dirtyString) {
            return Regex.Replace(dirtyString, "[^0-9]", "");
        } 
        #endregion

        public void DebugReason(string reason) {
            FadeText.ShowNotice(reason, 2);
        }

    }
}