using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.UI;
using WarGame_True.UGS.LobbyPack;

namespace WarGame_True.Infrastructure.NetworkPackage {
    /// <summary>
    /// ���ӹ�������������ApplicationController����
    /// </summary>
    public class ConnectionManager : MonoBehaviour {

        public static ConnectionManager Instance;

        [Header("���������")]
        [Tooltip("���ڵ���״̬Ҳ�Ὺ��")]
        private NetworkManager m_NetworkManager;
        public NetworkManager networkManager { 
            get {
                if(m_NetworkManager == null) {
                    m_NetworkManager = NetworkManager.Singleton;
                }
                return m_NetworkManager;
            }
            set { m_NetworkManager = value; }
        }

        private ConnectionState currentState;

        internal readonly OfflineState offlineState = new OfflineState();
        
        internal readonly HostingState hostingState = new HostingState();
        internal readonly StartHostState startHostState = new StartHostState();
        
        internal readonly ClientConnectedState clientConnectedState = new ClientConnectedState();
        internal readonly ClientConnectingState clientConnectingState = new ClientConnectingState();
        internal readonly ClientReconnectingState clientReconnectingState = new ClientReconnectingState();

        public void InitConnectManager(NetworkManager networkManager) {
            Instance = this;

            this.networkManager = networkManager;

            offlineState.SetConnectionManager(this);
            hostingState.SetConnectionManager(this);
            startHostState.SetConnectionManager(this);
            clientConnectedState.SetConnectionManager(this);
            clientConnectingState.SetConnectionManager(this);
            clientReconnectingState.SetConnectionManager(this);

            currentState = offlineState;

            // ע��ص�
            networkManager.OnClientConnectedCallback += OnClientConnectedCall;
            networkManager.OnClientDisconnectCallback += OnClientDisconnectCall;
            networkManager.OnServerStarted += OnServerStart;
            networkManager.OnServerStopped += OnServerStop;
            networkManager.ConnectionApprovalCallback = OnConnectionApproval;
            networkManager.OnTransportFailure += OnTransportFailure;
        }

        private void OnDestroy() {
            // ע���ص�
            // TODO: Ϊʲô��Ҫע�����ص������Ų�����
            networkManager.OnClientConnectedCallback -= OnClientConnectedCall;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnectCall;
            networkManager.OnServerStarted -= OnServerStart;
            networkManager.OnServerStopped -= OnServerStop;
            //networkManager.ConnectionApprovalCallback -= OnConnectionApproval;
            networkManager.OnTransportFailure -= OnTransportFailure;
        }

        internal void ChangeState(ConnectionState nextState) {
            Debug.Log($"����״̬�Ѿ��ı���:{currentState.GetType().Name} to {nextState.GetType().Name}");

            if (currentState != null) {
                currentState.Exit();
            }
            currentState = nextState;
            currentState.Enter();
        }

        #region �ⲿ�ӿ�

        public void StartClientIp(string playerName, string ipaddress, int port) {
            currentState.StartClientIP(playerName, ipaddress, port);
        }

        public void StartHostIp(string playerName, string ipaddress, int port) {
            currentState.StartHostIP(playerName, ipaddress, port);
        }

        public void StartClientLobby(string playerName) {
            currentState.StartClientLobby(playerName);
        }

        public void StartHostLobby(string playerName) {
            currentState.StartHostLobby(playerName);
        }

        #endregion

        #region �����¼��ص�

        private void OnClientConnectedCall(ulong clientId) {
            //DebugPanel.Instance.LogNotice("�ͻ������ӳɹ�����ǰ��state:" + currentState.GetType().ToString());
            //Debug.Log("�ͻ������ӳɹ�����ǰ��state:" + currentState.GetType().ToString());
            currentState.OnClientConnected(clientId);
        }

        private void OnClientDisconnectCall(ulong clientId) {
            //DebugPanel.Instance.LogNotice("�ͻ��˶Ͽ�����ǰ��state:" + currentState.GetType().ToString());
            //Debug.Log("�ͻ��˶Ͽ�����ǰ��state:" + currentState.GetType().ToString());
            currentState.OnClientDisconnect(clientId);
        }

        private void OnServerStart() {
            //DebugPanel.Instance.LogNotice("��������������ǰ��state:" + currentState.GetType().ToString());
            //Debug.Log("��������������ǰ��state:" + currentState.GetType().ToString());
            currentState.OnServerStarted();
        }

        /// <summary>
        /// �������Ͽ��ص�
        /// </summary>
        /// <param name="IsStopHost">The first parameter of this event will be set to <see cref="true"/> when stopping a host instance and <see cref="false"/> when stopping a server instance.</param>
        private void OnServerStop(bool IsStopHost) {
            //DebugPanel.Instance.LogNotice("�������Ͽ�����ǰ��state:" + currentState.GetType().ToString());
            //Debug.Log("�������Ͽ�����ǰ��state:" + currentState.GetType().ToString());
            currentState.OnServerStopped(IsStopHost);
        }

        private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
            // ERROR: �������ˣ�
            // 1.5 NOTICE: ��NetworkManager������ ��Approval ����ʹ�˺�����Ч�����򿪺�Ĭ����Ҫ������֤�������ӣ���Ϊ�鷳
            //DebugPanel.Instance.LogNotice("Approval �¼�!����ǰ��state:" + currentState.GetType().ToString());
            //Debug.Log("Approval �¼�!����ǰ��state:" + currentState.GetType().ToString());
            currentState.OnConnectApproval(request, response);
        }

        private void OnTransportFailure() {
            //DebugPanel.Instance.LogNotice("����ʧ�ܣ���ǰ��state:" + currentState.GetType().ToString());
            Debug.Log("����ʧ�ܣ���ǰ��state:" + currentState.GetType().ToString());
            currentState.OnTransportFailure();
        }
        #endregion
    }

    [Serializable]
    public class ConnectionPayload {
        //payload:�����������Ч�غ�
        //ͨ���ڴ�������ʱ��Ϊ��ʹ���ݴ�����ɿ���Ҫ��ԭʼ���ݷ������䣬
        //������ÿһ�����ݵ�ͷ��β������һ���ĸ�����Ϣ�������������Ĵ�С��У��λ�ȣ�
        //�������൱�ڸ��Ѿ�������ԭʼ���ݼ�һЩ���ף���Щ�������ʾ���ã�
        //ʹ��ԭʼ���ݲ��׶�ʧ��һ�����ݼ��ϡ����ס����γ��˴���ͨ���Ļ������䵥Ԫ��
        //��������֡�����ݰ��������е�ԭʼ���ݾ���payload

        public string playerId;

        public string playerName;

        public bool isDebug;
    }
}