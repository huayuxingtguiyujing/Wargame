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

        [Header("��������")]
        [SerializeField] InputItem PlayerNameInputItem;
        [SerializeField] InputItem IpInputItem;
        [SerializeField] InputItem PortInputItem;

        [Header("��ť���")]
        [SerializeField] Button HostButton;
        [SerializeField] Button JoinButton;
        [SerializeField] Button UseLobbyButton;

        [Header("����ָʾ��")]
        public GameObject LoadingIndicator;

        [Header("��ʾ")]
        [SerializeField] FadeText FadeText;

        public delegate void ConnectionDelegate();
        private ConnectionDelegate successCallback;
        private ConnectionDelegate failCallback;

        /// <summary>
        /// ��ʼ�� ConnectPanel,�ŵ�������������ִ��
        /// </summary>
        public void InitConnectPanel(ConnectionDelegate successCallback, ConnectionDelegate failCallback, UnityAction showLobbyPanel) {
            Instance = this;

            LoadingIndicator.SetActive(false);

            PlayerNameInputItem.InitInputItem(true, true, false);
            IpInputItem.InitInputItem(true, true, false);
            PortInputItem.InitInputItem(true, true, false);

            // ����valuechange�ص�
            IpInputItem.SetOnValueChangeCall(IPInputValueChange);
            PortInputItem.SetOnValueChangeCall(PortInputValueChange);

            HostButton.onClick.AddListener(HostWithIP);
            JoinButton.onClick.AddListener(JoinWithIP);
            UseLobbyButton.onClick.AddListener(showLobbyPanel);

            FadeText.InitText();

            this.successCallback = successCallback;
            this.failCallback = failCallback;
        }

        #region HostIP��JoinIP
        /// <summary>
        /// �Ե�ǰip��port������Ϸ
        /// </summary>
        public void HostWithIP() {
            string name = PlayerNameInputItem.GetInputText();
            string ip = IpInputItem.GetInputText();
            string port = PortInputItem.GetInputText();
            HostWithIP(name, ip, port);
        }
        private void HostWithIP(string name, string ip, string port) {
            Debug.Log("׼��������Ϸ:" + ip + "," + port);

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
        /// ����ָ����ip ������Ϸ
        /// </summary>
        public void JoinWithIP() {
            string name = PlayerNameInputItem.GetInputText();
            string ip = IpInputItem.GetInputText();
            string port = PortInputItem.GetInputText();
            JoinWithIP(name, ip, port);
        }
        private void JoinWithIP(string name, string ip, string port) {
            Debug.Log("׼��������Ϸ:" + ip + "," + port);

            int.TryParse(port, out int portNum);
            if(portNum <= 0) {
                portNum = defaultPortNum;
            }

            ConnectionManager.Instance.StartClientIp(name, ip, portNum);

            HandleConnectingUI();
        }

        /// <summary>
        /// չʾ���ؽ���
        /// </summary>
        private void HandleConnectingUI() {

            LoadingIndicator.SetActive(true);
            // ����ʱ�Ļص�
            void OnTimeElapsed() {
                Hide();
                LoadingIndicator.SetActive(false);
            }
            void OnLoadSuccess() {
                OnTimeElapsed();
                // TODO: ��תҳ��
                successCallback.Invoke();
            }
            void OnLoadFail() {
                OnTimeElapsed();
                // TODO: ��ʾ����ʧ����Ϣ
                failCallback.Invoke();
            }

            // ��ȡutp���
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
                //Debug.Log("�Ƿ�������:" + hasConnected);
                yield return new WaitForSeconds(1f);
                seconds--;
            }

            // ����ʱ���лص�
            if (hasConnected) {
                SuccessCall();
            } else {
                FailCall();
            }

        }

        #endregion


        #region ����IP��Port������
        /// <summary>
        /// IP����ص�
        /// </summary>
        public void IPInputValueChange(string dirtyString) {
            dirtyString = SantilizeIP(dirtyString);
            IpInputItem.SetInputText(dirtyString);
        }

        /// <summary>
        /// ���IP�зǷ����ַ�
        /// </summary>
        public string SantilizeIP(string dirtyString) {
            // IP�в��ܺ��г� ���� �� . ������ַ�
            // ʹ��������ʽ ƥ���ַ��ÿ��ַ����
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